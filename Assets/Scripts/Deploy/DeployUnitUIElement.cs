using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using PalmPoineer.Mobile;
using UnityEngine.EventSystems;
public enum DeployMode { Drag, Click, None }

/// <summary>
/// 布阵单位的UI元素
/// </summary>
public class DeployUnitUIElement : UIPanelBehaviour {
    public Action<int> SyncEnergy = delegate{};
    public Queue<int> ReadyDeployIndexQueue = new Queue<int>();
    public bool IsDragging;
    public static DeployMode CurrentDeployMode;

    private List<int> HaveDeployedIndex = new List<int>();
    private SnappingUnits Snapping_;
    private DeployDragHandeler DragHandeler_;
    private Button Btn_;
    private Image FocusImg_;
    private Image ShipImg_;
    private Text UnitCountText_;
    private Text NameText_;
    private Text WrapPointText_;
    private Text LevelText_;
    private int UnitCount_;
    private const float COLLIDER_DELETA_SIZE = 0.2F;

    private static DeployUnitUIElement CurrentFocus_;
    public static DeployUnitUIElement CurrentFocus {
        get {
            return CurrentFocus_;
        }
        set {
            CurrentFocus_ = value;
            DeployDragHandeler.InDragMode = (CurrentFocus_ == null);
        }
    }

    private bool FocusState_;
    public bool FocusState {
        set {
            if (FocusState_.Equals(value)) return;
            FocusState_ = value;
            if (!FocusState_) {//该类型的船已用完，Focus为false,布阵模式为None
                if (ReadyDeployIndexQueue.Count == 0)
                    CurrentDeployMode = DeployMode.None;
                else {
                    CurrentDeployMode = DeployMode.Drag;
                }
            }
            else {
                CurrentDeployMode = DeployMode.Click;
            }

            CurrentFocus = FocusState_ ? this : null;
            FocusImg_.enabled = FocusState_;
        }
    }

    public int UnitCount {
        set {
            UnitCount_ = value;
            UnitCountText_.text = string.Format( "x{0}", UnitCount_ );
        }
    }

    private string ShipName_;
    public string ShipName {
        set {
            ShipName_ = value;
            NameText_.text = ShipName_;
        }
    }

    private string IconName_;
    public string IconName {
        set {
            IconName_ = value;
            Global.SetSprite(ShipImg_, IconName_, true);
            ShipImg_.SetNativeSize();
        }
    }

    private int WrapPointCount_;
    public int WrapPointCount {
        get {
            return WrapPointCount_;
        }
        set {
            WrapPointCount_ = value;
            WrapPointText_.text = WrapPointCount_.ToString();
        }
    }

    private int Level_;
    public int Level {
        set {
            Level_ = value;
            LevelText_.text = string.Format("Lv{0}", Level_);
        }
    }
  
    /// <summary>
    /// 当前上阵的预备舰船，取队首
    /// </summary>
    private ClientShip CurrShipToDeploy {
        get {
            if( ReadyDeployIndexQueue.Count > 0 ) {
                int index = ReadyDeployIndexQueue.Peek();
                //Debug.Log( string.Format( "<color=yellow>OnDeploy Ship with index : {0}</color>", index ) );
                return PlayerSys.GetPlayerShipList()[index];
            }
            else { return null; }
        }
    }
    
    #region 初始化相关
    protected override void OnAwake() {
        Init();
    }


    /// <summary>
    /// 激活点击式布阵
    /// </summary>
    private void EnableClickMode() {
        Btn_ = GetComponentInChildren<Button>();
        //Btn_.onClick.AddListener(OnFocusItem);
        //DeploySceneManager.GridClickCallback += DeployWithModeClick;
    }

    /// <summary>
    /// 激活拖拽式布阵
    /// </summary>
    private void EnableDragMode() {
        DragHandeler_ = transform.FindChild("Item").gameObject.AddComponent<DeployDragHandeler>();
        DragHandeler_.OnDragBegin += delegate { SetElementStateOnDrag(true); };
        DragHandeler_.OnDraging += SysnDragInput;
        DragHandeler_.OnReleaseFinger += OnDragEnd;
        DragHandeler_.OnHoverUI += CancelLastDeploy;
    }

    private void InitUI() {
        WrapPointText_ = transform.FindChild("Item/Text_WarpPoint").GetComponent<Text>();
        ShipImg_ = transform.FindChild("Item/Image_Ship").GetComponent<Image>();
        FocusImg_ = transform.FindChild("Item/Image_Focus").GetComponent<Image>();
        UnitCountText_ = transform.FindChild("Item/Text_UnitCount").GetComponent<Text>();
        NameText_ = transform.FindChild("Item/Text_Name").GetComponent<Text>();
        LevelText_ = transform.FindChild("Item/Text_lv").GetComponent<Text>();
    }
    private void Init() {
        InitUI();
        EnableClickMode();
        EnableDragMode();
    }

