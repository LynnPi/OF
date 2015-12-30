using System;
using System.Collections;

public class ClientSkill {

    public proto.SkillReference Prop;
    public int ID {
        get;
        set;
    }

    public int Level {
        get;
        set;
    }


    public int CoolDownTime {
        get;
        set;
    }

    /// <summary>
    /// 弹药数
    /// </summary>
    public int Ammo {
        get;
        set;
    }

    /// <summary>
    /// 该技能控制的部件
    /// </summary>
    public ClientParts ControlPart {
        get;
        set;
    }

    public void StartCoolDown() {
        CoolDownTime = Prop.cd /2 + FightTicker.Ticker;
    }

    public bool IsCoolDownFinished() {
        if( FightTicker.Ticker >= CoolDownTime ) {
            return true;
        }

        return false;
    }

    // TODO : 要在proto里面定义一个服务器的skill proto对象
    public void ReadFromProto() {

    }

    // 临时接口
    public static ClientSkill CreateSkillByID(int Id) {
        var prop = GlobalConfig.GetSkillReference( Id );
        if (prop == null)
            return null;

        ClientSkill skill = new ClientSkill();
        skill.Prop = prop;
        skill.Level = 1;
        skill.Ammo = skill.Prop.ammo_num;
        skill.ID = Id;
        skill.CoolDownTime = skill.Prop.cd;

        return skill;
    }
}
