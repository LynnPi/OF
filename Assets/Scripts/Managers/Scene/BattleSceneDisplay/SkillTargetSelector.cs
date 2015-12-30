using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 技能目标显示用选择器
/// </summary>
public class SkillTargetSelector {

    /// <summary>
    /// 是否在矩形区域内
    /// </summary>
    /// <param name="trans1"></param>
    /// <param name="trans2"></param>
    /// <param name="width"></param>
    /// <param name="length"></param>
    /// <param name="vol"></param>
    /// <returns></returns>
    private static bool IsInRectangle( Transform trans1, Transform trans2, float width, float length, float vol ) {
        width += vol;
        length += vol;
        if( trans2.position.z > trans1.position.z ) {
            if( Mathf.Abs( trans2.position.x - trans1.position.x ) <= width && Mathf.Abs( trans2.position.z - trans1.position.z ) <= length )
                return true;
        }
        return false;
    }

    /// <summary>
    /// 是否在扇形区域内
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="trans2"></param>
    /// <param name="angle"></param>
    /// <param name="range"></param>
    /// <param name="vol"></param>
    /// <returns></returns>
    private static bool IsInSector( Vector3 pos1, Transform trans2, float angle, float range, float vol ) {
        range += vol;
        // 超过距离
        if( Vector3.Distance( pos1, trans2.position ) > range )
            return false;
        Vector3 dircetion = trans2.position - pos1;
        Vector3 forward = Vector3.forward;
        float tempAngle = Vector3.Angle( dircetion.normalized, forward );
        if( tempAngle * 2 > angle )
            return false;
        return true;
    }

    /// <summary>
    /// 是否在扇形区域内
    /// </summary>
    /// <param name="trans1"></param>
    /// <param name="trans2"></param>
    /// <param name="angle"></param>
    /// <param name="range"></param>
    /// <param name="vol"></param>
    /// <returns></returns>
    private static bool IsInSector( Transform trans1, Transform trans2, float angle, float range, float vol ) {
        return IsInSector( trans1.position, trans2, angle, range, vol );
    }

    private static bool IsInSkillRange( Transform trans1, Transform trans2, proto.SkillReference reference, int vol ) {
        int shapType = reference.shape_type;
        int length = reference.radiate_len;
        int width = reference.radiate_wid;
        int angle = reference.cast_angle;
        int range = reference.cast_range;
        if( shapType == Def.ShapeType.Rectangle ) {
            return IsInRectangle( trans1, trans2, width, length, vol );
        }
        else {
            return IsInSector( trans1, trans2, angle, range, vol );
        }
    }

    private static bool IsInSkillRange( Transform trans1, TeamDisplay ship2, proto.SkillReference reference ) {        
        int vol = ship2.GetShip().Reference.vol;
        Transform trans2 = ship2.GetTeamGo().transform;
        return IsInSkillRange( trans1, trans2, reference, vol );
    }

    private static bool IsInSkillRange( Transform trans1, TeamDisplay ship2, ClientSkill skill ) {
        return IsInSkillRange( trans1, ship2, skill.Prop );
    }

