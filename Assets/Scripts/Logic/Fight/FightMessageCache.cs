using System;
using System.Collections;


/// <summary>
/// 战斗消息容器
/// </summary>
public class FightMessageCache {

    public static readonly FightMessageCache Instance = new FightMessageCache();
    private proto.S2CFightFrameInfo FrameInfoMsg_ = new proto.S2CFightFrameInfo();
    private FightMessageCache() {

    }

    public void AddMessage( ProtoMessage message ) {
        if( message is proto.UnitBehavior ) {
            FrameInfoMsg_.add_behaviorsequences( message as proto.UnitBehavior );
        }
        else if(message is proto.UnderAttackInfo) {
            FrameInfoMsg_.add_underattacklist( message as proto.UnderAttackInfo );
        }
        else {
            FrameInfoMsg_.add_useskill( message as proto.UseSkill );
        }
    }

    public void ClearAll() {
        FrameInfoMsg_.Clear();
    }


    public proto.S2CFightFrameInfo GetFrameInfoMsg() {
        return FrameInfoMsg_;
    }
}
