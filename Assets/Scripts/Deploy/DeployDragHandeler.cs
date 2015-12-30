using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeployDragHandeler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public static GameObject ItemBeingDragged;
    public static bool InDragMode = true;

    public bool EnableDrag = true;
    public Action OnDragBegin = delegate { };
    public Action OnDraging = delegate { };
    public Action OnReleaseFinger = delegate { };
    public Action OnHoverUI = delegate { };
    public static readonly string BelongPanelName = "deploypanel";

    private Vector3 StartPosition_;
    private DeployUnitUIElement Item_;
    private DeployUnitUIElement Item {
        get {
            if( Item_ == null ) {
                Item_ = transform.parent.GetComponent<DeployUnitUIElement>();
            }
            return Item_;
        }
    }

    #region 输入输出接口
    public void OnBeginDrag( PointerEventData eventData ) {
        if (!InDragMode || !EnableDrag) return;

        CameraManager.SetTouchActive(false);

        OnDragBegin();

        if (DeployUnitUIElement.CurrentDeployMode == DeployMode.None) {
            DeployUnitUIElement.CurrentDeployMode = DeployMode.Drag;
        }
    }

    public void OnDrag( PointerEventData eventData ) {
        if (!InDragMode || !EnableDrag) return;
		
        if( Item.IsDragging ) return;

        if (CheckOnHoverGrid()) {
            Debugger.Log("OnDraging callback...");
            OnDraging();
        }
    }

    public void OnEndDrag( PointerEventData eventData ) {
        if (!InDragMode || !EnableDrag) return;

        CameraManager.SetTouchActive(true);
        CallbackOnHoverUI(eventData);  
        ItemBeingDragged = null;
        OnReleaseFinger();
    }
    #endregion


    #region 私有接口
    /// <summary>
    /// 拖动单位归位（原始位置）
    /// </summary>
    private void BackToOriginPosition() {
        transform.localPosition = StartPosition_;
    }

    /// <summary>
    /// 检查拖拽点是否投射在格子上
    /// </summary>
    /// <returns></returns>
    private bool CheckOnHoverGrid() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        DeploySceneManager.Instance.PlayerGridDrawer.GridCollider.Raycast(ray, out hit, Mathf.Infinity);
        return (hit.collider != null);
    }

    /// <summary>
    /// 检查拖拽结束时，释放点是否在UI上。
    /// 如果是，认为此次拖拽无效，进行相应操作回调。
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="uiName"></param>
    /// <returns></returns>
    private bool CallbackOnHoverUI(PointerEventData eventData) {
        if (BelongPanelName == string.Empty) {
            Debugger.LogError("target ui name is empty!");
            return false;
        }
        bool out_of_screen = Input.mousePosition.y <= 0;
        bool bHovered = eventData.hovered.Exists(ui => ui.name == BelongPanelName);
        Debugger.Log("bHovered: " + bHovered + " out_of_screen:" + out_of_screen);
        if (bHovered || out_of_screen) {
            //Debugger.Log("on hover ui, regard this as invalid drag, cancel deploy!");
            OnHoverUI();
        }
        return bHovered;
    }
    #endregion
}