using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PalmPoineer;
using PalmPoineer.Mobile;

public class TeamDisplay {

    private GameObject                      RootGo_;

    private ClientShip                      ClientShip_;

    private BattleShipInfo                  BattleShipInfo_;

    private List<ShipDisplay>               ShipList_ = new List<ShipDisplay>();

    private List<int>                       HurtShipIndexList = new List<int>();

    private List<AttackRangeDisplay>        AttackRangeDisplayList_ = new List<AttackRangeDisplay>();

    private List<AttackRangeDisplay>        SkillAttackRangeDisplayList = new List<AttackRangeDisplay>();

    private static Object                   HaloTemplate_;

    private GameObject                      HaloGo_;

    private bool                            IsShowSkillRange_ = false;

#if UNITY_EDITOR
    private List<GameObject>                DebugGoList_ = new List<GameObject>();
    public void ShowDebugGo( bool b ) {
        if( b ) {
            if( DebugGoList_.Count <= 0 ) {
                GameObject go = GameObject.Instantiate( Resources.Load( "Debug/Cylinder" ) ) as GameObject;
                go.transform.parent = RootGo_.transform;
                go.transform.localPosition = Vector3.zero + new Vector3( 0, -0.7f, 0 );
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = new Vector3( 1, 0, 1 ) * ClientShip_.Reference.vol * 2 + new Vector3( 0, 0.001f, 0 );
                DebugGoList_.Add( go );
                go = GameObject.Instantiate( Resources.Load( "Debug/Cube" ) ) as GameObject;
                go.transform.parent = RootGo_.transform;
                go.transform.localPosition = Vector3.zero + new Vector3( 0, -0.5f, 0 );
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = new Vector3( ClientShip_.Reference.hit_envelope_wid * 2, 0.001f, ClientShip_.Reference.hit_envelope_len * 2 );
                DebugGoList_.Add( go );
                for( int i=0; i < ClientShip_.PartsList.Count; i++ ) {
                    go = GameObject.Instantiate( Resources.Load( "Debug/Cylinder2" ) ) as GameObject;
                    go.transform.parent = RootGo_.transform;
                    go.transform.localPosition = Vector3.zero + new Vector3( 0, -1f, 0 );
                    go.transform.localEulerAngles = Vector3.zero;
                    go.transform.localScale = new Vector3( 1, 0, 1 ) * ClientShip_.PartsList[i].Reference.closein_range * 2 + new Vector3( 0, 0.001f, 0 );
                    DebugGoList_.Add( go );
                }
            }
        }

        for( int i=0; i < DebugGoList_.Count; i++ ) {
            DebugGoList_[i].SetActive( b );
        }
    }
#endif

    public bool IsShowSkillRange {
        get {
            return IsShowSkillRange_;
        }
    }

    public bool IsDead { get; private set; }    

    public GameObject GetTeamGo() {
        return RootGo_;
    }

    public bool IsCommanderShip() {
        return GetShip().IsCommanderShip();
    }

    public int GetShipStrait() {
        return GetShip().GetShipStrait();
    }

    public List<ShipDisplay> GetShipList() {
        return ShipList_;
    }

    public ClientParts GetPartsByID( int id ) {
        return ClientShip_.GetPartsByID( id );
    }

    public ClientShip GetShip() {
        return ClientShip_;
    }

    public ShipDisplay GetHurtShip() {
        int index = -1;
        int num = 0;
        if( ShipList_.Count <= 0 )
            return null;
        while( index == -1 ) {
            int temp = Random.Range( 0, ShipList_.Count );
            if( ShipList_[temp] != null ) {
                index = temp;
                break;
            }
            num++;
            if( num >= 50 )
                break;
        }
        if( index >= 0 ) {
            HurtShipIndexList.Add( index );
            return ShipList_[index];
        }
        return null;
    }

    public void CheckSkillRange() {
        if( IsShowSkillRange )
            ShowRange( true );
    }

