using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    class Buff {

        private proto.BuffReference Prop;
        private ShipEntity ShipEntity;
        private int BeginTick;

        public Buff( proto.BuffReference buffRef, ShipEntity shipEntity) {
            BeginTick = FightTicker.Ticker;
            ShipEntity = shipEntity;
            Prop = buffRef;
//            if( ShipEntity != null ){
//                 NowArmouredShield = ShipEntity.Ship.Reference.durability * (1 + ShipEntity.Ship.Reference.durability_growthrate * (ShipEntity.Ship.Level - 1));
//                 NowEnergyShield = ShipEntity.Ship.Reference.energy * (1 + ShipEntity.Ship.Reference.energy_growthrate * (ShipEntity.Ship.Level - 1));
//            }
 
        }

        /// <summary>
        /// 得到Buff开始的时间
        /// </summary>
        /// <returns></returns>
        public int GetBeginTick() {
            return BeginTick;
        }

        /// <summary>
        /// 得到持续时间
        /// </summary>
        /// <returns></returns>
        public int GetDurationMsTime() { 
            return Prop.time;  
        }

        /// <summary>
        /// 得到总效果值
        /// </summary>
        /// <returns></returns>
        public int GetEffectNum() {
            return Prop.effect_val;
        }

        /// <summary>
        /// 得到间隔时间
        /// </summary>
        /// <returns></returns>
        public int GetIntervalTime() {
            return Prop.intervaltime;
        }

        /// <summary>
        /// 得到Buff类型
        /// </summary>
        /// <returns></returns>
        public int GetBuffType() {
            return Prop.bufftype;
        }
    }
