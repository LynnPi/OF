using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// 布阵单位的操作菜单
/// </summary>
public class DeployUnitMenu : UIPanelBehaviour {
    /// <summary>
    /// 单位操作菜单
    /// </summary>
    private static DeployUnitMenu Instance_;
    public static DeployUnitMenu Instance {
        get {
            if (Instance_ == null) {
                GameObject ui = Global.CreateUI("DeployUnitMenu");
                ui.transform.SetParent(UIManager.SceneUICanvas.transform);
                ui.transform.localScale = Vector3.one;

                Instance_ = ui.AddComponent<DeployUnitMenu>();
            }
            return Instance_;
        }
    }


    /// <summary>
    /// 跟随目标
    /// </summary>
    public Transform CurrentFollowTarget;

    private Transform ContentRoot_;

    private Transform AnimRoot_;

    /// <summary>
    /// 取消布阵回调
    /// </summary>
    public Action<int> CancelClickCallback = delegate{};

    private List<MonoBehaviour> MenuContainer_ = new List<MonoBehaviour>();

    private bool IsShowing_;
    public bool IsShowing {
        get {
            return IsShowing_;
        }
        set {
            if (IsShowing_ == value) return;

            IsShowing_ = value;

            if (IsShowing_) {
                Show();
            }
            else {
                Hide();
            }
        }
    }

    private Animator Anim_;
    private Animator Anim {
        get {
            if (Anim_ == null) {
                Anim_ = GetComponentInChildren<Animator>();
            }
            return Anim_;
        }
    }



    private void Show() {
        //Follow(CurrentFollowTarget);
        Anim.SetBool(Animator.StringToHash("show"), true);
    }

    private void Hide() {
        Anim.SetBool(Animator.StringToHash("show"), false);
    }

    private void Start() {
        Init();
    }

    private void OnClickCancelBtn() {
        Debugger.Log("OnClickCancelBtn()...");
        /*
        if (AnimRoot_.GetComponent<CanvasGroup>().alpha < 0.7f) {
            return;
        }
         */
        Destroy(CurrentFollowTarget.gameObject);     

        CancelClickCallback(int.Parse(CurrentFollowTarget.name));
        //Destroy(gameObject); 
        IsShowing = false;
    }

    private void Init() {
        CustomizeMenu();
    }

    /// <summary>
    /// 根据情况定制菜单
    /// </summary>
    private void CustomizeMenu() {
        AnimRoot_ = transform.FindChild("Root");
        ContentRoot_ = transform.FindChild("Root/Background/Content");
        
        //目前只有取消布阵的功能
        GameObject cancelBtn = Global.CreateUI("DeployCancelBtn");
        Button cancel = cancelBtn.GetComponentInChildren<Button>();
        cancel.onClick.AddListener(OnClickCancelBtn);

        cancelBtn.transform.SetParent(ContentRoot_);
        cancelBtn.transform.localScale = Vector3.one;
        cancelBtn.transform.localPosition = new Vector3(5.7f, 10.1f, 0f);
        cancelBtn.transform.eulerAngles = new Vector3(0f, 0f, -70f);
        
        MenuContainer_.Add(cancel as MonoBehaviour);
    }

    /// <summary>
    /// 实时更新菜单的位置
    /// </summary>
    public void SetFollowTarget(Transform target) {
        CurrentFollowTarget = target;
    }

    void Update() {      
        //if (EventSystem.current.currentSelectedGameObject != null) {
        //    Debugger.Log("Update FrameCount: " + Time.frameCount + "currentSelectedGameobject: " + EventSystem.current.currentSelectedGameObject.name);
        //}

        if (CurrentFollowTarget == null) return;
        Camera WorldCam = CameraManager.Instance.MainCameraInstance;
        Vector3 worldPos = WorldCam.WorldToViewportPoint(CurrentFollowTarget.position);
        transform.position = CameraManager.Instance.UICamera.ViewportToWorldPoint(worldPos);
        worldPos = transform.localPosition;
        worldPos.z = 0f;
        transform.localPosition = worldPos;
    }

    public bool IsClickedMenu;

    /// <summary>
    /// 区分点击的是UI还是Collider
    /// </summary>
    /// <returns></returns>
    public bool IsClicked() {

        if (!IsShowing_) {
            Debugger.Log("Menu.IsClicked: isShowing == false");
            return false;
        }

        if (EventSystem.current.currentSelectedGameObject == null) {     
            //Debugger.Log("IsClicked(), FrameCount: " + Time.frameCount + " currentSelectedGameObject NOT exist!");
            return false;
        }
        else {
            //Debugger.Log("currentSelectedGameObject exist!");
            if (EventSystem.current.currentSelectedGameObject.tag == "DeployUnitMenu") {
                return true;
            }
            else {
                return false;
            }
        }
    }

    void OnDestroy() {
        //Instance_ = null;
    }

    public static void DestroyCurrent() {
        if (Instance_) {
            Destroy(Instance_.gameObject);
            Instance_ = null;
        }
    }


}
