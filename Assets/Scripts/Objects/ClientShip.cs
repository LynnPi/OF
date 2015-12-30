using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 客户端舰船对象
/// </summary>
public class ClientShip  {

    public proto.UnitReference Reference{set; get;}

    public int Id { set; get; }

    public int InFightID { set; get; }
    public int Level { set; get; }

    public Vector3 Position;

    // 提供按KEY查找
    public Dictionary<int, ClientParts> PartsDic = new Dictionary<int,ClientParts>(); 

    // 提供快速遍历
    public List<ClientParts> PartsList = new List<ClientParts>();

    // 技能列表 有且仅有指挥舰才有技能（指挥就是这么屌-_-）
    public Dictionary<int, ClientSkill> SkillDic = new Dictionary<int, ClientSkill>();

    // 阵形的坐标列表,在舰船为编队时有效,如果为空,说明为单一舰船,则设置为v3.zero,
    public Vector3[] FormationList;
        

    // 防御级别
    public int DefenseClass_;

    public int Armor {
        get;
        set;
    }

    public int Energy {
        get;
        set;
    }

    public int Durability {
        get;
        set;
    }
    public int MaxDurability {
        get {
            return ShipProperty_.MaxDurability;
        }
    }

    public int MaxArmorShield {
        get {
            return ShipProperty_.MaxArmor;
        }
    }

    public int MaxEnergyShield {
        get {
            return ShipProperty_.MaxEnergy;
        }
    }

    public int MaxShuttleTeam {
        get {
            return ShipProperty_.MaxShuttleTeam;
        }
    }

    public int MaxEccm {
        get {
            return ShipProperty_.MaxEccm;
        }
    }

    public int MaxSpeed {
        get {
            return ShipProperty_.MaxSpeed;
        }
    }

    public int MaxSwspeed {
        get {
            return ShipProperty_.MaxSwspeed;
        }
    }

    public int MaxForceShield {
        get {
            return ShipProperty_.MaxForceShield;
        }
    }


    /// <summary>
    /// 防御级别
    /// </summary>
    public int DC {
        get {
            return DefenseClass_;
        }
        set{
            DefenseClass_ = value;
        }
    }

    public string Name { get { return Reference.name; } }


    public ClientProperty ShipProperty_ = new ClientProperty();

    public void ReadFromId( int id ) {
        ShipProperty_.ClearAll();

        Id = id;
        Level = 1;
        ReadFromReference( GlobalConfig.GetUnitReference( Id ) );
        ShipProperty_.MaxDurability = Reference.durability;
        ShipProperty_.MaxArmor = Reference.armor;
        ShipProperty_.MaxEnergy = Reference.energy;
        ShipProperty_.MaxShuttleTeam = Reference.shuttle_team;
        ShipProperty_.MaxEccm = 0;  //因为舰船不自带，所以初始化为0
        ShipProperty_.MaxSpeed = Reference.speed_max;
        ShipProperty_.MaxSwspeed = Reference.sw_speed;
        ShipProperty_.MaxForceShield = 0;   //因为舰船不自带，所以初始化为0

        Durability = MaxDurability;
        DefenseClass_ = Reference.dc_initial; // 等级暂时不做处理

        // 默认开放所有的部件
        if( Reference.parts_1 != 0 ) {
            ClientParts parts = new ClientParts();
            parts.ReadFromProto( Reference.parts_1 );
            parts.Level = 1;
            parts.Owner = this;
            this.PartsDic[parts.Id] = parts;
            this.PartsList.Add( parts );
            if (this.Reference.unitstrait == Def.ShipTrait.CommanderShip)
                ActivateCommanderSkill( parts );
        }

        if( Reference.parts_2 != 0 ) {
            ClientParts parts = new ClientParts();
            parts.ReadFromProto( Reference.parts_2 );
            parts.Level = 1;
            parts.Owner = this;
            this.PartsDic[parts.Id] = parts;
            this.PartsList.Add( parts );
            if( this.Reference.unitstrait == Def.ShipTrait.CommanderShip )
                ActivateCommanderSkill( parts );
        }

        if( Reference.parts_3 != 0 ) {
            ClientParts parts = new ClientParts();
            parts.ReadFromProto( Reference.parts_3);
            parts.Level = 1;
            parts.Owner = this;
            this.PartsDic[parts.Id] = parts;
            this.PartsList.Add( parts );
            if( this.Reference.unitstrait == Def.ShipTrait.CommanderShip )
                ActivateCommanderSkill( parts );            
        }
    }

    public void ReadFromReference( proto.UnitReference srcReference ) {
        ShipProperty_.ClearAll();

        Reference = srcReference;
        Position = Vector3.zero;
    }

    public void ReadFromProto( proto.Ship protoShip ) {
        ShipProperty_.ClearAll();

        Id = protoShip.id;
        Level = protoShip.level;

        // TODO: 设置 unitReference 以及其他属性初始化
        ReadFromId( Id );
    }

    public ClientParts GetPartsByID(int id) {
        if (PartsDic.ContainsKey(id))
          return PartsDic[id];

        return null;
    }

    public bool IsCommanderShip() {
        return this.Reference.unitstrait == Def.ShipTrait.CommanderShip;
    }

    public int GetShipStrait() {
        return this.Reference.unitstrait;
    }

    // 
    public static ClientShip Clone( ClientShip ship ) {
        var newShip = new ClientShip();
        newShip.Id = ship.Id;
        newShip.InFightID = ship.InFightID;
        newShip.Level = ship.Level;
        newShip.Reference = ship.Reference;
        newShip.Position = ship.Position;
        newShip.DefenseClass_ = ship.DC;
        newShip.FormationList = ship.FormationList;

        for( int i = 0; i < ship.PartsList.Count; i++ ) {
            var parts = ship.PartsList[i];
            parts.Owner = newShip;
            parts.CoolDownTime = 0;
            newShip.PartsList.Add( parts );
        }

        foreach( var part in ship.PartsDic ) {
            newShip.PartsDic[part.Key] = part.Value;
        }

        foreach( var skill in ship.SkillDic ) {
            skill.Value.CoolDownTime = 0;
            newShip.SkillDic[skill.Key] = skill.Value;
        }

        newShip.ShipProperty_ = ClientProperty.Clone( ship.ShipProperty_);
        newShip.Durability = newShip.ShipProperty_.MaxDurability;

        newShip.Armor = ship.Armor;
        newShip.Energy = ship.Energy;
        return newShip;
    }


    /// <summary>
    /// 激活指挥技能（这个代码可能就是服务于Demo的）
    /// </summary>
    public void ActivateCommanderSkill(ClientParts part) {
        ClientSkill skill = ClientSkill.CreateSkillByID( part.Reference.skill_attach );
        if( skill == null )
            return;

        skill.ControlPart = part;
        this.SkillDic.Add( skill.ID,skill );
    }


    /// <summary>
    /// 按索引获取技能对象
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ClientSkill GetSkillByIndex( int index ) {
       int count = 0;
       foreach(var skill in SkillDic.Values){
           if( count == index )
               return skill;
           count++;
       }

       return null;
    }

    public ClientSkill GetSkillById( int id ) {
//         foreach( var skill in SkillDic.Values ) {
//             if( skill.ID == id )
//                 return skill;
//         }
        if( SkillDic.ContainsKey( id ) )
            return SkillDic[id];
        return null;
    }
}