    // 显示攻击范围
    public void ShowRange( bool b ) {
        if( AttackRangeDisplayList_.Count <= 0 ) return;
        foreach( var iter in AttackRangeDisplayList_ ) {
            if( iter != null )
                iter.gameObject.SetActive( b );
        }
        if( b ) {
            ShowSkillRange( !b );
        }
        IsShowSkillRange_ = false;
    }

    public void ShowSkillRange( bool b, int index = -1 ) {
        if( SkillAttackRangeDisplayList.Count <= 0 ) return;
        for( int i=0; i < SkillAttackRangeDisplayList.Count; i++ ) {
            if( SkillAttackRangeDisplayList[i] == null ) continue;
            if( i == index )
                SkillAttackRangeDisplayList[i].gameObject.SetActive( b );
            else
                SkillAttackRangeDisplayList[i].gameObject.SetActive( false );
        }

        if( b && index != -1 ) {
            ShowRange( !b );
            IsShowSkillRange_ = true;
        }
        else
            IsShowSkillRange_ = false;
    }

    // 显示选中状态(血条,选中光圈等)
    public void SetSelect( bool b ) {
        if( BattleShipInfo_ )
            BattleShipInfo_.SetSelect( b );
    }

    public bool IsPlayerShip() {
        if( RootGo_ == null )
            return false;
        return RootGo_.name.Contains( "Player" );
    }

    public void Init( ClientShip clientShip, Ship ship, List<GameObject> shipList, bool bPlayer ) {

        IsDead = false;

        RootGo_ = new GameObject( "Root_" + (bPlayer ? "Player" : "Enemy") + "_" );//+ clientShip.Reference.name );
        RootGo_.transform.position = clientShip.Position;

        ClientShip_ = clientShip;
        //Ship_ = ship;
        Vector3[] tempFormationList = clientShip.FormationList;

        for( int i=0; i < shipList.Count; i++ ) {
            ShipDisplay display = shipList[i].AddComponent<ShipDisplay>();
            display.Init( this, ship, clientShip.Position, tempFormationList == null ? Vector3.zero : tempFormationList[i], bPlayer );
            ShipList_.Add( display );
        }
        bool isAdvancedShip = (clientShip.Reference.id == 40000 || clientShip.Reference.id == 60000);//暂时这样写，到时候根据tag来

        BattleShipInfo_ = BattleShipInfo.Create( RootGo_.transform, this, clientShip.Name, ship.shieldRadius, bPlayer, isAdvancedShip );

        foreach( var iter in clientShip.PartsList ) {
            float atkRangeMin = iter.Reference.atk_range_min;
            float atkRangeMax = iter.Reference.atk_range_max;
            float atkMoveRange = iter.Reference.attack_move_range;
            float angle = iter.Reference.atk_angle;
            // 最近和最远的攻击范围
            AttackRangeDisplayList_.Add( AttackRangeDisplay.InitSectorAttackRange( bPlayer, RootGo_.transform, atkRangeMin, atkRangeMax, angle, Color.red ) );
            // 移动攻击范围
            AttackRangeDisplayList_.Add( AttackRangeDisplay.InitSectorAttackRange( bPlayer, RootGo_.transform, 0, atkMoveRange, angle, Color.gray ) );
        }

        // 是玩家并且是旗舰,生成技能范围
        if( bPlayer && clientShip.IsCommanderShip() ) {
            for( int i = 0; i < 3; i++ ) {
                ClientSkill skill = clientShip.GetSkillByIndex( i );
                AttackRangeDisplay display = null;
                if( skill != null ) {
                    proto.SkillReference reference = skill.Prop;
                    // 扇形
                    if( reference.skill_select_type == Def.SkillSelectType.CastScope ) {
                        display = AttackRangeDisplay.InitSectorAttackRange( true, RootGo_.transform, 0, reference.cast_range, reference.cast_angle, Color.blue );
                    }
                    //else if( reference.skill_select_type == Def.SkillSelectType.PlayerSelect ) {
                        // 圆形
                        //if( reference.shape_type == Def.ShapeType.Circle ) {
                        //    display = AttackRangeDisplay.InitCircleAttackRange( RootGo_.transform, reference.aoe_range, Color.blue );
                        //}
                    //}
                    else if( reference.skill_select_type == Def.SkillSelectType.NoSelection ) {
                        // 矩形或圆形
                        if( reference.shape_type == Def.ShapeType.Rectangle )
                            display = AttackRangeDisplay.InitRectangleAttackRange( RootGo_.transform, reference.radiate_len, reference.radiate_wid * 2, Color.blue );
                        else if( reference.shape_type == Def.ShapeType.Circle )
                            display = AttackRangeDisplay.InitCircleAttackRange( RootGo_.transform, reference.aoe_range, Color.blue );
                    }
                    if( display != null )
                        display.gameObject.name += "_skill";
                }
                SkillAttackRangeDisplayList.Add( display );
            }
        }

        if( HaloTemplate_ == null )
            HaloTemplate_ = Resources.Load( "Effect/Halo" );

        HaloGo_ = GameObject.Instantiate( HaloTemplate_ ) as GameObject;
        HaloGo_.transform.GetChild( 0 ).localPosition = Vector3.zero;
        HaloGo_.transform.parent = RootGo_.transform;
        HaloGo_.transform.localEulerAngles = Vector3.zero;
        HaloGo_.transform.localPosition = Vector3.zero + new Vector3( 0, -1, 0 );
        HaloGo_.transform.localScale = Vector3.one * clientShip.Reference.vol;
    }

