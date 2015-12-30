using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 目标选择器
/// </summary>
public class TargetSelector {

    /// <summary>
    /// 根据部件来搜索敌人
    /// </summary>
    /// <param name="Attacker"></param>
    /// <returns></returns>
    public static List<EnemyInfo> FindTarget( ClientParts parts, int campType ) {
        List<EnemyInfo> targeSelecttList = new List<EnemyInfo>();
        List<Entity> targetUnitList = EntityManager.Instance.DefenderUnitList;
        if( campType  == FightServiceDef.CampType.Camp_Defender ) {
            targetUnitList = EntityManager.Instance.AttackUnitList;
        }

        for( int i = 0; i < targetUnitList.Count; ++i ) {
            ShipEntity targetShip = targetUnitList[i] as ShipEntity;
            if( !targetShip.IsActive() )
                continue;
            float distanceSq = GetDistanceSq( parts.Owner, targetShip.Ship );

            // 先判断是否已经在最小攻击范围内
            if (IsInBlindZone(parts, distanceSq))
                continue;


            int moveAttackRange = (parts.MoveAttackRange + targetShip.Ship.Reference.vol) * (parts.MoveAttackRange + targetShip.Ship.Reference.vol);
            if( IsInCricularSector2( parts, targetShip, moveAttackRange, parts.AttackAngle ) ) {
                EnemyInfo enemy;
                enemy.DistanceSQ = distanceSq;
                enemy.EnemyShip = targetShip;
                enemy.InAttackRange = false;
                // 2015/7/8 InAttackRange的检查放到FilterTarget里面去，会省一些计算
                //enemy.InAttackRange = CheckInAttackRange(parts, targetShip.Ship.Reference.vol, distanceSq);
                targeSelecttList.Add( enemy );
            }
        }

        return FilterTarget( parts, targeSelecttList );
    }

    public static bool IsLateralTarget(ShipEntity srcEntity, ShipEntity targetEntity) {
        if( srcEntity.Ship.Reference.sw_priority_unitstrait == Def.ShipTrait.None
            && srcEntity.Ship.Reference.sw_priority_unitstrait_extend == Def.ShipExtendTrait.None )
            return false;

        if( srcEntity.Ship.Reference.sw_priority_unitstrait == targetEntity.Ship.Reference.unitstrait )
            return true;

        if( srcEntity.Ship.Reference.sw_priority_unitstrait == Def.ShipTrait.None 
            && srcEntity.Ship.Reference.sw_priority_unitstrait_extend == targetEntity.Ship.Reference.unitsextendtrait )
                return true;

        return false;
    }

    /// <summary>
    /// 获取侧移目标
    /// </summary>
    /// <returns></returns>
    public static ShipEntity GetLateralTarget(ShipEntity srcEntity) {

        int targetIndex = -1;
        float distance = float.MaxValue;
        for( int i = 0; i < EntityManager.Instance.AttackUnitList.Count; ++i ) {
            ShipEntity targetEntity = EntityManager.Instance.AttackUnitList[i] as ShipEntity;

            if( !IsLateralTarget( srcEntity, targetEntity ) )
                continue;


            if( srcEntity.Ship.Reference.sw_targetdecision_range < UnityEngine.Mathf.Abs( srcEntity.Ship.Position.z - targetEntity.Ship.Position.z ) )
                continue;

            float distSq = GetDistanceSq( srcEntity.Ship, targetEntity.Ship );

            if( distance > distSq ) {
                targetIndex = i;
                distance = distSq;
            }

        }

        if (targetIndex == -1)
            return null;

        return EntityManager.Instance.AttackUnitList[targetIndex] as ShipEntity;
    }

    /// <summary>
    /// 盲区判定
    /// </summary>
    /// <param name="parts"></param>
    /// <param name="DistSq"></param>
    /// <returns></returns>
    private static bool IsInBlindZone(ClientParts parts,float DistSq) {
        // 最小攻击距离为0，说明木有盲区
        if( parts.AttackRagneMin == 0 )
            return false;

        int blindZone = parts.AttackRagneMin * parts.AttackRagneMin;
        if( DistSq < blindZone )
            return true;

        return false;
    }

