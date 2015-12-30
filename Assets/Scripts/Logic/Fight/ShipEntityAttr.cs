using UnityEngine;
using System.Collections;


// 处理属性相关的代码
public partial class ShipEntity {

    public int GetDurability() {
        return ShipProperty_.Durability.Value;
    }

    public int GetArmouredShield() {
        return ShipProperty_.ArmouredShield.Value;
    }

    public int GetEnergyShield() {
        return ShipProperty_.EnergyShield.Value;
    }
    private void InitAllAttr() {
        this.ShipProperty_.Durability = new GameValue( 0, this.Ship.MaxDurability * this.Ship.Reference.stack_num );
        this.ShipProperty_.Durability.SetToMax();
        this.ShipProperty_.ArmouredShield = new GameValue( 0, this.Ship.MaxArmorShield );
        this.ShipProperty_.ArmouredShield.SetToMax();
        this.ShipProperty_.EnergyShield = new GameValue( 0, this.Ship.MaxEnergyShield );
        this.ShipProperty_.EnergyShield.SetToMax();

        this.ShipProperty_.ShuttleTeam = new GameValue( 0, this.Ship.MaxShuttleTeam );
        this.ShipProperty_.ShuttleTeam.SetToMax();
        this.ShipProperty_.Eccm = new GameValue( 0, this.Ship.MaxEccm );
        this.ShipProperty_.Eccm.SetToMax();
        this.ShipProperty_.Speed = new GameValue( 0, this.Ship.MaxSpeed );
        this.ShipProperty_.Speed.SetToMax();
        this.ShipProperty_.Swspeed = new GameValue( 0, this.Ship.MaxSwspeed );
        this.ShipProperty_.Swspeed.SetToMax();
        this.ShipProperty_.ForceShield = new GameValue( 0, this.Ship.MaxForceShield );
        this.ShipProperty_.ForceShield.SetToMax();

    }

    /// <summary>
    /// 激活部件效果
    /// </summary>
    private void AtcivePartEffect() {
        foreach( var part in this.Ship.PartsList ) {
            proto.PartReference partRef=part.Reference;
            int effectValue=partRef.effect_val * (1 + partRef.effect_val_growthrate * (part.Level - 1));
            switch( partRef.passive_effect_type ) {
                case Def.PassiveEffectType.None: break;
                case Def.PassiveEffectType.DurablilityChg:
                    this.ShipProperty_.Durability.Change( effectValue );
                    break;
                case Def.PassiveEffectType.ArmorChg:
                    this.ShipProperty_.ArmouredShield.Change( effectValue );
                    break;
                case Def.PassiveEffectType.ShieldConversionvalChg:
                    this.ShipProperty_.EnergyShield.Change( effectValue );
                    break;
                case Def.PassiveEffectType.ShuttleTeamAdd:
                    this.ShipProperty_.ShuttleTeam.Change( effectValue );
                    break;
                case Def.PassiveEffectType.Eccm:
                    this.ShipProperty_.Eccm.Change( effectValue );
                    break;
                case Def.PassiveEffectType.SpeedChg:
                    this.ShipProperty_.Speed.Change( effectValue );
                    break;
                case Def.PassiveEffectType.SwspeedChg:
                    this.ShipProperty_.Swspeed.Change( effectValue );
                    break;
                case Def.PassiveEffectType.ForceShield:
                    this.ShipProperty_.ForceShield.Change( effectValue );
                    break;
            }
        }
    }


    private void ApplyAttackEffect( int val, int EffectType ) {
        proto.UnderAttackInfo info = new proto.UnderAttackInfo();
        info.unitid = this.ID;
        switch( EffectType ) {
            case Def.PartsEffectType.LaserDmg:   // 激光伤害
                if( this.ShipProperty_.EnergyShield.Enough( val ) ) {
                    this.ShipProperty_.EnergyShield.Change( -val );
                    info.energy = -val;
                }
                else {
                    info.energy = -this.ShipProperty_.EnergyShield.Value;
                    info.durablility = - val + this.ShipProperty_.EnergyShield.Value;
                    this.ShipProperty_.EnergyShield.SetToMin();
                    this.ShipProperty_.Durability.Change(info.durablility);
                }
                break;
            case Def.PartsEffectType.KineticEnergyDmg: {  // 实弹伤害
                if( this.ShipProperty_.ArmouredShield.Enough( val ) ) {
                    this.ShipProperty_.ArmouredShield.Change( -val );
                    info.armor = -val;
                }
                else {
                    info.armor = -this.ShipProperty_.ArmouredShield.Value;
                    info.durablility = -val + this.ShipProperty_.ArmouredShield.Value;
                    this.ShipProperty_.ArmouredShield.SetToMin();
                    this.ShipProperty_.Durability.Change( info.durablility );
                }
                }
                break;
            case Def.PartsEffectType.ParticleDmg: { // 粒子束伤害
                    // TODO  力场护盾 百分比调整伤害
                    info.durablility -= val;
                    this.ShipProperty_.Durability.Change( info.durablility );
                }
                break;
        }

        if( this.Ship.Reference.stack ) {
            this.StackAliveNum_ = Mathf.CeilToInt( ((float)this.ShipProperty_.Durability.Value / this.ShipProperty_.Durability.Max) * this.Ship.Reference.stack_num );
        }

        FightMessageCache.Instance.AddMessage( info );
    }


    private int GetBaseAtkValue(ClientParts castPart, ShipEntity targetEntity) {
        int baseAtk = 0;

        // 攻防产生的系数
        int coefficient = 10000 +  (castPart.AC - targetEntity.Ship.DC) * 10000 / (castPart.AC + targetEntity.Ship.DC);
        
        // 应用系数调整
        baseAtk = Utility.GetValueByRate( castPart.Reference.effect_val, coefficient );

        // 优劣式增幅
        baseAtk = Utility.GetValueByRate( baseAtk, GetAdvantageIncrease( castPart, targetEntity ) );

        return baseAtk;
    }


    // 设置优势增幅
    private int GetAdvantageIncrease(ClientParts castPart, ShipEntity targetEntity) {
        if( castPart.Reference.advantage_unitstrait_type == targetEntity.Ship.Reference.unitstrait ) {
            return 15000;
        }
        else if( castPart.Reference.disadvantage_unitstrait_type == targetEntity.Ship.Reference.unitstrait ) {
            return 5000;
        }

        return 10000;
    }
}
