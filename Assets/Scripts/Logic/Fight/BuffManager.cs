using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class BuffManager {
    /// <summary>
    /// 所有BUFF的标记
    /// </summary>
    private Int32 StatusMask_;
    private ShipEntity ShipEntity;
    /// <summary>
    /// 所有BUFF的合集
    /// </summary>
    private Dictionary<int,Buff> BuffList;
    public BuffManager(ShipEntity shipEntity) {
        StatusMask_ = 0;
        BuffList=new Dictionary<int,Buff>();
        ShipEntity = shipEntity;
    }

    /// <summary>
    /// 增加BUFF
    /// </summary>
    /// <param name="buffId"></param>
    /// <param name="level"></param>
    public void AddBuff( int buffId, int level ) {

        var prop = GlobalConfig.GetBuffReference( buffId );

        Buff tempBuff = new Buff( prop, ShipEntity );
        if( !BuffList.ContainsKey( buffId ) ) {
            BuffList.Add( buffId, tempBuff );
        }
        else {
            //BUFF叠加机制
            if(  BuffList[buffId].GetEffectNum()<=tempBuff.GetEffectNum() ) {
                BuffList[buffId] = tempBuff;
            }                
        }
        StatusMask_ |= GetBuffStatusById( buffId );        
    }

    /// <summary>
    /// 检查身上是否有这个BUFF
    /// </summary>
    /// <param name="buffId"></param>
    /// <returns></returns>
    public bool CheckBuff( int buffId ) {
        if( (StatusMask_ & GetBuffStatusById( buffId )) != 0 )
            return true;
        return false;
    }

    /// <summary>
    /// 删除BUFF
    /// </summary>
    /// <param name="buffId"></param>
    public void DelBuff( int buffId ) {
        StatusMask_ &= ~GetBuffStatusById( buffId );
        if( BuffList.ContainsKey( buffId ) ) {
            BuffList.Remove( buffId);
        }
    }

    /// <summary>
    /// 自动对BUFF的更新
    /// </summary>
    public void Update() {
        foreach( var buff in BuffList ) {
            //如果间隔时间为0。即一直持续则跳过
            if(FightTicker.Ticker - buff.Value.GetBeginTick() > buff.Value.GetDurationMsTime()){
                DelBuff(buff.Key);
                continue;
            }
            if( buff.Value.GetIntervalTime() == 0 )
                continue;
                //如果BUFF有间隔作用时间
            else if( (FightTicker.Ticker - buff.Value.GetBeginTick()) % buff.Value.GetIntervalTime() == 0) {
                //使用BUFF效果
                BuffEffect.Instance.CalcBuffEffect( ShipEntity,buff.Value.GetBuffType() );
            }
        }
    }

    /// <summary>
    /// 将BuffId化为Buff的状态类型
    /// </summary>
    private int GetBuffStatusById( int buffId ) {
        proto.BuffReference buffRef= GlobalConfig.GetBuffReference( buffId );
        switch( buffRef.bufftype ) {
            case Def.BuffType.ArmouredShield:
                return BuffStatus.ArmouredShield;
            case Def.BuffType.EnergyShield:
                return BuffStatus.EnergyShield;
            default: return BuffStatus.None;
        }
    }

    // 状态类型
    public static class BuffStatus {
        public const int None = 0;          //  无效
        public const int ArmouredShield = 1;          // 装甲护盾
        public const int EnergyShield   = 2;          // 能量护盾
    }
}
