using System.IO;
using System.Net.Security;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Net;

// 具体消息处理回调类型
public delegate void PackHandle ( int Pid, string SessionID, byte[] Msg, int MsgSize );

// proto消息头定义
public struct ProtoHead {
    public byte Pid;
    public string Sessionid;
}

// 包大小定义
public static class NetMsg {
    public static readonly int MsgDataSize;
    public static readonly int MsgBufSize;
    public static readonly int MsgSendHeadSize;
    public static readonly int MsgReceiveHeadSize;
    public static readonly int MsgProtoHeadSize;

    static NetMsg () {
        MsgDataSize = 64 * 1024;
        MsgBufSize = 32 * 1024;
        MsgSendHeadSize = 5;
        MsgReceiveHeadSize = 4;
        MsgProtoHeadSize = 255;
    }
}

public class HttpGameNetwork : MonoBehaviour {
    public static HttpGameNetwork           Instance { private set; get; }
    private static byte[]                   ProtoHeadBuf;
    private static byte[]                   ProtoDataBuf;
    //private static byte[]                   TempBuffer = new byte[ 1024 * 64 ];
    private static int                      SessionId = 0;
    private static int                      Gid       = 0;
    public static int                       SerialNumber = 1;             // 当前消息流水号
    private static int                      ReConnectTimes = 0;           // 服务端重连次数
    private static List<byte[]>             msgBuffList = new List<byte[]>();
    
    // 回调函数列表
    private static Dictionary<int, PackHandle> mDispatcher;

    void Awake () {
        HttpGameNetwork.Instance = this;
        // 创建包体缓冲
        ProtoHeadBuf = new byte[ NetMsg.MsgProtoHeadSize ];
        ProtoDataBuf = new byte[ NetMsg.MsgBufSize ];
        mDispatcher = new Dictionary<int, PackHandle> ();
    }

    // Use this for initialization
    void Start () {
        // 注册服务器错误消息函数
        //Register ( ProtoID.S2CSystem, OnS2CSystem );
        // 初始化各个模块网络消息
        PlayerSys.RegisterMsg();
    }

    private static void OnS2CSystem ( int pid, string sessionId, byte[] msg, int size ) {
        proto.S2CSystem tempMsg = new proto.S2CSystem ();
        tempMsg.Parse ( msg, 0, size );
        if ( tempMsg.msg_type == proto.S2CSystem.Type.InvalidSession || tempMsg.msg_type == proto.S2CSystem.Type.NotInGame ) {
            //Global.Instance.SetSysNotice ( SysNoticType.InvalidSession );
        }
        //Global.LoadSceneByName ( BuiltinScenes.Login );
    }

    public static bool Register ( int MsgID, PackHandle fn ) {
        if ( mDispatcher.ContainsKey ( MsgID ) ) {
            Debug.Log ( "msgid:" + MsgID + " is registered" );
            return false;
        }
        mDispatcher.Add ( MsgID, fn );
        return true;
    }

    public static int GetSessionID () {
        return SessionId;
    }

    public static void SendMsg ( int id, ProtoMessage msg ) {
        if ( msgBuffList.Count >= 3 ) {
            Debug.Log ("消息队列缓冲已满");
            return;
        }

        proto.MessageHead head = new proto.MessageHead ();
        head.pid = id;
        head.msgid = SerialNumber;

        if ( id != ProtoID.C2SLogin ) {
            head.session_id = SessionId;
            head.gid        = PlayerSys.GetSN();
            if ( head.session_id == 0 ) {
                Debug.Log ( "Send Msg error: Have not session_id" );
                return;
            }
        }
        else {
            SerialNumber = 1;
            head.msgid = SerialNumber;
            msgBuffList.Clear ();
        }


        byte headSize = ( byte )head.ByteSize ();
        short msgSize = ( short )msg.ByteSize ();
        // 总包大小
        System.Int64 allSize = ( System.Int64 )( headSize + msgSize + NetMsg.MsgSendHeadSize );
        byte[] sendBuf = new byte[ allSize ];

        // 写包头
        sendBuf[ 0 ] = ( byte )( uint )( allSize & 0xff );
        sendBuf[ 1 ] = ( byte )( uint )( ( allSize >> 8 ) & 0xff );
        sendBuf[ 2 ] = ( byte )( uint )( ( allSize >> 16 ) & 0xff );
        sendBuf[ 3 ] = ( byte )( uint )( ( allSize >> 32 ) & 0xff );
        sendBuf[ 4 ] = headSize;

        // 转换为2进制
        head.Serialize ( ProtoHeadBuf, 0 );
        msg.Serialize ( ProtoDataBuf, 0 );

        System.Buffer.BlockCopy ( ProtoHeadBuf, 0, sendBuf, NetMsg.MsgSendHeadSize, headSize );
        System.Buffer.BlockCopy ( ProtoDataBuf, 0, sendBuf, NetMsg.MsgSendHeadSize + headSize, msgSize );

        // 心跳包不阻塞玩家输入
        if ( id != ProtoID.C2SHeartBeat && id != ProtoID.C2SSetCustomSetting )
            NetworkIndicator.Instance.StartActivityIndicator ();
        msgBuffList.Add (sendBuf);
        if (msgBuffList.Count == 1)
            Instance.StartCoroutine ( OnPost ( msgBuffList[0] ) );
    
        SerialNumber++;
    }

