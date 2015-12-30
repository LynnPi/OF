using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using UnityEngine.UI;

/// <summary>
/// 通用功能类
/// </summary>
class Utility {
    private static readonly MD5CryptoServiceProvider Md5Crypto = new MD5CryptoServiceProvider();

    public static string Md5( byte[] data ) {
        byte[] bin = Md5Crypto.ComputeHash( data );
        return BitConverter.ToString( bin ).Replace( "-", "" ).ToLower();
    }

    public static string Md5( string input ){
        return Md5( System.Text.Encoding.Default.GetBytes( input ) );
    }

    /// <summary>
    /// 根据文件完整路径创建文件所在目录
    /// </summary>
    /// <param name="filePath"></param>
    public static void CreateDirByFilePath( string filePath ){
        string path = Path.GetDirectoryName( filePath );
        if( string.IsNullOrEmpty( path ) ) return;
        DirectoryInfo di = new DirectoryInfo( path );
        if ( !di.Exists ) di.Create();
    }

    public static bool CheckWww( WWW www ){
        if( string.IsNullOrEmpty( www.error ) ) return true;
        Debug.LogError( www.error );
        return false;
    }

    public static IEnumerator NguiRollingIntLabel ( Text label, int start, int end, float elapse ) {
        float startTime = 0;
        label.text = "+" + start;
        yield return null;
        while ( startTime < elapse ) {
            startTime += Time.deltaTime;
            label.text = "+" + ( start == end ? end.ToString () : ( ( end - start ) * ( startTime / elapse ) ).ToString ( "##,###" ) );
            yield return null;
        }
        label.text = "+" + ( start == end ? end.ToString () : end.ToString ( "##,###" ) );
    }

    #region 设置GameObject以及其所有子GameObject的层
    public static void SetLayerRecursively( GameObject obj, string layerName ){
        SetLayerRecursively( obj, LayerMask.NameToLayer( layerName ) );
    }

    public static void SetLayerRecursively( GameObject obj, int layer ) {
        obj.layer = layer;
        foreach ( Transform child in obj.transform ) {
            SetLayerRecursively( child.gameObject, layer );
        }
    }
    #endregion

    public static Dictionary<string,string> HttpParseQuery( string query ){
        Dictionary<string,string> result = new Dictionary< string, string >();
        if( query.Length > 0 && query[ 0 ] == 63 ){//63是问号
            query = query.Substring( 1 );
        }
        string[] namevalues = query.Split( '&' );
        foreach( string namevalue in namevalues ){
            string[] nv = namevalue.Split( '=' );
            if( nv.Length != 2 ){
                continue;
            }
            string key = WWW.UnEscapeURL( nv[ 0 ], Encoding.UTF8 );
            string value = WWW.UnEscapeURL( nv[ 1 ], Encoding.UTF8 );
            result[ key ] = value;
            Debug.Log( key + ":" + value );
        }
        return result;
    }

    /// <summary>
    /// 计算整型数值的万分比值，跟服务端的算法一致
    /// </summary>
    /// <param name="v"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static int GetValueByRate( int v, int rate ){
        float r = rate / Def.MAX_RATE_FLOAT;
        float val = ( (float)v )*r;
        return (int)Math.Floor( val );
    }

    public static float GetValueByRate( float v, int rate ) {
        float r = rate / Def.MAX_RATE_FLOAT;
        float val = ((float)v) * r;
        return val;
    }

    /// <summary>
    /// 重置并播放所有UITweener
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static void ResetAndPlayTweeners( GameObject go, bool includeChild = false ){
        /*
        UITweener[] all = includeChild ? go.GetComponentsInChildren<UITweener>() : go.GetComponents<UITweener>();
        foreach( UITweener t in all ){
            t.ResetToBeginning();
            t.PlayForward();
        }
         */
    }

    /// <summary>
    /// 获取最长的Tweener时间
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static float GetMaxTweenerDuration( GameObject go ){
        float maxdur = 0;
        /*
        UITweener[] all = go.GetComponents<UITweener>();
        foreach ( UITweener t in all ){
            float dur = t.duration + t.delay;
            maxdur = dur > maxdur ? dur : maxdur;
        }
         */
        return maxdur;
    }

    /// <summary>
    /// 根据名称查找子GameObject
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Transform FindChildWithName( Transform parent, string name ) {
        if ( parent.name == name ) return parent;
        foreach ( Transform child in parent ) {
            if ( child.name == name ) return child;
            Transform result = FindChildWithName( child, name );
            if ( result ) return result;
        }
        return null;
    }

    /// <summary>
    /// 删除所有子节点
    /// </summary>
    /// <param name="parent"></param>
    public static void DestroyChildren( Transform parent ) {
        for( int i = parent.childCount - 1; i >= 0; i-- ) {
            Transform child = parent.GetChild( i );
            GameObject.Destroy( child.gameObject );
        }
    }
}