    #endregion

    #region 布阵逻辑相关
    /// <summary>
    /// 拖拽上阵主逻辑
    /// </summary>
    private IEnumerator OnDeploying( Vector3 pos ) {
        ClientShip deployingTarget = CurrShipToDeploy;

        if( deployingTarget == null ) {
            Debugger.LogError( "ClientShip Data is null!" );
            yield break;
        }

        Ship ship = AssetLoader.GetShipDefine( deployingTarget.Reference.model_res );

        if( ship == null ) {
            Debugger.LogError( string.Format( "can't create ship, id : {0}, modelRes : {1}", deployingTarget.Reference.id, deployingTarget.Reference.model_res ) );
            yield break;
        }

        //保证缓存中有资源
        yield return StartCoroutine(AssetLoader.PreloadShipModel(ship));

        float shipWrapRadius = (float)deployingTarget.Reference.vol;

        GameObject group = CreateShipGroup(deployingTarget, ship.ModelPath, shipWrapRadius);

        EquipColliderForSnapping(group, shipWrapRadius);
        EquipSnappingComponent(group, shipWrapRadius);
        SyncUnitStateByDeployMode(pos);  
    }

    private void SyncUnitStateByDeployMode(Vector3 pos) {
        if (CurrentDeployMode == DeployMode.Drag) {
            Snapping_.OnDragFromUI();
        }
        else {
            Snapping_.OnClickFromUI(pos, RefreshDeployStatus);
        }
    }

    private GameObject CreateShipGroup(ClientShip targetShip, string targetShipResPath, float shipWrapRadius) {
        GameObject group = new GameObject();
        group.transform.SetParent(DeploySceneManager.Instance.PlayerGridDrawer.GridInstance.transform);

        //约定组名为目标舰船在数据索引值，记录之，方便传给战斗布阵单元的坐标
        group.name = PlayerSys.GetPlayerShipList().IndexOf(targetShip).ToString();
        // 初始化阵形
        BattleSys.SetFormation( true, targetShip );
        //成组上阵，组中舰船的数量
        int groupMemberCount = targetShip.Reference.stack ? targetShip.Reference.stack_num : 1;

        BoxCollider tempCollider = null;
        for (int i = 0; i < groupMemberCount; i++) {
            Vector3 pos = targetShip.FormationList == null ? Vector3.zero : targetShip.FormationList[i];
            GameObject member = CreateShip(targetShipResPath, group.transform, GridDrawer.OffsetByWrapRadius(pos, shipWrapRadius));

            if (tempCollider == null) tempCollider = member.GetComponent<BoxCollider>();
            Destroy(member.GetComponent<BoxCollider>());
            DestoryTrailEffect(member);
        }

        //添加tag，用于点选式布阵的碰撞检测
        group.tag = "ShipModel";

        return group;
    }

    private void EquipColliderForSnapping(GameObject targetShip, float shipWrapRadius) {
        //添加碰撞盒，以供拖拽
        BoxCollider collider = targetShip.AddComponent<BoxCollider>();

        //布阵根据封套给定的碰撞盒大小，两个单位完全毗邻时会产生交叉现象，
        //这里考虑使用一个减值
        float size = 2 * shipWrapRadius - COLLIDER_DELETA_SIZE;
        collider.size = new Vector3(size, 1f, size);//布阵时Y值不能给大了，不然靠前的单位会挡住靠后的单位

        //奇数倍的封套半径情况下，碰撞盒会产生半格的偏移
        collider.center = new Vector3(
            shipWrapRadius % GridDrawer.GRID_SPACE_UNIT_SIZE, 0f, 
            shipWrapRadius % GridDrawer.GRID_SPACE_UNIT_SIZE);
    }

    private SnappingUnits EquipSnappingComponent(GameObject attachTarget, float shipWrapRadius) {
        //添加布阵格子组件，以供自动排布
        Snapping_ = attachTarget.AddComponent<SnappingUnits>();

        //每个可拖拽单元都受到同一个RectGrid的约束
        Snapping_.Grid = DeploySceneManager.Instance.PlayerGridDrawer.GridInstance;
        Snapping_.ShipWrapRadius = shipWrapRadius;
        Snapping_.ShipIocnName = IconName_;
        Snapping_.ArrowGroup = attachTarget.AddComponent<DeployUnitDisplayHelper>();
        DeployUnitDisplayHelper.SetCurrent(Snapping_.ArrowGroup, attachTarget.transform, shipWrapRadius);
        Snapping_.RebackCursorCallback += delegate { OnDragEnd(); };

        return Snapping_;
    }