    public void Move( float x, float z , int moveType) {
        ClientShip_.Position.x = x;
        ClientShip_.Position.z = z;
        LeanTween.move( RootGo_, ClientShip_.Position, FightServiceDef.FRAME_INTERVAL_TIME_F ).setEase( LeanTweenType.linear );
        if( ShipList_.Count == 1 ) {
            ShipList_[0].Move( x, z, moveType);
            return;
        }
        for( int i = 0; i < ShipList_.Count; i++ ) {
            if( ShipList_[i] == null ) continue;
            ShipList_[i].Move( x, z, moveType );
        }
    }

    public void UseSkill( proto.UseSkill skill ) {
        if( ShipList_.Count < 1 ) {
            Debug.Log( "there is no ship can use skill" );
            return;
        }
        // 编队必定不能使用技能
        if( ShipList_.Count > 1 ) {
            Debug.Log( "skill can not used by team" );
            return;
        }
        if( ShipList_[0] == null ) {
            Debug.Log( "no ship to use skill" );
            return;
        }
        ShipList_[0].UseSkill( skill );
    }

    public void ShipFire( proto.PartFireEvent fireEvent ) {
        if( ShipList_.Count == 1 ) {
            ShipList_[0].ShipFire( fireEvent );
            return;
        }
        for( int i=0; i < ShipList_.Count; i++ ) {
            if( ShipList_[i] == null ) continue;
            Global.Instance.StartCoroutine( ShipFireIE( ShipList_[i], fireEvent ) );
        }
    }

    public void BreakAway() {
        IsDead = true;
        HideAllInfo();
        ShowRange( false );
        ShowSkillRange( false );
        for( int i = 0; i < ShipList_.Count; i++ ) {
            if( ShipList_[i] == null ) continue;
            Global.Instance.StartCoroutine( ShipList_[i].BreakAway() );
        }
    }

    private IEnumerator ShipFireIE( ShipDisplay display, proto.PartFireEvent fireEvent ) {
        yield return new WaitForSeconds( Random.Range( 0f, 1f ) );
        display.ShipFire( fireEvent );
    }

    public void HideAllInfo() {
        if( RootGo_ != null )
            RootGo_.SetActive( false );
        if( BattleShipInfo_ != null )
            BattleShipInfo_.HideInfo();
    }