    /// <summary>
    /// 距离平方
    /// 快速判断距离办法
    /// </summary>
    /// <param name="Attacker"></param>
    /// <param name="Target"></param>
    /// <returns></returns>
    private static float GetDistanceSq( ClientShip Attacker, ClientShip Target ) {
        return ((Target.Position.x - Attacker.Position.x) * (Target.Position.x - Attacker.Position.x)) + ((Target.Position.z - Attacker.Position.z) * (Target.Position.z - Attacker.Position.z));
    }

    /// <summary>
    /// 距离平方
    /// 快速判断距离
    /// </summary>
    /// <returns></returns>
    private static float GetDistanceSq(ShipEntity shipEntity, float x, float y, float z) {
        return (shipEntity.Ship.Position.x - x) * (shipEntity.Ship.Position.x - x) + (shipEntity.Ship.Position.z - z) * (shipEntity.Ship.Position.z - z);
    }

    /// <summary>
    /// 判断是否进入进入了警戒范围
    /// </summary>
    /// <param name="parts"></param>
    /// <param name="Distance"></param>
    /// <returns></returns>
    private static bool CheckInGuardRange( ClientParts parts, float distanceSq ) {
        // TODO: 暂时不检查角度 
        float guardRangeSq = parts.MoveAttackRange * parts.MoveAttackRange;
        if( distanceSq <= guardRangeSq )
            return true;

        return false;
    }

    /// <summary>
    /// 判断是否在攻击范围内
    /// </summary>
    /// <param name="parts"></param>
    /// <param name="targetShip"></param>
    /// <returns></returns>
    private static bool CheckInAttackRange(ClientParts parts, float distanceSq) {
        // TODO：暂时不检查角度
        float AttackRangeSq = parts.AttackRangeMax * parts.AttackRangeMax;
        if( distanceSq <= AttackRangeSq )
           return true;

        return false;
    }

    private static bool CheckInAttackRange(ClientParts parts, int targetRaduis, float distanceSq) {
        float attackRangeSQ = (parts.AttackRangeMax + targetRaduis)*(parts.AttackRangeMax+ targetRaduis);
        return distanceSq <= attackRangeSQ;
    }


    /// <summary>
    /// 过滤目标
    /// </summary>
    /// <param name="targetList"></param>
    /// <returns></returns>
    private static List<EnemyInfo> FilterTarget(ClientParts part, List<EnemyInfo> targetList ) {

        if( targetList.Count == 0 )
            return targetList;

        float distanceSq = float.MaxValue;
        float minDistanceSq = float.MaxValue;
        int targetIndex = 0;
        int priorityIndex = -1;

        // TODO
        // 此处需要优化和完善
        List<EnemyInfo> enemyList = new List<EnemyInfo>();
        for( int i = 0; i < targetList.Count; ++i ) {
            // 这里出现了嵌套的if判断
            // 主要从优化出发，考虑一趟遍历就做完所有筛选判断
            EnemyInfo targetInfo = targetList[i];

            // 记录下偏小的一个距离
            if( minDistanceSq > targetInfo.DistanceSQ )
                minDistanceSq = targetInfo.DistanceSQ;

            // 判断是否是高优先级目标
            bool prioprityTarget = false;
            if( IsPriorityTarget( part, targetInfo.EnemyShip ) ) {
                if( priorityIndex == -1 )
                    prioprityTarget = true;
                else if( targetInfo.DistanceSQ < distanceSq )
                    prioprityTarget = true;
            }

            if( prioprityTarget ) {
                distanceSq = targetInfo.DistanceSQ;
                targetIndex = i;
                priorityIndex = i;
                continue;
            }


            // 如果前面的优先级判定不成立，则遍历出距离最近的目标
            if( targetList[i].DistanceSQ < distanceSq ) {
                distanceSq = targetList[i].DistanceSQ;
                targetIndex = i;
            }
        }

        if( priorityIndex != -1 )
            targetIndex = priorityIndex;

        // 对攻击距离进行修正
        EnemyInfo targetEnemy = targetList[targetIndex];
        targetEnemy.DistanceSQ = minDistanceSq;
        targetEnemy.InAttackRange = CheckInAttackRange( part, targetEnemy.EnemyShip.Ship.Reference.vol, minDistanceSq );
        enemyList.Add( targetEnemy );
        return enemyList;
    }

