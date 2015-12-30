using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// 敌人信息
public struct EnemyInfo {
    public float DistanceSQ; // 距离平方
    public ShipEntity EnemyShip; 
    public bool       InAttackRange;
}

/// <summary>
/// FightService 相关定义
/// </summary>
public static class FightServiceDef {

    // 战斗帧间隔毫秒(整型)
    public static int FRAME_INTERVAL_TIME = 25;

    // 战斗帧间隔毫秒(浮点数)
    public static float FRAME_INTERVAL_TIME_F = FRAME_INTERVAL_TIME / 1000.0f;


    // 速度标量
    public static float SPEED_SCALAR = 0.2f;

    /// <summary>
    /// 阵营类型
    /// </summary>
    public static class CampType {
        static public int Camp_Attacker = 1;
        static public int Camp_Defender = 2;
    }
}

// 延时部件攻击效果信息结构
public struct DelayPartAtkEffectInfo {
    public int CasterID;
    public int CasterPartID;
    public int TriggerFrame;          // 逻辑层这里记录到期的帧数
    public int Val;

    public bool CanTrigger(int curTick) {
        if( TriggerFrame <= curTick )
            return true;

        return false;
    }
}

public static class PartState {
    public const int Idle = 0;    // 空闲
    public const int Guard = 1;   // 警戒
    public const int Attack = 2;  // 攻击
}

// 消息类型
public static class TelegramType{
    public const int UnderAttack = 1;      // 遭受炮火攻击
    public const int SkillLead   = 2;      // 引导技能
    public const int SkillTrigger = 3;     // 使用技能
    public const int UnderSkillAttack = 4; // 遭受技能攻击
    public const int BreakAway        = 5; // 脱离战场
}


/// <summary>
/// Entity之间的通讯报文结构
/// </summary>
public class Telegram {
    private static int IDMaker = 0;
    public int GUID;            // 消息唯一标识符，我们要保证在延时触发的Tick一样的情况下，先加入队列的报文先被处理
    public int SenderID;        // Send Entity ID
    public int ReceiverID;      // Receive Entity ID
    public int MsgType;         // MsgType
    public int TriggerFrame;    // TriggerTick
    public int Val1;
    public int Val2;
    public float Val3;
    public float Val4;
    public float Val5;

    public Telegram() {
        GUID = IDMaker++;
        SenderID = 0;
        ReceiverID = 0;
        MsgType = 0;
        TriggerFrame = 0;
    }
}
