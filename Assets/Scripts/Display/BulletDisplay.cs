using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PalmPoineer;
using PalmPoineer.Mobile;

public class BulletTarget {
    public TeamDisplay  Target;
    public int delayFrame;
}

public class BulletDisplay : MonoBehaviour {

    /// <summary>
    /// 子弹
    /// </summary>
    /// <param name="attackShipTrans"></param>
    /// <param name="attackPartTrans"></param>
    /// <param name="target"></param>
    /// <param name="attackPart"></param>
    /// <param name="part"></param>
    /// <param name="delayTime"></param>
    public static void ShowBullet( float srcX, Transform attackPartTrans, List<BulletTarget> targetList, ClientParts attackPart,
        SpellType spellType, string bulletPath, Effect hurtEffect, Vector3 position, ClientSkill skill ) {

        // 范围攻击特殊处理
        if( spellType == SpellType.AOE ) {
            BulletDisplay display = InitBullet( attackPartTrans, null, null, spellType, bulletPath, hurtEffect );
            if( display == null ) return;
            display.SetAOE( targetList, position, skill );
            return;
        }

        // 其他攻击,一个子弹只对应一个目标
        if( targetList.Count != 1 ) return;

        if( targetList[0].Target == null ) return;

        ShipDisplay targetShip = targetList[0].Target.GetHurtShip();
        if( targetShip == null ) return;
        float delayTime = FightServiceDef.FRAME_INTERVAL_TIME_F * targetList[0].delayFrame;

        Transform targetTrans = targetShip.GetTransformByName( targetShip.GetHitPoint( srcX, spellType ) );
        if( targetTrans == null )
            targetTrans = targetShip.Trans;

        // 导弹特殊处理,从左右发子弹
        if( spellType == SpellType.Missile ) {
            // TODO: 临时增加逻辑,防御节点一边扔3颗导弹
            // 后面要考虑走配置
            int missileCout = 1;
            if( attackPart.Owner.Reference.unitstrait == Def.ShipTrait.DefenseBuild )
                missileCout = 3;
            for( int i=0; i < missileCout; i++ ) {
                BulletDisplay display = InitBullet( attackPartTrans, targetShip, targetTrans, spellType, bulletPath, hurtEffect );
                if( display == null ) return;
                display.SetMissile( attackPartTrans.name.Contains( "left" ), delayTime );
            }
        }
        // 机枪特殊处理,做成扫射效果
        else if( spellType == SpellType.MachineGun ) {
            for( int i=0; i < attackPart.Reference.continuity_times; i++ ) {
                BulletDisplay display = InitBullet( attackPartTrans, targetShip, targetTrans, spellType, bulletPath, hurtEffect );
                display.SetGun( delayTime, (float)attackPart.Reference.continuity_interval / 1000f * i );
            }
        }
        else {
            BulletDisplay display = InitBullet( attackPartTrans, targetShip, targetTrans, spellType, bulletPath, hurtEffect );
            display.SetLaserOrConnon( delayTime );
        }
    }

    private static BulletDisplay InitBullet( Transform attackTrans, ShipDisplay targetShip, Transform targetTrans, SpellType spellType, string bulletPath, Effect hurtEffect ) {
        GameObject bullet = AssetLoader.PlayEffect( bulletPath );
        if( bullet == null ) {
            Debug.Log( "can not find bullet:" + bulletPath );
            bullet = new GameObject();
            //return null;
        }
        bullet.name = spellType.ToString();
        bullet.transform.position = attackTrans.position;
        bullet.transform.rotation = attackTrans.rotation;
        BulletDisplay display = bullet.GetComponent<BulletDisplay>();
        if( display == null )
            display = bullet.AddComponent<BulletDisplay>();
        display.SetCache( attackTrans, targetShip, targetTrans, spellType, bulletPath, hurtEffect );
        return display;
    }

    Transform   AttackTrans_;
    Transform   TargetTrans_;
    ShipDisplay TargetShip_;
    SpellType   SpellType_;
    string      BulletPath_;
    Effect      HurtEffect_;

    private void SetCache( Transform attackTrans, ShipDisplay targetShip, Transform targetTrans, SpellType spellType, string bulletPath, Effect hurtEffect ) {
        AttackTrans_ = attackTrans;
        TargetTrans_ = targetTrans;
        TargetShip_ = targetShip;
        SpellType_ = spellType;
        BulletPath_ = bulletPath;
        HurtEffect_ = hurtEffect;
    }

    private void SetAOE( List<BulletTarget> targetList, Vector3 position, ClientSkill skill ) {
        StartCoroutine( ShowAOE( targetList, position, skill ) );
    }

    private void SetGun( float moveTime, float delayTime ) {
        StartCoroutine( ShowGun( moveTime, delayTime ) );
    }

