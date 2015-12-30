using UnityEngine;
using System.Collections;

public class DeployArrowCell : MonoBehaviour {
   
    private Animator ArrowAnimator_;
    public Animator ArrowAnimator {
        get {
            if (ArrowAnimator_ == null) {
                ArrowAnimator_ = GetComponentInChildren<Animator>();
            }
            return ArrowAnimator_;
        }
    }

    /// <summary>
    /// 资源
    /// </summary>
    private static GameObject Prefab_;
    private static GameObject Prefab {
        get {
            if (Prefab_ == null) {
                Prefab_ = Resources.Load("deploy/arrow/select") as GameObject;
            }
            return Prefab_;
        }
    }

    public Vector3 FaceDirction_;

    public static DeployArrowCell CreateInstance(Transform parent, Vector3 dir, float size) {
        GameObject go = Instantiate(Prefab) as GameObject;
        go.transform.SetParent(parent);
        //go.transform.localScale = Vector3.one;

        DeployArrowCell instance = go.AddComponent<DeployArrowCell>();
        instance.FaceDirction_ = dir;
        instance.ResetFaceDirection();
        instance.ResetPosition(size);

        return instance;
    }

    /// <summary>
    /// 设置朝向
    /// </summary>
    /// <param name="dir"></param>
    public void ResetFaceDirection() {
        if (FaceDirction_ == Vector3.forward)
            transform.eulerAngles = Vector3.zero;
        else if (FaceDirction_ == Vector3.back)
            transform.eulerAngles = -180f * Vector3.up;
        else if (FaceDirction_ == Vector3.right)
            transform.eulerAngles = 90f * Vector3.up;
        else if (FaceDirction_ == Vector3.left)
            transform.eulerAngles = -90f * Vector3.up;
        else {}
    }

    /// <summary>
    /// 根据半径设置坐标
    /// </summary>
    /// <param name="size"></param>
    public void ResetPosition(float size) {
        transform.localPosition = size * FaceDirction_;
    }

}
