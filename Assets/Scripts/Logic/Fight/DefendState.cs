using UnityEngine;
using System.Collections;

/// <summary>
/// 防御状态
/// </summary>
public class DefendState : State<ShipEntity> {

    public static readonly DefendState Instance = new DefendState(); 
    public override void Enter( ShipEntity entity ) {

    }

    public override void Execute( ShipEntity entity ) {
        entity.RunPartsAI();
        if( entity.Ship.Reference.sw_targetdecision_range > 0 ) {
            // 侧移判定
           if (entity.GetPartState() == PartState.Attack)
               return ;

           ShipEntity targetEntity = TargetSelector.GetLateralTarget( entity );
           if( targetEntity == null )
               return;
           bool lateralRight = targetEntity.Ship.Position.x > entity.Ship.Position.x;

           if( ShipCollision.CanLateral( entity, targetEntity, lateralRight ) )
               entity.LateralMove( lateralRight );
        }
    }

    public override void Exit( ShipEntity entity ) {

    }
}
