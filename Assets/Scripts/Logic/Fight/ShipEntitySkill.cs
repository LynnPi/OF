using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class ShipEntity : Entity {

    /// <summary>
    /// 执行技能
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="targetID"></param>
    private void DoSkill( ClientSkill skill, int targetID , Vector3 position) {

        proto.UseSkill useSkill = new proto.UseSkill();
        useSkill.skillid = skill.ID;
        useSkill.unitid = this.ID;
        useSkill.partid = skill.ControlPart.Id;
        useSkill.skillstage = Def.SkillStage.Casting;
        useSkill.posx = (int)(position.x * 10000);
        useSkill.posy = (int)(position.y * 10000);
        useSkill.posz = (int)(position.z * 10000);
        FightMessageCache.Instance.AddMessage( useSkill );

        var targetList = TargetSelector.FindSkillTarget( this, skill, targetID, position);

        for( int i = 0; i < targetList.Count; ++i ) {
            ShipEntity target = targetList[i];
            proto.PartFireInfo fireInfo = new proto.PartFireInfo();
            fireInfo.targetid = target.ID;
            fireInfo.delayframe = skill.Prop.continued_time;
            if( skill.Prop.missle_vel > 0 ) {
                float distance = Vector3.Distance( this.Ship.Position, target.Ship.Position );
                fireInfo.delayframe = Mathf.CeilToInt( distance / (skill.Prop.missle_vel * FightServiceDef.SPEED_SCALAR) );
            }
            useSkill.add_fireinfolist( fireInfo );

            // 给目标发一条被技能打击的消息
            MessageDispatcher.Instance.DispatchMessage( TelegramType.UnderSkillAttack, this.ID, target.ID, fireInfo.delayframe, skill.Prop.effect_val, skill.Prop.skill_effect_type );
        }
    }
}
