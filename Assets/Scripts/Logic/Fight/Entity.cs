using UnityEngine;
using System.Collections;

/// <summary>
/// 逻辑实体
/// </summary>
public class Entity {

    public int ID {
        get;
        set;
    }

    public virtual bool HandleMessage( Telegram msg ) {
        return false;
    }

    public virtual bool IsActive() {
        return false;
    }

    public virtual void  Update() {

    }
}
