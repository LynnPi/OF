using UnityEngine;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PalmPoineer;

public class Launcher : MonoBehaviour {

    private int     NowNum;
    private int     MaxNum;
    private int     ChildNowNum;
    private int     ChildMaxNum;
    

    private void Awake() {

        AssetLoader.Init();
        // 设置永不休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //设置最高帧数
        //Application.targetFrameRate = 45;
        GameObject go = new GameObject( "UnityGuiConsole" );
        go.AddComponent<UnityGuiConsole>();

        Debug.Log( Application.platform );
        Debug.Log( "Application.streamingAssetsPath: "      + Application.streamingAssetsPath );
        Debug.Log( "Application.persistentDataPath: "       + Application.persistentDataPath );
        Debug.Log( "Application.temporaryCachePath: "       + Application.temporaryCachePath );
        Debug.Log( "QualitySettings.GetQualityLevel(): "    + QualitySettings.GetQualityLevel() );
        Debug.Log( "QualitySettings.antiAliasing"           + QualitySettings.antiAliasing );
        Debug.Log( "QualitySettings.vSyncCount"             + QualitySettings.vSyncCount );

        CheckResource( true );

        StartCoroutine( ShowUpdateProgress() );
    }

    private void OnDialogClick() {
        StartCoroutine( UpdateLocalResource() );
    }

    private IEnumerator ShowUpdateProgress() {
        NowNum = 0;
        MaxNum = 8;
        float value = 0;
        while( true ) {
            // 个数的百分比
            value = NowNum == MaxNum ? 1 : ((float)NowNum / (float)MaxNum);
            if( ChildMaxNum != 0 ) {
                value += (((float)ChildNowNum / (float)ChildMaxNum) * (1f / (float)MaxNum));
            }
            //progressbar.value = value;
            yield return null;
        }
    }

    private IEnumerator ShowHint() {
        yield break;
        //HintLabel.gameObject.SetActive( true );
        //while( true ) {
        //    int hintNum = HintContent_.Count;
        //    var hintIndex = Random.Range( 0, hintNum - 1 );
        //    HintLabel.text = HintContent_[hintIndex];
        //    yield return new WaitForSeconds( 2.0f );
        //}
    }

    /// <summary>
    /// 检查资源
    /// </summary>
    /// <param name="bLan">是否局域网</param>
    private void CheckResource( bool bLan ) {
        if( bLan ) {
			RuntimeSetting.UpdateServerListPath(Application.streamingAssetsPath+"/res" /*"http://lan.palmpioneer.com:9000 " */);
            if( Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer ) {
                RuntimeSetting.UpdateRemoteResourceUri( "internal.palmpioneer.com", "of/ofgameres/trunk/android/" );
            }
            else if( Application.platform == RuntimePlatform.IPhonePlayer ) {
                RuntimeSetting.UpdateRemoteResourceUri( "internal.palmpioneer.com", "of/ofgameres/trunk/ios/" );
            }
            else if( Application.platform == RuntimePlatform.Android ) {
                RuntimeSetting.UpdateRemoteResourceUri( "internal.palmpioneer.com", "of/ofgameres/trunk/android/" );
            }
            else {
                Debug.LogError( "Unsupport Platform:" + Application.platform );
                return;
            }
        }
        else {
            RuntimeSetting.UpdateServerListPath( "http://115.28.3.196:9000" );
            if( Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer ) {
                RuntimeSetting.UpdateRemoteResourceUri( "download.palmpioneer.com", "ofgameres/trunk/android/" );
            }
            else if( Application.platform == RuntimePlatform.Android ) {
                RuntimeSetting.UpdateRemoteResourceUri( "download.palmpioneer.com", "ofgameres/trunk/android/" );
            }
            else if( Application.platform == RuntimePlatform.IPhonePlayer ) {
                RuntimeSetting.UpdateRemoteResourceUri( "download.palmpioneer.com", "ofgameres/trunk/ios/" );
            }
            else {
                Debug.LogError( "Unsupport Platform:" + Application.platform );
                return;
            }
        }
        StartCoroutine( UpdateLocalResource() );
    }