    private static bool IsPriorityTarget(ClientParts castPart, ShipEntity targetEntity){

        if (castPart.Reference.priority_unitstrait == Def.ShipTrait.None
            && castPart.Reference.priority_unitstrait_extend == Def.ShipExtendTrait.None ) {
                return false;
        }

        if( castPart.Reference.priority_unitstrait == targetEntity.Ship.Reference.unitstrait) {
            return true;
        }

        if( castPart.Reference.priority_unitstrait == Def.ShipTrait.None
            &&castPart.Reference.priority_unitstrait_extend == targetEntity.Ship.Reference.unitsextendtrait ) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 扇形区域检测算法标准版，先实现这个
    /// </summary>
    /// <param name="parts">攻击部件</param>
    /// <param name="targetShip">敌方单位</param>
    /// <param name="squareRange">检测范围平方</param>
    /// <param name="theta">扇形角度</param>
    /// <returns></returns>
    private static bool IsInCritricalSector( ClientParts parts, ClientShip targetShip, float squareRange, float theta ) {

//         float distanceSq = GetDistanceSq( parts.Owner, targetShip );
//         if( distanceSq > squareRange )
//             return false;
        // D = P - C
        float dx = targetShip.Position.x - parts.Owner.Position.x;
        float dz = targetShip.Position.z - parts.Owner.Position.z;

        // |D|^2 = (dx^2 + dz^2)
        float disntaceSq = dx * dx + dz * dz;

        // |D|^2 > r^2
        if( disntaceSq > squareRange )
            return false;

        float distance = Mathf.Sqrt( disntaceSq );


        // Normalize D
        dx /= distance;
        dz /= distance;

        return true;
    }

    /// <summary>
    /// 判断是否在矩形类的方法1 
    /// </summary>
    /// <param name="caster"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private static bool IsInRectangle1(ShipEntity caster, ShipEntity target, int length, int width) {
        length += target.Ship.Reference.vol;
        width += target.Ship.Reference.vol;
        if( Mathf.Abs( target.Ship.Position.z - caster.Ship.Position.z) <= length && Mathf.Abs( target.Ship.Position.x - caster.Ship.Position.x ) <= width ) 
            return true;

        return false;
    }

    /// <summary>
    /// 是否在圆形区域
    /// </summary>
    /// <param name="target"></param>
    /// <param name="radius"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static bool IsInCircle(ShipEntity target, float radius,  float x, float y, float z) {
        float distanceSq = GetDistanceSq(target, x, y, z);

        if( distanceSq < radius * radius )
            return true;