    public void Destroy() {
        for( int i=0; i < ShipList_.Count; i++ ) {
            if( ShipList_[i] == null ) continue;
            if( ShipList_[i].IsDead ) continue;
            ShipList_[i].Destroy();
        }
        
        foreach( var iter in AttackRangeDisplayList_ )
            GameObject.Destroy( iter.gameObject );
        foreach( var iter in SkillAttackRangeDisplayList ) {
            if( iter == null ) continue;
            GameObject.Destroy( iter.gameObject );
        }

#if UNITY_EDITOR
        foreach( var iter in DebugGoList_ ) {
            GameObject.Destroy( iter );
        }
        DebugGoList_.Clear();
#endif

        GameObject.Destroy( HaloGo_ );        
        GameObject.Destroy( RootGo_ );
        if( BattleShipInfo_ != null && BattleShipInfo_.gameObject )
            GameObject.Destroy( BattleShipInfo_.gameObject );

        ShipList_.Clear();
        AttackRangeDisplayList_.Clear();
        SkillAttackRangeDisplayList.Clear();
    }

    public void ModifyHp( proto.UnderAttackInfo underAttackInfo ) {

        // 得到优先目标
        int index = 0;
        if( HurtShipIndexList.Count > 0 ) {
            index = HurtShipIndexList[HurtShipIndexList.Count - 1];
            HurtShipIndexList.RemoveAt( HurtShipIndexList.Count - 1 );
        }
        
        // 处理伤害值
        ClientShip_.Durability += underAttackInfo.durablility;
        ClientShip_.Armor += underAttackInfo.armor;
        ClientShip_.Energy += underAttackInfo.energy;

        // 同步血条
        BattleShipInfo_.UpdateUIByUnderAttack( ClientShip_.Durability, ClientShip_.Energy, ClientShip_.Armor );

        // 单个船的直接算
        if( ShipList_.Count == 1 ) {
            // 若为0,销毁
            if( ClientShip_.Durability <= 0 ) {
                IsDead = true;
                ShipList_[index].StartCoroutine( ShipList_[index].ShowDead() );
                // 我方旗舰
                if( IsPlayerShip() && IsCommanderShip() ) {
                    BattleSceneDisplayManager.PlayerCommanderShipDead();
                }
                // 敌方基地
                if( !IsPlayerShip() && GetShipStrait() == Def.ShipTrait.Build ) {
                    BattleSceneDisplayManager.EnemyCommanderShipDead( RootGo_ );
                }
                Destroy();
            }
            return;
        }

        // 多个船的 根据当前血量算出需要爆炸几个船
        int maxDurability = ClientShip_.MaxDurability * ShipList_.Count;
        int maxDeadNum = (int)(ShipList_.Count * (1f - (float)ClientShip_.Durability / (float)maxDurability));
        int haveDeadNum = 0;

        int deadNum = 0;
        for( int i=0; i < ShipList_.Count; i++ ) {
            if( ShipList_[i] == null ) {
                deadNum++;
            }
            else {
                if( ShipList_[i].IsDead )
                    deadNum++;
            }
        }

        for( int i=0; i < ShipList_.Count; i++ ) {
            if( ShipList_[i] != null ) {
                if( ShipList_[i].IsDead ) {
                    haveDeadNum++;
                }
            }
            else
                haveDeadNum++;
        }

        int needDeadNum = maxDeadNum - haveDeadNum;
        needDeadNum = needDeadNum < 0 ? 0 : needDeadNum;

        if( needDeadNum > 0 && ShipList_[index] != null ) {
            if( !ShipList_[index].IsDead ) {
                ShipList_[index].StartCoroutine( ShipList_[index].ShowDead() );
                needDeadNum--;
            }
        }
        for( int i=0; i < ShipList_.Count; i++ ) {
            if( needDeadNum <= 0 ) break;
            if( ShipList_[i] == null ) continue;
            if( ShipList_[i].IsDead ) continue;
            ShipList_[i].StartCoroutine( ShipList_[i].ShowDead() );
            needDeadNum--;
        }

        if( ClientShip_.Durability <= 0 ) {
            IsDead = true;
            Destroy();
        }
    }

    public void SyncAttr( proto.UnitAttri attribute ) {
        ClientShip_.Armor = attribute.armor;
        ClientShip_.Durability = attribute.durablility;
        ClientShip_.Energy = attribute.energy;

        this.BattleShipInfo_.SyncAttr( ClientShip_.Durability, ClientShip_.Energy, ClientShip_.Armor );
    }
}