using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PalmPoineer;
using PalmPoineer.Mobile;

public class BattleSceneDisplayManager : MonoBehaviour {

    public static BattleSceneDisplayManager Instance { get; private set; }


    private static bool IsSelectedSkillTarget_;

    public static bool IsSelectedSkillTarget {
        get {
            return IsSelectedSkillTarget_;
        }
        set {
            IsSelectedSkillTarget_ = value;
        }
    }

    private static ClientSkill SelectedSkill_;

    public static ClientSkill SelectedSkill {
        get {
            return SelectedSkill_;
        }
        set {
            SelectedSkill_ = value;
        }
    }

    private static int SelectedSkillIndex_ = -1;

    public static int SelectedSkillIndex {
        get {
            return SelectedSkillIndex_;
        }
        set {
            SelectedSkillIndex_ = value;
        }
    }

    private List<GameObject> UnitDisplayList = new List<GameObject>();

    private Dictionary<int,TeamDisplay> PlayerTeamDisplayList = new Dictionary<int, TeamDisplay>();

    private Dictionary<int,TeamDisplay> EnemyTeamDisplayList = new Dictionary<int, TeamDisplay>();

    private bool bShowDebug = false;

    //private GameObject Grid_;

    private GFGridRenderCamera GridRenderCtrl_;

    private List<BattleArrowGroup> BattleArrowGroupList = new List<BattleArrowGroup>();

    private BattleArrowGroup SelectedPlayerShipArrowGroup_;

    // 技能范围圆
    private GameObject  SkillRangeCircle_;

    // 技能选择参数(vector3或teamdisplay)
    private object SkillSelectParam_;

    public static event Action<int> EventOnSkillTrigger;
    public static event Action<int> EventOnSkillSing;
    public object SkillSelectParam {
        get {
            return SkillSelectParam_;
        }

        private set { }
    }

    // 玩家旗舰死亡
    public static event Action EventOnPlayerCommanderShipDead  = delegate { };
    // 敌人基地死亡
    public static event Action EventOnEnemyCommanderShipDead  = delegate { };

    private void OnDestroy() {
        BattleSys.EventOnFightFrameInfo -= BattleSys_EventOnFightFrameInfo;
        BattleSys.EventOnFightReport -= BattleSys_EventOnFightReport;
        CameraManager.EventOnClickBlankScreenZone -= CameraManager_EventOnClickBlankScreenZone;
        CameraManager.EventOnChangeCameraStatus -= CameraManager_EventOnChangeCameraStatus;
        CameraManager.EventOnClickShip -= CameraManager_EventOnClickShip;

        //GameObject.Destroy( Grid_ );
        Destroy( GridRenderCtrl_ );
        DeployBoundaryDisplayer.EnableVectorCanvas(false);
#if UNITY_EDITOR
        StopCoroutine( "DebugThread" );
#endif
    }

    private BattleFieldGridDrawer BattleFieldGridDrawer_;
    private void Awake() {
        Instance = this;
        CameraManager.Instance.SetCameraType( CameraManager.CameraStatusType.NormalLerp );
        BattleSys.EventOnFightFrameInfo += BattleSys_EventOnFightFrameInfo;
        BattleSys.EventOnFightReport += BattleSys_EventOnFightReport;
        CameraManager.EventOnClickBlankScreenZone += CameraManager_EventOnClickBlankScreenZone;
        CameraManager.EventOnChangeCameraStatus += CameraManager_EventOnChangeCameraStatus;
        CameraManager.EventOnClickShip += CameraManager_EventOnClickShip;

        CheckFight();
        InitGrid();
        StartCoroutine(DrawBoundary());
#if UNITY_EDITOR
        StartCoroutine( "DebugThread" );
#endif
    }

#if UNITY_EDITOR
    private void ShowDebugGo( Dictionary<int, TeamDisplay> list ) {
        foreach( var iter in list ) {
            if( iter.Value == null ) continue;
            if( iter.Value.IsDead ) continue;
            iter.Value.ShowDebugGo( bShowDebug );
        }
    }

    private void CheckDebugGoList() {
        ShowDebugGo( PlayerTeamDisplayList );
        ShowDebugGo( EnemyTeamDisplayList );
    }

    private IEnumerator DebugThread() {
        while( true ) {
            if( Input.GetKeyDown( KeyCode.Escape ) ) {
                bShowDebug = !bShowDebug;
                CheckDebugGoList();
            }
            yield return null;
        }
    }
#endif