    /// <summary>
    /// ship2是否在ship1的技能范围内
    /// </summary>
    /// <param name="ship1"></param>
    /// <param name="ship2"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    private static bool IsInSkillRange( TeamDisplay ship1, TeamDisplay ship2, ClientSkill skill ) {
        Transform trans1 = ship1.GetTeamGo().transform;
        return IsInSkillRange( trans1, ship2, skill );
    }

    /// <summary>
    /// ship是否在pos为圆心的范围内
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="ship"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    private static bool IsInSkillRange( Vector3 pos, TeamDisplay ship, ClientSkill skill ) {
        proto.SkillReference reference = skill.Prop;
        int shapType = reference.shape_type;
        if( shapType != Def.ShapeType.Circle )
            return false;
        int angle = 360;
        int range = reference.aoe_range;
        int vol = ship.GetShip().Reference.vol;
        return IsInSector( pos, ship.GetTeamGo().transform, angle, range, vol );
    }

    /// <summary>
    /// 获得距离
    /// </summary>
    /// <param name="trans1"></param>
    /// <param name="trans2"></param>
    /// <returns></returns>
    private static float GetDistance( Transform trans1, Transform trans2 ) {
        return Vector3.Distance( trans1.position, trans2.position );
        //float dx = trans2.position.x - trans1.position.x;
        //float dz = trans2.position.z - trans1.position.z;
        //return (dx * dx) + (dz * dz);
    }

    /// <summary>
    /// 得到所有舰船的数据结构
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetAllShipData( Dictionary<int, TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        foreach( var iter in list ) {
            if( iter.Value == null ) continue;
            if( iter.Value.IsDead ) continue;
            result.Add( iter.Value );
        }
        return result;
    }

    /// <summary>
    /// 得到所有舰船
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetAllShip( Dictionary<int, TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        foreach( var iter in list ) {
            if( iter.Value == null ) continue;
            if( iter.Value.IsDead ) continue;
            TeamDisplay tempTrans = iter.Value;
            result.Add( tempTrans );
        }
        return result;
    }

    /// <summary>
    /// 在list里找到和shipTrans最近的船
    /// </summary>
    /// <param name="shipTrans"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetClosestShip( Transform shipTrans, List<TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        if( list == null )
            return result;
        float resultDistance = float.MaxValue;
        float distance = float.MaxValue;
        TeamDisplay trans = null;
        for( int i=0; i < list.Count; i++ ) {
            Transform tempTrans = list[i].GetTeamGo().transform;
            // 获取距离最近的
            distance = GetDistance( shipTrans, tempTrans );
            if( resultDistance > distance ) {
                resultDistance = distance;
                trans = list[i];
            }
        }
        if( trans != null )
            result.Add( trans );
        return result;
    }

    /// <summary>
    /// 在list里找到和shipTrans最远的船
    /// </summary>
    /// <param name="shipTrans"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetFarthestShip( Transform shipTrans, List<TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        if( list == null )
            return result;
        float resultDistance = 0;
        float distance = 0;
        TeamDisplay trans = null;
        for( int i=0; i < list.Count; i++ ) {
            Transform tempTrans = list[i].GetTeamGo().transform;
            // 获取距离最远的
            distance = GetDistance( shipTrans, tempTrans );
            if( resultDistance < distance ) {
                resultDistance = distance;
                trans = list[i];
            }
        }
        if( trans != null )
            result.Add( trans );
        return result;
    }

    /// <summary>
    /// 攻击范围内最近
    /// </summary>
    /// <param name="shipTrans"></param>
    /// <param name="skill"></param>
    /// <param name="bPlayer"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetInAttackRangeClosestShip( Transform shipTrans, ClientSkill skill, Dictionary<int, TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        List<TeamDisplay> tempList = GetInAttackRangeAllShip( shipTrans, skill, list );
        result.AddRange( GetClosestShip( shipTrans, tempList ) );
        return result;
    }

    /// <summary>
    /// 得到攻击范围内所有
    /// </summary>
    /// <param name="shipTrans"></param>
    /// <param name="skill"></param>
    /// <param name="bPlayer"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetInAttackRangeAllShip( Transform shipTrans, ClientSkill skill, Dictionary<int, TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        foreach( var iter in list ) {
            if( iter.Value == null ) continue;
            if( iter.Value.IsDead ) continue;
            // 需要在范围内
            if( !IsInSkillRange( shipTrans, iter.Value, skill ) ) continue;
            TeamDisplay tempTrans = iter.Value;
            result.Add( tempTrans );
        }
        return result;
    }

    /// <summary>
    /// 得到攻击范围内所有
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="skill"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetInAttackRangeAllShip( Vector3 pos, ClientSkill skill, Dictionary<int, TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        foreach( var iter in list ) {
            if( iter.Value == null ) continue;
            if( iter.Value.IsDead ) continue;
            // 需要在范围内
            if( !IsInSkillRange( pos, iter.Value, skill ) ) continue;
            TeamDisplay tempTrans = iter.Value;
            result.Add( tempTrans );
        }
        return result;
    }

    /// <summary>
    /// 耐久值最低的船
    /// </summary>
    /// <param name="shipTrans"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetDurabilityLowestShip( List<TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        float tempValue = float.MaxValue;
        TeamDisplay trans = null;
        for( int i=0; i < list.Count; i++ ) {
            // 获取耐久最低的      
            ClientShip ship = list[i].GetShip();
            if( ship == null ) continue;
            if( tempValue > ship.Durability ) {
                tempValue = ship.Durability;
                trans = list[i];
            }
        }
        if( trans != null )
            result.Add( trans );
        return result;
    }

    /// <summary>
    /// 装甲值最低的船
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetArmorLowestShip( List<TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        float tempValue = float.MaxValue;
        TeamDisplay trans = null;
        for( int i=0; i < list.Count; i++ ) {
            // 获取装甲最低的      
            ClientShip ship = list[i].GetShip();
            if( ship == null ) continue;
            if( tempValue > ship.Armor ) {
                tempValue = ship.Armor;
                trans = list[i];
            }
        }
        if( trans != null )
            result.Add( trans );
        return result;
    }

    /// <summary>
    /// 能量最低的船
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetEnergyLowestShip( List<TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        float tempValue = float.MaxValue;
        TeamDisplay trans = null;
        for( int i=0; i < list.Count; i++ ) {
            // 获取能量最低的      
            ClientShip ship = list[i].GetShip();
            if( ship == null ) continue;
            if( tempValue > ship.Energy ) {
                tempValue = ship.Energy;
                trans = list[i];
            }
        }
        if( trans != null )
            result.Add( trans );
        return result;
    }

    /// <summary>
    /// 所有建筑
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetAllBuilding( List<TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        for( int i=0; i < list.Count; i++ ) {
            // 获取建筑      
            if( list[i].GetShipStrait() == Def.ShipTrait.Build )
                result.Add( list[i] );
        }
        return result;
    }

    /// <summary>
    /// 最近的建筑
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static List<TeamDisplay> GetClosestBuilding( Transform shipTrans, List<TeamDisplay> list ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        List<TeamDisplay> all = GetAllBuilding( list );
        float resultDistance = float.MaxValue;
        float distance = float.MaxValue;
        TeamDisplay trans = null;
        for( int i=0; i < list.Count; i++ ) {
            Transform tempTrans = all[i].GetTeamGo().transform;
            // 获取距离最远的
            distance = GetDistance( shipTrans, tempTrans );
            if( resultDistance > distance ) {
                resultDistance = distance;
                trans = all[i];
            }
        }
        if( trans != null )
            result.Add( trans );
        return result;
    }

    /// <summary>
    /// 得到技能目标表
    /// </summary>
    /// <param name="shipTrans"></param>
    /// <param name="skill"></param>
    /// <param name="playerList"></param>
    /// <param name="enemyList"></param>
    /// <returns></returns>
    private static List<TeamDisplay> FindSkillTarget( Transform shipTrans, ClientSkill skill, Dictionary<int, TeamDisplay> playerList, Dictionary<int, TeamDisplay> enemyList ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        int castTargetType = skill.Prop.cast_target_type;
        switch( castTargetType ) {
            case Def.CastTargetType.Default:                        // 攻击范围内最近敌方
                result.AddRange( GetInAttackRangeClosestShip( shipTrans, skill, enemyList ) );
                break;
            case Def.CastTargetType.InAttackRangeAllEnemy:          // 攻击范围内的所有敌方
                result.AddRange( GetInAttackRangeAllShip( shipTrans, skill, enemyList ) );
                break;
            case Def.CastTargetType.InAttackRangeClosestFriend:     // 攻击范围内的最近友方
                result.AddRange( GetInAttackRangeAllShip( shipTrans, skill, playerList ) );
                break;
            case Def.CastTargetType.InAttackRangeAllFriend:         // 攻击范围内的所有友方
                result.AddRange( GetInAttackRangeAllShip( shipTrans, skill, playerList ) );
                break;
            case Def.CastTargetType.Self:                           // 自身特殊处理,避免重复入表
                break;
            case Def.CastTargetType.BothSides:                      // 敌友双方
                if( skill.Prop.skill_select_type == Def.SkillSelectType.PlayerSelect ) {
                    // 如果是需要玩家选择的,那么则把所有舰船入表,作预先显示
                    result.AddRange( GetAllShip( playerList ) );
                    result.AddRange( GetAllShip( enemyList ) );
                }
                else if( skill.Prop.skill_select_type == Def.SkillSelectType.NoSelection ) {
                    // 不可选择的,那么把所有在攻击范围内的舰船入表
                    result.AddRange( GetInAttackRangeAllShip( shipTrans, skill, playerList ) );
                    result.AddRange( GetInAttackRangeAllShip( shipTrans, skill, enemyList ) );
                }
                break;
            case Def.CastTargetType.Friend:                         // 友方
                if( skill.Prop.skill_select_type == Def.SkillSelectType.PlayerSelect ) {
                    // 如果是需要玩家选择的,那么则把所有舰船入表,作预先显示
                    result.AddRange( GetAllShip( playerList ) );
                }
                else if( skill.Prop.skill_select_type == Def.SkillSelectType.NoSelection ) {
                    // 不可选择的,那么把所有在攻击范围内的舰船入表
                    result.AddRange( GetInAttackRangeAllShip( shipTrans, skill, playerList ) );
                }
                break;
            case Def.CastTargetType.Enemy:                          // 敌方
                if( skill.Prop.skill_select_type == Def.SkillSelectType.PlayerSelect ) {
                    // 如果是需要玩家选择的,那么则把所有舰船入表,作预先显示
                    result.AddRange( GetAllShip( enemyList ) );
                }
                else if( skill.Prop.skill_select_type == Def.SkillSelectType.NoSelection ) {
                    // 不可选择的,那么把所有在攻击范围内的舰船入表
                    result.AddRange( GetInAttackRangeAllShip( shipTrans, skill, enemyList ) );
                }
                break;
            case Def.CastTargetType.ClosestFriend:                  // 最近友方
                result.AddRange( GetClosestShip( shipTrans, GetAllShip( playerList ) ) );
                break;
            case Def.CastTargetType.FarthestFriend:                 // 最远友方
                result.AddRange( GetFarthestShip( shipTrans, GetAllShip( playerList ) ) );
                break;
            case Def.CastTargetType.DurabilityLowestFriend:         // 耐久最低友方
                result.AddRange( GetDurabilityLowestShip( GetAllShipData( playerList ) ) );
                break;
            case Def.CastTargetType.ArmorLowestFriend:		        // 装甲值最低友方
                result.AddRange( GetArmorLowestShip( GetAllShipData( playerList ) ) );
                break;
            case Def.CastTargetType.EnergyLowestFriend:		        // 能量值最低友方
                result.AddRange( GetEnergyLowestShip( GetAllShipData( playerList ) ) );
                break;
            case Def.CastTargetType.AllFriendBuilding:		        // 所有友方建筑
                result.AddRange( GetAllBuilding( GetAllShipData( playerList ) ) );
                break;
            case Def.CastTargetType.ClosestFriendBuilding:		    // 最近友方建筑
                result.AddRange( GetClosestBuilding( shipTrans, GetAllShipData( playerList ) ) );
                break;
            case Def.CastTargetType.AllFriend:		                // 全体友方
                result.AddRange( GetAllShip( playerList ) );
                break;
            case Def.CastTargetType.ClosestEnemy:		            // 最近敌方
                result.AddRange( GetClosestShip( shipTrans, GetAllShip( enemyList ) ) );
                break;
            case Def.CastTargetType.FarthestEnemy:		            // 最远敌方
                result.AddRange( GetFarthestShip( shipTrans, GetAllShip( enemyList ) ) );
                break;
            case Def.CastTargetType.DurabilityLowestEnemy:		    // 耐久值最低敌方
                result.AddRange( GetDurabilityLowestShip( GetAllShipData( enemyList ) ) );
                break;
            case Def.CastTargetType.ArmorLowestEnemy:		        // 装甲值最低敌方
                result.AddRange( GetArmorLowestShip( GetAllShipData( enemyList ) ) );
                break;
            case Def.CastTargetType.EnergyLowestEnemy:		        // 能量值最低敌方
                result.AddRange( GetEnergyLowestShip( GetAllShipData( enemyList ) ) );
                break;
            case Def.CastTargetType.AllEnemyBuilding:		        // 所有敌方建筑
                result.AddRange( GetAllBuilding( GetAllShipData( enemyList ) ) );
                break;
            case Def.CastTargetType.ClosestEnemyBuilding:		    // 最近敌方建筑
                result.AddRange( GetClosestBuilding( shipTrans, GetAllShipData( enemyList ) ) );
                break;
            case Def.CastTargetType.AllEnemy:		                // 全体敌方
                result.AddRange( GetAllShip( enemyList ) );
                break;
            case Def.CastTargetType.All:		                    // 全体
                result.AddRange( GetAllShip( playerList ) );
                result.AddRange( GetAllShip( enemyList ) );
                break;
        }
        return result;
    }

    /// <summary>
    /// 根据技能配置,距离,角度得到最终的目标表
    /// </summary>
    /// <param name="shipTrans"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    public static List<TeamDisplay> FindSkillTarget( TeamDisplay ship, ClientSkill skill ) {
        // 目标表
        List<TeamDisplay> result = new List<TeamDisplay>();
        if( skill.Prop.cast_target_type != Def.CastTargetType.Self ) {
            Dictionary<int, TeamDisplay> playerList = BattleSceneDisplayManager.Instance.GetTeamDisplayList( true );
            Dictionary<int, TeamDisplay> enemyList = BattleSceneDisplayManager.Instance.GetTeamDisplayList( false );
            result.AddRange( FindSkillTarget( ship.GetTeamGo().transform, skill, playerList, enemyList ) );
        }
        else
            result.Add( ship );
        return result;
    }

    /// <summary>
    /// 玩家点击场景后,根据点击的位置和技能配置得到最终目标表
    /// </summary>
    /// <param name="position"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    public static List<TeamDisplay> FindSkillTarget( Vector3 position, ClientSkill skill ) {
        List<TeamDisplay> result = new List<TeamDisplay>();
        if( skill.Prop.shape_type != Def.ShapeType.Circle )
            return result;

        Dictionary<int, TeamDisplay> playerList = BattleSceneDisplayManager.Instance.GetTeamDisplayList( true );
        Dictionary<int, TeamDisplay> enemyList = BattleSceneDisplayManager.Instance.GetTeamDisplayList( false );
        int castTargetType = skill.Prop.cast_target_type;
        switch( castTargetType ) {
                // 范围攻击,不应该只包含自己
            case Def.CastTargetType.Self:
                break;
            case Def.CastTargetType.BothSides:
                result.AddRange( GetInAttackRangeAllShip( position, skill, playerList ) );
                result.AddRange( GetInAttackRangeAllShip( position, skill, enemyList ) );
                break;
            case Def.CastTargetType.Friend:
                result.AddRange( GetInAttackRangeAllShip( position, skill, playerList ) );
                break;
            case Def.CastTargetType.Enemy:
                result.AddRange( GetInAttackRangeAllShip( position, skill, enemyList ) );
                break;
        }
        return result;
    }
}