    private void SetMissile( bool bLeft, float delayTime ) {        
        StartCoroutine( ShowMissile( bLeft, delayTime ) );
    }

    private void SetLaserOrConnon( float delayTime ) {
        if( SpellType_ == SpellType.Laser )
            StartCoroutine( ShowLaser( delayTime ) );
        else if( SpellType_ == SpellType.Cannon )
            StartCoroutine( ShowCannon( delayTime ) );
    }

    /// <summary>
    /// 销毁
    /// </summary>
    private void Destroy( bool bCache ) {
        StopAllCoroutines();
        if( bCache )
            AssetLoader.Destroy( BulletPath_, this.gameObject );
        else
            GameObject.Destroy( this.gameObject );
    }

    private IEnumerator UpdatePosition() {
        while( true ) {
            if( AttackTrans_ == null ) break;
            transform.position = AttackTrans_.position;
            yield return null;
        }
    }

    private IEnumerator ShowAOE( List<BulletTarget> targetList, Vector3 position, ClientSkill skill ) {
        if( skill.Prop.skill_select_type != Def.SkillSelectType.PlayerSelect ) {
            StartCoroutine( UpdatePosition() );
            CoroutineJoin join = new CoroutineJoin( AssetLoader.Instance );
            for( int i=0; i < targetList.Count; i++ ) {
                BulletTarget bulletTarget = targetList[i];
                TeamDisplay target = bulletTarget.Target;
                if( target == null ) continue;
                join.StartSubtask( ShowHurtEffect( target, bulletTarget.delayFrame ) );
            }
            yield return join.WaitForAll();
        }
        else {
            Global.ShowHurtEffect( null, HurtEffect_, position );
        }
        yield return null;
        StartCoroutine( WaitAndDestroy( true, 2f ) );
    }

    private IEnumerator ShowHurtEffect( TeamDisplay targetTeam, float delayFrame ) {
        yield return new WaitForSeconds( FightServiceDef.FRAME_INTERVAL_TIME_F * delayFrame );
        ShipDisplay targetShip = targetTeam.GetHurtShip();
        if( targetTeam == null ) yield break;
        if( AttackTrans_ == null ) yield break;
        if( targetShip == null ) yield break;
        Transform trans = targetShip.GetTransformByName( targetShip.GetHitPoint( AttackTrans_.position.x, SpellType_ ) );
        if( trans == null )
            trans = targetShip.Trans;
        Global.ShowHurtEffect( trans, HurtEffect_, trans.position );
    }

    /// <summary>
    /// 激光
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowLaser( float delayTime ) {
        LineRenderer[] renderer = gameObject.GetComponentsInChildren<LineRenderer>();
        float allTime = Time.time + delayTime;
        float range = 1.5f;
        float time = 0;
        bool bX = Random.Range( 0, 100 ) > 50;
        bool bY = Random.Range( 0, 100 ) > 50;

        // 如果是能量护盾,显示护盾
        if( TargetTrans_ != null ) {
            if( TargetShip_.IsEnergyArmor() ) {
                TargetShip_.ShowEnergyArmor( delayTime );
            }
        }

        Vector3 attackPos = Vector3.zero;
        //if( AttackTrans_ != null )
            attackPos = AttackTrans_.position;

        // 这样处理是为了处理 施放船和目标船都在移动时,需要同时更新攻击点和受击点
        while( Time.time <= allTime ) {
            for( int i=0; i < renderer.Length; i++ ) {

                if( AttackTrans_ != null ) {
                    attackPos = AttackTrans_.position;
                    renderer[i].SetPosition( 0, attackPos );
                }
                // 激光扫射效果
                if( TargetTrans_ != null ) {
                    if( TargetShip_.IsEnergyArmor() ) {
                        // 有护盾时没有被攻击特效
                        Vector3 targetPos = TargetTrans_.position + new Vector3( bX ? (range * time) : 0, bY ? (range * time) : 0 );
                        targetPos = TargetShip_.GetNewHurtPoint( attackPos, targetPos );
                        renderer[i].SetPosition( 1, targetPos );
                    }
                    else {
                        Vector3 targetPos = TargetTrans_.position + new Vector3( bX ? (range * time) : 0, bY ? (range * time) : 0 );
                        renderer[i].SetPosition( 1, targetPos );
                        if( time % Random.Range( 0.1f, 0.3f ) == 0 ) {
                            Global.ShowHurtEffect( TargetTrans_, HurtEffect_, targetPos );
                        }
                    }
                }
            }
            time += Time.deltaTime;
            yield return null;
        }
        StartCoroutine( WaitAndDestroy() );
    }

