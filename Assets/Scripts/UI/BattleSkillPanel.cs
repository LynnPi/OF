using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class BattleSkillPanel : UIPanelBehaviour {
    public static BattleSkillPanel Instance { get; private set; }

    private ClientShip CommandShip_;

    private bool CanClick_ = false;

    protected override void OnAwake() {
        Instance = this;
    }

    protected override void OnRegEvent() {
        CameraManager.EventOnClickBlankScreenZone += CameraManager_EventOnClickBlankScreenZone;
        CameraManager.EventOnClickShip += CameraManager_EventOnClickShip;
    }

    protected override void OnUnregEvent() {
        CameraManager.EventOnClickBlankScreenZone -= CameraManager_EventOnClickBlankScreenZone;
        CameraManager.EventOnClickShip -= CameraManager_EventOnClickShip;
    }

    private void CameraManager_EventOnClickBlankScreenZone() {
        FoldSkillPanel();
    }

    private void CameraManager_EventOnClickShip( GameObject obj ) {
        // 如果点击的是玩家的船且不是旗舰,那么需要隐藏技能面板
        if( obj.name.Contains( "PlayerShip" ) && !obj.name.Contains( "Commander" ) ) {
            FoldSkillPanel();
        }
    }

    public void FoldSkillPanel() {
        TeamDisplay display = BattleSceneDisplayManager.GetCommanderShipTeam();
        if( display == null ) return;
        PanelAnim_.SetTrigger("skill_panel_hide" );
        BattleSceneDisplayManager.ResetSkill();
    }

    protected override void OnShow( params object[] args ) {
        Init();
    }

    protected override void OnClose() {
        Reset();
    }

    private void CloseSkillMenu() {
        BattleSceneDisplayManager.SelectedSkillIndex = -1;
        BattleSceneDisplayManager.SelectedSkill = null;
    }

    private void ShowSelecteTargetNotice() {
        ClientSkill skill = BattleSceneDisplayManager.SelectedSkill;
        if( skill == null ) return;
        // 如果是默认,提示玩家选择一个目标舰船
        if( skill.Prop.shape_type == Def.ShapeType.Default )
            SampleNotePopWindow.Instance.ShowMessage( 1006 );
        // 如果是范围攻击,提示玩家选择一个目标点
        else if( skill.Prop.shape_type == Def.ShapeType.Circle )
            SampleNotePopWindow.Instance.ShowMessage( 1007 );
    }

    /// <summary>
    /// 初始化各种状态数据
    /// </summary>
    private void Init() {
        //取旗舰技能
        CommandShip_ = BattleSys.GetCommanderShip();

        if( CommandShip_ != null ) {
            InitSkillPanel();
            InitCtrlBtn();
            InitSkillBtnList();
            InitCancelBtn();
        }
        else { }
        CanClick_ = true;
    }

    /// <summary>
    /// 技能控制按钮
    /// </summary>
    private BattleSkillPanelCtrlButton CtrlBtn_;
    /// <summary>
    /// 初始化技能面板控制按钮（控制技能列表的展开和关闭的按钮）
    /// </summary>
    private void InitCtrlBtn() {
        CtrlBtnRoot_ = transform.FindChild( "Root/Entrance" );
        GameObject go = Global.CreateUI( "BattleSkillEntrance", CtrlBtnRoot_.gameObject );
        CtrlBtn_ = go.AddComponent<BattleSkillPanelCtrlButton>();

        CtrlBtn_.EnableEntrance( true );//现在默认打开就是激活技能的，技能激活的条件？？？

        CtrlBtn_.ClickEntranceCallback += delegate {
            if( !CanClick_ ) return;
            PanelAnim_.SetTrigger("skill_pamel_show");
            SetClickSkillCtrl();
        };
    }

    /// <summary>
    /// 技能按钮列表
    /// </summary>
    private List<BattleSkillButton> SkillList_;
    private Transform SkillListRoot_;
    private Transform CtrlBtnRoot_;
    private Animator PanelAnim_;
    private int SkillBtnCount = 3;
    private void InitSkillBtnList() {
        SkillList_ = new List<BattleSkillButton>();
        SkillListRoot_ = transform.FindChild( "Root/SkillList/Content" );
        for( int i = 0; i < SkillBtnCount; i++ ) {
            GameObject go = Global.CreateUI( "BattleSkillButton", SkillListRoot_.gameObject );
            go.name = i.ToString();
            BattleSkillButton script = go.AddComponent<BattleSkillButton>();
            SkillList_.Add( script );
            ClientSkill skill = CommandShip_.GetSkillByIndex(i);
            if (skill == null) {//为空的时候理解为未解锁，现在没这个状态，以后再加
                //进入未解锁状态
            }
            else {
                script.SkillID = skill.ID;
                bool isAble = true;//是否激活，待取
                script.ConfigIcon(skill.Prop.disableicon, skill.Prop.enableicon);
                script.EnableSkill(isAble);
                BattleSceneDisplayManager.EventOnSkillSing += script.ForbidFireSkill;
                BattleSceneDisplayManager.EventOnSkillTrigger += script.CoolDownSkill;
            }
            
            script.SelectedSkillCallback += SetSelectSkill;
            script.FireSkillCallback += FireSkill;
        }
    }
    private void OnClickCancel() {
        PanelAnim_.SetTrigger("skill_panel_hide");
        BattleSceneDisplayManager.CastSkillComplete();
        BattleSceneDisplayManager.CheckCommanderAttackRange();
        for( int i = 0; i < SkillList_.Count; i++ ) {            
            //如果处于冷却状态，不应该被打断
            if( !SkillList_[i].IsCooling ) {
                SkillList_[i].EnableSkill( true );
            }
        }
    }

    private void InitCancelBtn() {
        Button btn = transform.FindChild("Root/SkillList/Button_Cancel").GetComponent<Button>();
        btn.onClick.AddListener(OnClickCancel);
    }
    /// <summary>
    /// 重置各种状态数据
    /// </summary>
    private void Reset() {
        //清除旗舰技能引用，因为下次进入该面板，有无都有可能
        CommandShip_ = null;
        //清除技能控制按钮
        if( CtrlBtn_ != null )
            Destroy( CtrlBtn_.gameObject );
        CtrlBtn_ = null;

        //清除技能按钮列表
        if( SkillList_ != null ) {
            for( int i = SkillBtnCount - 1; i >= 0; i-- ) {
                BattleSceneDisplayManager.EventOnSkillSing -= SkillList_[i].ForbidFireSkill;
                BattleSceneDisplayManager.EventOnSkillTrigger -= SkillList_[i].CoolDownSkill;
                Destroy( SkillList_[i].gameObject );
            }
        }
        SkillList_ = null;
        SkillListRoot_ = null;
        CtrlBtnRoot_ = null;
        BattleSceneDisplayManager.EventOnPlayerCommanderShipDead -= OnCommandShipDie;
    }

    /// <summary>
    /// 初始化整个大面板
    /// </summary>
    private void InitSkillPanel() {
        if (PanelAnim_ == null) 
            PanelAnim_ = GetComponent<Animator>();
        BattleSceneDisplayManager.EventOnPlayerCommanderShipDead += OnCommandShipDie;
    }

    /// <summary>
    /// 旗舰死亡时，销毁技能面板，播放相应动画
    /// </summary>
    private void OnCommandShipDie() {
        Debugger.Log( "OnCommandShipDie()..." );
        //播放销毁动画

        if(PanelAnim_.GetCurrentAnimatorStateInfo(0).IsName("SkillHiding")){
            PanelAnim_.SetTrigger("destroy_on_ctrl_show");
        }
        else if (PanelAnim_.GetCurrentAnimatorStateInfo(0).IsName("SkillShowing")) {
            PanelAnim_.SetTrigger("destroy_on_skill_show");
        }
        else if (PanelAnim_.GetCurrentAnimatorStateInfo(0).IsName("Normal")) {
            PanelAnim_.SetTrigger("destroy_on_ctrl_show");
        }
    }

    /// <summary>
    /// 点击技能面板旗舰按钮
    /// </summary>
    void SetClickSkillCtrl() {
        // 镜头锁定到旗舰
        GameObject commanderShipObj = BattleSceneDisplayManager.GetCommanderShipObj();
        if( commanderShipObj == null )
            return;        
        for( int i = 0; i < SkillList_.Count; i++ ) {
            //如果处于冷却状态，不应该被打断
            if( !SkillList_[i].IsCooling ) {
                SkillList_[i].EnableSkill( true );
            }
        }
        CameraManager.ClickGameObjectInBattle( commanderShipObj );
    }

    /// <summary>
    /// 选中对应index的技能, by lxl
    /// </summary>
    /// <param name="index"></param>
    void SetSelectSkill( int index ) {
        if( index != BattleSceneDisplayManager.SelectedSkillIndex ) {
            for( int i = 0; i < SkillList_.Count; i++ ) {
                if( i != index ) {
                    //如果处于冷却状态，不应该被打断
                    if( !SkillList_[i].IsCooling ) {
                        SkillList_[i].EnableSkill( true );
                    }
                }
            }
        }

        // 一般技能不需要选择目标
        BattleSceneDisplayManager.IsSelectedSkillTarget = true;
        // 缓存技能index
        BattleSceneDisplayManager.SelectedSkillIndex = index;
        // 缓存选中技能
        BattleSceneDisplayManager.SelectedSkill = CommandShip_.GetSkillByIndex( index );
        ClientSkill skill = BattleSceneDisplayManager.SelectedSkill;
        // 如果是需要玩家选择目标的技能
        if( skill.Prop.skill_select_type == Def.SkillSelectType.PlayerSelect ) {
            // 需要选择目标
            BattleSceneDisplayManager.IsSelectedSkillTarget = false;
            // 提示玩家选择目标
            ShowSelecteTargetNotice();
        }
        // 相机设置为技能选择模式
        CameraManager.Instance.SetCameraType( CameraManager.CameraStatusType.SkillSelect );
        // 开始显示技能范围和目标选择
        BattleSceneDisplayManager.ShowCommanderShipSkillRange();
    }

    /// <summary>
    /// 释放当前选中的技能, by lxl
    /// </summary>
    private void FireSkill( int currentClickIndex ) {
        // 旗舰
        ClientShip commandShip = BattleSys.GetCommanderShip();
        if( commandShip == null ) return;
        bool isSelectedSkillTarget = BattleSceneDisplayManager.IsSelectedSkillTarget;        
        // 玩家还没选择目标,给提示
        if( !isSelectedSkillTarget ) {
            // 提示玩家选择目标
            ShowSelecteTargetNotice();
            return;
        }
        // 得到选中的技能
        ClientSkill skill = BattleSceneDisplayManager.SelectedSkill;
        Vector3 pos = Vector3.zero;
        int fightID = -1;
        // 如果是玩家选择的技能
        if( skill.Prop.skill_select_type == Def.SkillSelectType.PlayerSelect ) {
            // 得到玩家选择的位置或目标
            if( skill.Prop.shape_type == Def.ShapeType.Default ) {
                TeamDisplay display = (TeamDisplay)BattleSceneDisplayManager.Instance.SkillSelectParam;
                fightID = display.GetShip().InFightID;
            }
            else if( skill.Prop.shape_type == Def.ShapeType.Circle ) {
                pos = (Vector3)BattleSceneDisplayManager.Instance.SkillSelectParam;
            }
        }
        // 使用技能
        FightService.Instance.UseCommanderSkill( commandShip.InFightID, skill.ID, fightID, pos );

        // 技能释放完毕后要重设选择状态
        BattleSceneDisplayManager.CastSkillComplete();
        BattleSceneDisplayManager.CheckCommanderAttackRange();
    }
}
