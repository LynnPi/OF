using UnityEngine;
using System.Collections;

public struct EntityProperty {

    /// <summary>
    /// 耐久值
    /// </summary>
    public GameValue Durability;
    /// <summary>
    /// 装甲护盾值
    /// </summary>
    public GameValue ArmouredShield;
    /// <summary>
    /// 能量护盾值
    /// </summary>
    public GameValue EnergyShield;
    /// <summary>
    /// 单位舰载机组
    /// </summary>
    public GameValue ShuttleTeam;
    /// <summary>
    /// 抵抗ECM BUFF
    /// </summary>
    public GameValue Eccm;
    /// <summary>
    /// 单位速度
    /// </summary>
    public GameValue Speed;
    /// <summary>
    /// 单位侧移速度
    /// </summary>
    public GameValue Swspeed;
    /// <summary>
    /// 力场护盾
    /// </summary>
    public GameValue ForceShield;
}
