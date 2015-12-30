using System;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;

public class ProtoID {

    public const int C2SLogin = 1;
    public const int S2CLogin = 2;
    public const int C2SHeartBeat = 1;
    public const int S2CHeartBeat = 2;
    public const int S2CSetCustomSetting  = 3;
    public const int C2SSetCustomSetting = 4;
}

public partial class Def {
    //常用
    public const int MAX_RATE_INT = 10000;//几率分母值
    public const float MAX_RATE_FLOAT = 10000.0f;//几率分母值
    public const string SPRITE_INVALID = "null";
    public const int MAX_PLAYER_IB = 2000000000;
    public const int DB_TIMEOUT = 10;

    public static UnityEngine.Color DamageColor = new UnityEngine.Color ( 211 / 255.0f, 22 / 255.0f, 26 / 255.0f );
    public static UnityEngine.Color CureColor = new UnityEngine.Color ( 1 / 255.0f, 199 / 255.0f, 61 / 255.0f );

    public static class BuffType {
        public const int ArmouredShield=1;  //装甲护盾
        public const int EnergyShield=2;    //能量护盾
    }

    // 状态类型
    public static class StatusType {
        public const int Dizziness = 1;          // 眩晕
        public const int Clocaking = 2;          // 隐身
        public const int Silence   = 4;          // 沉默
        public const int ImuPhy = 8;             // 免疫物理攻击
        public const int ImuMag = 16;            // 免疫魔法攻击
        public const int ImuAll = 32;            // 免疫所有攻击
        public const int LifeShiled = 64;        // 生命护盾
    }

    public static class ServerStatusType {
        /// <summary>
        /// 维护
        /// </summary>
        public const int Maintain = 0;
        /// <summary>
        /// 新服
        /// </summary>
        public const int NewServer = 1;
        /// <summary>
        /// 良好
        /// </summary>
        public const int Well = 2;
        /// <summary>
        /// 爆满
        /// </summary>
        public const int Full = 3;
        /// <summary>
        /// 未开启
        /// </summary>
        public const int NotOpen = 4;
    }


    public static class GenErrorType {
        public const int OK = 0;
        public const int Code_Not_Exist = 1;        // 激活码不存在
        public const int Code_Not_OnTime = 2;       // 激活码不在激活时间段内
        public const int Code_Sid_Error = 3;        // 激活区服错误
        public const int Code_Plaform_Error = 4;    // 平台错误
        public const int Gift_Not_Exist = 5;        // 激活码礼包不存在
        public const int Gift_AlreadyGet = 6;       // 该激活码的礼包已经领取
    }

    public static class RechargeOpType {
        public const int RequestNewBill = 1;        // 请求新的订单
        public const int UnProcesBill   = 2;        // 未完成订单
        public const int VerifyBill     = 3;        // 验证账单
    }

    public static bool RateRand ( int rate ) {
        return UnityEngine.Random.Range ( 0, Def.MAX_RATE_INT ) <= rate;
    }


    public static class MoveType {
        public const int Forward = 1;  // 向前推进
        public const int Lateral = 2;  // 侧移
    }

    //技能阶段
    public static class SkillStage {
        public const int Sing = 1;    // 吟唱 (策划这边叫技能引导,反正都一样，我们叫吟唱好了)
        public const int Casting = 2; // 施展
    }

    // 舰船类型
    public static class ShipType {
        public const int None = 0;
        public const int Build = 1;         // 建筑类
        public const int Boat  = 2;         // 小型舰
        public const int WarShip = 3;       // 大型舰
        public const int CommanderShip = 4; // 指挥舰
    }

    /// <summary>
    /// 舰艇特性
    /// </summary>
    public static class ShipTrait {
        public const int None = 0;
        public const int Attack = 1;  // 攻击型
        public const int Defend = 2;  // 防御型
        public const int Support = 3; // 支援型
        public const int Carrier = 4; // 载机型
        public const int CommanderShip = 5; // 指挥舰
        public const int Build       = 6; // 建筑类
        public const int Boat        = 7; // 艇类
        public const int DefenseBuild = 8; // 防御设施
    }

