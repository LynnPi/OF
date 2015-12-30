using UnityEngine;
using System.Collections;

/// <summary>
/// 靠近状态
/// </summary>
public class SeekState : State<ShipEntity> {

    public static readonly SeekState Instance = new SeekState();
    public override void Enter( ShipEntity entity ) {

    }

    public override void Execute( ShipEntity entity ) {
        entity.RunPartsAI();
        if( entity.GetPartState() != PartState.Attack ) {
            if( ShipCollision.CanSeek( entity ) )
                entity.Move();
        }
        else {
            entity.FsmInstance.ChangeState(LockAttackState.Instance);
        }

        if( entity.Ship.Position.z >= BattlefieldEnvironment.Instance.BattlefieldZBoundary ) {
            // 通过消息通知脱离战场
            MessageDispatcher.Instance.DispatchMessage( TelegramType.BreakAway, entity.ID, entity.ID, 1 );
        }
    }

    public override void Exit( ShipEntity entity ) {

    }
}
