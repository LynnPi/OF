using UnityEngine;
using System.Collections;
using System;
public class SnappingUnits : MonoBehaviour {
    public static Material IntersectMaterial;
    public static Material CanPlacedMaterial;
    public static bool InvalidClickDeploy;

    public Action RebackCursorCallback = delegate { };
    public float ShipWrapRadius = 0f;//舰船单位的封套半径
    public string ShipIocnName;
    public DeployUnitDisplayHelper ArrowGroup;

    private GFGrid Grid_;
    private Transform CachedTransform_; 
    private Collider GridCollider_;
    private Renderer[] Renderers_;
    private Material DefaultMaterial_;

    private Material CurrentMaterial_;
    private Vector3 LastTouchDownPosition_;
    private Vector3 LastValidPosition_;
    private bool FollowIconShow_;
    public enum DeployState {
        OnPrepare     = 0,    //准备布阵
        OnSnapping    = 1,    //正在布阵中
        OnPlaced      = 2,    //完成布阵
    }

    private DeployState State_;
    public DeployState State {
        get {
            return State_;
        }
        set {
            State_ = value;
            Debugger.Log("Set State: " + State_.ToString());
            RefreshViewByState();
        }
    }

    private int Intersecting_ = 0;
    /// <summary>
    /// 与之产生交叉的对象的个数
    /// </summary>
    public int Intersecting {
        get {
            return Intersecting_;
        }
        set {
            Intersecting_ = value;

            if (Intersecting_ > 0) {//有交叉（1个或多个）     
                SetMaterial(IntersectMaterial);
                DeployUnitDisplayHelper.Current.BottomPlane.SetMaterial(true);
            }
            else if (Intersecting_ == 0) {//没有交叉
                SetMaterial(CanPlacedMaterial);
                DeployUnitDisplayHelper.Current.BottomPlane.SetMaterial(false);
            }
            else {
                Debugger.LogError("Intersecting Count Error! Count is : " + Intersecting_);
            }
        }
    }

    private DeployUnitFollowIcon FollowIcon_;
    public DeployUnitFollowIcon FollowIcon {
        get {
            if(FollowIcon_ == null){
                GameObject ui = Global.CreateUI("DeployFollowIcon");
                ui.transform.SetParent(UIManager.SceneUICanvas.transform);
                ui.transform.localScale = Vector3.one;

                FollowIcon_ = ui.AddComponent<DeployUnitFollowIcon>();
                FollowIcon_.RootTrans = transform;
                FollowIcon_.SetIcon(ShipIocnName);
                FollowIcon_.Snapping_ = this;
            }
            return FollowIcon_;
        }
        set {
            FollowIcon_ = value;
        }
    }

    public GFGrid Grid {
        get { return Grid_;}
        set {
            Grid_ = value;
            if( Grid_ ) {
                GridCollider_ = Grid_.gameObject.GetComponent<Collider>();
                if( GridCollider_ ) {
                    //perform an initial align and snap the objects to the bottom
                    Grid_.AlignTransform( CachedTransform_ );
                    CachedTransform_.position = DeployUnitPositionCorrector.CalculateOffsetY(Grid_, CachedTransform_);
                }
            }
        }
    }

    private BoxCollider Collider_;
    public BoxCollider Collider {
        get {
            if (Collider_ == null) {
                Collider_ = GetComponent<BoxCollider>();
            }
            return Collider_;
        }
    }

	private void DragObject(){
        if( !Grid_ || !GridCollider_ ) return;

#if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBPLAYER
         if (Input.touchCount > 1) {
            Debugger.Log("<color=red>more than one finger touched, do nothing! count: </color>" + Input.touchCount);
            return;
        } 
#endif

        Vector3 cursorWorldPoint = ShootRay();
        
		CachedTransform_.position = cursorWorldPoint;
		Grid_.AlignTransform(CachedTransform_);
        CachedTransform_.position = DeployUnitPositionCorrector.CalculateOffsetY(Grid_, CachedTransform_);
        //DeployUnitPositionCorrector.CorrectOffsetXZ(Grid_, CachedTransform_, ShipWrapRadius);

        DeployUnitMenu.Instance.SetFollowTarget(transform);
        if (!FollowIconShow_) //仅在第一次显示跟随图标
            FollowIcon.Follow();
	}

    private Vector3 ShootRay() {
		RaycastHit hit;
		GridCollider_.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity);