    #region Logic callback

    /// <summary>
    /// 战斗回调
    /// </summary>
    /// <param name="obj"></param>
    private void BattleSys_EventOnFightFrameInfo( proto.S2CFightFrameInfo obj ) {

        if( UserStatusPanel.Instance != null ) {
            UserStatusPanel.Instance.ShowTime( obj.fighttick / 1000 );
        }

        for( int i = 0; i < obj.underattacklist_size(); i++ ) {
            proto.UnderAttackInfo underAttackInfo = obj.underattacklist( i );
            TeamDisplay display = GetTeamDisplay( underAttackInfo.unitid );
            display.ModifyHp( underAttackInfo );
        }

        for( int i = 0; i < obj.behaviorsequences_size(); i++ ) {
            proto.UnitBehavior unit = obj.behaviorsequences( i );
            TeamDisplay display = GetTeamDisplay( unit.unitid );
            if( unit.breakaway ) {
                display.BreakAway();
                continue;
            }

            if( unit.has_position() ) {
                display.Move( ((float)unit.position.posx) / 10000f, ((float)unit.position.posz) / 10000f, unit.position.movetype );
            }
            // 部件开火信息表
            for( int eventIndex = 0; eventIndex < unit.partfireevent_size(); eventIndex++ ) {
                proto.PartFireEvent fireEvent = unit.partfireevent( eventIndex );
                if( fireEvent == null ) {
                    Debug.LogError( "PartFireEvent is null" );
                    continue;
                }
                display.ShipFire( fireEvent );
            }
        }

        for( int i=0; i < obj.useskill_size(); i++ ) {
            proto.UseSkill skill = obj.useskill( i );
            if( skill.skillstage == Def.SkillStage.Sing && EventOnSkillSing != null) {
                EventOnSkillSing(skill.skillid);
            }else if(skill.skillstage == Def.SkillStage.Casting && EventOnSkillTrigger != null){
                EventOnSkillTrigger( skill.skillid );
            }
            TeamDisplay display = GetTeamDisplay( skill.unitid );
            display.UseSkill( skill );
        }
    }

    private void BattleSys_EventOnFightReport( proto.S2CFightReport obj ) {
        StartCoroutine( ShowFightReport( obj ) );
    }

    private void CameraManager_EventOnClickBlankScreenZone() {
        ResetRange();
        ResetSelect();
    }

    private void CameraManager_EventOnChangeCameraStatus( CameraManager.CameraStatusType statusType ) {
        if( statusType == CameraManager.CameraStatusType.Free || statusType == CameraManager.CameraStatusType.NormalLerp ) {            
            // 重置选中状态
            ResetSelect();
            // 判断是否需要显示旗舰的普通攻击范围
            CheckCommanderAttackRange();
        }
    }

    private void CameraManager_EventOnClickShip( GameObject obj ) {
        // 显示选中,攻击范围
        ShowSelect( obj.transform );
        ShowRange( obj.transform );
    }

    #endregion

    #region member func

    /// <summary>
    /// 重置队伍的选中状态
    /// </summary>
    /// <param name="list"></param>
    private void ResetSelect( Dictionary<int, TeamDisplay> list, bool bResetArrow = true ) {
        foreach( var iter in list ) {
            iter.Value.SetSelect( false );
        }
        if( !bResetArrow ) return;
        if( SelectedPlayerShipArrowGroup_ == null ) return;
        if( !SelectedPlayerShipArrowGroup_.gameObject.activeSelf ) return;
        SelectedPlayerShipArrowGroup_.ChangeParent( null, 0 );
    }

    /// <summary>
    /// 重置所有队伍选中状态
    /// </summary>
    private void ResetSelect() {
        ResetSelect( PlayerTeamDisplayList );
        ResetSelect( EnemyTeamDisplayList );
    }

    /// <summary>
    /// 重置队伍的攻击范围显示
    /// </summary>
    /// <param name="list"></param>
    private void ResetRange( Dictionary<int, TeamDisplay> list ) {
        foreach( var iter in list ) {
            iter.Value.ShowRange( false );
            iter.Value.ShowSkillRange( false );
        }
    }

    /// <summary>
    /// 重置所有队伍的攻击范围显示
    /// </summary>
    private void ResetRange() {
        ResetRange( PlayerTeamDisplayList );
        ResetRange( EnemyTeamDisplayList );
    }