    private GameObject CreateShip(string resPath, Transform parent, Vector3 pos) {
        GameObject go = AssetLoader.GetInstantiatedGameObject(resPath);
        AssetLoader.ReplaceShader(go);
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        return go;
    }
    /// <summary>
    /// 布阵删除拖尾效果
    /// </summary>
    /// <param name="go"></param>
    private void DestoryTrailEffect(GameObject go) {
        GameObject trail = go.transform.FindChild("fire").gameObject;
        if (trail) Destroy(trail);
    }

    private void RefreshDeployStatus() {
        //出队列
        if( ReadyDeployIndexQueue.Count > 0 ) {
            int index = ReadyDeployIndexQueue.Dequeue();
            //Debugger.Log( "Dequeue index : " + index );
            HaveDeployedIndex.Add(index);
            UnitCount = --UnitCount_;
            SyncEnergy(-WrapPointCount_);
            if( ReadyDeployIndexQueue.Count == 0 ) {
                FocusState = false;
                SetDragAbility(false);
                DeployUnitUIElement.CurrentFocus = null;
            }
        }
    }


    private void CancelDeploy(int index) {
        Debugger.Log("CancelDeploy()...index: " + index);
        if (HaveDeployedIndex.Contains(index)) {
            HaveDeployedIndex.Remove(index);
            ReadyDeployIndexQueue.Enqueue(index);
            UnitCount = ++UnitCount_;
            SetDragAbility(true);
            SyncEnergy(WrapPointCount_);
        }
       
    }
    #endregion

    #region 点选式布阵方式
    private void OnFocusItem() {
        if (this.Equals(DeployUnitUIElement.CurrentFocus)) {
            this.FocusState = false;
            DeployUnitUIElement.CurrentFocus = null;
        }
        else {
            if (DeployUnitUIElement.CurrentFocus)
                DeployUnitUIElement.CurrentFocus.FocusState = false;
            DeployUnitUIElement.CurrentFocus = this;
            this.FocusState = true; 
        }
    }
 
    private void DeployWithModeClick(Vector3 pos) {
        bool dontClickOnUI = EventSystem.current.currentSelectedGameObject == null;
        if (FocusState_ && dontClickOnUI) {
            //Debug.Log( "DeployWithModeClick(), item : " + gameObject.name );
            StartCoroutine(OnDeploying(pos));
            //RefreshDeployStatus();   
        }
    }
    #endregion

    #region 拖拽式布阵方式
    private void SysnDragInput() {
        Debugger.Log("SysnDragInput()...");
        if( ReadyDeployIndexQueue.Count > 0 ) {
            IsDragging = true;
            StartCoroutine( OnDeploying( Vector3.zero ) );
        }
    }
    /// <summary>
    /// 此次拖拽结束
    /// </summary>
    private void OnDragEnd() {
        Debugger.Log("OnDragEnd()");
        IsDragging = false;

        SetElementStateOnDrag(false);
        if (Snapping_ == null) {
            StopAllCoroutines();           
            return;//未拖拽到格子上的情况
            
        }
        bool deploySucceed = Snapping_.OnEndDragFromUI();
        Debugger.Log("deploySucceed : " + deploySucceed);
        if (deploySucceed) {
            RefreshDeployStatus();
            DeployUnitMenu.Instance.CancelClickCallback += CancelDeploy;
            Snapping_ = null;
        }
    }
    #endregion


    private void SetElementStateOnDrag(bool dragging) {
        FocusImg_.enabled = dragging;
    }

    private void CancelLastDeploy() {
        Debugger.Log("CancelLastDeploy()...");
        if (Snapping_ != null) {//第1次无效，第2次也无效时的容错 
            if (Snapping_.FollowIcon) {
                Destroy(Snapping_.FollowIcon.gameObject);
                Snapping_.FollowIcon = null;
            }

            Destroy(Snapping_.gameObject);
            Snapping_ = null;
            //DeployDragHandeler.InDragMode = false;
            IsDragging = false;
        }        
    }
    void OnDestroy() {
        DeploySceneManager.Instance.GridClickCallback -= DeployWithModeClick;
    }

    public void SetDragAbility(bool enable) {
        if (DragHandeler_.EnableDrag.Equals(enable)) return;

        Btn_.interactable = enable;
        DragHandeler_.EnableDrag = enable;
    }
}