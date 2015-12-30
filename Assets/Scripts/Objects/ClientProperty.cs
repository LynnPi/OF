using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ClientProperty {

    // 耐久上限 相当于HP
    public int MaxDurability { get; set; }

    // 最大能量值
    public int MaxEnergy { get; set; }

    // 护甲上限 
    public int MaxArmor { get; set; }

    // 单位护盾
    public int MaxShield { get; set; }

    /// <summary>
    /// 单位舰载机组
    /// </summary>
    public int MaxShuttleTeam { get; set; }
    /// <summary>
    /// 抵抗ECM BUFF
    /// </summary>
    public int MaxEccm { get; set; }
    /// <summary>
    /// 单位速度
    /// </summary>
    public int MaxSpeed { get; set; }
    /// <summary>
    /// 单位侧移速度
    /// </summary>
    public int MaxSwspeed { get; set; }
    /// <summary>
    /// 力场护盾
    /// </summary>
    public int MaxForceShield { get; set; }

    public void ClearAll() {
        MaxDurability = 0;
        MaxEnergy = 0;
        MaxArmor = 0;
        MaxShield = 0;

        MaxShuttleTeam = 0;
        MaxEccm = 0;
        MaxSpeed = 0;
        MaxSwspeed = 0;
        MaxForceShield = 0;
    }

    public static ClientProperty Clone( ClientProperty srcPro ) {
        ClientProperty newProperty = new ClientProperty();
        newProperty.MaxDurability = srcPro.MaxDurability;
        newProperty.MaxEnergy     = srcPro.MaxEnergy;
        newProperty.MaxArmor      = srcPro.MaxArmor;
        newProperty.MaxShield     = srcPro.MaxShield;

        newProperty.MaxShuttleTeam = srcPro.MaxShuttleTeam;
        newProperty.MaxEccm = srcPro.MaxEccm;
        newProperty.MaxSpeed = srcPro.MaxSpeed;
        newProperty.MaxSwspeed = srcPro.MaxSwspeed;
        newProperty.MaxForceShield = srcPro.MaxForceShield;

        return newProperty;
    }
}
