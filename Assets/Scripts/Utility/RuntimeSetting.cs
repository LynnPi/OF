using System;
using System.Net;

/// <summary>
/// 运行时各种全局配置
/// </summary>
class RuntimeSetting {

    /// <summary>
    /// 资源文件版本控制xml路径
    /// </summary>
    public static readonly string FileInfoPath = "res/fileinfo.xml";

    /// <summary>
    /// 资源服务器域名
    /// </summary>
    public static string RemoteResourceServerDomain { get; private set; }

    /// <summary>
    /// 资源文件夹路径
    /// </summary>
    public static Uri RemoteResourceUri { get; private set; }

    /// <summary>
    /// 游戏服务器IP
    /// </summary>
    public static string GameServerIp = "";

    /// <summary>
    /// 游戏服务器端口
    /// </summary>
    public static int GameServerPort = 0;

    /// <summary>
    /// 登录服务器列表
    /// </summary>
    public static string ServerlistPath { get; private set; }

    /// <summary>
    /// 配置表路径
    /// </summary>
    public static string ReferenceFilePath = "res/bins/reference.bytes";

    /// <summary>
    /// 配置表版本
    /// </summary>
    public static int ServerReferenceVersion = 0;

    public static void UpdateServerListPath( string path ) {
        ServerlistPath = path;
    }

    public static void UpdateRemoteResourceUri( string domain, string resFolder ){
        //为了避免网络供应商根据域名对下载的文件进行缓存，这里需要将域名转换成IP地址
//         domain = domain.Replace( "http://", "" );
//         string ip = domain;
//         try{
//             IPAddress[] ips = Dns.GetHostAddresses( domain );
//             if( ips.Length > 0 ) ip = ips[ 0 ].ToString();
//         }
//         catch( Exception ex ){
//             UnityEngine.Debug.Log( ex.ToString() );
//         }        
//         Uri uriBase = new Uri( "http://" + ip );
        RemoteResourceServerDomain = domain;
        Uri uriBase = new Uri( "http://" + RemoteResourceServerDomain );
        RemoteResourceUri = new Uri ( uriBase, resFolder );
        UnityEngine.Debug.Log( "RemoteResourceUri Updated: " + RemoteResourceUri.AbsoluteUri );
    }
}
