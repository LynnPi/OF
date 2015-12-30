using UnityEngine;
using System.Collections;

/// <summary>
/// 战场区域格子绘制器
/// </summary>
public class BattleFieldGridDrawer : GridDrawer {
    protected override Vector3 GridSize {
        get {
            return new Vector3(
                GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).battlefield_wid, 
                0f,
                GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).basearea_len);
        }
    }
    protected override GFColorVector3 GridColors {
        get {
            return new GFColorVector3(
            new Color(28f / 255f, 77f / 255f, 197f / 255f, 88f / 255f),
            new Color(43f / 255f, 94f / 255f, 216f / 255f, 175f / 255f),
            new Color(77f / 255f, 108f / 255f, 223f / 255f, 70f / 255f)
            );
        }
    }

    protected override Vector3 AreaPosition {
        get {
            return Vector3.back * GridSize.z;
        }
    }

    protected override Transform Root {
        get {
            return BattleSceneDisplayManager.Instance.transform;
        }
    }
}
