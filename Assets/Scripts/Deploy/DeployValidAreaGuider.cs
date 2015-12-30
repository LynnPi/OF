using UnityEngine;
using System.Collections;

/// <summary>
/// 可布阵的区域提示器
/// </summary>
public class DeployValidAreaGuider : MonoBehaviour {

    private GameObject ModelInstance_;
    private GameObject ModelInstance {
        get {
            if (ModelInstance_ == null) {
                ModelInstance_ = Instantiate(Prefab) as GameObject;
                ModelInstance_.transform.SetParent(transform);
                ModelInstance_.transform.localPosition = Vector3.back * (
                2 * GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).basearea_len
                + 0
                + GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).deployarea_len);
                ModelInstance_.transform.localScale = Vector3.one;
            }
            return ModelInstance_;
        }
    }

    private GameObject Prefab_;
    private GameObject Prefab {
        get {
            if (Prefab_ == null) {
                Prefab_ = Resources.Load("deploy/area_guide/area_guide_plane") as GameObject;
            }
            return Prefab_;
        }
    }

    private Animator Anim_;
    private Animator Anim {
        get {
            if (Anim_ == null) {
                Anim_ = ModelInstance.GetComponentInChildren<Animator>();
            }
            return Anim_;
        }
    }

    public static DeployValidAreaGuider CreateInstance() {
        GameObject attach = new GameObject("__ValidDeployFieldGuider__");
        attach.transform.SetParent(DeploySceneManager.Instance.transform);
        DeployValidAreaGuider instance = attach.AddComponent<DeployValidAreaGuider>();
        return instance;
    }

    public void SetSize(float x, float z) {
        ModelInstance.transform.localScale = new Vector3(x, 1, z);
    }

    public void ShowGuideAnimation(bool isShow) {
        Anim.SetBool(Animator.StringToHash("breathe"), isShow);
    }
}
