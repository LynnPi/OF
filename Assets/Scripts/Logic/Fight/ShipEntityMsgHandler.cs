using UnityEngine;
using System.Collections;

/// <summary>
/// 处理消息相关的代码放这里
/// </summary>
public partial class ShipEntity {

    /// <summary>
    /// 处理 Entity 消息
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public override bool HandleMessage( Telegram msg ) {
        switch( msg.MsgType ) {
            case TelegramType.UnderAttack: {
                    OnUndarAttack( msg );
                }
                return true;
            case TelegramType.SkillLead: {
                    OnSkillLead( msg );
                }
                return true;
            case TelegramType.SkillTrigger: {
                    OnSkillTrigger( msg );
                }
                return true;
            case TelegramType.UnderSkillAttack: {
                    OnUnderSkillAttack( msg );
                }
                return true;
            case TelegramType.BreakAway: {
                    OnBreakAway( msg );
                }
                return true;

        }

        return false;
    }

    /// <summary>
    /// 遭受攻击
    /// </summary>
    /// <param name="msg"></param>
    private void OnUndarAttack( Telegram msg ) {

        ApplyAttackEffect( msg.Val1, msg.Val2 );
        if( !this.IsActive() ) {
            EntityManager.Instance.RemoveEntity( this );
            FightAnalyzer.Instance.OnUnitDestory( this );
        }
    }

    /// <summary>
    /// 技能引导
    /// </summary>
    /// <param name="msg"></param>
    private void OnSkillLead( Telegram msg ) {
        ClientSkill skill = this.Ship.SkillDic[msg.Val1];
        skill.StartCoolDown();
        proto.UseSkill useSkill = new proto.UseSkill();
        useSkill.unitid = this.ID;
        useSkill.skillid = skill.ID;
        useSkill.partid = skill.ControlPart.Id;
        useSkill.skillstage = Def.SkillStage.Sing;
        useSkill.posx = (int)(msg.Val3 * 10000);
        useSkill.posy = (int)(msg.Val4 * 10000);
        useSkill.posz = (int)(msg.Val5 * 10000);

        FightMessageCache.Instance.AddMessage( useSkill );

        // 再给自己发一条技能触发的延时消息
        MessageDispatcher.Instance.DispatchMessage( TelegramType.SkillTrigger, this.ID, this.ID, skill.Prop.skill_lead_time, msg.Val1, msg.Val2, msg.Val3, msg.Val4, msg.Val5 );
    }

    /// <summary>
    /// 技能触发
    /// </summary>
    /// <param name="msg"></param>
    private void OnSkillTrigger( Telegram msg ) {
        ClientSkill skill = this.Ship.SkillDic[msg.Val1];
        int targetID = msg.Val2;
        Vector3 vPosition;
        vPosition.x = msg.Val3;
        vPosition.y = msg.Val4;
        vPosition.z = msg.Val5;

        this.DoSkill( skill, targetID, vPosition );
    }

    /// <summary>
    /// 遭受技能攻击
    /// </summary>
    /// <param name="msg"></param>
    private void OnUnderSkillAttack( Telegram msg ) {
        // 先暂时用和UnderAttack一个逻辑
        ApplyAttackEffect( msg.Val1, msg.Val2 );

        if( !this.IsActive() ) {
            EntityManager.Instance.RemoveEntity( this );
            FightAnalyzer.Instance.OnUnitDestory( this );
        }
    }

    private void OnBreakAway( Telegram msg ) {

        this.BehaviorMsg_.Clear();
        this.BehaviorMsg_.unitid = this.ID;
        this.BehaviorMsg_.breakaway = true;
        FightMessageCache.Instance.AddMessage( this.BehaviorMsg_ );

        EntityManager.Instance.RemoveEntity( this );
    }
}
