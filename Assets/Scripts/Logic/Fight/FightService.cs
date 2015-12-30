using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// FightService 战斗服务 
/// OF 战斗逻辑层入口
/// </summary>
public class FightService : MonoBehaviour {

    private static FightService Instance_  = null;

    private Timer TickTimer_ = new Timer();        // 战斗定时器
    private bool  Combating = true;                // 战斗状态
    private float ReimbursedTime_ = 0;             // 补偿时间
    public static FightService Instance {
        get {
            if( Instance_ == null ) {
                GameObject go = new GameObject( "FightService" );
                Instance_ = go.AddComponent<FightService>();
            }

            return Instance_;
        }
    }

    /// <summary>
    /// 初始化战斗Service
    /// </summary>
    /// <param name="AttackerTeam"></param>
    /// <param name="DefenderTeam"></param>
    public void InitService( List<ClientShip> AttackerTeam, List<ClientShip> DefenderTeam ) {
        FightTicker.InitTick();
        BattlefieldEnvironment.Instance.Init();
        FightAnalyzer.Instance.Init( AttackerTeam.Count, DefenderTeam.Count );
        this.SetupFightTeam( AttackerTeam, DefenderTeam );
    }

    public void BeginFight() {

        // TODO 先计算玩家的属性并同步给客户端
        SyncAllUnitAttr();
        Combating = true;
    }

    /// <summary>
    /// 撤退,放弃战斗
    /// </summary>
    public void GiveupFight() {
        // TODO: 撤退逻辑
        Combating = false;
    }

    private void LateUpdate() {
        if( Combating )
            FightServiceRun();
    }

    // 战斗核心循环
    private void FightServiceRun() {

        if( TickTimer_.ToNextTick( FightServiceDef.FRAME_INTERVAL_TIME_F - ReimbursedTime_ ) ) {

            float startTime = Time.realtimeSinceStartup;
            // 1、清空消息缓存
            FightMessageCache.Instance.ClearAll();

            // 2、先处理延时的消息
            MessageDispatcher.Instance.DispatchDelayedMessages();

            // 3、执行单位AI
            for( int i = 0; i < EntityManager.Instance.AttackUnitList.Count; i++ ) {
                Entity entity = EntityManager.Instance.AttackUnitList[i];
                entity.Update();
            }

            for( int i = 0; i < EntityManager.Instance.DefenderUnitList.Count; i++ ) {
                Entity entity = EntityManager.Instance.DefenderUnitList[i];
                entity.Update();
            }


            // 4、更新战场统计
            FightTicker.Tick();
            proto.S2CFightFrameInfo FraneInfoMsg = FightMessageCache.Instance.GetFrameInfoMsg();
            FraneInfoMsg.framecount = FightTicker.FrameCount;
            FraneInfoMsg.fighttick = FightTicker.Ticker;

            // 5、把消息传递到表现层
            BattleSys.OnFightFrameInfo( FraneInfoMsg );


            // 6、判断战斗是否结束
            if( FightAnalyzer.Instance.FightOver() ) {
                Combating = false;
            }

            ReimbursedTime_ = Time.realtimeSinceStartup - startTime;
        }
    }


    void SetupFightTeam( List<ClientShip> AttackerTeam, List<ClientShip> DefenderTeam ) {
        Combating = false;
        EntityManager.Instance.ClearAll();
        MessageDispatcher.Instance.ClearAll();
        for( int i = 0; i < AttackerTeam.Count; ++i ) {
            ClientShip clientship = ClientShip.Clone(AttackerTeam[i]);
            ShipEntity ship = new ShipEntity(clientship, FightServiceDef.CampType.Camp_Attacker);
            EntityManager.Instance.RegisterEntity(ship);
        }

        for( int i = 0; i < DefenderTeam.Count; i++ ) {
            ClientShip clientship = ClientShip.Clone(DefenderTeam[i]);
            ShipEntity ship = new ShipEntity( clientship, FightServiceDef.CampType.Camp_Defender );
            EntityManager.Instance.RegisterEntity( ship );
        }
    }

    /// <summary>
    /// 使用指挥技能
    /// </summary>
    /// <param name="unitID"></param>
    /// <param name="skillID"></param>
    /// <param name="targetID">目标ID</param>
    /// <param name="targetPos">目标坐标</param>
    public void UseCommanderSkill(int unitID, int skillID, int targetID, Vector3 targetPos) {
        Entity entity = EntityManager.Instance.GetEntityByID(unitID);
        if( entity == null )
            return;

        var shipEntity = entity as ShipEntity;
        shipEntity.RequestUseCommanderShip( skillID, targetID, targetPos );
    }

    /// <summary>
    /// 同步所有单位的属性
    /// </summary>
    private void SyncAllUnitAttr() {
        proto.S2CSyncUnitAttr attrList = new proto.S2CSyncUnitAttr();
        for( int i = 0; i < EntityManager.Instance.AttackUnitList.Count; ++i ) {
            proto.UnitAttri attr = new proto.UnitAttri();
            ShipEntity entity = EntityManager.Instance.AttackUnitList[i] as ShipEntity;
            attr.unitid = entity.ID;
            attr.durablility = entity.GetDurability();
            attr.armor = entity.GetArmouredShield();
            attr.energy = entity.GetEnergyShield();
            attrList.add_unitattrlist( attr );
        }

        for( int i = 0; i < EntityManager.Instance.DefenderUnitList.Count; ++i ) {
            proto.UnitAttri attr = new proto.UnitAttri();
            ShipEntity entity = EntityManager.Instance.DefenderUnitList[i] as ShipEntity;
            attr.unitid = entity.ID;
            attr.durablility = entity.GetDurability();
            attr.armor = entity.GetArmouredShield();
            attr.energy = entity.GetEnergyShield();
            attrList.add_unitattrlist( attr );
        }

        BattleSys.OnSyncUnitAttr( attrList );
    }

    void OnDestroy() {
        Instance_ = null;
    }
}
