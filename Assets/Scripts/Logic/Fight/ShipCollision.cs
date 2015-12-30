using System;
using System.Collections;


/// <summary>
/// 舰艇碰撞检测
/// </summary>
public class ShipCollision {


    /// <summary>
    /// 碰撞值修正值
    /// </summary>
    private static int Collision_Adjust = 1;

    /// <summary>
    /// 检查是否可以推进
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// 
    public static bool CanSeek( ShipEntity srcEntity ) {

        for( int i = 0; i < EntityManager.Instance.AttackUnitList.Count; ++i ) {
            var targetEntity = EntityManager.Instance.AttackUnitList[i] as ShipEntity;

            if( targetEntity == srcEntity )
                continue;

            // 不对在自己后方的单位进行检测
            if( targetEntity.Ship.Position.z <= srcEntity.Ship.Position.z )
                continue;

            if( IsCollision(srcEntity, targetEntity) )
                return false;
        }

        for( int i = 0; i < EntityManager.Instance.DefenderUnitList.Count; ++i ) {
            var targetEntity = EntityManager.Instance.DefenderUnitList[i] as ShipEntity;

            if( targetEntity == srcEntity )
                continue;

            if( targetEntity.Ship.Position.z <= srcEntity.Ship.Position.z )
                continue;

            if( IsCollision( srcEntity, targetEntity ) )
                return false;
        }

        return true;
    }


    /// <summary>
    /// 能否侧移
    /// </summary>
    /// <param name="srcEntity"></param>
    /// <returns></returns>
    public static bool CanLateral(ShipEntity srcEntity, ShipEntity lateralTarget, bool right) {
        if( Math.Abs( srcEntity.Ship.Position.x - lateralTarget.Ship.Position.x ) <= FightServiceDef.SPEED_SCALAR )
            return false;
        for( int i = 0; i < EntityManager.Instance.DefenderUnitList.Count; ++i ) {
            var targetEntity = EntityManager.Instance.DefenderUnitList[i] as ShipEntity;

            if( targetEntity.Ship.Position.z > srcEntity.Ship.Position.z )
                continue;

            if( right && targetEntity.Ship.Position.x < srcEntity.Ship.Position.x )
                continue;
            else if(!right && targetEntity.Ship.Position.x > srcEntity.Ship.Position.x )
                continue;

            if( srcEntity == targetEntity)
                continue;

            if( IsCollision( srcEntity, targetEntity ) )
                return false;
        }

        return true;
    }


    /// <summary>
    /// 获取前方的阻挡对象
    /// </summary>
    /// <param name="srcEntity"></param>
    /// <returns></returns>
    public static ShipEntity GetObstructTarget(ShipEntity srcEntity) {
        float minDist = float.MaxValue;
        ShipEntity obstructTarget = null;
        for( int i = 0; i < EntityManager.Instance.AttackUnitList.Count; ++i ) {
            ShipEntity target = EntityManager.Instance.AttackUnitList[i] as ShipEntity;
            if( srcEntity.Ship.Position.z > target.Ship.Position.z )
                continue;

            if( srcEntity.ID == target.ID )
                continue;
         
            // X轴的距离
            // 增加0.1个距离修正
            float xDist = Math.Abs( srcEntity.Ship.Position.x - target.Ship.Position.x ) + 0.1f;


            if( xDist > (srcEntity.Ship.Reference.vol + target.Ship.Reference.vol) )
                continue;

            float zDist = Math.Abs(Math.Abs(srcEntity.Ship.Position.z) - Math.Abs(target.Ship.Position.z));

            // 取一个较小距离的
            if( zDist < minDist ) {
                minDist = zDist;
                obstructTarget = target;
            }
        }


        for( int i = 0; i < EntityManager.Instance.DefenderUnitList.Count; ++i ) {
            ShipEntity target = EntityManager.Instance.DefenderUnitList[i] as ShipEntity;
            if( srcEntity.Ship.Position.z > target.Ship.Position.z )
                continue;

            if( srcEntity.ID == target.ID )
                continue;


            // X轴的距离
            // 增加0.1个距离修正
            float xDist = Math.Abs( srcEntity.Ship.Position.x - target.Ship.Position.x ) + 0.1f;

            if( xDist > (srcEntity.Ship.Reference.vol + target.Ship.Reference.vol) )
                continue;

            float zDist = Math.Abs( Math.Abs(srcEntity.Ship.Position.z) - Math.Abs(target.Ship.Position.z ));

            // 取一个较小距离的
            if( zDist < minDist ) {
                minDist = zDist;
                obstructTarget = target;
            }
        }

        return obstructTarget;
    }


    /// <summary>
    /// 球体碰撞检测---简易版
    /// </summary>
    /// <param name="srcEntity"></param>
    /// <param name="targetEntity"></param>
    /// <returns></returns>
    private static bool IsCollision(ShipEntity srcEntity, ShipEntity targetEntity) {

        int radius = (srcEntity.Ship.Reference.vol + targetEntity.Ship.Reference.vol - Collision_Adjust);
        int radiusSQ = radius * radius;
        float distanceSQ = (targetEntity.Ship.Position.x - srcEntity.Ship.Position.x) * (targetEntity.Ship.Position.x - srcEntity.Ship.Position.x) + (targetEntity.Ship.Position.z - srcEntity.Ship.Position.z) * (targetEntity.Ship.Position.z - srcEntity.Ship.Position.z);

        return radiusSQ >= distanceSQ;
    }
}
