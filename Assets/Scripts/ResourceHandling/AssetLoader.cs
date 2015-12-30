using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using PalmPoineer;
using PalmPoineer.Mobile;
using Object = UnityEngine.Object;

public class ServerInfo {
    public int          Id;
    public string       Name;
    public string       Ip;
    public int          Port;
    public int          Status;
    public int          Attribute;
    public int          PvPPort;
    public int          Recommend;
}

public class BundleRequest {
    public AssetBundle Bundle;
    public string Error;
    public bool Ok {
        get { return string.IsNullOrEmpty( Error ); }
    }
}

public class AssetLoader : MonoBehaviour {

    public static AssetLoader Instance { get; private set; }

    /// <summary>
    /// 缓存对象
    /// </summary>
    class Cache {
        public bool bLoaded = false;
        /// <summary>
        /// UnityEngine.Object
        /// </summary>
        public AssetBundle Bundle = null;
        /// <summary>
        /// bundle里的MainAsset
        /// </summary>
        public UnityEngine.Object MainAsset = null;
        /// <summary>
        /// 切换地图的时候不清除
        /// </summary>
        public bool DontDestoryOnLoad = false;

        /// <summary>
        /// 实例化了的GameObject
        /// 用于一些需要在预加载过程中同时实例化的资源
        /// </summary>
        public GameObject InstantiatedGameObject;

        /// <summary>
        /// 已取过的GO要缓存起来，以便统一销毁
        /// </summary>
        private  List<GameObject> GoInUsed_ = new List<GameObject>();

        /// <summary>
        /// 获取一个实例化的GO
        /// </summary>
        /// <returns></returns>
        public GameObject GetInstantiatedGameObject() {
            GameObject go;
            if( !InstantiatedGameObject ) {
                if( MainAsset == null )
                    return null;
                go = Instantiate( MainAsset ) as GameObject;
                GoInUsed_.Add( go );
                return go;
            }
            go = InstantiatedGameObject;
            InstantiatedGameObject = null;
            return go;
        }

        /// <summary>
        /// 销毁所有获取过的GO
        /// </summary>
        public void DestroyAllInstantiatedGameObject() {
            foreach( var go in GoInUsed_ ) {
                if( go )
                    Destroy( go );
            }
            GoInUsed_.Clear();
        }
    }

    // 舰船配置
    private static readonly Dictionary<string, Ship>                ShipDefines = new Dictionary<string, Ship>();

    // 资源缓存
    private static readonly Dictionary<string, Cache>		        CachedAssets = new Dictionary<string, Cache>();

    // 战斗中的缓存,用于重复利用避免重复gc和加载
    private static Dictionary<string,List<GameObject>>              BattleCacheList = new Dictionary<string, List<GameObject>>();

    public static List<ServerInfo> ServerInfoList { get; private set; }

    public static XmlResourceList RemoteResourceList { get; private set; }//每次启动时加载的最新资源表信息
    public static XmlResourceList BuiltinResourceList { get; private set; }//在StreamingAsset里的资源列表

    public static void Init() {
        if( !Instance ) {
            GameObject go = new GameObject( "AssetLoader" );
            //Object.DontDestroyOnLoad( go );
            go.AddComponent<AssetLoader>();
        }
    }

    void Awake() {
        Instance = this;
    }

    void Start() {
        StartCoroutine( ReadBuiltinResourceList() );
    }

    public static void ClearAllBattleCaches() {
        foreach( var iter in BattleCacheList ) {
            List<GameObject> list = iter.Value;
            foreach( var listIter in list ) {
                Destroy( listIter );
            }
        }
        BattleCacheList.Clear();
    }

    public static void Destroy( string path, GameObject go ) {
        if( go == null ) return;
        List<GameObject> list;
        BattleCacheList.TryGetValue( path, out list );
        if( list == null ) {
            list = new List<GameObject>();
        }
        go.SetActive( false );
        list.Add( go );
        BattleCacheList[path] = list;
    }

