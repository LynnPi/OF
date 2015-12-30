using UnityEngine;
using System.Collections;
using Vectrosity;

/// <summary>
/// 边界线绘制
/// </summary>
public class DeployBoundaryDisplayer : MonoBehaviour {
    public const string MAT_BOUNDARY_DEPLOY_PLAYER = "Boundary/Materials/boundary_deploy_player";
    public const string MAT_BOUNDARY_DEPLOY_ENEMY = "Boundary/Materials/boundary_deploy_enemy";
    public const string MAT_BOUNDARY_BATTLE_FIELD = "Boundary/Materials/boundary_battle_field";

    public const float DEFAULT_LINE_WIDTH = 10F;

    public static GameObject VectorCanvas3D_;

    private VectorLine line_;

    private Material LineMaterial_;

    private Vector3[] LinePoints_;

    private float LineWidth_;

    public static void EnableVectorCanvas(bool able) {
        //Debugger.Log("EnableVectorCanvas: " + able + "    time: " + Time.time);
        if (VectorCanvas3D_)
            VectorCanvas3D_.gameObject.SetActive(able);
    }

    public static DeployBoundaryDisplayer CreateInstance(Transform parent, Vector3[] linePoints, Material lineMaterial, float lineWidth = DEFAULT_LINE_WIDTH) {
        DeployBoundaryDisplayer instance;
        GameObject go = new GameObject("BoundaryDisplayer");
        go.transform.SetParent(parent);
        instance = go.AddComponent<DeployBoundaryDisplayer>();

        instance.LinePoints_ = linePoints;
        instance.LineMaterial_ = lineMaterial;
        instance.LineWidth_ = lineWidth;

        instance.Draw();
        return instance;
    }

    /// <summary>
    /// 根据中心物计算边界点列表
    /// </summary>
    /// <param name="centerObj"></param>
    /// <returns></returns>
    public static Vector3[] CalculateLinePointList() {
        //TODO 边界绘制方式要更换，暂时不做
        int width = GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).battlefield_wid;
        int length = GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).basearea_len;
        Vector3[] pointList = new Vector3[]{
            new Vector3(width,0f,-2*length),
            new Vector3(width,0f,0f),
            new Vector3(-width,0f,0f),
            new Vector3(-width,0f,-2*length),
            new Vector3(width,0f,-2*length)
        };
        return pointList;
    }

    private void Draw() {
        if (LineMaterial_ == null) {
            Debugger.LogError("Not config Material!");
            return;
        }

        string lineName = "boundary_line_created_time_" + Time.time;

        line_ = new VectorLine(lineName, LinePoints_, LineMaterial_, LineWidth_, LineType.Continuous, Joins.Weld);

        if (!VectorCanvas3D_) {//----------第一次创建----------
            line_.Draw3D();//先画
            GameObject vectorCanvas = GameObject.Find("VectorCanvas");//后找
            Destroy(vectorCanvas);//用不到却生成了，删除之
            VectorCanvas3D_ = GameObject.Find("VectorCanvas3D");
            VectorCanvas3D_.layer = LayerMask.NameToLayer("Default");
        }
        else {
            for (int i = VectorCanvas3D_.transform.childCount - 1; i >= 0; i--) {
                Destroy(VectorCanvas3D_.transform.GetChild(i).gameObject);//先删
            }
            line_.Draw3D();//后画
            EnableVectorCanvas(true);       
        }
    }
}
