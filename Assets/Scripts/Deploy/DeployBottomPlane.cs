using UnityEngine;
using System.Collections;

public class DeployBottomPlane : MonoBehaviour {
    private GameObject Instance_;
    private GameObject Instance {
        get {
            if(Instance_ == null){
                Instance_ = transform.GetChild(0).gameObject;
            }
            return Instance_;
        }
    }

    public void SetActive(bool active) {
        Instance.SetActive(active);
    }

    private Renderer PlaneRenderer_;
 
    /// <summary>
    /// 资源
    /// </summary>
    private static GameObject Prefab_;
    private static GameObject Prefab {
        get {
            if (Prefab_ == null) {
                Prefab_ = Resources.Load("deploy/plane/selected_plane") as GameObject;
            }
            return Prefab_;
        }
    }

    private Material IntersectMaterial_;
    private Material IntersectMaterial {
        get {
            if (IntersectMaterial_ == null) {
                IntersectMaterial_ = Resources.Load("deploy/plane/deploy_intersect") as Material;
            }
            return IntersectMaterial_;
        }
    }

    private Material NormalMaterial_;
    private Material NormalMaterial {
        get {
            if (NormalMaterial_ == null) {
                NormalMaterial_ = Resources.Load("deploy/plane/deploy_normal") as Material;
            }
            return NormalMaterial_;
        }
    }

    /// <summary>
    /// 根据封套半径缩放模型
    /// </summary>
    /// <param name="size"></param>
    public void ResetSize(float size) {
        transform.localScale = 2 * size * Vector3.one;
    }

    public void ResetPosition(float size) {
        transform.localPosition = Vector3.zero;
    }

    public void SetMaterial(bool intersecting) {
        if (intersecting)
            PlaneRenderer_.material = IntersectMaterial;
        else
            PlaneRenderer_.material = NormalMaterial;
    }

    public static DeployBottomPlane CreateInstance(Transform parent, float size) {
        GameObject go = Instantiate(Prefab) as GameObject;
        go.transform.SetParent(parent);

        DeployBottomPlane instance = go.AddComponent<DeployBottomPlane>();
        instance.ResetSize(size);

        return instance;
    }

    void Awake() {
        PlaneRenderer_ = GetComponentInChildren<Renderer>();
    }

}