    /// <summary>
    /// 舰艇扩展特性，主要用于部件开火目标的优先级判断
    /// </summary>
    public static class ShipExtendTrait {
        public const int None         = 0;
        public const int WarShip      = 1; // 大型舰
        public const int CarrierPlane = 2; // 舰载飞行器
        public const int Lander       = 3; // 登陆器
        public const int Boss         = 4; // Boss
        public const int Other        = 5; // 其他类
    }

    // 部件目标类型
    public static class PartsTargetType {
        public const int None = 0;
        public const int Default = 1;                    // 默认
        public const int Self = 2;                       // 自身
        public const int Random = 3;                     // 随机
        public const int ALL    = 4;                     // 敌友全体
        public const int ClosestFriend = 5;              // 最近友方
        public const int FarthestFriend = 6;             // 最远友方
        public const int RandomFriend = 7;               // 随机友方
        public const int AllFriend    = 8;               // 全部友方
        public const int InAttackRangeClosestFriend = 9; // 攻击范围内最近友方
        public const int InAttackRangeallFriend = 10;    // 攻击范围内所有友方
        public const int DurabilityLowestFriend = 11;    // 耐久值最低友方
        public const int ArmorLowestFriend = 12;         // 装甲值最低友方
        public const int Energylowestfriend = 13;        // 能量值最低友方
        public const int AllFriendBuilding  = 14;        // 所有友方建筑
        public const int ClosestFriendBuilding = 15;     // 最近友方建筑
        public const int ClosestEnemy = 16;              // 最近敌方
        public const int FarthestEnemy = 17;             // 最远敌方
        public const int RandomEnemy = 18;               // 随机敌方
        public const int AllEnemy = 19;                  // 全部敌方
        public const int InAttackRangeallEnemy = 20;     // 攻击范围内所有敌方
        public const int DurabilityLowestEnemy = 21;     // 耐久值最低敌方
        public const int ArmorLowestEnemy = 22;          // 装甲值最低敌方
        public const int EnergyLowestEnemy = 23;          // 能量值最低敌方
        public const int AllEnemyBuilding = 24;           // 所有敌方建筑
        public const int ClosestEnemyBuilding = 25;       // 最近敌方建筑
        public const int HitMeEnemy = 26;                 // 攻击自己的单位
    }

    // 部件效果类型
    public static class PartsEffectType {
        public const int NONE = 0;
        public const int LaserDmg = 1;             // 光束伤害
        public const int KineticEnergyDmg = 2;     // 实弹伤害
        public const int ParticleDmg = 3;          // 粒子束伤害
        public const int DurabilityChg = 4;        // 恢复单位耐久度
        public const int ArmorRec = 5;             // 恢复单位装甲值
        public const int EnergyRec = 6;            // 恢复单位能量
        public const int Ecm       = 7;            // 对目标赋予ecm buff
    }

    public static class ShapeType {
        public const int Default = 0;           // 默认
        public const int Rectangle = 1;         // 矩形
        public const int Circle    = 2;         // 圆形
    }


    // 技能选择类型
    public static class SkillSelectType {
        public const int None = 0;              // 空
        public const int CastScope = 1;         // 技能判定范围
        public const int NoSelection = 2;       // 不需要选择
        public const int PlayerSelect = 3;      // 玩家全战场选择类
    }


    public static class SkillTargetType {
        public const int None = 0;           // 空
        public const int Default = 1;        // 默认
        public const int Self = 2;           // 自身
        public const int Random = 3;         // 随机
        public const int All = 4;            // 敌方全体
        public const int ClosestFriend = 5;  // 最近友方
        public const int FarthestFriend = 6; // 最远友方
        public const int RandomFriend = 7;   // 随机友方
        public const int AllFriend    = 8;   // 所有友方
        public const int InAttackRangeCloseestFriend = 9; // 攻击范围内的最近友方
        public const int InAttackRangeAllFriend = 10;     // 攻击范围内的所有友方
        public const int DurabilityLowestfriend = 11;     // 耐久最低的友方
        public const int ArmorLowestFriend      = 12;     // 护甲最低的友方
        public const int EnergyLowestFriend     = 13;     // 装甲值最低的友方
        public const int AllFriendBuilding      = 14;     // 所有友方建筑
        public const int ClosestFriendBuilding  = 15;     // 最近敌方建筑
        public const int ClosestEnemy           = 16;     // 最近敌方
        public const int FarthestEnemy          = 17;     // 最远敌方
        public const int RandomEnemy            = 18;     // 最远敌方
        public const int AllEnemy               = 19;     // 所有敌人
        public const int InAttackRangeAllEnemy  = 20;     // 攻击范围内的所有目标
        public const int DurabilityLowestEnemy  = 21;     // 耐久最低的敌方
        public const int ArrmorLowestEnemy      = 22;     // 装甲耐久
        public const int EnergyLowestEnemy      = 23;     // 能量值最低地方
        public const int AllEnemyBuilding       = 24;     // 所有敌方建筑
        public const int ClosestEnemyBuilding   = 25;     // 最近敌方建筑
    }

