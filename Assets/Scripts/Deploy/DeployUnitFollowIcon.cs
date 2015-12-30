using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 布阵拖拽跟随的单位图标
/// </summary>
public class DeployUnitFollowIcon : MonoBehaviour {
    private float OffsetY = 150f;
    private float OffsetX = 0f;
    /// <summary>
    /// 跟随对象
    /// </summary>
    public Transform RootTrans;

    public SnappingUnits Snapping_;

    private Image StateImg_;
    private Image StateImg {
        get {
            if (StateImg_ == null) {
                StateImg_ = transform.FindChild("Background").GetComponent<Image>();
            }
            return StateImg_;
        }
    }

    private Image IconImg_;
    private Image IconImg {
        get {
            if (IconImg_ == null) {
                IconImg_ = transform.FindChild("Icon").GetComponent<Image>();
            }
            return IconImg_;
        }
    }

    private bool EnablePlaced_;
    public bool EnablePlaced {
        get {
            return EnablePlaced_;
        }
        set {
            if (EnablePlaced_.Equals(value)) return;
            EnablePlaced_ = value;
            string stateSprName = EnablePlaced_ ? "icon_substrate2_2" : "icon_substrate2_1";
            Global.SetSprite(StateImg, stateSprName, false);
            IconImg.color = EnablePlaced_ ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f);
        }
    }

    public void SetIcon(string iconName) {
        Global.SetSprite(IconImg, iconName, false);
    }

    public void Follow() {
        Camera WorldCam = CameraManager.Instance.MainCamera.GetComponent<Camera>();
        Vector3 worldPos = WorldCam.WorldToViewportPoint(RootTrans.position);
        transform.position = CameraManager.Instance.UICamera.ViewportToWorldPoint(worldPos);
        worldPos = transform.localPosition;
        worldPos.z = 0f;
        transform.localPosition = new Vector3(OffsetX, OffsetY, 0f) + worldPos;

        EnablePlaced = Snapping_.Intersecting == 0;
    }

}