    /// <summary>
    /// 更新本地资源
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateLocalResource() {

        // 下载服务器列表
        //yield return StartCoroutine( AssetLoader.DownloadServerList() );
//         if( AssetLoader.ServerInfoList == null || AssetLoader.ServerInfoList.Count <= 0 ) {
//             UGUIMessageBox.Show( "提示", "网络异常，请检查网络", "重试", OnDialogClick );
//             yield break;
//         }
        //NowNum++;


#if STANDALONE

        // 下载远程资源列表
        yield return StartCoroutine( AssetLoader.DownloadRemoteResourceList() );
        NowNum++;
        // 预加载舰船配置资源
        yield return AssetLoader.Instance.PreloadShipDefines();
        NowNum++;
        // 预加载子弹
        yield return StartCoroutine( AssetLoader.Instance.PreloadBullet() );
        NowNum++;
        // 预加载特效
        yield return StartCoroutine( AssetLoader.Instance.PreloadEffects() );
        NowNum++;
        // 预加载音效
        yield return StartCoroutine( AssetLoader.Instance.PreloadAudio() );
        NowNum++;
        // 预加载依赖项UI资源
        yield return StartCoroutine( AssetLoader.Instance.PreloadDependUI() );
        NowNum++;
        // 预加载UI
        yield return StartCoroutine( AssetLoader.Instance.PreloadAllUI() );
        NowNum++;
#else
        // 下载远程资源列表
        yield return StartCoroutine( AssetLoader.DownloadRemoteResourceList() );
        NowNum++;

        if( AssetLoader.RemoteResourceList == null ) {
            UGUIMessageBox.Show( "提示", "网络异常，请检查网络", "重试", OnDialogClick );
            yield break;
        }

        // 下载远程资源
        yield return StartCoroutine( DownloadRemoteResource() );
        NowNum++;
        // 预加载舰船配置资源
        yield return AssetLoader.Instance.PreloadShipDefines();
        NowNum++;
        // 预加载子弹
        yield return StartCoroutine( AssetLoader.Instance.PreloadBullet() );
        NowNum++;
        // 预加载特效
        yield return StartCoroutine( AssetLoader.Instance.PreloadEffects() );
        NowNum++;
        // 预加载音效
        yield return StartCoroutine( AssetLoader.Instance.PreloadAudio() );
        NowNum++;
        // 预加载依赖项UI资源
        yield return StartCoroutine( AssetLoader.Instance.PreloadDependUI() );
        NowNum++;
        // 预加载UI
        yield return StartCoroutine( AssetLoader.Instance.PreloadAllUI() );
        NowNum++;
#endif

        // 创建global
        GameObject globalGo = new GameObject( "Global" );
        if( globalGo != null ) {
            globalGo.AddComponent<Global>();
            globalGo.AddComponent<HttpGameNetwork>();
        }
        // 创建公共UI资源表,用作动态换图片
        Global.Instance.InitPublicUIList();        

        // 进入登录场景
        SceneManager.EnterScene( SceneManager.SceneType.Login );
    }

    /// <summary>
    /// 下载资源
    /// </summary>
    /// <returns></returns>
    private IEnumerator DownloadRemoteResource() {

        if( AssetLoader.RemoteResourceList.GetCount() <= 0 ) {
            yield break;
        }

        XmlResourceList.Item[] items = AssetLoader.RemoteResourceList.GetAllItems();
        XmlResourceList localList = new XmlResourceList();
        XmlResourceList saveList = new XmlResourceList();

        string localFileInfoPath_ = Path.Combine( Application.persistentDataPath, RuntimeSetting.FileInfoPath );
        if( File.Exists( localFileInfoPath_ ) ) {
            localList.ReadFromXml( File.ReadAllText( localFileInfoPath_ ) );
            Debug.Log( localFileInfoPath_ + " EXIST" );
            Debug.Log( "Total:" + localList.GetCount() );
        }
        else {
            Debug.Log( localFileInfoPath_ + " NOT EXIST" );
        }

        ChildMaxNum = items.Length;
        ChildNowNum = 0;
        for( int i = 0; i < ChildMaxNum; i++ ) {
            XmlResourceList.Item item = items[i];
            XmlResourceList.Item localItem = localList.GetItem( item.Path );
            XmlResourceList.Item builtinItem = AssetLoader.BuiltinResourceList.GetItem( item.Path );
            if( item.Deleted ) {
                ChildNowNum++;                
                continue;
            }

            if( builtinItem != null && item.Version <= builtinItem.Version ) {
                ChildNowNum++;
                saveList.AddToItems( item );
                continue;
            }
            if( localItem != null && item.Version <= localItem.Version ) {
                ChildNowNum++;
                saveList.AddToItems( item );
                continue;
            }

            if( item.Path.Contains( "fileinfo.xml" ) ) {
                continue;
            }

            string readPath = AssetLoader.GetResourceRemoteUrl( item.Path );
            string writePath = Path.Combine( Application.persistentDataPath, item.Path );

            yield return StartCoroutine( DownloadToLocal( readPath, writePath ) );
            if( !File.Exists( writePath ) ) {
                Debug.LogError( "Download file failed:" + readPath );
                yield break;
            }
            ChildNowNum++;
            saveList.AddToItems( item );
        }
        Utility.CreateDirByFilePath( localFileInfoPath_ );
        saveList.Save( localFileInfoPath_ );
        Debug.Log( "Write new resource list: " + localFileInfoPath_ );
    }

    private IEnumerator DownloadToLocal( string remotePath, string localPath ) {
        if( !string.IsNullOrEmpty( remotePath ) ) {
            using( WWW www = new WWW( remotePath ) ) {
                yield return www;
                if( !Utility.CheckWww( www ) ) yield break;
                // 创建文件夹
                Utility.CreateDirByFilePath( localPath );
                byte[] writeBytes = www.bytes;
                File.WriteAllBytes( localPath, writeBytes );
            }
        }
        ChildNowNum++;
    }
}