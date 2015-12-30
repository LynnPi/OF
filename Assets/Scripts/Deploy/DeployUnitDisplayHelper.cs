using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 布阵单位表现辅助器
/// </summary>
public class DeployUnitDisplayHelper : MonoBehaviour {
    public enum Status { Idle, Open, Close }

    private float GroupSize_;

    private readonly Vector3[] FaceDirections = new Vector3[] {
        Vector3.left, 
        Vector3.right,
        Vector3.forward,
        Vector3.back 
    };

    private Vector3 LastClickPos_;

    private readonly Dictionary<Status, string> AnimMap_ = new Dictionary<Status, string>(){
        {Status.Idle, "open"},//先open,再idle
        {Status.Open, "open"}, 
        {Status.Close, "close"}
    };

    private Status AnimStatus_ = Status.Close;
    public Status AnimStatus {
        get {
            return AnimStatus_;
        }
        set {
            if (AnimStatus_.Equals(value)) return;

            AnimStatus_ = value;
            Debugger.Log("AnimStatus_ : " + AnimStatus_);
            foreach (var cell in ArrowGroup) {
                cell.ArrowAnimator.SetTrigger(AnimMap_[AnimStatus_]);
            }
        }
    }

    /// <summary>
    /// 箭头组
    /// </summary>
    private static List<DeployArrowCell> ArrowGroup_;
    private List<DeployArrowCell> ArrowGroup {
        get {
            if (ArrowGroup_ == null) {
                ArrowGroup_ = new List<DeployArrowCell>();
                //初始化箭头组
                foreach (var dir in FaceDirections) {
                    DeployArrowCell cell = DeployArrowCell.CreateInstance(GroupGameObject_.transform, dir, GroupSize_);
                    ArrowGroup_.Add(cell);
                }
            }
            return ArrowGroup_;
        }
    }

    /// <summary>
    /// 底板
    /// </summary>
    private static DeployBottomPlane BottomPlane_;
    public DeployBottomPlane BottomPlane {
        get {
            if (BottomPlane_ == null) {
                //初始化底板
                BottomPlane_ = DeployBottomPlane.CreateInstance(GroupGameObject_.transform, GroupSize_);
            }
            return BottomPlane_;
        }
    }

    private static DeployUnitDisplayHelper Current_;
    public static DeployUnitDisplayHelper Current { 
        get {
            return Current_;
        } 
        set {
            Current_ = value;
            if (Current == null) {
                ClearInfo();
            }
        } 
    }
    private static GameObject GroupGameObject_;

    void Awake() {
        GroupSize_ = GetComponent<SnappingUnits>().ShipWrapRadius;
    }

    /// <summary>
    /// 重置箭头区域大小
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="AreaSize"></param>
    public static void SetCurrent(DeployUnitDisplayHelper group, Transform parent, float size) {
        if (Current == null) {
            
            GroupGameObject_ = new GameObject("_ArrowGroup_");
            ResetGroupGameObject(parent, size);
           
            Current = group;
            Current.GroupSize_ = size; 
            foreach (var cell in Current.ArrowGroup) cell.ResetPosition(size);
            Current.BottomPlane.ResetSize(size);
            Current.BottomPlane.ResetPosition(size);
        }
        else if(!Current.Equals(group)){
            Current.GetComponent<SnappingUnits>().State = SnappingUnits.DeployState.OnPlaced;

            ResetGroupGameObject(parent, size);
            //Current.StartCoroutine(DelayReset(parent));
            Current = group;//设置新的 
            foreach (var cell in Current.ArrowGroup) cell.ResetPosition(size);
            Current.BottomPlane.ResetSize(size);
            Current.BottomPlane.ResetPosition(size);
        }
    }

    private static void ResetGroupGameObject(Transform parent, float size) {    
        GroupGameObject_.transform.SetParent(parent);
        GroupGameObject_.transform.localScale = Vector3.one;
        GroupGameObject_.transform.localPosition = new Vector3(size % GridDrawer.GRID_SPACE_UNIT_SIZE, 0f, size % GridDrawer.GRID_SPACE_UNIT_SIZE); ;
    }

    
    private static IEnumerator DelayReset(Transform t) {
        float closeAnimDuration = 35f / 60f;//箭头的关闭动画是25帧
        yield return new WaitForSeconds(closeAnimDuration);
        GroupGameObject_.transform.SetParent(t);
        GroupGameObject_.transform.localScale = Vector3.one;
        GroupGameObject_.transform.localPosition = Vector3.zero;
    }

    void OnDestroy() {
        ClearInfo();
    }

    private static void ClearInfo() {
        Destroy(GroupGameObject_);
        ArrowGroup_ = null;
        BottomPlane_ = null;
        GroupGameObject_ = null;
        DeployDragHandeler.InDragMode = true;
    }

}
