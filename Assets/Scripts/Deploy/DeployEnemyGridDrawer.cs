using UnityEngine;
using System.Collections;
using PalmPoineer.Mobile;

/// <summary>
/// 布阵敌方格子绘制器
/// </summary>
public class DeployEnemyGridDrawer : GridDrawer {

    protected override Vector3 GridSize {
        get {
            return new Vector3(
                GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).battlefield_wid, 
                0f,
                GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).basearea_len);
        }
    }

    protected override Vector3 AreaPosition {
        get {
            return Vector3.back * GridSize.z;
        }
    }

    protected override GFColorVector3 GridColors {
        get {
            return new GFColorVector3(
            new Color(99f / 255f, 28f / 255f, 20f / 255f, 175f / 255f),
            new Color(43f / 255f, 94f / 255f, 216f / 255f, 175f / 255f),
            new Color(122f / 255f, 57f / 255f, 45f / 255f, 175f / 255f)
            );
        }
    }

    protected override Transform Root {
        get {
            return DeploySceneManager.Instance.transform;
        }
    }

    protected override void Init() {
        base.Init();
    }


}