    /// <summary>
    /// 显示trans所属队伍的攻击范围,关闭所属队伍列表内其他队伍的攻击范围
    /// </summary>
    /// <param name="trans"></param>
    private void ShowRange( Transform trans ) {
        Dictionary<int,TeamDisplay> teamList = GetTeamDisplayList( trans );
        ResetRange( teamList );
        if( trans == null ) {
            return;
        }
        TeamDisplay display = GetTeamDisplay( trans );
        if( display != null )
            display.ShowRange( true );
    }

    /// <summary>
    /// 显示trans所属队伍的选中状态,关闭所属队伍列表内其他队伍的选中状态
    /// </summary>
    /// <param name="trans"></param>
    private void ShowSelect( Transform trans ) {
        if( trans == null ) return;
        Dictionary<int,TeamDisplay> teamList = GetTeamDisplayList( trans );
        TeamDisplay display = GetTeamDisplay( trans );
        if( display == null ) return;
        ResetSelect( teamList, display.IsPlayerShip() );
        display.SetSelect( true );
        if( !display.IsPlayerShip() )
            return;
        GameObject teamGo = display.GetTeamGo();
        if( teamGo == null ) return;
        Transform teamGoTrans = teamGo.transform;
        ClientShip ship = display.GetShip();
        float vol = ship.Reference.vol;
        if( SelectedPlayerShipArrowGroup_ == null ) {
            SelectedPlayerShipArrowGroup_ = BattleArrowGroup.InitArrowGroup( teamGoTrans, vol, false );
        }
        else {
            SelectedPlayerShipArrowGroup_.ChangeParent( teamGoTrans, vol );
        }
    }

    private static void CheckClickSkillTargetPosition( Vector3 position ) {
        Instance.SkillSelectParam_ = position;
        // 更新范围的位置
        if( Instance.SkillRangeCircle_ == null ) {
            AttackRangeDisplay display = AttackRangeDisplay.InitCircleAttackRange( position, SelectedSkill.Prop.aoe_range, Color.green );
            Instance.SkillRangeCircle_ = display.gameObject;
        }
        else {
            Instance.SkillRangeCircle_.transform.position = position;
        }
        Instance.SkillRangeCircle_.SetActive( true );
        IsSelectedSkillTarget = true;
    }

    private static void CheckClickSkillTarget( GameObject go ) {
        if( go == null ) return;
        // 判断点击的是否是可被点击的船
        TeamDisplay display = Instance.GetTeamDisplay( go.transform );
        List<TeamDisplay> list =SkillTargetSelector.FindSkillTarget( GetCommanderShipTeam(), SelectedSkill );
        if( !list.Contains( display ) )
            return;
        Instance.SkillSelectParam_ = display;
        IsSelectedSkillTarget = true;
    }

    private IEnumerator Fight() {
        // 初始化数据
        BattleSys.InitFight();
        // 初始化相机位置
        CameraManager.Instance.InitBattleCamera();
        // 加载场景物件
        StartCoroutine( LoadSceneUnits() );
        // 根据布阵创建所有舰船
        yield return StartCoroutine( CreateAllShips() );
        // 开启相机控制循环
        CameraManager.Instance.StartCameraThread( PlayerTeamDisplayList );
        // 播放开始特效
        yield return StartCoroutine( PlayFightStart() );
        // 舰船进场
        yield return StartCoroutine( AllShipEnterScene() );
        // 等待一秒后开始战斗
        yield return new WaitForSeconds( 1f );
        // 开始战斗
        FightService.Instance.BeginFight();
    }

    /// <summary>
    /// 加载场景物件
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadSceneUnits() {
        yield break;
    }

    /// <summary>
    /// 创建所有舰船
    /// </summary>
    /// <returns></returns>
    private IEnumerator CreateAllShips() {
        CoroutineJoin join = new CoroutineJoin( AssetLoader.Instance );
        // player
        for( int i=0; i < BattleSys.GetShipCount( true ); i++ ) {
            ClientShip clientShip = BattleSys.GetShipByIndex( true, i );
            join.StartSubtask( CreateShip( true, clientShip ) );
        }

        // enemy
        for( int i=0; i < BattleSys.GetShipCount( false ); i++ ) {
            ClientShip clientShip = BattleSys.GetShipByIndex( false, i );
            join.StartSubtask( CreateShip( false, clientShip ) );
        }
        yield return join.WaitForAll();
    }

