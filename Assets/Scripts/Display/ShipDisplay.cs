using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PalmPoineer;
using PalmPoineer.Mobile;

public class ShipDisplay : MonoBehaviour {

    private Transform                       Trans_;

    private GameObject                      KineticArmorGo_;

    private GameObject                      EnergyArmorGo_;

    private float                           EnergyArmorTime_;

    private Dictionary<string,Transform> 	TargetRef_;

    private Ship                            Ship_;

    private TeamDisplay                     Team_;

    private bool                            IsDead_ = false;

    private Vector3                         OffsetPosition_;

    private List<GameObject>                SingEffectList_ = new List<GameObject>();

    public Transform Trans {
        get {
            return Trans_;
        }
    }

    public bool IsDead {
        get {
            return IsDead_;
        }
    }

    public TeamDisplay GetTeam() {
        return Team_;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="bPlayer"></param>
    /// <param name="pos"></param>
    public void Init( TeamDisplay team, Ship ship, Vector3 pos, Vector3 offsetPos, bool bPlayer ) {

        if( Trans_ == null )
            Trans_ = GetComponent<Transform>();

        TargetRef_ = new Dictionary<string, Transform>();
        EnumerateChildren( TargetRef_, Trans_ );

        KineticArmorGo_ = AssetLoader.GetInstantiatedGameObject( GlobalEffectDefine.PhysicsShield );
        if( KineticArmorGo_ != null ) {
            AssetLoader.ReplaceShader( KineticArmorGo_ );
            KineticArmorGo_.transform.parent = transform;
            KineticArmorGo_.transform.localPosition = Vector3.zero;
            KineticArmorGo_.transform.localEulerAngles = Vector3.zero;
            KineticArmorGo_.transform.localScale = Vector3.one * ship.shieldRadius * 2;
            KineticArmorGo_.SetActive( false );
        }

        EnergyArmorGo_ = AssetLoader.GetInstantiatedGameObject( GlobalEffectDefine.PhysicsShield );
        if( EnergyArmorGo_ != null ) {
            AssetLoader.ReplaceShader( EnergyArmorGo_ );
            EnergyArmorGo_.transform.parent = transform;
            EnergyArmorGo_.transform.localPosition = Vector3.zero;
            EnergyArmorGo_.transform.localEulerAngles = Vector3.zero;
            EnergyArmorGo_.transform.localScale = Vector3.one * ship.shieldRadius * 2;
            EnergyArmorGo_.SetActive( false );
        }
        
        Team_ = team;
        Ship_ = ship;
        OffsetPosition_ = offsetPos;        
        pos += OffsetPosition_;
        Trans_.position = pos;
        Trans_.eulerAngles = new Vector3( 0, bPlayer ? 0 : 180, 0 );
        OffsetPosition_ -= new Vector3( 0, OffsetPosition_.y, 0 );
    }

    public Vector3 GetNewHurtPoint( Vector3 attackPos, Vector3 targetPos ) {
        return targetPos + (attackPos - targetPos).normalized * GetShieldRadius();
    }

    public float GetShieldRadius() {
        return Ship_.shieldRadius;
    }

    public bool IsKineticEnergyDmg() {
        return (Team_.GetShip().Armor > 0);
    }    

    public void ShowKineticArmor() {
        if( !IsKineticEnergyDmg() ) return;
        if( KineticArmorGo_ == null ) return;
        if( KineticArmorGo_.activeSelf ) return;
        StopCoroutine( "KineticArmor" );
        StartCoroutine( "KineticArmor" );
    }

    private IEnumerator KineticArmor() {
        KineticArmorGo_.SetActive( true );
        Global.PlaySound( "res/audio/physics_shield.audio", KineticArmorGo_.transform.position );
        yield return new WaitForSeconds( 0.95f );
        KineticArmorGo_.SetActive( false );
    }

    public bool IsEnergyArmor() {
        return (Team_.GetShip().Energy > 0);
    }

    public void ShowEnergyArmor( float time ) {
        if( !IsEnergyArmor() ) return;
        if( EnergyArmorGo_ == null ) return;
        if( EnergyArmorGo_.activeSelf ) return;
        EnergyArmorTime_ = time;
        StopCoroutine( "EnergyArmor" );
        StartCoroutine( "EnergyArmor" );
    }

    private IEnumerator EnergyArmor() {
        EnergyArmorGo_.SetActive( true );
        Global.PlaySound( "res/audio/physics_shield.audio", EnergyArmorGo_.transform.position );
        yield return new WaitForSeconds( EnergyArmorTime_ );
        EnergyArmorGo_.SetActive( false );
    }

    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void Move( float x, float z , int moveType) {
        Vector3 pos = new Vector3( x, Trans_.position.y, z ) + OffsetPosition_;
        if (moveType == Def.MoveType.Forward)
            Trans_.LookAt( pos );
        LeanTween.move( this.gameObject, pos, FightServiceDef.FRAME_INTERVAL_TIME_F ).setEase( LeanTweenType.linear );
    }

    /// <summary>
    /// 施放技能
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="spell"></param>
    public void UseSkill( proto.UseSkill skill ) {
        proto.SkillReference skillReference = GlobalConfig.GetSkillReference( skill.skillid );
        if( skillReference == null ) {
            Debug.Log( "cant find skill reference:" + skill.skillid );
            return;
        }
        if( skill.skillstage == Def.SkillStage.Sing ) {
            SingSkill( GetSkill( skill.partid ) );
        }
        else if( skill.skillstage == Def.SkillStage.Casting ) {
            StartCoroutine( CastSkill( skill ) );
        }
    }

    /// <summary>
    /// 舰船开火
    /// </summary>
    /// <param name="skill"></param>
    public void ShipFire( proto.PartFireEvent fireEvent ) {
        if( Trans_ == null ) return;
        Part part = GetPart( fireEvent.partid );
        if( part == null ) {
            Debug.LogError( "can't find part:" + fireEvent.partid );
            return;
        }
        CastPartFire( fireEvent, part );
    }

    /// <summary>
    /// 脱离战场
    /// </summary>
    public IEnumerator BreakAway() {
        yield return new WaitForSeconds( Random.Range( 0.5f, 3.0f ) );
        Global.Instance.ShowEffect( this.transform.position, GlobalEffectDefine.ShipEscape, Vector3.one );
        Global.Instance.ShowEffect( this.transform.position, GlobalEffectDefine.ShipTransition, Vector3.one );
        yield return new WaitForSeconds( 2.0f );
        LeanTween.move( this.gameObject,
            new Vector3( this.transform.position.x, this.transform.position.y, this.transform.position.z + 1000 ), FightServiceDef.FRAME_INTERVAL_TIME_F );
    }

    /// <summary>
    /// 销毁
    /// </summary>
    public void Destroy() {
        StopAllCoroutines();
        for( int i=0; i < SingEffectList_.Count; i++ ) {
            if( SingEffectList_[i] == null ) continue;
            Destroy( SingEffectList_[i] );
        }
        SingEffectList_.Clear();
        Destroy( gameObject );
    }

    /// <summary>
    /// 根据名字得到子项
    /// </summary>
    /// <param name="name"></param>
    /// <param name="result"></param>
    public Transform GetTransformByName( string name ) {
        Transform result;
        TargetRef_.TryGetValue( name, out result );        
        return result;
    }

    /// <summary>
    /// 得到伤害位置
    /// </summary>
    /// <param name="srcX"></param>
    /// <param name="spell"></param>
    /// <returns></returns>
    public string GetHitPoint( float srcX, SpellType spellType ) {
        string[] hitPointList = null;
        if( Trans_ == null || Ship_ == null )
            return string.Empty;
        if( spellType == SpellType.Cannon || spellType == SpellType.MachineGun ) {
            // 右边
            if( srcX >= Trans_.position.x + Ship_.shieldRadius ) {
                hitPointList = Ship_.hitPointRight;
            }
            // 左边
            else if( srcX <= Trans_.position.x - Ship_.shieldRadius ) {
                hitPointList = Ship_.hitPointLeft;
            }
            else
                hitPointList = Ship_.hitPointFront;
        }
        else
            hitPointList = Ship_.hitPointFront;
        if( hitPointList == null || hitPointList.Length < 1 ) {
            return string.Empty;
        }
        return hitPointList[Random.Range( 0, hitPointList.Length )];
    }

    public IEnumerator ShowDead() {
        IsDead_ = true;

        float delayDestroyTime = 0.5f;
        float delayShakeTime = 0;
        string path = GlobalEffectDefine.ShipDie01;
        int strait = Team_.GetShipStrait();
        float radius = Ship_.shieldRadius;
        if( strait == Def.ShipTrait.Build ) {
            path = GlobalEffectDefine.ShipDie02;
            delayDestroyTime = 0.85f;
            delayShakeTime = 2.5f;
            radius = 1;
        }
        Global.Instance.StartCoroutine( Global.Instance.ShowDeadEffect( Trans_.position, Trans_.rotation, radius, path ) );
        // 等0-2.5秒
        yield return new WaitForSeconds( delayShakeTime );
        // 屏幕抖动
        if( strait == Def.ShipTrait.CommanderShip || strait == Def.ShipTrait.Build )
            CameraManager.Shake( 1f, 1.5f );
        else if( strait == Def.ShipTrait.Attack || strait == Def.ShipTrait.Defend || strait == Def.ShipTrait.Support || strait == Def.ShipTrait.Carrier )
            CameraManager.Shake( 1f, 1.5f );
        // 等0.5-0.85秒
        yield return new WaitForSeconds( delayDestroyTime );
        Destroy();
    }

    /// <summary>
    /// 检索子项并保存
    /// </summary>
    /// <param name="container"></param>
    /// <param name="parent"></param>
    private void EnumerateChildren( Dictionary<string, Transform> container, Transform parent ) {
        foreach( Transform child in parent ) {
            TargetRef_.Add( child.name, child );
            EnumerateChildren( container, child );
        }
    }

    /// <summary>
    /// 根据ID得到技能
    /// </summary>
    /// <param name="skillID"></param>
    /// <returns></returns>
    private Skill GetSkill( int skillID ) {
        if( Ship_.skillList == null ) return null;
        for( int i=0; i < Ship_.skillList.Length; i++ ) {
            Skill skill = Ship_.skillList[i];
            if( skill == null ) continue;
            if( skill.partID != skillID ) continue;
            return skill;
        }
        return null;
    }

    /// <summary>
    /// 根据部件ID得到部件
    /// </summary>
    /// <param name="partID"></param>
    /// <returns></returns>
    private Part GetPart( int partID ) {
        for( int i=0; i < Ship_.partList.Length; i++ ) {
            Part spell = Ship_.partList[i];
            if( spell == null ) continue;
            if( spell.partID != partID ) continue;
            return spell;
        }
        return null;
    }

    /// <summary>
    /// 部件开火
    /// </summary>
    /// <param name="fireEvent"></param>
    /// <param name="part"></param>
    private void CastPartFire( proto.PartFireEvent fireEvent, Part part ) {
        ShowCast( part );
        // 范围攻击,只生成一个子弹
        if( part.partType == SpellType.AOE ) {
            List<BulletTarget> targetList = new List<BulletTarget>();
            for( int i=0; i < fireEvent.fireinfo_size(); i++ ) {
                proto.PartFireInfo info = fireEvent.fireinfo( i );
                TeamDisplay target = BattleSceneDisplayManager.Instance.GetTeamDisplay( info.targetid );
                if( target == null ) continue;
                BulletTarget bulletTarget = new BulletTarget();
                bulletTarget.delayFrame = info.delayframe;
                bulletTarget.Target = target;
                targetList.Add( bulletTarget );
            }
            ClientParts parts = Team_.GetPartsByID( part.partID );
            ShowBullet( parts, targetList, part.partType, part.attackPoint, part.bulletPath, part.hitEffect, Vector3.zero );
            return;
        }
        // 个体攻击,每个对象都生成子弹
        for( int i=0; i < fireEvent.fireinfo_size(); i++ ) {
            proto.PartFireInfo info = fireEvent.fireinfo( i );
            TeamDisplay target = BattleSceneDisplayManager.Instance.GetTeamDisplay( info.targetid );
            if( target == null ) continue;
            List<BulletTarget> targetList = new List<BulletTarget>();
            BulletTarget bulletTarget = new BulletTarget();
            bulletTarget.delayFrame = info.delayframe;
            bulletTarget.Target = target;
            targetList.Add( bulletTarget );
            ClientParts parts = Team_.GetPartsByID( part.partID );
            ShowBullet( parts, targetList, part.partType, part.attackPoint, part.bulletPath, part.hitEffect, Vector3.zero );
        }
    }

    private IEnumerator CastSkill( proto.UseSkill useSkill ) {
        Skill skill = GetSkill( useSkill.partid );
        if( skill == null ) {
            Debug.Log( "skill is null" );
            yield break;
        }
        ShowCast( skill );
        ClientSkill clientSkill = BattleSys.GetCommanderShip().GetSkillById( useSkill.skillid );
        // 范围攻击,只生成一个子弹
        if( skill.partType == SpellType.AOE ) {
            List<BulletTarget> targetList = new List<BulletTarget>();
            for( int i=0; i < useSkill.fireinfolist_size(); i++ ) {
                proto.PartFireInfo info = useSkill.fireinfolist( i );
                TeamDisplay target = BattleSceneDisplayManager.Instance.GetTeamDisplay( info.targetid );
                if( target == null ) continue;
                BulletTarget bulletTarget = new BulletTarget();
                bulletTarget.delayFrame = info.delayframe;
                bulletTarget.Target = target;
                targetList.Add( bulletTarget );
            }
            ClientParts parts = Team_.GetPartsByID( skill.partID );
            float x = (float)useSkill.posx/10000f;
            float y = (float)useSkill.posy/10000f;
            float z = (float)useSkill.posz/10000f;
            Vector3 position = new Vector3( x, y, z );
            ShowBullet( parts, targetList, skill.partType, skill.attackPoint, skill.bulletPath, skill.hitEffect, position, clientSkill );
        }
        // 个体攻击,每个对象都生成子弹
        else{
            for( int i=0; i < useSkill.fireinfolist_size(); i++ ) {
                proto.PartFireInfo info = useSkill.fireinfolist( i );
                TeamDisplay target = BattleSceneDisplayManager.Instance.GetTeamDisplay( info.targetid );
                if( target == null ) continue;
                List<BulletTarget> targetList = new List<BulletTarget>();
                BulletTarget bulletTarget = new BulletTarget();
                bulletTarget.delayFrame = info.delayframe;
                bulletTarget.Target = target;
                targetList.Add( bulletTarget );
                ClientParts parts = Team_.GetPartsByID( skill.partID );
                ShowBullet( parts, targetList, skill.partType, skill.attackPoint, skill.bulletPath, skill.hitEffect, Vector3.zero, clientSkill );
            }
        }
        yield return new WaitForSeconds( 1.2f );
        for( int i=0; i < SingEffectList_.Count; i++ ) {
            if( SingEffectList_[i] == null ) continue;
            Destroy( SingEffectList_[i] );
        }
        SingEffectList_.Clear();
    }

    private void SingSkill( Skill skill ) {
        if( skill == null ) {
            Debug.Log( "skill is null" );
            return;
        }
        Transform attackPoint = GetTransformByName( skill.attackPoint );
        if( attackPoint == null ) {
            Debug.Log( "cant find part:" + name );
            return;
        }
        foreach( Transform iter in attackPoint ) {
            Effect effect = skill.singEffect;
            GameObject go = Global.Instance.PlaySingEffect( iter, effect );
            if( go != null )
                SingEffectList_.Add( go );
        }
    }

    private void ShowCast( Skill skill ) {
        Transform attackPoint = GetTransformByName( skill.attackPoint );
        if( attackPoint == null ) {
            Debug.Log( "cant find part:" + name );
        }
        foreach( Transform iter in attackPoint ) {
            Effect effect = skill.attackEffect;
            Global.Instance.StartCoroutine( Global.Instance.PlayEffect( iter, effect, Vector3.zero, true ) );
        }
    }

    /// <summary>
    /// 播放施放
    /// </summary>
    /// <param name="part"></param>
    private void ShowCast( Part part ) {
        Transform attackPoint = GetTransformByName( part.attackPoint );
        if( attackPoint == null ) {
            Debug.Log( "cant find part:" + name );
        }
        foreach( Transform iter in attackPoint ) {
            Effect effect = part.attackEffect;
            Global.Instance.StartCoroutine( Global.Instance.PlayEffect( iter, effect, Vector3.zero, true ) );
        }
    }

    /// <summary>
    /// 播放子弹
    /// </summary>
    /// <param name="part"></param>
    /// <param name="target"></param>
    /// <param name="delayFrame"></param>
    private void ShowBullet( ClientParts clientPart, List<BulletTarget> targetList, SpellType spellType, string attackPoint, string bulletPath,
        Effect hurtEffect, Vector3 position, ClientSkill skill = null ) {
        Transform attackTransRoot = GetTransformByName( attackPoint );
        foreach( Transform attackTrans in attackTransRoot ) {
            BulletDisplay.ShowBullet( attackTrans.position.x, attackTrans, targetList, clientPart, spellType, bulletPath, hurtEffect, position, skill );
        }
    }
}