    /// <summary>
    /// 导弹
    /// </summary>
    /// <param name="delayTime"></param>
    /// <returns></returns>
    private IEnumerator ShowMissile( bool bLeft, float delayTime ) {
        Vector3 secondPos = AttackTrans_.position + (AttackTrans_.forward * 10);
        Vector3 thirdPos = TargetTrans_.position + (TargetTrans_.forward * 20);
        Vector3 lastPos = TargetTrans_.position;
        // 为了让导弹显示的差异化,各种随机坐标
        bool bIsPlayer = AttackTrans_.position.z < TargetTrans_.position.z;
        // 敌人
        if( !bIsPlayer ) {
            if( AttackTrans_.position.x > TargetTrans_.position.x ) {
                secondPos += (AttackTrans_.right * Random.Range( 0, 10 ));
                thirdPos += (TargetTrans_.right * Random.Range( 5, 8 ));
                lastPos += (TargetTrans_.right * Random.Range( 0, 3 ));
            }
            else {
                secondPos += (-AttackTrans_.right * Random.Range( 0, 10 ));
                thirdPos += (-TargetTrans_.right * Random.Range( 5, 8 ));
                lastPos += (-TargetTrans_.right * Random.Range( 0, 3 ));
            }
        }
        // 玩家
        else {
            if( AttackTrans_.position.x > TargetTrans_.position.x ) {
                secondPos += (-AttackTrans_.right * Random.Range( 0, 10 ));
                thirdPos += (-TargetTrans_.right * Random.Range( 5, 8 ));
                lastPos += (-TargetTrans_.right * Random.Range( 0, 3 ));
            }
            else {
                secondPos += (AttackTrans_.right * Random.Range( 0, 10 ));
                thirdPos += (TargetTrans_.right * Random.Range( 5, 8 ));
                lastPos += (TargetTrans_.right * Random.Range( 0, 3 ));
            }
        }
        // 如果敌人有护盾,显示护盾,修正最终目标位置
        if( TargetShip_.IsKineticEnergyDmg() ) {
            lastPos = TargetShip_.GetNewHurtPoint( AttackTrans_.position, lastPos );
            thirdPos += TargetTrans_.forward * TargetShip_.GetShieldRadius() * 5;
        }
        // 随机时间,产生表现差异化
        delayTime += Random.Range( -0.5f, 0.5f );
        
        Vector3[] v3 = new Vector3[] { AttackTrans_.position, thirdPos, secondPos, lastPos };
        LeanTween.move( gameObject, v3, delayTime ).setEase( LeanTweenType.linear ).setOrientToPath( true );
        yield return new WaitForSeconds( delayTime );
        if( TargetShip_ != null ) {
            if( TargetShip_.IsKineticEnergyDmg() ) {
                TargetShip_.ShowKineticArmor();
            }
        }
        Global.ShowHurtEffect( TargetTrans_, HurtEffect_, lastPos );
        StartCoroutine( WaitAndDestroy( false ) );
    }

    /// <summary>
    /// 加农炮
    /// </summary>
    /// <param name="delayTime"></param>
    /// <returns></returns>
    private IEnumerator ShowCannon( float delayTime ) {
        Vector3 targetPos = TargetTrans_.position;
        if( TargetShip_.IsKineticEnergyDmg() ) {
            targetPos = TargetShip_.GetNewHurtPoint( AttackTrans_.position, targetPos );
        }
        transform.LookAt( targetPos );
        LeanTween.move( this.gameObject, targetPos, delayTime );
        yield return new WaitForSeconds( delayTime );
        if( TargetShip_ != null ) {
            if( TargetShip_.IsKineticEnergyDmg() ) {
                TargetShip_.ShowKineticArmor();
            }
        }
        Global.ShowHurtEffect( TargetTrans_, HurtEffect_, targetPos );
        StartCoroutine( WaitAndDestroy() );
    }

    private IEnumerator ShowGun( float moveTime, float delayTime ) {
        Vector3 v3 = TargetTrans_.position + new Vector3( Random.Range( -10, 11 ), 0, Random.Range( -10, 11 ) );
        if( TargetShip_.IsKineticEnergyDmg() ) {
            v3 = TargetShip_.GetNewHurtPoint( AttackTrans_.position, v3 );
        }
        yield return new WaitForSeconds( delayTime );
        transform.LookAt( v3 );
        LeanTween.move( this.gameObject, v3, moveTime );
        yield return new WaitForSeconds( moveTime );
        if( TargetShip_ != null ) {
            if( TargetShip_.IsKineticEnergyDmg() ) {
                TargetShip_.ShowKineticArmor();
            }
        }
        Global.ShowHurtEffect( TargetTrans_, HurtEffect_, v3 );
        StartCoroutine( WaitAndDestroy() );
    }

    /// <summary>
    /// 等待后销毁
    /// </summary>
    /// <param name="delayTime"></param>
    /// <returns></returns>
    private IEnumerator WaitAndDestroy( bool bCache = true, float delayTime = 0f ) {
        yield return new WaitForSeconds( delayTime );
        Destroy( bCache );
    }
}