    public static void ClearSendMsgBuffer () {
        msgBuffList.Clear ();
    }

    private static IEnumerator OnPost ( byte[] buf ) {

        string url = string.Format ( "http://{0}:{1}/client", RuntimeSetting.GameServerIp,
            RuntimeSetting.GameServerPort );
        using ( WWW www = new WWW ( url, buf ) ) {
            yield return www;
            if ( !string.IsNullOrEmpty ( www.error ) ) {
                Debug.LogError ( www.error );
                if ( Application.loadedLevelName == BuiltinScenes.Login ) {
                    NetworkIndicator.Instance.StopActivityIndicator ();
                    //Global.Instance.SetSysNotice ( 14 );
                }
                else {
                    // 如果post失败
                    ReConnectTimes++;
                    if ( ReConnectTimes >= 3 ) {
                        // TODO: // 尝试次数太多。。。。
                        Debug.LogError ("网络已断！！！！");
                        NetworkIndicator.Instance.StopActivityIndicator ();
                        //Global.Instance.SetSysNotice ( 14 );
                        ReConnectTimes = 0;
                        msgBuffList.Clear ();
                        SerialNumber = 1;
                        //Global.LoadSceneByName ( BuiltinScenes.Login );
                        //PlatformSDK.CurrentSDK.PlayerLoginOut ( Global.GetLastEnterServer ().Id, PlayerInfo.Instance.Sn.ToString (), PlayerInfo.Instance.Name, "0", PlayerSys.GetVipLevel ().ToString (), PlayerInfo.Instance.Exp.ToString (), PlayerInfo.Instance.Level, PackageSys.GetCoins (), PackageSys.GetGold ().ToString (), PlayerInfo.Instance.total_deposit.ToString () );
                        //PlatformSDK.CurrentSDK.LoginOut ();
                        yield break;
                    }
                    Debug.LogError ( "断线重连ing.." + ReConnectTimes.ToString () );
                    yield return new WaitForSeconds (0.2f);
                    Instance.StartCoroutine ( OnPost ( msgBuffList[0] ) );
                }
                yield break;
            }
            OnResponse ( www.bytes, www.bytes.Length );
            ReConnectTimes = 0;
            if (msgBuffList.Count>0)
                msgBuffList.RemoveAt ( 0 );
            if ( msgBuffList.Count > 0 )
                Instance.StartCoroutine ( OnPost (msgBuffList[0]) );
        }
    }

//     private static void Post ( byte[] buf ) {
//         try {
//             HttpWebRequest r = WebRequest.Create ( string.Format ( "http://{0}:{1}/client", RuntimeSetting.GameServerIp, RuntimeSetting.GameServerPort ) ) as HttpWebRequest;
//             r.Method = "POST";
//             r.ContentType = "application/x-www-form-urlencoded";
//             r.AuthenticationLevel = AuthenticationLevel.None;
//             Stream s = r.GetRequestStream ();
//             s.Write ( buf, 0, buf.Length );
//             s.Close ();
//             HttpWebResponse response = r.GetResponse () as HttpWebResponse;
//             if ( response == null ) {
//                 return;
//             }
//             s = response.GetResponseStream ();
//             int len = s.Read ( TempBuffer, 0, 1024 * 32 );
//             s.Close ();
//             OnResponse ( TempBuffer, len );
//         }
//         catch ( System.Exception e ) {
//             UnityEngine.Debug.LogException ( e );
//         }
//     }

//     void OnGUI () {
//         if(GUI.Button(new Rect(0, 0, 100,50), "消息单发")){
//             proto.C2SChat msg = new proto.C2SChat ();
//             msg.channel = Def.ChannelType.World;
//             msg.content = "你好";
//             SendMsg (ProtoID.C2SChat, msg);
//         }
// 
//         if (GUI.Button(new Rect(0,50, 100, 50), "消息补发")){
//             StartCoroutine(OnPost(LastPostMsgBuff));
//         }
// 
//     }
    private static void OnResponse ( byte[] buf, int totalsize ) {
        try {
            int pid = 0;
            //Debug.Log("OnResponse:" + totalsize);
            if ( totalsize < NetMsg.MsgReceiveHeadSize ) {
                Debug.LogError ( "size < NetMsg.MsgReceiveHeadSize:" + totalsize );
                NetworkIndicator.Instance.StopActivityIndicator ();
                return;
            }
            int correctsize = buf[ 0 ] + ( buf[ 1 ] << 8 ) + ( buf[ 2 ] << 16 ) + ( buf[ 3 ] << 32 );
            if ( totalsize != correctsize ) {
                Debug.LogError ( "size != correctsize," + totalsize + "," + correctsize );
                NetworkIndicator.Instance.StopActivityIndicator ();
                return;
            }
            // 初始化开始点
            int byteIndex = NetMsg.MsgReceiveHeadSize;
            // 有可能一个包里有多个消息,所以需要循环
            while ( byteIndex < totalsize ) {
                // proto消息包头
                int protoHeadSize = buf[ byteIndex ];
                byteIndex++;
                // 初始化proto消息头
                byte[] mProHeadBuf = new byte[ protoHeadSize ];
                int mProHeadSize = 0;
                while ( byteIndex < totalsize && mProHeadSize < protoHeadSize ) {
                    mProHeadBuf[ mProHeadSize ] = buf[ byteIndex ];
                    ++mProHeadSize;
                    ++byteIndex;
                }
                // 读完后的大小与服务器的大小是否相同
                if ( mProHeadSize != protoHeadSize ) {
                    Debug.Log ( "error : mSession.mProHeadSize : " + mProHeadSize +
                        " != ProHeadSize : " + protoHeadSize );
                    return;
                }
                // 数据
                proto.MessageHead tempHead = new proto.MessageHead ();
                tempHead.Parse ( mProHeadBuf, 0, protoHeadSize );
                pid = tempHead.pid;
                if (tempHead.session_id != 0 ) {
                    SessionId = tempHead.session_id;
                    Gid      =  tempHead.gid;
                }

                // 包体
                int first = buf[ byteIndex ];
                ++byteIndex;
                int second = ( buf[ byteIndex ] << 8 );
                ++byteIndex;
                // 大小
                int DataBufSize = first + second;
                // 初始化包体
                byte[] mDataBuf = new byte[ DataBufSize ];
                int mDataSize = 0;
                // 取包体
                while ( byteIndex < totalsize && mDataSize < DataBufSize ) {
                    mDataBuf[ mDataSize ] = buf[ byteIndex ];
                    ++mDataSize;
                    ++byteIndex;
                }
                // 验证大小
                if ( mDataSize != DataBufSize ) {
                    Debug.Log ( "error : mSession.mDataSize : " + mDataSize +
                        " != DataBufSize : " + DataBufSize );
                    return;
                }
                PackHandle fn = null;
                //Debug.LogError ( "OnReceivePack:" + pid );
                if ( !mDispatcher.TryGetValue ( pid, out fn ) ) {
                    //Debug.LogError("dispatcher of pid:" + pid + ", not found");
                    continue;
                }
                if ( null == fn ) {
                    //Debug.LogError( "dispatcher of pid:" + pid + ", fn == null" );
                    continue;
                }
                // 解析内容
                fn ( pid, SessionId.ToString(), mDataBuf, mDataSize );
                if ( pid != ProtoID.S2CHeartBeat && pid != ProtoID.S2CSetCustomSetting ) {
                    NetworkIndicator.Instance.StopActivityIndicator ();
                }
            }

        }
        catch ( System.Exception er ) {
            Debug.LogException ( er );
            NetworkIndicator.Instance.StopActivityIndicator ();
        }
    }
}