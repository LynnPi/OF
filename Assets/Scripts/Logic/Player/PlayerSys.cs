using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Timer = System.Timers.Timer;

/// <summary>
/// 玩家类
/// </summary>
public static class PlayerSys {

    private static string Name_;
    private static string Pwd_;

    private static PlayerInfo Info_;


    public static event Action EventOnLogin;

    public static void RegisterMsg() {

    }

    public static void ResetPlayerInfo() {
        if( Info_ == null )
            Info_ = new PlayerInfo();
        Info_.ResetInfo();
    }

    public static int GetSN () {
        return Info_.Sn;
    }

    public static List<ClientShip> GetPlayerShipList() {
        return Info_.ClientShipList_;
    }

    public static void Login() {
        // todo 临时的,以后用实际数据
        // 旗舰
        ClientShip ship = new ClientShip();
        ship.ReadFromId( 40000 );
        ship.InFightID = 0;
        Info_.ClientShipList_.Add( ship );

        int fightID = 1;
        // 导弹护卫舰
        for( int i=0; i < 3; i++ ) {
            ship = new ClientShip();
            ship.ReadFromId( 30000 );
            ship.InFightID = fightID;
            Info_.ClientShipList_.Add( ship );
            fightID++;
        }

        // 机炮艇
        for( int i=0; i < 4; i++ ) {
            ship = new ClientShip();
            ship.ReadFromId( 10000 );
            ship.InFightID = fightID;
            Info_.ClientShipList_.Add( ship );
            fightID++;
        }

        if( EventOnLogin != null )
            EventOnLogin();
    }

    /// <summary>
    /// 布阵
    /// </summary>
    /// <param name="index">玩家舰船表中的索引</param>
    /// <param name="position">位置</param>
    /// <returns></returns>
    public static bool Formation( int index, Vector3 position ) {
        if( index < 0 || index >= Info_.ClientShipList_.Count ) return false;
        BattleSys.SetPlayerShip( Info_.ClientShipList_[index], position );
        return true;
    }
}