    private IEnumerator CreateShip( bool bPlayer, ClientShip clientShip ) {
        if( clientShip == null ) {
            Debug.LogError( "can't create,clientShip is null" );
            yield break;
        }
        Ship ship = AssetLoader.GetShipDefine( clientShip.Reference.model_res );
        if( ship == null ) {
            Debug.LogError( "can't create ship,id is:" + clientShip.Reference.id + ",modelres is:" + clientShip.Reference.model_res );
            yield break;
        }

        TeamDisplay display = new TeamDisplay();
        List<GameObject> shipList = new List<GameObject>();
        int stackNum = 1;
        if( clientShip.Reference.stack )
            stackNum = clientShip.Reference.stack_num;
        for( int i=0; i < stackNum; i++ ) {
            GameObject shipGo = AssetLoader.GetInstantiatedGameObject( ship.ModelPath );
            if( !shipGo ) {
                yield return StartCoroutine( AssetLoader.PreloadShipModel( ship ) );
                shipGo = AssetLoader.GetInstantiatedGameObject( ship.ModelPath );
            }
            AssetLoader.ReplaceShader( shipGo );
            shipGo.name = (bPlayer ? "PlayerShip" : "EnemyShip") + clientShip.InFightID;
            if( clientShip.IsCommanderShip() )
                shipGo.name += "_Commander";
            AssetLoader.SetShowParts( ship, shipGo );

            Animation animation = shipGo.GetComponent<Animation>();
            if( animation != null )
                animation.cullingType = AnimationCullingType.AlwaysAnimate;
            shipList.Add( shipGo );
        }

        display.Init( clientShip, ship, shipList, bPlayer );
        if( bPlayer )
            PlayerTeamDisplayList.Add( clientShip.InFightID, display );
        else
            EnemyTeamDisplayList.Add( clientShip.InFightID, display );
    }

    /// <summary>
    /// 播放开始战斗特效
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayFightStart() {
        yield break;
    }

    /// <summary>
    /// 所有舰船进场
    /// </summary>
    /// <returns></returns>
    private IEnumerator AllShipEnterScene() {
        yield break;
    }

    /// <summary>
    /// 显示战报
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private IEnumerator ShowFightReport( proto.S2CFightReport result ) {
        yield return new WaitForSeconds( 5.5f );
        AudioManager.Instance.PlayMusic = false;
        yield return new WaitForSeconds( 0.5f );
        // 播放战斗胜利和失败的音效的代码先放这个地方。
        if( result.fightresult == proto.S2CFightReport.Result.Fail ) {
            Global.PlaySound( "res/audio/youlose.audio", Vector3.zero, false );
        }
        else {
            Global.PlaySound( "res/audio/youwin.audio", Vector3.zero, false );
        }
        UIManager.ShowPanel<BattleResultPanel>( result );
    }

