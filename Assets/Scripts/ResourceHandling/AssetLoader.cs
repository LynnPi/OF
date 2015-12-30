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
    /// �������
    /// </summary>
    class Cache {
        public bool bLoaded = false;
        /// <summary>
        /// UnityEngine.Object
        /// </summary>
        public AssetBundle Bundle = null;
        /// <summary>
        /// bundle���MainAsset
        /// </summary>
        public UnityEngine.Object MainAsset = null;
        /// <summary>
        /// �л���ͼ��ʱ�����
        /// </summary>
        public bool DontDestoryOnLoad = false;

        /// <summary>
        /// ʵ�����˵�GameObject
        /// ����һЩ��Ҫ��Ԥ���ع�����ͬʱʵ��������Դ
        /// </summary>
        public GameObject InstantiatedGameObject;

        /// <summary>
        /// ��ȡ����GOҪ�����������Ա�ͳһ����
        /// </summary>
        private  List<GameObject> GoInUsed_ = new List<GameObject>();

        /// <summary>
        /// ��ȡһ��ʵ������GO
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
        /// �������л�ȡ����GO
        /// </summary>
        public void DestroyAllInstantiatedGameObject() {
            foreach( var go in GoInUsed_ ) {
                if( go )
                    Destroy( go );
            }
            GoInUsed_.Clear();
        }
    }

    // ��������
    private static readonly Dictionary<string, Ship>                ShipDefines = new Dictionary<string, Ship>();

    // ��Դ����
    private static readonly Dictionary<string, Cache>		        CachedAssets = new Dictionary<string, Cache>();

    // ս���еĻ���,�����ظ����ñ����ظ�gc�ͼ���
    private static Dictionary<string,List<GameObject>>              BattleCacheList = new Dictionary<string, List<GameObject>>();

    public static List<ServerInfo> ServerInfoList { get; private set; }

    public static XmlResourceList RemoteResourceList { get; private set; }//ÿ������ʱ���ص�������Դ����Ϣ
    public static XmlResourceList BuiltinResourceList { get; private set; }//��StreamingAsset�����Դ�б�

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
    /// ��ȡʵ�������GameObject
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
    /// Ԥ���ؽ�������
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
    /// ����·����ȡ����������
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
    /// Ԥ������Դ
    /// </summary>
    /// <param name="assetPathes"></param>
    /// <param name="dontDestoryOnLoad"></param>
    /// <param name="needBundle">�Ƿ���Ҫ����bundle���������Ҫ�������AssetBundle.Unload</param>
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
    /// ִ��Ԥ���ز���
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="dontDestoryOnLoad">�л�������ʱ���Ƿ�����</param>
    /// <param name="needBundle">�Ƿ���Ҫ����bundle���������Ҫ�������AssetBundle.Unload</param>
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
    /// ����·���ͷŻ������Դ
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
    /// �ͷŻ������Դ
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
    /// ���ط������б�
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
            // ѭ������servers
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
    /// ����Զ����Դ�б�
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
            RemoteResourceList.ReadFromXml( File.ReadAllText( infoPath ) ); //TODO:�쳣����
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
    /// ��ȡ������Դ�б�streaming assetĿ¼���
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
            BuiltinResourceList.ReadFromXml( File.ReadAllText( infoPath ) ); //TODO:�쳣����
        }
        else {
            Debug.Log( infoPath + " NOT FOUND" );
        }
    }

    /// <summary>
    /// ������Դ���·��ȡ��������http��ԴURL
    /// ���磺resPath = "res/fileinfo.xml"������"http://mobilex.game9z.com/wycs/res/fileinfo.xml"
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public static string GetResourceRemoteUrl( string resPath ) {
        Uri uriRes = new Uri( RuntimeSetting.RemoteResourceUri, resPath );
        return uriRes.AbsoluteUri;
    }

    /// <summary>
    /// ������Դ���·��ȡ�������ı�����ԴURL
    /// ���磺resPath = "res/fileinfo.xml"������"file:///c:/abc/wycs/res/fileinfo.xml"
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
            //���ﱾ���Ƿ���null�����ϲ��������Ի��Ƿ���һ����Ч��www
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