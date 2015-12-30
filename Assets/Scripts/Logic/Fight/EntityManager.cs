using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 逻辑实体管理
/// </summary>
public class EntityManager {

    public readonly static EntityManager Instance = new EntityManager();

    // 提供按照快速查询
    private Dictionary<int, Entity> EntityDic_ = new Dictionary<int, Entity>();

    // 提供搜索目标的时候快速遍历,拆分成攻击方和防御方
    public List<Entity> AttackUnitList = new List<Entity>();
    public List<Entity> DefenderUnitList = new List<Entity>();

    private EntityManager() {

    }

    public void ClearAll(){
        EntityDic_.Clear();
        AttackUnitList.Clear();
        DefenderUnitList.Clear();
    }

    /// <summary>
    /// 注册Entity
    /// </summary>
    /// <param name="newEntity"></param>
    public void RegisterEntity( Entity newEntity ) {
        EntityDic_.Add( newEntity.ID, newEntity );
        
        // 这里这么写其实有点纠结
        var shipEntity = newEntity as ShipEntity;
        if( shipEntity.CampType == FightServiceDef.CampType.Camp_Attacker ) {
            this.AttackUnitList.Add( newEntity );
        }
        else {
            this.DefenderUnitList.Add( newEntity );
        }
    }

    /// <summary>
    /// 获取Entity
    /// </summary>
    /// <param name="id"></param>
    public Entity GetEntityByID( int id ) {
        if( EntityDic_.ContainsKey( id ) ) {
            return EntityDic_[id];
        }

        return null;
    }

    /// <summary>
    /// 删除Entity
    /// </summary>
    /// <param name="entity"></param>
    public void RemoveEntity( Entity entity ) {

        this.EntityDic_.Remove( entity.ID );
        AttackUnitList.Remove( entity );
        DefenderUnitList.Remove( entity );
    }
}
