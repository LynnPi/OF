#region
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using PalmPoineer;
using PalmPoineer.Mobile;
#endregion


public class Global : MonoBehaviour {
    public static Global Instance { get; private set; }

    public static int LocalReferenceVersion;

    private static DateTime ServerTime;

    private static List<Sprite> SpriteList = new List<Sprite>();

    public void CancelHeartBeat () {
        CancelInvoke ( "SendHeartBeat" );
    }

    private void OnDestroy () {

    }

    void OnApplicationFocus( bool focusStatus ) {
        
    }

    void OnApplicationPause( bool pauseStatus ) {
        
    }

    private void Awake () {
        Debugger.EnableLog = true;//日志开关

        Instance = this;
        gameObject.AddComponent<CameraManager> ();
        gameObject.AddComponent<AudioManager> ();
        
        //DontDestroyOnLoad ( gameObject );
        GlobalConfig.LoadXml ();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if ( !Application.isEditor )
            Application.targetFrameRate = 30;

        LocalReferenceVersion = PlayerPrefs.GetInt ( "ReferenceVersion", 0 );

        StartCoroutine( LoadBin() );
    }

    static public GameObject AddChild( GameObject parent, GameObject prefab ) {
        GameObject go = GameObject.Instantiate( prefab ) as GameObject;
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo( go, "Create Object" );
#endif
        if( go != null && parent != null ) {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }

    public static GameObject CreateUI( string uiName, GameObject attachParent = null ) {
        uiName = uiName.ToLower();
        GameObject go = AssetLoader.GetInstantiatedGameObject( AssetLoader.GetUIPath( uiName ) );
        if( go == null )
            return go;
        go.name = uiName;  
        if( attachParent ) {
            go.transform.SetParent( attachParent.transform );
        }
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;//ui面板归位
        rt.sizeDelta = Vector2.zero;//统一以锚点为基准expand
        
        return go;
    }

    public static IEnumerator LerpSlider ( Slider slider, int maxValueNumber, int finallyValue, int finallyMaxValue ) {
        if ( slider == null ) yield break;
        float speed = 2f;
        while ( maxValueNumber > 0 ) {
            while ( slider.value < 1 ) {
                slider.value += ( speed * Time.deltaTime );
                yield return null;
            }
            maxValueNumber--;
            slider.value = 0;
            yield return null;
        }
        float percentage = ( float )finallyValue / ( float )finallyMaxValue;
        while ( slider.value < percentage ) {
            slider.value += ( speed * Time.deltaTime );
            yield return null;
        }
        slider.value = slider.value != percentage ? percentage : slider.value;
    }

    public static IEnumerator LerpSlider ( Slider slider, float maxValue, float finallyValue ) {
        if ( slider == null ) yield break;
        float speed = 2f;
        while ( maxValue < finallyValue ) {
            maxValue += Time.deltaTime * speed;
            if ( maxValue > finallyValue )
                slider.value = finallyValue % 1.0f;
            else
                slider.value = maxValue % 1.0f;
            yield return null;
        }
    }

    public static IEnumerator TypewriterDialog ( Text label, string content, int charsPerSecond = 40 ) {
        label.text = "";
        string text = "";
        int offset = 0;
        float nextChar = 0f;
        text = content;

        while( offset < text.Length ) {
            if( nextChar <= Time.time ) {
                charsPerSecond = Mathf.Max( 1, charsPerSecond );
                float delay = 1f / charsPerSecond;
                char c = text[offset];
                if( c == '.' || c == '\n' || c == '!' || c == '?' )
                    delay *= 4f;
                else if( c == '[' ) {
                    int index = text.IndexOf( ']', offset );
                    if( index > 0 )
                        offset = index;
                }
                nextChar = Time.time + delay;
                label.text = text.Substring( 0, ++offset );
            }
            yield return null;
        }
        label.text = content;
    }

    public static void ClearChild ( GameObject go ) {
        if ( !go ) return;
        List<GameObject> goList = new List<GameObject> ();
        for ( int i = 0; i < go.transform.childCount; i++ )
            goList.Add ( go.transform.GetChild ( i ).gameObject );

        go.transform.DetachChildren ();
        for ( int i = goList.Count - 1; i >= 0; i-- ) {
            Destroy ( goList[ i ] );
            goList.RemoveAt ( i );
        }
    }

    public static void WriteAccountAndPassword ( string Account, string Password ) {
        PlayerPrefs.SetString ( "Account", Account );
        PlayerPrefs.SetString ( "Password", Password );
        PlayerPrefs.Save ();
    }

    public static void WriteEnterServer ( string server ) {
        PlayerPrefs.SetString ( "LastEnterServer", server );
        PlayerPrefs.Save ();
    }

    /// <summary>
    /// 获得帐号密码
    /// </summary>
    /// <param name="Account"></param>
    /// <param name="Password"></param>
    /// <returns></returns>
    public static bool GetAccountAndPassword ( out string Account, out string Password ) {
        if ( !PlayerPrefs.HasKey ( "Account" ) || !PlayerPrefs.HasKey ( "Password" ) ) {
            Account = "";
            Password = "";
            return false;
        }
        Account = PlayerPrefs.GetString ( "Account" );
        Password = PlayerPrefs.GetString ( "Password" );
        return true;
    }

    /// <summary>
    /// 获得上次登录服务器
    /// </summary>
    /// <returns></returns>
    public static ServerInfo GetLastEnterServer() {
        ServerInfo serverInfo = null;
        string lastServer = !PlayerPrefs.HasKey( "LastEnterServer" ) ? string.Empty : PlayerPrefs.GetString( "LastEnterServer" );

        List<ServerInfo> list = AssetLoader.ServerInfoList;
        for ( int i = 0; i < list.Count; i++ ) {
            if ( list[ i ] == null ) continue;
            if ( list[ i ].Ip == lastServer ) {
                serverInfo = list[ i ];
            }
        }
        return serverInfo;
    }

    public static IEnumerator LoadBin () {
#if STANDALONE
                string localPath = Path.Combine( Application.streamingAssetsPath, "res/bins/reference.bytes" );
        {
            if( !localPath.Contains( "://" ) ) {
                localPath = "file:///" + localPath;
            }
            using( WWW www = new WWW( localPath ) ) {
                yield return www;
                GlobalConfig.InitBin();
                GlobalConfig.AnalyzeBin( www.bytes );
            }
        }
#else
        string localPath = Path.Combine( Application.persistentDataPath, "res/bins/reference.bytes" );
        //if ( Global.LocalReferenceVersion != RuntimeSetting.ServerReferenceVersion ) {
        //    // 下载服务器配置
        //    using ( WWW www = new WWW ( RuntimeSetting.ReferenceFilePath ) ) {
        //        yield return www;
        //        Debug.Log ( "Reference File loaded: " + www.error );
        //        if ( !string.IsNullOrEmpty ( www.error ) ) {
        //            Debug.Log ( "load bin fail" );

        //            yield break;
        //        }
        //        binFilePath = RuntimeSetting.ReferenceFilePath;
        //        GlobalConfig.InitBin ();
        //        GlobalConfig.AnalyzeBin ( www.bytes );
        //        Utility.CreateDirByFilePath ( localPath );
        //        File.WriteAllBytes ( localPath, www.bytes );
        //        Global.LocalReferenceVersion = RuntimeSetting.ServerReferenceVersion;
        //        PlayerPrefs.SetInt ( "ReferenceVersion", Global.LocalReferenceVersion );
        //        Debug.Log ( "Current Reference Version:" + Global.LocalReferenceVersion );               
        //    }
        //}
        //else
        {
            if( !localPath.Contains( "://" ) ) {
                localPath = "file:///" + localPath;
            }
            using( WWW www = new WWW( localPath ) ) {
                yield return www;
                GlobalConfig.InitBin();
                GlobalConfig.AnalyzeBin( www.bytes );
            }
        }
#endif

    }

    public void ShowSysNotice ( int ID ) {
        ShowNotice ( GlobalConfig.GetSysNotice ( ID ) );
    }

    public void ShowNotice ( string content ) {
        //if ( NoticeBar_ == null )
        //    return;
        //if ( content == null )
        //    return;
        //if ( string.IsNullOrEmpty ( content ) )
        //    return;
        //var tweenAlpha = NoticeBar_.GetComponent<TweenAlpha> ();
        //tweenAlpha.value = 1.0f;
        //tweenAlpha.from = 1.0f;
        //tweenAlpha.to = 0.0f;
        //TweenAlpha.Begin ( NoticeBar_, 2, 0 );

        //if ( !NoticeBar_.activeSelf )
        //    NoticeBar_.SetActive ( true );
        //UILabel label = NoticeBar_.GetComponent<UILabel> ();
        //label.text = "[f1c32e]" + content;
        //NoticeBarSprite_.width = label.width + 150;
    }

    public static void SetServerTime ( DateTime time ) {
        ServerTime = time;
    }

    public static DateTime GetServerTime () {
        return ServerTime;
    }

    public static void SetSprite ( Image image, string spriteName, bool bReset = false ) {
        if( image == null ) return;
        foreach( Sprite iter in SpriteList ) {
            if( iter.name != spriteName ) continue;
            image.sprite = iter;
            image.enabled = true;
            if( bReset ) {
                RectTransform rect = image.gameObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2( iter.textureRect.width, iter.textureRect.height );
            }
            return;
        }
        image.sprite = null;
        image.enabled = false;
    }

    public static List<string> GetPublicUIPathList() {
        List<string> list = new List<string>();
        list.Add( "res/ui/public.ui" );
        return list;
    }

    public static List<string> GetDependUIPathList() {
        List<string> list = new List<string>();
        list.AddRange( GetPublicUIPathList() );
        list.Add("res/ui/atlas_pop_window.ui");
        list.Add("res/ui/atlas_login.ui");
        list.Add("res/ui/atlas_user_status.ui");
        list.Add("res/ui/atlas_deploy.ui");
        list.Add("res/ui/atlas_battle.ui");
        return list;
    }
    
    public void InitPublicUIList() {
        List<string> list = GetPublicUIPathList();
        for( int i=0; i < list.Count; i++ ) {
            AssetBundle bundle = AssetLoader.GetBundle( list[i] );
            Sprite[] imageList = bundle.LoadAllAssets<Sprite>();
            SpriteList.AddRange( imageList.ToList() );
        }
        // 卸载依赖
        string[] tempList = new string[list.Count];
        for( int i=0; i < list.Count; i++ ) {
            tempList[i] = AssetLoader.GetUIPath( list[i] );
        }
        AssetLoader.UnloadCaches( tempList );
    }

    public static void PlaySound( string path, Vector3 position, bool threeD = true ) {
        Instance.StartCoroutine( Instance.DoPlaySound( path, position, threeD ) );
    }

    /// <summary>
    /// 显示伤害
    /// </summary>
    /// <param name="srcX">攻击者坐标</param>
    /// <param name="part">部件配置</param>
    /// <param name="damage">作用值</param>
    /// <param name="delayFrame">延迟幀数</param>
    /// <returns></returns>
    public static void ShowHurtEffect( Transform trans, Effect hurtEffect, Vector3 position ) {
        // 播放受击特效
        Instance.StartCoroutine( Instance.PlayEffect( trans, hurtEffect, position, false ) );
    }

    /// <summary>
    /// 死亡爆炸特效
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public IEnumerator ShowDeadEffect( Vector3 position, Quaternion rotation, float radius, string path ) {
        GameObject go = AssetLoader.PlayEffect( path );
        if( go != null ) {
            go.transform.localScale = new Vector3( radius, radius, radius );
            go.transform.position = position;
            go.transform.rotation = rotation;
        }
        PlaySound( "res/audio/destroy.audio", position );
        if( path == GlobalEffectDefine.ShipDie02 )
            yield return new WaitForSeconds( 5 );
        else
            yield return new WaitForSeconds( 3.8f );
        AssetLoader.Destroy( path, go );
    }

    /// <summary>
    /// 定点播放自销毁特效的接口
    /// </summary>
    public void ShowEffect(Vector3 position, string effectPath, Vector3 scale) {
        GameObject go = AssetLoader.PlayEffect( effectPath );
        if( go != null ) {
            go.transform.localScale = scale;
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// 吟唱特效
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="effect"></param>
    /// <returns></returns>
    public GameObject PlaySingEffect( Transform trans, Effect effect ) {
        if( effect == null )
            return null;
        if( string.IsNullOrEmpty( effect.effectPath ) )
            return null;

        GameObject go = AssetLoader.PlayEffect( effect.effectPath );
        if( go == null ) {
            Debug.Log( "can not find effect:" + effect.effectPath );
            return go;
        }
        go.transform.parent = trans;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        PlaySound( effect.audioPath, go.transform.position );

        return go;
    }

    /// <summary>
    /// 播放特效
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="effect"></param>
    /// <param name="position"></param>
    /// <param name="isBind"></param>
    /// <returns></returns>
    public IEnumerator PlayEffect( Transform trans, Effect effect, Vector3 position, bool isBind ) {
        CoroutineJoin join = new CoroutineJoin( AssetLoader.Instance );
        join.StartSubtask( DoPlayEffect( trans, effect, position, isBind ) );
        join.StartSubtask( DoPlaySound( effect.audioPath, position, true ) );
        yield return join.WaitForAll();
    }

    private IEnumerator DoPlayEffect( Transform trans, Effect effect, Vector3 position, bool isBind ) {        
        if( effect == null )
            yield break;
        if( string.IsNullOrEmpty( effect.effectPath ) )
            yield break;

        GameObject go = AssetLoader.PlayEffect( effect.effectPath );
        if( go == null ) {
            Debug.Log( "can not find effect:" + effect.effectPath );
            yield break;
        }

        if( trans == null ) {
            go.transform.position = position;
            go.transform.transform.rotation = Quaternion.identity;
        }
        else {
            if( isBind ) {
                go.transform.parent = trans;
                go.transform.localPosition = position;
                go.transform.localRotation = Quaternion.identity;
            }
            else {
                go.transform.position = position;
                go.transform.transform.rotation = trans.rotation;
            }
        }
        yield return new WaitForSeconds( effect.duration );
        AssetLoader.Destroy( effect.effectPath, go );
    }

    private IEnumerator DoPlaySound( string audioPath, Vector3 position, bool threeD, float delay = 0 ) {
        if( string.IsNullOrEmpty( audioPath ) )
            yield break;

        if( delay > float.Epsilon )
            yield return new WaitForSeconds( delay );

        UnityEngine.Object res = AssetLoader.GetAsset( audioPath );
        if( res ) {
            AudioManager.PlaySoundByClip( res as AudioClip, position, threeD );
        }
    }
}