        return false;
    }

    /// <summary>
    /// 扇形区域算法优化版
    /// 1、优化半径检测开平方
    /// 2、优化夹角选择开平方
    /// </summary>
    /// <param name="?"></param>
    /// <returns></returns>
    private static bool IsInCricularSector1( ClientParts parts, ClientShip targetShip ) {
        return true;
    }

    /// <summary>
    /// 扇形区域检测，U3D版本
    /// </summary>
    /// <param name="parts"></param>
    /// <param name="targetShip"></param>
    /// <returns></returns>
    private static bool IsInCricularSector2( ClientParts parts, ShipEntity targetUnit, float squareRange, float theta ) {
        ClientShip targetShip = targetUnit.Ship;
        float dx = targetShip.Position.x - parts.Owner.Position.x;
        float dz = targetShip.Position.z - parts.Owner.Position.z;

        // |D|^2 = (dx^2 + dz^2)
        float disntaceSq = dx * dx + dz * dz;

        // |D|^2 > r^2
        if( disntaceSq > squareRange )
            return false;

        Vector3 targetDir = targetShip.Position - parts.Owner.Position;
        

        Vector3 forward = new Vector3( 0, 0, 1 );
        if( targetUnit.CampType == FightServiceDef.CampType.Camp_Attacker )
            forward.z = -1;

        float angle = Vector3.Angle( targetDir.normalized, forward );
        
        // 角度小于扇形的角度，说明目标的原型在扇形区域内
        if( angle * 2 <= theta )
            return true;

        // 近防距离检查
        if( parts.Reference.closein_range > 0 ) {
            int closein_range = (parts.Reference.closein_range + targetShip.Reference.vol) * ((parts.Reference.closein_range + targetShip.Reference.vol));
            if( closein_range >= disntaceSq ) {
                return true;
            }
        }

        return false;
    }


    public static List<ShipEntity> FindSkillTarget( ShipEntity caster, ClientSkill skill, int targetID, Vector3 position ) {
        List<ShipEntity> targetList = new List<ShipEntity>();

        if( skill.Prop.cast_target_type == Def.CastTargetType.Self ) {
            // 针对自身
            targetList.Add( caster );
        }
        else if( skill.Prop.skill_select_type == Def.SkillSelectType.CastScope ) {
            FilterSkillTargetByCastTargetType( caster, skill, ref targetList );

            if (skill.Prop.shape_type == Def.ShapeType.Circle && targetList.Count == 1){
                // Shape为Circle的时候，为溅射伤害
                ShipEntity centerTarget = targetList[0];
                FilterAllInCircleSkillTarget(skill, centerTarget.Ship.Position, EntityManager.Instance.DefenderUnitList, ref targetList);
            }
        }
        else if( skill.Prop.skill_select_type == Def.SkillSelectType.PlayerSelect ) {
            if( skill.Prop.shape_type == Def.ShapeType.Default ) {
                // 选定一个目标来攻击
                ShipEntity targetEntity = EntityManager.Instance.GetEntityByID( targetID ) as ShipEntity;
                if( targetEntity != null ) {
                    targetList.Add( targetEntity );
                }
            }
            else if( skill.Prop.shape_type == Def.ShapeType.Circle ) {
                int castTargetType = skill.Prop.cast_target_type;
                switch( castTargetType ) {
                    case Def.CastTargetType.BothSides:
                        FilterAllInCircleSkillTarget( skill, position, EntityManager.Instance.AttackUnitList, ref targetList );
                        FilterAllInCircleSkillTarget( skill, position, EntityManager.Instance.DefenderUnitList, ref targetList );
                        break;
                    case Def.CastTargetType.Friend:
                        FilterAllInCircleSkillTarget( skill, position, EntityManager.Instance.AttackUnitList, ref targetList );
                        break;
                    case Def.CastTargetType.Enemy:
                        FilterAllInCircleSkillTarget( skill, position, EntityManager.Instance.DefenderUnitList, ref targetList );
                        break;
                }
            }
        }
        else if( skill.Prop.skill_select_type == Def.SkillSelectType.NoSelection ) {
            FilterSkillTargetByCastTargetType( caster, skill, ref targetList );
        }

        return targetList;
    }


    private static void FilterAllInCircleSkillTarget( ClientSkill skill, Vector3 position, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetShip = entityList[i] as ShipEntity;
            if( IsInCircle( targetShip, skill.Prop.aoe_range + targetShip.Ship.Reference.vol, position.x, position.y, position.z ) ) {
                targetList.Add( targetShip );
            }
        }
    }

    // 根据Shape过滤目标，提供给6~8种CastTargetType使用
    private static void FilterSkillTargetByShape(ShipEntity caster,  ClientSkill skill, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetShip = entityList[i] as ShipEntity;
            if( skill.Prop.shape_type == Def.ShapeType.Circle ) {
                if( !IsInCircle( targetShip, skill.Prop.aoe_range + targetShip.Ship.Reference.vol, caster.Ship.Position.x, caster.Ship.Position.y, caster.Ship.Position.z ) ) 
                    continue;
            }
            else if (skill.Prop.shape_type == Def.ShapeType.Rectangle){
                if (!IsInRectangle1(caster, targetShip, skill.Prop.radiate_len, skill.Prop.radiate_wid))
                    continue;
            }

            targetList.Add( targetShip );
        }
    }

    /// <summary>
    /// 根据技能的释放目标来过滤目标目标列表
    /// </summary>
    /// <param name="caster"></param>
    /// <param name="skill"></param>
    /// <param name="targetList"></param>
    private static void FilterSkillTargetByCastTargetType(ShipEntity caster, ClientSkill skill,  ref List<ShipEntity> targetList) {
        switch( skill.Prop.cast_target_type ) {
            case Def.CastTargetType.Default: // 默认为最近的地方
                GetClosestDefaultEnemy( caster, skill,ref targetList );
                break;
            case Def.CastTargetType.InAttackRangeAllEnemy: //攻击范围内所有敌方
                GetInAttackRangeAllEnemy(caster, skill, ref targetList);
                break;
            case Def.CastTargetType.InAttackRangeClosestFriend: // 攻击范围内最近友方
                GetInAttackRangeClosestFriend( caster, skill, ref targetList );
                break;
            case Def.CastTargetType.InAttackRangeAllFriend: // 攻击范围内的所有友方
                GetInAttackRangeAllFriend( caster, skill, ref targetList );
                break;
            case Def.CastTargetType.Self:
                targetList.Add(caster);
                break;
            case Def.CastTargetType.BothSides:
                GetBothSidesTarget( caster, skill, ref targetList );
                break;
            case Def.CastTargetType.Friend:
                GetFriend( caster, skill, ref targetList );
                break;
            case Def.CastTargetType.Enemy:
                GetEnemy( caster, skill, ref targetList );
                break;
            case Def.CastTargetType.ClosestFriend:
                GetCloseTarget(caster, EntityManager.Instance.AttackUnitList, ref targetList);
                break;
            case Def.CastTargetType.FarthestFriend:
                GetFarthestTarget(caster, EntityManager.Instance.AttackUnitList, ref targetList);
                break;
            case Def.CastTargetType.DurabilityLowestFriend:
                GetDurabilityLowestTarget( caster, EntityManager.Instance.AttackUnitList, ref targetList );
                break;
            case Def.CastTargetType.ArmorLowestFriend:
                GetArmorLowestTarget( caster, EntityManager.Instance.AttackUnitList, ref targetList );
                break;
            case Def.CastTargetType.EnergyLowestFriend:
                GetEnergyLowestTarget( caster, EntityManager.Instance.AttackUnitList, ref targetList );
                break;
            case Def.CastTargetType.AllFriendBuilding:
                GetAllTargetBuilding(caster, EntityManager.Instance.AttackUnitList, ref targetList);
                break;
            case Def.CastTargetType.ClosestFriendBuilding:
                GetClosestTargetBuilding( caster, EntityManager.Instance.AttackUnitList, ref targetList );
                break;
            case Def.CastTargetType.AllFriend:
                GetAllTarget(caster, EntityManager.Instance.AttackUnitList, ref targetList);
                break;
            case Def.CastTargetType.ClosestEnemy:
                GetCloseTarget( caster, EntityManager.Instance.DefenderUnitList, ref targetList );
                break;
            case Def.CastTargetType.FarthestEnemy:
                GetFarthestTarget( caster, EntityManager.Instance.DefenderUnitList, ref targetList );
                break;
            case Def.CastTargetType.DurabilityLowestEnemy:
                GetDurabilityLowestTarget( caster, EntityManager.Instance.DefenderUnitList, ref targetList );
                break;
            case Def.CastTargetType.ArmorLowestEnemy:
                GetArmorLowestTarget( caster, EntityManager.Instance.DefenderUnitList, ref targetList );
                break;
            case Def.CastTargetType.EnergyLowestEnemy:
                GetEnergyLowestTarget( caster, EntityManager.Instance.DefenderUnitList, ref targetList );
                break;
            case Def.CastTargetType.AllEnemyBuilding:
                GetAllTargetBuilding( caster, EntityManager.Instance.DefenderUnitList, ref targetList );
                break;
            case Def.CastTargetType.ClosestEnemyBuilding:
                GetClosestTargetBuilding( caster, EntityManager.Instance.DefenderUnitList, ref targetList );
                break;
            case Def.CastTargetType.AllEnemy:
                GetAllTarget( caster, EntityManager.Instance.AttackUnitList, ref targetList );
                GetAllTarget( caster, EntityManager.Instance.DefenderUnitList, ref targetList );
                break;
        }
    }

    // 距离最近的敌人
    private static void GetClosestDefaultEnemy(ShipEntity caster, ClientSkill skill, ref List<ShipEntity> targetList) {
        
        float distSeq = float.MaxValue;
        int targetIndex = -1;
        for( int i = 0; i < EntityManager.Instance.DefenderUnitList.Count; ++i ) {
            ShipEntity targetShip = EntityManager.Instance.DefenderUnitList[i] as ShipEntity;
            int seqDistance = (skill.Prop.cast_range + targetShip.Ship.Reference.vol) * (skill.Prop.cast_range + targetShip.Ship.Reference.vol);
            if(!IsInCricularSector2( skill.ControlPart, targetShip, seqDistance, skill.Prop.cast_angle ) )
                continue;
            float targetDistSeq = GetDistanceSq( caster.Ship, targetShip.Ship );
            if( targetDistSeq < distSeq ) {
                distSeq = targetDistSeq;
                targetIndex = i;
            }
        }

        if (targetIndex != -1)
            targetList.Add( EntityManager.Instance.DefenderUnitList[targetIndex] as ShipEntity );
    }

    // 攻击范围内所有敌方
    // 针对InAttackRangeAllEnemy类型的特殊实现
    private static void GetInAttackRangeAllEnemy( ShipEntity caster, ClientSkill skill, ref List<ShipEntity> targetList ) {
        int seqDistance = skill.Prop.cast_range * skill.Prop.cast_range;
        for( int i = 0; i < EntityManager.Instance.DefenderUnitList.Count; ++i ) {
            ShipEntity targetShip = EntityManager.Instance.DefenderUnitList[i] as ShipEntity;
            if( IsInCricularSector2( skill.ControlPart, targetShip, seqDistance , skill.Prop.cast_angle) ) {
                targetList.Add( targetShip );
            }
        }
    }

    // 攻击范围内最近的友方
    // InAttackRangeClosestFriend类型的特殊实现
    private static void GetInAttackRangeClosestFriend(ShipEntity caster, ClientSkill skill, ref List<ShipEntity> targetList) {
        int seqDist = skill.Prop.cast_range * skill.Prop.cast_range;
        int targetIndex = 0;
        float dist = float.MaxValue;
        for( int i = 0; i < EntityManager.Instance.AttackUnitList.Count; ++i ) {
            ShipEntity targetEntity = EntityManager.Instance.AttackUnitList[i] as ShipEntity;

            if( IsInCricularSector2( skill.ControlPart, targetEntity, seqDist, skill.Prop.cast_angle ) ) {
                float distSeq = GetDistanceSq( caster.Ship, targetEntity.Ship );
                if( distSeq < dist ) {
                    dist = distSeq;
                    targetIndex = i;
                }
            }
        }

        targetList.Add( EntityManager.Instance.AttackUnitList[targetIndex] as ShipEntity );
    }

    // 攻击范围的全体友方
    // InAttackRangeAllFriend类型的特殊实现
    private static void GetInAttackRangeAllFriend(ShipEntity caster, ClientSkill skill, ref List<ShipEntity> targetList) {
        int seqDist = skill.Prop.cast_range * skill.Prop.cast_range;
        for( int i = 0; i < EntityManager.Instance.AttackUnitList.Count; ++i ) {
            ShipEntity targetEntity = EntityManager.Instance.AttackUnitList[i] as ShipEntity;
            if( IsInCricularSector2( skill.ControlPart, targetEntity, seqDist, skill.Prop.cast_angle ) ) {
                targetList.Add( targetEntity );
            }
        }
    }

    // 全体
    // BothSides 类型的特殊实现
    private static void GetBothSidesTarget( ShipEntity caster, ClientSkill skill, ref List<ShipEntity> targetList ) {
        FilterSkillTargetByShape(caster, skill, EntityManager.Instance.AttackUnitList, ref targetList);
        FilterSkillTargetByShape( caster, skill, EntityManager.Instance.DefenderUnitList, ref targetList );
    }

    // 友方
    // Friend的具体实现
    private static void GetFriend(ShipEntity caster, ClientSkill skill, ref List<ShipEntity>  targetList) {
        FilterSkillTargetByShape(caster, skill, EntityManager.Instance.AttackUnitList, ref targetList);
    }

    // 敌方
    // Enemy类型的具体实现
    private static void GetEnemy(ShipEntity caster, ClientSkill skill, ref List<ShipEntity> targetList){
        FilterSkillTargetByShape( caster, skill, EntityManager.Instance.DefenderUnitList, ref targetList );
    }

    // 最近友方
    // ClosestFriend类型的具体实现
    private static void GetCloseTarget( ShipEntity caster, List<Entity> entityList,ref List<ShipEntity> targetList ) {
        float minDistSq = float.MaxValue;
        int targetIndex = 0;
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetEntity = entityList[i] as ShipEntity;
            float targetDistSq = GetDistanceSq( caster.Ship, targetEntity.Ship );
            if( targetDistSq < minDistSq ) {
                minDistSq = targetDistSq;
                targetIndex = i;
            }
        }

        targetList.Add( entityList[targetIndex] as ShipEntity );
    }

    // 最远友方
    // FarthestFriend类型的具体实现
    private static void GetFarthestTarget( ShipEntity caster, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        float maxDistsq = 0;
        int targetIndex = 0;
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetEntity = entityList[i] as ShipEntity;
            float targetDistSq = GetDistanceSq( caster.Ship, targetEntity.Ship );
            if( targetDistSq > maxDistsq ) {
                maxDistsq = targetDistSq;
                targetIndex = i;
            }
        }

        targetList.Add( entityList[targetIndex] as ShipEntity );
    }

    // 耐久最低的友方
    // DurabilityLowestFriend类型的具体实现
    private static void GetDurabilityLowestTarget( ShipEntity caster, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        int minDurability = int.MaxValue;
        int targetIndex = 0;
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetEntity = entityList[i] as ShipEntity;
            if( minDurability > targetEntity.GetDurability() ) {
                targetIndex = i;
                minDurability = targetEntity.GetDurability();
            }
        }

        targetList.Add( EntityManager.Instance.AttackUnitList[targetIndex] as ShipEntity );
    }

    // 装甲最低友方
    // ArmorLowestFriendle类型的具体实现
    private static void GetArmorLowestTarget( ShipEntity caster, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        int minArmor = int.MaxValue;
        int targetIndex = 0;
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetEntity = entityList[i] as ShipEntity;
            if( minArmor > targetEntity.GetArmouredShield() ) {
                targetIndex = i;
                minArmor = targetEntity.GetArmouredShield();
            }
        }

        targetList.Add( entityList[targetIndex] as ShipEntity );
    }


    // 能量最低
    // EnergyLowestFriend 具体实现
    private static void GetEnergyLowestTarget( ShipEntity caster, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        int minArmor = int.MaxValue;
        int targetIndex = 0;
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetEntity = entityList[i] as ShipEntity;
            if( minArmor > targetEntity.GetArmouredShield() ) {
                targetIndex = i;
                minArmor = targetEntity.GetArmouredShield();
            }
        }

        targetList.Add( entityList[targetIndex] as ShipEntity );
    }

    // 获取所有的建筑目标
    private static void GetAllTargetBuilding( ShipEntity caster, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetEntity = entityList[i] as ShipEntity;
            if( targetEntity.Ship.GetShipStrait() == Def.ShipTrait.Build ) {
                targetList.Add(targetEntity);
            }
        }
    }

    // 获取最近的目标建筑
    private static void GetClosestTargetBuilding( ShipEntity caster, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        float minDistSq = float.MaxValue;
        int targetIndex = 0;
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetEntity = entityList[i] as ShipEntity;

            if( targetEntity.Ship.GetShipStrait() == Def.ShipTrait.Build   ) {
                float targetDistSq = GetDistanceSq( caster.Ship, targetEntity.Ship );
                if( targetDistSq < minDistSq ) {
                    minDistSq = targetDistSq;
                    targetIndex = i;
                }
            }
        }

        targetList.Add( targetList[targetIndex] );
    }

    // 获取全体目标
    private static void GetAllTarget( ShipEntity caster, List<Entity> entityList, ref List<ShipEntity> targetList ) {
        for( int i = 0; i < entityList.Count; ++i ) {
            ShipEntity targetEntity = entityList[i] as ShipEntity;
            targetList.Add( targetEntity );
        }

    }
}
