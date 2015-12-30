using UnityEngine;
using System.Collections;

/// <summary>
/// 锁定攻击
/// </summary>
public class LockAttackState : State<ShipEntity> {
    public static readonly LockAttackState Instance = new LockAttackState();
    public override void Enter( ShipEntity entity ) {

    }

    public override void Execute( ShipEntity entity ) {
        entity.RunPartsAI();
        if( entity.GetPartState() != PartState.Attack ) {
            entity.FsmInstance.ChangeState( SeekState.Instance);
        }
    }

    public override void Exit( ShipEntity entity ) {

    }
}