    /// <summary>
    /// 显示旗舰技能目标的框
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayCheckCommanderShipSkillTarget() {
        GameObject shipGo = GetCommanderShipObj();
        if( shipGo == null )
            yield break;
        TeamDisplay commanderDisplay = GetCommanderShipTeam();
        if( commanderDisplay == null )
            yield break;
        ClientSkill skill = SelectedSkill_;
        if( skill == null )
            yield break;

        // 技能选择时的标志
        while( true ) {
            // 如果旗舰不存在了
            if( shipGo == null ) {
                yield break;
            }

            if( skill.Prop.skill_select_type == Def.SkillSelectType.CastScope || skill.Prop.skill_select_type == Def.SkillSelectType.NoSelection ) {
                List<TeamDisplay> list1 = SkillTargetSelector.FindSkillTarget( commanderDisplay, skill );
                // 矩形
                if( skill.Prop.shape_type == Def.ShapeType.Rectangle ) {
                    CheckLastFrameArrowGroupList( list1 );
                    CheckAllSkillTarget( list1 );
                }
                // 圆形或扇形
                else if( skill.Prop.shape_type == Def.ShapeType.Circle || skill.Prop.shape_type == Def.ShapeType.Default ) {
                    CheckLastFrameArrowGroupList( list1 );
                    CheckAllSkillTarget( list1 );
                }
            }
            else if( skill.Prop.skill_select_type == Def.SkillSelectType.PlayerSelect ) {
                // 为0时让玩家点击屏幕选择舰船或空处,显示可点击的舰船
                if( !IsSelectedSkillTarget ) {
                    List<TeamDisplay> list2 = SkillTargetSelector.FindSkillTarget( commanderDisplay, skill );
                    CheckLastFrameArrowGroupList( list2 );
                    CheckAllSkillTarget( list2 );
                }
                // 根据玩家点击的地方做显示
                else {
                    if( SelectedSkill.Prop.shape_type == Def.ShapeType.Circle ) {
                        // 知道玩家点击的位置了,需要根据这个位置得到周围的目标舰船
                        Vector3 targetPosition = (Vector3)SkillSelectParam_;
                        List<TeamDisplay> list3 = SkillTargetSelector.FindSkillTarget( targetPosition, skill );
                        CheckLastFrameArrowGroupList( list3 );
                        CheckAllSkillTarget( list3 );
                    }
                    else if( SelectedSkill.Prop.shape_type == Def.ShapeType.Default ) {
                        TeamDisplay targetDisplay = (TeamDisplay)SkillSelectParam_;
                        if( targetDisplay != null && !targetDisplay.IsDead ) {
                            List<TeamDisplay> list4 = new List<TeamDisplay>();
                            list4.Add( targetDisplay );
                            CheckLastFrameArrowGroupList( list4 );
                            CheckAllSkillTarget( list4 );
                        }
                        else {
                            // 如果目标船死亡了,那么玩家要重新指定一个目标
                            IsSelectedSkillTarget = false;
                        }                        
                    }
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 清除技能目标框列表
    /// </summary>
    private void ClearArrowGroupList() {
        for( int i=BattleArrowGroupList.Count - 1; i >= 0; i-- ) {
            if( BattleArrowGroupList[i] != null ) {
                Destroy( BattleArrowGroupList[i].gameObject );
            }
            BattleArrowGroupList.RemoveAt( i );
        }
        BattleArrowGroupList.Clear();
    }    

    /// <summary>
    /// 把trans设置为目标,如果表内有空的,那么修改空的,否则新建一个
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="size"></param>
    private void SetArrowTarget( Transform trans, float size ) {
        // 先循环找无目标的箭头
        foreach( var iter in BattleArrowGroupList ) {
            if( iter == null ) continue;
            Transform parentTrans = iter.GetParent();
            if( parentTrans != null )
                continue;
            iter.ChangeParent( trans, size );
            return;
        }
        // 没有找到,那么新建一个
        BattleArrowGroupList.Add( BattleArrowGroup.InitArrowGroup( trans, size, true ) );
    }

    /// <summary>
    /// 判断所有舰船是否在技能范围
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="list"></param>
    private void CheckAllSkillTarget( List<TeamDisplay> list ) {
        for( int i=BattleArrowGroupList.Count - 1; i >= 0; i-- ) {
            if( BattleArrowGroupList[i] == null )
                BattleArrowGroupList.RemoveAt( i );
        }
        for( int i=0; i < list.Count; i++ ) {
            GameObject team = list[i].GetTeamGo();
            if( team == null ) continue;
            Transform targetTrans = team.transform;
            if( IsContainsArrow( targetTrans ) ) continue;
            ClientShip ship = list[i].GetShip();
            float vol = ship.Reference.vol;
            SetArrowTarget( targetTrans, vol );
        }
    }

    private bool IsContainsArrow( Transform trans ) {
        for( int i=0; i < BattleArrowGroupList.Count; i++ ) {
            if( BattleArrowGroupList[i].GetParent() == trans )
                return true;
        }
        return false;
    }

    private bool IsContains( List<TeamDisplay> list, Transform trans ) {
        for( int i=0; i < list.Count; i++ ) {
            if( list[i].GetTeamGo().transform == trans )
                return true;
        }
        return false;
    }

    /// <summary>
    /// 检查上一帧的目标框列表是否需要修改
    /// </summary>
    /// <param name="list"></param>
    private void CheckLastFrameArrowGroupList( List<TeamDisplay> list ) {
        foreach( var iter in BattleArrowGroupList ) {
            if( iter == null ) continue;
            Transform parentTrans = iter.GetParent();
            if( parentTrans == null ) continue;
            // 如果目标表内无此舰船
            if( !IsContains( list, parentTrans ) )
                iter.ChangeParent( null, 0 );
        }
    }

    private void HideShipAllInfo( Dictionary<int,TeamDisplay> list ) {
        foreach( var iter in list ) {
            if( iter.Value == null ) continue;
            iter.Value.HideAllInfo();
        }
    }

    private DeployBoundaryDisplayer BattleFieldBoundaryDisplayer_;
    private IEnumerator DrawBoundary() {
        yield return null;

        Vector3[] linePoints;
        Material lineMaterial;
        linePoints = DeployBoundaryDisplayer.CalculateLinePointList();
        lineMaterial = Resources.Load(DeployBoundaryDisplayer.MAT_BOUNDARY_BATTLE_FIELD) as Material;
        BattleFieldBoundaryDisplayer_ = DeployBoundaryDisplayer.CreateInstance(transform, linePoints, lineMaterial);
        BattleFieldBoundaryDisplayer_.gameObject.name = "__BattleFieldBoundaryDisplayer__";
        EventOnEnemyCommanderShipDead += delegate { DeployBoundaryDisplayer.EnableVectorCanvas(false); };
    }
    private void InitGrid() {
        if (GridRenderCtrl_ == null)
            GridRenderCtrl_ = CameraManager.Instance.MainCamera.AddComponent<GFGridRenderCamera>();

        GameObject drawerGo = new GameObject("__BattleFieldGridDrawer__");
        BattleFieldGridDrawer_ = drawerGo.AddComponent<BattleFieldGridDrawer>();
        EventOnEnemyCommanderShipDead += delegate { BattleFieldGridDrawer_.EnableRenderGrid(false); };
    }
    #endregion

    #region interface

    public static void PlayerCommanderShipDead() {
        ResetSkill();
        EventOnPlayerCommanderShipDead();
    }

    public static void EnemyCommanderShipDead( GameObject go ) {
        // 通知相机动画
        CameraManager.Instance.EnemyCommanderShipDead( go );
        // 隐藏所有船的信息
        Instance.HideShipAllInfo( Instance.GetTeamDisplayList( true ) );
        Instance.HideShipAllInfo( Instance.GetTeamDisplayList( false ) );
        // 隐藏选择,范围显示
        Instance.ResetSelect();
        Instance.ResetRange();
        // 通知关闭技能面板
        EventOnPlayerCommanderShipDead();
        // 通知关闭线框绘制
        EventOnEnemyCommanderShipDead();
    }

    public static void ResetSkill() {
        SelectedSkill = null;
        SelectedSkillIndex = -1;
        IsSelectedSkillTarget = false;
        if( Instance.SkillRangeCircle_ != null )
            Instance.SkillRangeCircle_.SetActive( false );
        // 停止之前的技能目标锁定
        Instance.StopCoroutine( "DelayCheckCommanderShipSkillTarget" );
        // 清除所有的目标框
        Instance.ClearArrowGroupList();
    }

    /// <summary>
    /// 选择技能释放完毕
    /// </summary>
    public static void CastSkillComplete() {
        CameraManager.Instance.SetCameraType( CameraManager.CameraStatusType.Free );
        ResetSkill();
    }

    public static void SelectSkillTarget( Gesture gesture, float cameraPositionY ) {
        if( SelectedSkill == null ) return;
        // 只有圆形范围攻击才可点击空处
        if( SelectedSkill.Prop.shape_type == Def.ShapeType.Circle ) {
            Vector3 position = Vector3.zero;
            if( gesture.pickedObject == null ) {
                position = gesture.GetTouchToWorldPoint( cameraPositionY );
                position.y = 0;
            }
            else
                position = gesture.pickedObject.transform.position;
            CheckClickSkillTargetPosition( position );
        }
        // 只有默认才可点击单一船舰
        else if( SelectedSkill.Prop.shape_type == Def.ShapeType.Default ) {
            CheckClickSkillTarget( gesture.pickedObject );
        }
    }

    /// <summary>
    /// 检查旗舰的范围显示是否需要重置
    /// </summary>
    public static void CheckCommanderAttackRange() {
        TeamDisplay team = GetCommanderShipTeam();
        if( team == null ) return;
        team.CheckSkillRange();
    }

    /// <summary>
    /// 显示旗舰技能范围
    /// </summary>
    public static void ShowCommanderShipSkillRange() {
        TeamDisplay team = GetCommanderShipTeam();
        if( team == null ) return;
        team.ShowRange( false );
        team.ShowSkillRange( true, SelectedSkillIndex );
        Instance.CheckCommanderShipSkillTarget();
    }

    public static TeamDisplay GetCommanderShipTeam() {
        GameObject ship = GetCommanderShipObj();
        if( ship == null ) return null;
        ShipDisplay display = ship.GetComponent<ShipDisplay>();
        if( display == null ) return null;
        return display.GetTeam();
    }

    /// <summary>
    /// 获得旗舰的gameobject
    /// </summary>
    /// <returns></returns>
    public static GameObject GetCommanderShipObj() {
        if( Instance == null ) return null;
        foreach( var iter in Instance.PlayerTeamDisplayList ) {
            if( iter.Value == null )
                continue;
            if( !iter.Value.IsCommanderShip() )
                continue;
            ShipDisplay display = iter.Value.GetHurtShip();
            if( display == null )
                continue;
            return display.gameObject;
        }
        return null;
    }

    public void CheckFight() {
        Reset();
        StartCoroutine( Fight() );
    }

    /// <summary>
    /// 通过bool得到对应队伍列表
    /// </summary>
    /// <param name="bPlayer">是否玩家的</param>
    /// <returns></returns>
    public Dictionary<int, TeamDisplay> GetTeamDisplayList( bool bPlayer ) {
        return bPlayer ? PlayerTeamDisplayList : EnemyTeamDisplayList;
    }

    /// <summary>
    /// 通过transform得到所属的队伍列表
    /// </summary>
    /// <param name="trans"></param>
    /// <returns></returns>
    public Dictionary<int, TeamDisplay> GetTeamDisplayList( Transform trans ) {
        return (trans.name.Contains( "PlayerShip" ) ? PlayerTeamDisplayList : EnemyTeamDisplayList);
    }

    /// <summary>
    /// 通过transform得到所属队伍
    /// </summary>
    /// <param name="trans"></param>
    /// <returns></returns>
    public TeamDisplay GetTeamDisplay( Transform trans ) {
        Dictionary<int,TeamDisplay> teamList = GetTeamDisplayList( trans );
        foreach( var iter in teamList ) {
            if( iter.Value == null ) continue;
            List<ShipDisplay> shipList = iter.Value.GetShipList();
            for( int i=0; i < shipList.Count; i++ ) {
                if( shipList[i] == null ) continue;
                if( shipList[i].transform != trans ) continue;
                return iter.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// 通过id得到所属的队伍列表
    /// </summary>
    /// <param name="fightID"></param>
    /// <returns></returns>
    public Dictionary<int, TeamDisplay> GetTeamDisplayList( int fightID ) {
        if( PlayerTeamDisplayList.ContainsKey( fightID ) )
            return PlayerTeamDisplayList;
        else if( EnemyTeamDisplayList.ContainsKey( fightID ) )
            return EnemyTeamDisplayList;
        return null;
    }

    /// <summary>
    /// 通过id得到所属队伍
    /// </summary>
    /// <param name="fightID"></param>
    /// <returns></returns>
    public TeamDisplay GetTeamDisplay( int fightID ) {
        Dictionary<int, TeamDisplay> list = GetTeamDisplayList( fightID );
        TeamDisplay result = null;
        list.TryGetValue( fightID, out result );
        return result;
    }

    /// <summary>
    /// 重置数据
    /// </summary>
    public void Reset() {
        FightService.Instance.GiveupFight();
        StopAllCoroutines();
        foreach( var iter in UnitDisplayList ) {
            if( iter != null )
                Destroy( iter );
        }
        foreach( var iter in PlayerTeamDisplayList ) {
            if( iter.Value != null )
                iter.Value.Destroy();
        }
        foreach( var iter in EnemyTeamDisplayList ) {
            if( iter.Value != null )
                iter.Value.Destroy();
        }
        if( SkillRangeCircle_ != null )
            Destroy( SkillRangeCircle_ );

        UnitDisplayList.Clear();
        PlayerTeamDisplayList.Clear();
        EnemyTeamDisplayList.Clear();

        if( UserStatusPanel.Instance != null )
            UserStatusPanel.Instance.ShowTime( 0 );
    }

    public void CheckCommanderShipSkillTarget() {
        // 停止之前的技能目标锁定
        StopCoroutine( "DelayCheckCommanderShipSkillTarget" );
        // 清除所有的目标框
        ClearArrowGroupList();
        TeamDisplay display =  GetCommanderShipTeam();
        if( display == null )
            return;
        ClientSkill skill = SelectedSkill;
        if( skill == null )
            return;
        // 开始新的技能目标锁定
        StartCoroutine( "DelayCheckCommanderShipSkillTarget" );
    }
    #endregion
}