    public static GameObject PlayEffect( string effectPath ) {
        List<GameObject> list;
        BattleCacheList.TryGetValue( effectPath, out list );
        if( list == null || list.Count <= 0 ) {
            Object effectGo = AssetLoader.GetAsset( effectPath );
            if( effectGo == null ) {
                return null;
            }
            GameObject go = Object.Instantiate( effectGo ) as GameObject;
            ReplaceShader( go );
            return go;
        }
        else {
            for( int i=list.Count-1; i >= 0; i-- ) {
                if( list[i] == null ) {
                    list.RemoveAt( i );
                    continue;
                }
            }
            if( list.Count <= 0 ) {
                BattleCacheList[effectPath] = list;
                return PlayEffect( effectPath );
            }

            GameObject go = list[0];
            list.RemoveAt( 0 );
            go.SetActive( true );
            return go;
        }
    }

    public static void ReplaceShader( GameObject go ) {
        if( !Application.isEditor ) {
            return;
        }
        if( go == null )
            return;
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        foreach( Renderer r in renderers ) {
            foreach( Material mat in r.sharedMaterials ) {
                if( !mat ) {
                    Debug.LogError( "Error", go );
                    continue;
                }
                if( !mat.shader ) continue;
                mat.shader = Shader.Find( mat.shader.name );
            }
        }
    }

    public static Object GetAsset( string path ) {
        Cache temp = null;
        CachedAssets.TryGetValue( path, out temp );
        if( temp == null )
            return null;
        return temp.MainAsset;
    }

    /// <summary>
    /// 获取实例化后的GameObject
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static GameObject GetInstantiatedGameObject( string path ) {
        Cache temp = null;
        CachedAssets.TryGetValue( path, out temp );
        if( temp == null )
            return null;
        return temp.GetInstantiatedGameObject();
    }

    public static AssetBundle GetBundle( string path ) {
        Cache temp = null;
        CachedAssets.TryGetValue( path, out temp );
        if( temp == null )
            return null;
        return temp.Bundle;
    }

    public static string GetUIPath( string uiName ) {
        if( !uiName.Contains( "res/ui/" ) )
            uiName = "res/ui/" + uiName;
        if( !uiName.Contains( ".ui" ) )
            uiName += ".ui";
        return uiName;
    }

    /// <summary>
    /// 预加载舰船数据
    /// </summary>
    /// <returns></returns>
    public Coroutine PreloadShipDefines() {
        XmlResourceList.Item[] allShip = null;
        allShip = RemoteResourceList.FindItem( PalmPoineer.Defines.AssetExt.ShipDefine );
        if( allShip == null ) {
            Debug.Log( "Ship List is null" );
            return null;
        }
        CoroutineJoin join = new CoroutineJoin( this );
        foreach( var item in allShip ) {
            join.StartSubtask( LoadShip( item.Path ) );
        }
        Debug.Log( "Total ship Defines: " + allShip.Length );
        return join.WaitForAll();
    }

    public IEnumerator PreloadEffects() {
        XmlResourceList.Item[] allEffects = AssetLoader.RemoteResourceList.FindItem( Defines.AssetExt.Effect );
        string[] effectList = new string[allEffects.Length];
        for( int i=0; i < allEffects.Length; i++ ) {
            effectList[i] = allEffects[i].Path;
        }
        yield return StartCoroutine( AssetLoader.PreloadAssets( true, false, effectList ) );
    }

    public IEnumerator PreloadBullet() {
        XmlResourceList.Item[] allBullets = AssetLoader.RemoteResourceList.FindItem( Defines.AssetExt.Bullet );
        string[] bulletList = new string[allBullets.Length];
        for( int i=0; i < allBullets.Length; i++ ) {
            bulletList[i] = allBullets[i].Path;
        }
        yield return StartCoroutine( AssetLoader.PreloadAssets( true, false, bulletList ) );
    }

    public IEnumerator PreloadAudio() {
        XmlResourceList.Item[] allAudios = AssetLoader.RemoteResourceList.FindItem( Defines.AssetExt.AudioClip );
        string[] audioList = new string[allAudios.Length];
        for( int i=0; i < allAudios.Length; i++ ) {
            audioList[i] = allAudios[i].Path;
        }
        yield return StartCoroutine( AssetLoader.PreloadAssets( true, false, audioList ) );
    }

    public IEnumerator PreloadDependUI() {
        List<string> list = Global.GetDependUIPathList();
        for( int i=0; i < list.Count; i++ )
            yield return StartCoroutine( AssetLoader.PreloadAssets( true, true, list[i] ) );
    }