    public static class CastTargetType {
        public const int None = 0;
	    public const int Default = 1;                          // 攻击范围内的最近敌方
	    public const int InAttackRangeAllEnemy = 2;            // 攻击范围内的所有敌方
        public const int InAttackRangeClosestFriend = 3;       // 攻击范围内最近友方
	    public const int InAttackRangeAllFriend = 4;           // 攻击范围内所有友方
	    public const int Self = 5;                             // 自身
	    public const int BothSides = 6;                        // 敌友全体
	    public const int Friend = 7;                           // 友方
	    public const int Enemy = 8;                            // 敌方
	    public const int ClosestFriend = 9;                    // 最近友方
        public const int FarthestFriend = 10;                  // 最远友方
    	public const int DurabilityLowestFriend = 11;          // 耐久最低友方
	    public const int ArmorLowestFriend = 12;               // 装甲最低友方
    	public const int EnergyLowestFriend = 13;              // 能量最低友方
        public const int AllFriendBuilding = 14;               // 所有友方建筑
	    public const int ClosestFriendBuilding = 15;           // 最近友方建筑
	    public const int AllFriend = 16;                       // 全体友方
        public const int ClosestEnemy = 17;                    // 最近敌方
	    public const int FarthestEnemy = 18;                   // 最远友方
	    public const int DurabilityLowestEnemy = 19;           // 耐久值最低的敌方
	    public const int ArmorLowestEnemy = 20;                // 装甲值最低的敌方
	    public const int EnergyLowestEnemy = 21;               // 能量值最低的敌方
	    public const int AllEnemyBuilding = 22;                // 所有敌方建筑
	    public const int ClosestEnemyBuilding = 23;            // 最近敌方建筑
	    public const int AllEnemy = 24;                        // 所有敌方
        public const int All = 25;                             // 全体
    }

    public static class SkillEffectType {
        public const int None = 0;
        public const int LaserDmg = 1;                   // 光束伤害
        public const int KineticEnergyDmg = 2;           // 实弹伤害
        public const int ParticleDmg      = 3;           // 粒子束伤害
        public const int DurablilityRec   = 4;           // 恢复单位耐久度
        public const int ArmorRec         = 5;           // 恢复单位装甲值
        public const int EneryRec         = 6;           // 恢复单位能量
        public const int SpeedChg         = 7;           // 单位速度变化
    }

    // 状态类型
    public static class BuffStatus {
        public const int None = 0;                  //  无效
        public const int ArmouredShield = 1;        // 装甲护盾
        public const int EnergyShield = 2;          // 能量护盾
    }

    public static class PassiveEffectType {
        public const int None = 0;
        public const int DurablilityChg = 1;         // 单位耐久度变化
        public const int ArmorChg       = 2;         // 单位装甲值变化
        public const int ShieldConversionvalChg = 3; // 单位能量护盾转换值变化
        public const int ShuttleTeamAdd = 4;         // 单位增加舰载机组
        public const int Eccm           = 5;         // 抵抗Ecm_buff,
        public const int SpeedChg       = 6;         // 单位速度变化
        public const int SwspeedChg     = 7;         // 单位侧移速度变化
        public const int ForceShield    = 8;         // 力场护盾
    }

    // 战场类型
    public static class BattlefieldType {
        public const int PveMap = 1;         // PVE 战场
        public const int PveMission = 2;     // PVE 任务战
        public const int PvpAggression = 4;  // PVE 侵略战
        public const int Fleet         = 5;  // PVP 舰队战战场
    }

    // 战斗评级
    public static class BattleGrade {
        public const int D = 1;
        public const int C = 2;
        public const int B = 3;
        public const int A = 4;
        public const int S = 5;
    }
}