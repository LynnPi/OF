using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     战斗类型
/// </summary>
public enum BattleType{
    /// <summary>
    ///     位置
    /// </summary>
    Unknow = 0,

}

public static class BattleSys{

    private static List<ClientShip> PlayerShipList = new List<ClientShip>();
    private static List<ClientShip> EnemyShipList = new List<ClientShip>();

    private static int NowMapID_;

    public static int NowMapID {
        get {
            return NowMapID_;
        }
    }

    public static event Action EventOnEnterBattle;
    public static event Action< proto.S2CFightFrameInfo > EventOnFightFrameInfo;
    public static event Action<proto.S2CFightReport> EventOnFightReport;
    public static event Action<proto.S2CSyncUnitAttr> EventOnSyncUnitAttr;

    /// <summary>
    /// 得到列表元素个数
    /// </summary>
    /// <param name="bPlayer"></param>
    /// <returns></returns>
    public static int GetShipCount( bool bPlayer ) {
        return (bPlayer ? PlayerShipList.Count : EnemyShipList.Count);
    }

    public static ClientShip GetCommanderShip() {
        for( int i=0; i < PlayerShipList.Count; i++ ) {
            if( PlayerShipList[i] == null ) continue;
            if( PlayerShipList[i].IsCommanderShip() )
                return PlayerShipList[i];
        }
        return null;
    }

    public static List<ClientShip> GetPlayerShipList() {
        return PlayerShipList;
    }

    public static ClientShip GetFarestEnemy() {
        ClientShip result = null;
        for( int i = 0; i < EnemyShipList.Count; i++ ) {
            ClientShip ship = EnemyShipList[i];            
            if( result == null )
                result = ship;
            else {
                if( result.Position.z < ship.Position.z ) {
                    result = ship;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 根据索引得到舰船信息
    /// </summary>
    /// <param name="bPlayer"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static ClientShip GetShipByIndex( bool bPlayer, int index ) {
        List<ClientShip> list = bPlayer ? PlayerShipList : EnemyShipList;
        if( index < 0 || index >= list.Count ) return null;
        return list[index];
    }

    public static void SetPlayerShip( ClientShip ship, Vector3 position ) {
        ClientShip newShip = ClientShip.Clone( ship );
        newShip.Position = position;
        PlayerShipList.Add( newShip );
    }

    public static void ResetPlayerBattleShip() {
        PlayerShipList.Clear();
    }

    public static void InitBattleInfo( int id ) {
        // 缓存地图ID
        NowMapID_ = id;
        // 初始化敌人
        SetEnemyShipList();
        // 初始化阵形,玩家的阵形在布阵时由布阵系统初始化
        //SetFormation( true, PlayerShipList );
        SetFormation( false, EnemyShipList );
    }

    public static void EnterBattle() {
        if( PlayerShipList.Count <= 0 )
            return;
        if( EventOnEnterBattle != null )
            EventOnEnterBattle();
    }

    public static void SetFormation( bool bPlayer, ClientShip ship ) {
        if( ship == null ) return;
        // 随机一个阵形
        int stackNum = 1;
        if( ship.Reference.stack )
            stackNum = ship.Reference.stack_num;
        if( stackNum > 1 ) {
            ship.FormationList = GlobalConfig.GetFormationList( bPlayer );
        }
    }

    private static void SetFormation( bool bPlayer, List<ClientShip> shipList ) {
        foreach( var iter in shipList ) {
            SetFormation( bPlayer, iter );
        }
    }

    private static void SetEnemyShipList() {
        EnemyShipList.Clear();
        // todo 临时敌人,到时用服务器下发数据

        var enemyUnitList = GlobalConfig.GetUnitLisByBattlefieldByID( NowMapID_ );
        int id = 50;
        for( int i = 0; i < enemyUnitList.Count; i++ ) {
            ClientShip ship = new ClientShip();
            ship.ReadFromId( enemyUnitList[i].units_id );
            ship.Position = new Vector3( enemyUnitList[i].x_position, 0, enemyUnitList[i].z_position );
            ship.InFightID = id++;
            EnemyShipList.Add( ship );
        }
    }

    private static bool SetShip( bool bPlayer, proto.UnitAttri attribute ) {
        List<ClientShip> shipList = bPlayer ? PlayerShipList : EnemyShipList;
        foreach( var iter in shipList ) {
            if( iter.InFightID != attribute.unitid ) continue;
            iter.Armor = attribute.armor;
            iter.Durability = attribute.durablility;
            iter.Energy = attribute.energy;
            return true;
        }
        return SetShip( !bPlayer, attribute );
    }

    /// <summary>
    /// 初始化战斗
    /// </summary>
    /// <returns></returns>
    public static void InitFight() {
        FightService.Instance.InitService( PlayerShipList, EnemyShipList );
    }

    public static void OnFightFrameInfo( proto.S2CFightFrameInfo msg ) {
        if (EventOnFightFrameInfo != null)
            EventOnFightFrameInfo(msg);
    }

    public static void OnFightReport( proto.S2CFightReport msg ) {
        if( EventOnFightReport != null )
            EventOnFightReport( msg );
    }

    public static void OnSyncUnitAttr( proto.S2CSyncUnitAttr msg ) {

        for( int i=0; i < msg.unitattrlist_size(); i++ ) {
            proto.UnitAttri attribute = msg.unitattrlist( i );
            //SetShip( true, attribute );
            // TODO:: 
            // 临时解决办法，待重构
            var teamDisplay = BattleSceneDisplayManager.Instance.GetTeamDisplay( attribute.unitid );
            if( teamDisplay != null ) {
                teamDisplay.SyncAttr( attribute );
            }
        }

        if( EventOnSyncUnitAttr != null )
            EventOnSyncUnitAttr( msg );
    }
}