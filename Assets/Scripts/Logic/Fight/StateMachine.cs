using UnityEngine;
using System.Collections;

/// <summary>
/// 状态机
/// </summary>
public class StateMachine<T> {

    private T Owner_;

    /// <summary>
    /// TODO，暂时只需要维护一个State
    /// </summary>
    private State<T> CurrentState_;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    public StateMachine( T owner ) {
        Owner_ = owner;
    }

    /// <summary>
    /// 设置当前状态
    /// </summary>
    /// <param name="newState"></param>
    public void SetCurrentState(State<T> newState) {
        CurrentState_ = newState;
    }

    /// <summary>
    /// 状态机刷新
    /// </summary>
    public void Update() {
        CurrentState_.Execute( Owner_ );
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(State<T> newState) {

        CurrentState_.Exit(Owner_);

        CurrentState_ = newState;
        
        CurrentState_.Enter( Owner_ );
    }
}