    public IEnumerator PreloadAllUI() {
        XmlResourceList.Item[] allUserInterfaces = AssetLoader.RemoteResourceList.FindItem( Defines.AssetExt.UIPrefab );
        string[] uiList = new string[allUserInterfaces.Length];
        for( int i=0; i < allUserInterfaces.Length; i++ ) {
            uiList[i] = allUserInterfaces[i].Path;
        }
        yield return StartCoroutine( AssetLoader.PreloadAssets( true, false, uiList ) );
    }

    /// <summary>
    /// 根据路径读取舰船并缓存
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static IEnumerator LoadShip( string path ) {
        using( WWW www = SmartCreateWww( path ) ) {
            if( www != null ) {
                yield return www;
                if( !Utility.CheckWww( www ) ) yield break;
                string assetName = Path.GetFileNameWithoutExtension( path );
                if( !www.assetBundle.Contains( assetName ) ) {
                    Debug.Log( "Error, assetBundle doesn't have Ship Define: " + path );
                    yield break;
                }
                Object obj = www.assetBundle.LoadAsset<Object>( assetName );
                PalmPoineer.Mobile.Ship shipDef = obj as PalmPoineer.Mobile.Ship;
                www.assetBundle.Unload( false );
                if( !shipDef ) {
                    Debug.Log( "Error, Load Ship Define Failed: " + path );
                    yield break;
                }
                if( ShipDefines.ContainsKey( assetName ) ) {
                    Debug.Log( "ship config list already has key:" + assetName );
                    yield break;
                }
                ShipDefines.Add( assetName, shipDef );
            }
            else {
                yield return null;
            }
        }
    }

    /// <summary>
    /// 预加载资源
    /// </summary>
    /// <param name="assetPathes"></param>
    /// <param name="dontDestoryOnLoad"></param>
    /// <param name="needBundle">是否需要保留bundle，如果不需要，会调用AssetBundle.Unload</param>
    /// <returns></returns>
    public static IEnumerator PreloadAssets( bool dontDestoryOnLoad, bool needBundle, params string[] assetPathes ) {
        CoroutineJoin join = new CoroutineJoin( AssetLoader.Instance );
        foreach( var path in assetPathes ) {
            if( CachedAssets.ContainsKey( path ) ) {
                while( !CachedAssets[path].bLoaded ) {
                    yield return null;
                }
                continue;
            }
            Cache c = new Cache();
            c.bLoaded = false;
            CachedAssets[path] = c;
            join.StartSubtask( DoPreloadAsset( path, dontDestoryOnLoad, needBundle ) );
        }
        yield return join.WaitForAll();
    }

    /// <summary>
    /// 执行预加载操作
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="dontDestoryOnLoad">切换场景的时候是否销毁</param>
    /// <param name="needBundle">是否需要保留bundle，如果不需要，会调用AssetBundle.Unload</param>
    /// <returns></returns>
    private static IEnumerator DoPreloadAsset( string assetPath, bool dontDestoryOnLoad, bool needBundle ) {
        using( WWW www = SmartCreateWww( assetPath ) ) {
            if( www != null ) {
                yield return www;
                if( !Utility.CheckWww( www ) ) yield break;
                Cache ca = new Cache();
                string assetName = Path.GetFileNameWithoutExtension( assetPath );
                if( !assetPath.Contains( ".audio" ) ) {
                    assetName += ".prefab";
                }
                ca.MainAsset = www.assetBundle.LoadAsset<Object>( assetName );
                if( needBundle ) {
                    ca.Bundle = www.assetBundle;
                }
                else {
                    www.assetBundle.Unload( false );
                }
                ca.DontDestoryOnLoad = dontDestoryOnLoad;
                ca.bLoaded = true;
                CachedAssets[assetPath] = ca;
                www.Dispose();
            }
        }
    }