		return hit.collider != null ? hit.point : CachedTransform_.position;
	}

	private void SetupRigidbody(){
		Rigidbody rb = GetComponent<Rigidbody>();
		if(!rb) 
			rb = gameObject.AddComponent<Rigidbody>();
		rb.isKinematic=false;
		rb.useGravity=false;
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}

	private void ConstructTrigger(){
        GameObject go = new GameObject("IntersectionTrigger");
		go.transform.parent = transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
		go.transform.localRotation = Quaternion.identity;

        BoxCollider blockCol = GetComponent<BoxCollider>();
        BoxCollider IntersectionCol = go.AddComponent<BoxCollider>();
        IntersectionCol.size = blockCol.size;
        IntersectionCol.center = blockCol.center;
        IntersectionCol.isTrigger = true;
      
		IntersectionTrigger script = go.AddComponent<IntersectionTrigger>();
		script.SetSnappingScript(this);
	}

    #region 布阵模型交叉处理

    public void SetMaterial(Material mat) {
        //Debugger.Log("SetMaterial()..." + mat.name);
        if (CurrentMaterial_ == mat) return;
        foreach (var item in Renderers_) {
            if (item) item.material = mat;
        }
    }

    /// <summary>
    /// 同步交叉对象数量
    /// </summary>
    /// <param name="intersecting"></param>
    public void SyncIntersectAmount(bool intersecting) {
        if (State != DeployState.OnSnapping) return;
        Intersecting = intersecting ? Intersecting_ + 1 : Intersecting_ - 1;
    }

    #endregion
    
    #region 玩家输入
    IEnumerator OnMouseDown() {
        yield return new WaitForEndOfFrame();

        if (DeployUnitMenu.Instance.IsClicked()) yield break;

        LastTouchDownPosition_ = Input.mousePosition;
   
        //检查有没有点选式布阵的Focus单元.如果有，那么取消其Focus状态，不允许点选式布阵
        if (DeployUnitUIElement.CurrentFocus != null) {
            DeployUnitUIElement.CurrentFocus.FocusState = false;
        }

        if (State == DeployState.OnPrepare) {
            //准备状态下，鼠标按下应该锁住摄像机，禁止移动
            CameraManager.SetTouchActive(false);

            //记录拖动之前的有效位置
            LastValidPosition_ = CachedTransform_.position;

            //当处于准备状态，点击操作被认为是拖拽的开始
            State = DeployState.OnSnapping;
        }
    }

    IEnumerator OnMouseUp() {
        yield return new WaitForEndOfFrame();

        if (DeployUnitMenu.Instance.IsClicked()) yield break;

        CameraManager.SetTouchActive(true);

        DeployUnitDisplayHelper.SetCurrent(ArrowGroup, transform, ShipWrapRadius);

        if (DeployUnitMenu.Instance.CurrentFollowTarget != transform) {
            DeployUnitMenu.Instance.CurrentFollowTarget = transform;
        }

        if (State == DeployState.OnPlaced) {
            bool condition;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
            condition = Input.mousePosition == LastTouchDownPosition_;
#else
            float delta_time = Input.GetTouch(0).deltaTime;
            Debugger.Log("touch delta_time :" + delta_time);
            condition = Input.GetTouch(0).deltaPosition.sqrMagnitude < 100f && Input.GetTouch(0).deltaTime < 0.5f;
#endif
            if (!condition) yield break;

            //当处于“布阵完成”时，且是点击操作（即按下与弹起是同一点）鼠标弹起被认为确认进入“准备布阵”状态
            State = DeployState.OnPrepare;                                      
        }
        else if (State == DeployState.OnSnapping) {
            //当处于“正在布阵”时，鼠标弹起被认为确认进入“完成布阵”状态
            State = DeployState.OnPlaced;

            //此时应当检查是否有交叉，如有，则返回上一个有效位置
            if (Intersecting_ > 0) {
                CachedTransform_.position = LastValidPosition_;
                Intersecting_ = 0;
            }
        }
    }
    
    /// <summary>
    /// 拖拽布阵方式下的初始化逻辑
    /// </summary>
    public void OnDragFromUI() {
        Debugger.Log("OnDragFromUI()...");
        //一旦拖出来，就打开拖拽开关，开始不断更新坐标
        //beingDragged = true;
        //记录当前拖拽的对象
        DeployDragHandeler.ItemBeingDragged = gameObject;
        //更新布阵状态为：正在布阵中
        State = DeployState.OnSnapping;
        Debugger.Log("OnDragFromUI(), set state deploying");
    }

    private void CancelThisDeploy() {
        if(FollowIcon_){
            Destroy(FollowIcon_.gameObject);
            FollowIcon_ = null;
        }

        Destroy(gameObject);
        DeployUnitDisplayHelper.Current = null;
        if (DeployUnitUIElement.CurrentFocus) {
            DeployUnitUIElement.CurrentFocus.FocusState = false;
            DeployUnitUIElement.CurrentFocus = null;
        }
        SampleNotePopWindow.Instance.ShowMessage(1001);
    }

    /// <summary>
    /// 拖拽布阵方式下的拖拽结束时逻辑
    /// </summary>
    public bool OnEndDragFromUI() {
        bool deploySucceed = false;
        //检查是否存在交叉
        if (Intersecting_ > 0) {
            //当存在交叉时，取消这次非法布阵
            CancelThisDeploy();  
        }
        else {
            //不存在交叉时
            deploySucceed = true;
            Destroy(FollowIcon_.gameObject);
            FollowIcon_ = null;
            FollowIconShow_ = true;
        }
        State = DeployState.OnPlaced;
        return deploySucceed;
    }

    /// <summary>
    /// 点击布阵方式下的初始化逻辑
    /// </summary>
    /// <param name="pickedPos"></param>
    public void OnClickFromUI(Vector3 pickedPos, System.Action invalidCallback) {
        StartCoroutine(SyncClickFromUI(pickedPos, invalidCallback));
    }

    private IEnumerator SyncClickFromUI(Vector3 pickedPos, System.Action InvalidCallback) {
        CachedTransform_.position = pickedPos;
        //更正位置
        CachedTransform_.position = DeployUnitPositionCorrector.CalculateOffsetY(Grid_, CachedTransform_);
        //DeployUnitPositionCorrector.CorrectOffsetXZ(Grid_, CachedTransform_, ShipWrapRadius);
        yield return new WaitForSeconds(0.05f);

        if (InvalidClickDeploy) {
            //点选布阵时有交叉
            InvalidClickDeploy = false;
            //Debugger.Log("<color=yellow>Click Deploy Exist Intersection!Cancel Deploy!</color>");
            CancelThisDeploy();           
        }
        else {
            //更新布阵状态为：完成布阵
            State = DeployState.OnPlaced;
            InvalidCallback();
        }
    }
    #endregion

    #region 显示相关
    /// <summary>
    /// 根据状态刷新布阵时的表现，包括箭头和底板
    /// </summary>
    private void RefreshViewByState() {
        if (State_ == DeployState.OnPrepare) {          
            //准备布阵状态：箭头->显示；底板->显示；材质->默认；菜单->显示
            DeployUnitMenu.Instance.IsShowing = true;
            DeployUnitDisplayHelper.Current.AnimStatus = DeployUnitDisplayHelper.Status.Idle;
            DeployUnitDisplayHelper.Current.BottomPlane.SetActive(false);
            
            SetMaterial(DefaultMaterial_);
            DeploySceneManager.Instance.ValidDeployAreaGuider.ShowGuideAnimation(false);  
        }
        else if (State == DeployState.OnSnapping) {
            //正在布阵状态：箭头->隐藏；底板->显示；材质->特殊（绿色or红色）  
            DeployUnitMenu.Instance.IsShowing = false;
            DeployUnitDisplayHelper.Current.AnimStatus = DeployUnitDisplayHelper.Status.Close;
            DeployUnitDisplayHelper.Current.BottomPlane.SetActive(true);
            DeployUnitDisplayHelper.Current.BottomPlane.SetMaterial(Intersecting_ > 0);
            SetMaterial(CanPlacedMaterial);
            DeploySceneManager.Instance.ValidDeployAreaGuider.ShowGuideAnimation(true);         
        }
        else {
            //完成布阵状态：箭头->隐藏；底板->隐藏；材质->默认
            if (DeployUnitDisplayHelper.Current) {//无效布阵时，不存在
                DeployUnitDisplayHelper.Current.AnimStatus = DeployUnitDisplayHelper.Status.Close;            
                DeployUnitDisplayHelper.Current.BottomPlane.SetActive(false);
            }
            DeployUnitMenu.Instance.IsShowing = false;
            SetMaterial(DefaultMaterial_);
            DeploySceneManager.Instance.ValidDeployAreaGuider.ShowGuideAnimation(false);


            bool isCursorRelocked = FollowIcon_ != null;//检测是否重新回去鼠标光标（从其他应用程序切回来）
            if (isCursorRelocked) {
                RebackCursorCallback();
            }

        }
    }
    #endregion
 
    private void OnTouchBlankSpace() {
        if (Input.GetMouseButtonDown(0) && DeployUnitDisplayHelper.Current == ArrowGroup) {
            RaycastHit hit;
            Collider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);

            if (hit.collider != null) return;

            if (DeployUnitMenu.Instance.IsClicked()) return;

            ArrowGroup.AnimStatus = DeployUnitDisplayHelper.Status.Close;//点击屏幕其他区域，取消箭头idle状态
            State = DeployState.OnPlaced;
        }
    }

    void Awake() {
        CachedTransform_ = transform;
        if (Grid_) {
            GridCollider_ = Grid_.gameObject.GetComponent<Collider>();
            if (GridCollider_) {
                //perform an initial align and snap the objects to the bottom
                Grid_.AlignTransform(CachedTransform_);
                CachedTransform_.position = DeployUnitPositionCorrector.CalculateOffsetY(Grid_, CachedTransform_);
            }
        }
        SetupRigidbody();
        ConstructTrigger();

        Renderers_ = GetComponentsInChildren<Renderer>();
        DefaultMaterial_ = Renderers_[0].material;
    }

    void Update() {
        OnTouchBlankSpace();
    }

    void FixedUpdate() {
        if (State != DeployState.OnSnapping) return;
        if (Input.mousePosition == LastTouchDownPosition_) return;//当没产生手指滑动时，不拖拽！

        DragObject();
    }

    void OnDestroy() {
        if (FollowIcon_) {
            Destroy(FollowIcon_.gameObject);
        }
    }
}
