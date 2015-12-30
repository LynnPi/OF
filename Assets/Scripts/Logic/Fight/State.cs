using UnityEngine;
using System.Collections;


/// <summary>
/// 状态基类
/// </summary>
public abstract class State<T>  {

    /// <summary>
    /// 进入
    /// </summary>
    /// <param name="entity"></param>
    abstract public void Enter( T entity );

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="entity"></param>
    abstract public void Execute( T entity );

    /// <summary>
    /// 退出
    /// </summary>
    /// <param name="entity"></param>
    abstract public void Exit( T entity );
}