    /// <summary>
    /// 根据路径释放缓存的资源
    /// </summary>
    /// <param name="paths"></param>
    public static void UnloadCaches( params string[] paths ) {
        foreach( string path in paths ) {
            if( CachedAssets.ContainsKey( path ) ) {
                Cache c = CachedAssets[path];
                if( c.Bundle != null )
                    c.Bundle.Unload( false );
                CachedAssets.Remove( path );
            }
        }
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    /// <summary>
    /// 释放缓存的资源
    /// </summary>
    public static void ReleaseCaches() {
        string[] all = CachedAssets.Keys.ToArray();
        foreach( var path in all ) {
            Cache c = CachedAssets[path];
            if( c.DontDestoryOnLoad ) continue;
            if( c.Bundle != null )
                c.Bundle.Unload( false );
            c.DestroyAllInstantiatedGameObject();
            CachedAssets.Remove( path );
        }
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    public static IEnumerator PreloadShipModel( Ship ship ) {
        yield return Instance.StartCoroutine( PreloadAssets( false, false, ship.ModelPath ) );
    }

    /// <summary>
    /// 下载服务器列表
    /// </summary>
    /// <returns></returns>
    public static IEnumerator DownloadServerList() {
        if( ServerInfoList != null )
            ServerInfoList.Clear();
        else
            ServerInfoList = new List<ServerInfo>();
        using( WWW www = new WWW( RuntimeSetting.ServerlistPath ) ) {
            yield return www;
            if( !string.IsNullOrEmpty( www.error ) ) {
                Debug.Log( www.error );
                yield break;
            }
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml( www.text );
            XmlNodeList NodeList = XmlDoc.SelectSingleNode( "ServerList" ).ChildNodes;
            // 循环所有servers
            foreach( XmlNode Node in NodeList ) {
                if( Node.Name == "RefsCfg" ) {
                    foreach( XmlNode child in Node.ChildNodes ) {
                        if( child.Name == "Url" ) {
                            RuntimeSetting.ReferenceFilePath = child.InnerText;
                        }
                        if( child.Name == "Version" ) {
                            Debug.Log( "Reference Version:" + child.InnerText );
                            RuntimeSetting.ServerReferenceVersion = int.Parse( child.InnerText );
                        }

                    }
                }
                else {
                    ServerInfo info = new ServerInfo();
                    foreach( XmlNode child in Node.ChildNodes ) {
                        if( child.Name == "Id" ) {
                            info.Id = int.Parse( child.InnerText );
                        }
                        else if( child.Name == "Name" ) {
                            info.Name = child.InnerText;
                        }
                        else if( child.Name == "Ip" ) {
                            info.Ip = child.InnerText;
                        }
                        else if( child.Name == "Port" ) {
                            info.Port = int.Parse( child.InnerText );
                        }
                        else if( child.Name == "Status" ) {
                            info.Status = int.Parse( child.InnerText );
                        }
                        else if( child.Name == "Attribute" ) {
                            info.Attribute = int.Parse( child.InnerText );
                        }
                        else if( child.Name == "PvPPort" ) {
                            info.PvPPort = int.Parse( child.InnerText );
                        }
                        else if( child.Name == "Recommend" ) {
                            info.Recommend = int.Parse( child.InnerText );
                        }
                    }
                    Debug.Log( "Server Info: Name:" + info.Name + ",IP:" + info.Ip + ",Port:" + info.Port + ",PVPPort" + info.PvPPort + ",Status:" + info.Status + ",Attribute:" + info.Attribute );
                    ServerInfoList.Add( info );
                }
            }
        }
    }

    /// <summary>
    /// 加载远程资源列表
    /// </summary>
    /// <returns></returns>
    public static IEnumerator DownloadRemoteResourceList() {


#if STANDALONE

        string infoPath = Path.Combine( Application.streamingAssetsPath, RuntimeSetting.FileInfoPath );
        RemoteResourceList = new XmlResourceList();
        if( infoPath.Contains( "://" ) ) {
            using( WWW www = new WWW( infoPath ) ) {
                yield return www;
                if( !string.IsNullOrEmpty( www.error ) ) {
                    Debug.Log( www.error );
                    yield break;
                }
                RemoteResourceList.ReadFromXml( www.text );
            }
        }
        else if( File.Exists( infoPath ) ) {
            RemoteResourceList.ReadFromXml( File.ReadAllText( infoPath ) ); //TODO:异常处理
        }
        else {
            Debug.Log( infoPath + " NOT FOUND" );
        }
#else
        Uri uri = new Uri( RuntimeSetting.RemoteResourceUri, RuntimeSetting.FileInfoPath );
        using( WWW www = new WWW( uri.AbsoluteUri ) ) {
            yield return www;
            if( !string.IsNullOrEmpty( www.error ) ) {

                Debug.Log( www.error );
                yield break;
            }
            RemoteResourceList = new XmlResourceList();
            RemoteResourceList.ReadFromXml( www.text );
        }
#endif

    }

    /// <summary>
    /// 读取本地资源列表，streaming asset目录里的
    /// </summary>
    private IEnumerator ReadBuiltinResourceList() {
        string infoPath = Path.Combine( Application.streamingAssetsPath, RuntimeSetting.FileInfoPath );
        BuiltinResourceList = new XmlResourceList();
        if( infoPath.Contains( "://" ) ) {
            using( WWW www = new WWW( infoPath ) ) {
                yield return www;
                if( !string.IsNullOrEmpty( www.error ) ) {
                    Debug.Log( www.error );
                    yield break;
                }
                BuiltinResourceList.ReadFromXml( www.text );
            }
        }
        else if( File.Exists( infoPath ) ) {
            BuiltinResourceList.ReadFromXml( File.ReadAllText( infoPath ) ); //TODO:异常处理
        }
        else {
            Debug.Log( infoPath + " NOT FOUND" );
        }
    }

    /// <summary>
    /// 根据资源相对路径取得完整的http资源URL
    /// 例如：resPath = "res/fileinfo.xml"，返回"http://mobilex.game9z.com/wycs/res/fileinfo.xml"
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public static string GetResourceRemoteUrl( string resPath ) {
        Uri uriRes = new Uri( RuntimeSetting.RemoteResourceUri, resPath );
        return uriRes.AbsoluteUri;
    }

    /// <summary>
    /// 根据资源相对路径取得完整的本地资源URL
    /// 例如：resPath = "res/fileinfo.xml"，返回"file:///c:/abc/wycs/res/fileinfo.xml"
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public static string GetResourceLocalUrl( string resPath ) {
        return "file:///" + UnityEngine.Application.persistentDataPath + "/" + resPath;
    }

    public static WWW SmartCreateWww( string resPath ) {

    #if STANDALONE
        string url = Path.Combine( Application.streamingAssetsPath, resPath );
        if( !url.Contains( "://" ) ) {
            url = "file:///" + url;
        }
    #else
        XmlResourceList.Item item = RemoteResourceList.GetItem( resPath );
        XmlResourceList.Item builintItem = BuiltinResourceList.GetItem( resPath );
        if( item == null || item.Deleted ) {
            Debug.LogError( "Resource not in ResourceList: " + resPath );
            return null;
        }
        string url = "";
        if( builintItem != null && item.Version <= builintItem.Version ) {
            url = Path.Combine( Application.streamingAssetsPath, resPath );
            if( !url.Contains( "://" ) ) {
                url = "file:///" + url;
            }
        }
        else {
            url = "file:///" + UnityEngine.Application.persistentDataPath + "/" + resPath;
        }
#endif
        return new WWW( url );
    }

    public static WWW CreateRemoteWww( string resPath ) {
        XmlResourceList.Item item = RemoteResourceList.GetItem( resPath );
        if( item == null || item.Deleted ) {
            Debug.LogError( "Resource not in ResourceList: " + resPath );
            //这里本来是返回null，但上层会出错，所以还是返回一个无效的www
            return new WWW( GetResourceRemoteUrl( resPath ) );
        }
        return new WWW( GetResourceRemoteUrl( resPath ) );
    }

    public static Ship GetShipDefine( string resName ) {
        if( ShipDefines.ContainsKey( resName ) )
            return ShipDefines[resName];
        else {
            Debug.LogError( "Error Ship Define Not Found: " + resName );
            return null;
        }
    }

    public static void SetShowParts( Ship ship, GameObject go ) {
        //for( int i=0; i < ship.Parts.Length; i++ ) {
        //    Transform temp = go.transform.FindChild( ship.Parts[i].PartPath );
        //    if( temp == null ) {
        //        Debug.Log( "can not found " + ship.Parts[i].PartPath + " on Character config:" + chr.name + ",on " + go.name );
        //        continue;
        //    }
        //    go.transform.FindChild( ship.Parts[i].PartPath ).gameObject.SetActive( chr.Parts[i].Enabled );
        //}
    }
}