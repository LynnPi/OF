using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial class ShipEntity : Entity {

    public ClientShip Ship;
    public int CampType;
    public StateMachine<ShipEntity> FsmInstance;
    private BuffManager BuffManager;
    private int StackAliveNum_;                         // 编队内存活的数量
    private EntityProperty ShipProperty_;
    

    // 保存一个实例，减轻GC
    proto.UnitBehavior BehaviorMsg_ = new proto.UnitBehavior();
    public ShipEntity( ClientShip ship, int campType ) {
        base.ID = ship.InFightID;
        CampType = campType;
        Ship = ship;
        InitAllAttr();
        FsmInstance = new StateMachine<ShipEntity>( this );
        BuffManager=new BuffManager(this);
        StackAliveNum_ = ship.Reference.stack_num;
        
        if( CampType == FightServiceDef.CampType.Camp_Attacker ) {
            FsmInstance.SetCurrentState( SeekState.Instance );
        }
        else {
            FsmInstance.SetCurrentState( DefendState.Instance );
        }
    }
    public override bool IsActive() {
        if( ShipProperty_.Durability.Value > 0 )
            return true;

        return false;
    }

    public override void Update() {
        BehaviorMsg_.Clear();
        BehaviorMsg_.unitid = Ship.InFightID;
        BuffManager.Update();
        FsmInstance.Update();

        //
        if (BehaviorMsg_.has_position() || BehaviorMsg_.partfireevent_size() != 0){
            FightMessageCache.Instance.AddMessage( BehaviorMsg_ );
        }
    }

    private proto.PartFireInfo PartFire( ClientParts part, ShipEntity target ) {
        proto.PartFireInfo fireInfo = new proto.PartFireInfo();
        fireInfo.targetid = target.ID;

        if (part.Reference.missle_vel > 0){
            float distance = Vector3.Distance(this.Ship.Position, target.Ship.Position);
            int delayFrame = Mathf.CeilToInt( distance / (part.Reference.missle_vel * FightServiceDef.SPEED_SCALAR) );
            fireInfo.delayframe = delayFrame;
        }else{
            int delayFrame = Mathf.CeilToInt(part.Reference.continued_time / FightServiceDef.FRAME_INTERVAL_TIME);
            fireInfo.delayframe = delayFrame;
        }

        int baseAtk = GetBaseAtkValue( part, target );
        if( Ship.Reference.stack) {
            baseAtk *= StackAliveNum_;
        }
        MessageDispatcher.Instance.DispatchMessage( TelegramType.UnderAttack, this.ID, target.ID, fireInfo.delayframe, baseAtk, part.Reference.effect_type );

        return fireInfo;
    }

    /// <summary>
    /// 部件逻辑
    /// </summary>
    public void RunPartsAI() {

        for( int i = 0; i < Ship.PartsList.Count; ++i ) {
            var part = Ship.PartsList[i];
            var enemyList = TargetSelector.FindTarget(part, CampType);
            if( enemyList.Count == 0 ) {
                part.PartState = PartState.Idle;
                continue;
            }
            part.PartState = PartState.Guard;
            proto.PartFireEvent partFireEvent = new proto.PartFireEvent();
            partFireEvent.partid = part.Id;
            for( int enemyIndex = 0; enemyIndex < enemyList.Count; ++enemyIndex ) {
                EnemyInfo enemyInfo = enemyList[enemyIndex];
                if( enemyInfo.InAttackRange )
                    part.PartState = PartState.Attack;
                if( !part.IsCoolDownFinished() )
                    continue;
                part.StartCoolDown();
                partFireEvent.add_fireinfo( PartFire( part, enemyInfo.EnemyShip ) );
            }
            if( partFireEvent.fireinfo_size() != 0 )
                BehaviorMsg_.add_partfireevent( partFireEvent );
        }
    }

    /// <summary>
    /// 部件状态会影响舰艇的状态，具体可以查询文档，状态转换表
    /// </summary>
    /// <returns></returns>
    public int GetPartState(){
        // 有部件已经达到攻击射程，则判断整体为
        int state = PartState.Idle ;
        for( int i = 0; i < Ship.PartsList.Count; ++i ) {
            ClientParts part = Ship.PartsList[i];
            if( part.PartState == PartState.Attack )
                return PartState.Attack;
            if( part.PartState == PartState.Guard )
                state = PartState.Guard;
        }

        return state;
    }

    public void Move() {
        
        // 暴力尝试中
        ShipEntity shipEntity =  ShipCollision.GetObstructTarget( this );
        int myVol = this.Ship.Reference.vol;
        int targetVol = 0;
        float zDistance = float.MaxValue;
        if(shipEntity != null){
            targetVol = shipEntity.Ship.Reference.vol;
            zDistance = Mathf.Abs(this.Ship.Position.z - shipEntity.Ship.Position.z);
        }
        float standSpeed = Utility.GetValueByRate( FightServiceDef.SPEED_SCALAR, this.Ship.Reference.speed_max );
        float accBaseSpeed = zDistance - targetVol-2.5f*myVol;
        float instantaneousSpeed = this.Ship.Reference.speed_max / 10000.0f + accBaseSpeed * this.Ship.Reference.acc_speed / 100000.0f;
        instantaneousSpeed = FightServiceDef.SPEED_SCALAR * instantaneousSpeed;

        if( instantaneousSpeed > standSpeed )
            instantaneousSpeed = standSpeed;

        if( instantaneousSpeed < 0 )
            return;

        Ship.Position.z += instantaneousSpeed;

        proto.UnitPos curPos = new proto.UnitPos();
        curPos.posx = (int)(Ship.Position.x * 10000);
        curPos.posz = (int)(Ship.Position.z * 10000);
        curPos.movetype = Def.MoveType.Forward;
        BehaviorMsg_.position = curPos;
    }

    public void LateralMove(bool right) {
        if (right)
            Ship.Position.x += Utility.GetValueByRate( FightServiceDef.SPEED_SCALAR, this.Ship.Reference.sw_speed );
        else
            Ship.Position.x -= Utility.GetValueByRate( FightServiceDef.SPEED_SCALAR, this.Ship.Reference.sw_speed );
        proto.UnitPos curPos = new proto.UnitPos();
        curPos.posx = (int)(Ship.Position.x * 10000);
        curPos.posz = (int)(Ship.Position.z * 10000);
        curPos.movetype = Def.MoveType.Lateral;
        BehaviorMsg_.position = curPos;
    }

    // 请求使用指挥官技能
    public void RequestUseCommanderShip( int skillid, int targetID, Vector3 targetPos ) {
        if( this.Ship.SkillDic.ContainsKey(skillid) && this.Ship.SkillDic[skillid].IsCoolDownFinished()) {
            // 给自己发一条吟唱技能的消息
            MessageDispatcher.Instance.DispatchMessage(TelegramType.SkillLead, this.ID, this.ID, 1, skillid, targetID,targetPos.x, targetPos.y, targetPos.z);
        }
    }
}
