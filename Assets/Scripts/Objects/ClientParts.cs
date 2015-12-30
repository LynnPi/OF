using UnityEngine;
using System.Collections;

/// <summary>
/// 客户端用的舰船部件
/// </summary>
public class ClientParts {

    public int Id;
    public int Level;
    public ClientShip Owner;     // 维护一个ship的引用,可以通过部件查询到船体

    // for fight
    public int CoolDownTime = 0; // CD时间
    public int PartState = 0;    // 部件状态
    public int AttackClass_;
    public proto.PartReference Reference { get;  set; }


    /// <summary>
    /// 部件名称
    /// </summary>
    public string Name {
        get{
            return Reference.name;
        }
    }

    /// <summary>
    /// 警戒范围
    /// </summary>
    public int MoveAttackRange {
        get {
            return Reference.attack_move_range;
        }
    }

    /// <summary>
    /// 警戒角度
    /// </summary>
    public int AttackAngle {
        get {
            return Reference.atk_angle;
        }
    }

    /// <summary>
    /// 最小攻击距离
    /// </summary>
    public int AttackRangeMin {
        get {
            return Reference.atk_range_min;
        }
    }

    /// <summary>
    /// 最大攻击距离
    /// </summary>
    public int AttackRangeMax {
        get {
            return Reference.atk_range_max;
        }
    }



    // 最小攻击范围
    public int AttackRagneMin {
        get {
            return Reference.atk_range_min;
        }
    }

    /// <summary>
    /// 攻击级别 
    /// </summary>
    public int AC {
        get {
            return AttackClass_;
        }
        set {
            AttackClass_ = value;
        }
    }

    // TODO : 要在proto里面定义一个服务器的Parts对象
    public void ReadFromProto(int id) {
        Id = id;
        Level = 1;
        Reference = GlobalConfig.GetPartReference( Id );

        AttackClass_ = Reference.ac_initial;
    }

    /// <summary>
    /// 开始冷却
    /// </summary>
    public void StartCoolDown() {
        CoolDownTime = Reference.cd + FightTicker.Ticker;
    }

    /// <summary>
    /// 冷却完成
    /// </summary>
    /// <returns></returns>
    public bool IsCoolDownFinished() {
        if( FightTicker.Ticker >= CoolDownTime ) {
            return true;
        }

        return false;
    }
}
