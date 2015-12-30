using UnityEngine;
using System.Collections;


/// <summary>
/// 布阵阵型记录器
/// </summary>
public class DeployFormationRecorder : MonoBehaviour {
    /// <summary>
    /// 记录布阵情况（索引，对应世界坐标）
    /// </summary>
    public void RecordDeployInfo() {
        Transform formationRoot = DeploySceneManager.Instance.PlayerGridDrawer.GridInstance.transform;
        for (int i = 0; i < formationRoot.childCount; i++) {
            Transform t = formationRoot.transform.GetChild(i);
            int index = int.Parse(t.name);
            //Debug.Log( string.Format( "<color=green>RecordDeployInfo, index : {0}, position : {1}</color>", index, t.position ) );
            ClientShip cs = PlayerSys.GetPlayerShipList()[index];
            if (cs == null) {
                Debugger.LogError("RecordDeployInfo Failed! At index : " + index);
                return;
            }

            PlayerSys.Formation(index, t.position + new Vector3(cs.Reference.vol % GridDrawer.GRID_SPACE_UNIT_SIZE, 0f, cs.Reference.vol % GridDrawer.GRID_SPACE_UNIT_SIZE));//纠正偏移
        }
    }


}
