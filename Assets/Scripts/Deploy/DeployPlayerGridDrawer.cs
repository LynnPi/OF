using UnityEngine;
using System.Collections;

/// <summary>
/// 布阵己方格子绘制器
/// </summary>
public class DeployPlayerGridDrawer : GridDrawer {
    public BoxCollider GridCollider;
    private const int MAX_SIZE = 1000;
    /// <summary>
    /// 有效的布阵格子区域
    /// </summary>
    private GFRectGrid ValidDeployGrid;

    protected override Vector3 GridSize {
        get {
            return new Vector3(MAX_SIZE, 0F, MAX_SIZE);
        }
    }

    protected override Vector3 AreaPosition {
        get {
            return Vector3.back * (
                2 * GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).basearea_len
                + 0//间隙
                + GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).deployarea_len);
        }
    }
      
    protected override GFColorVector3 GridColors {
        get {
            return new GFColorVector3(
            new Color(43f / 255f, 94f / 255f, 216f / 255f, 175f / 255f),
            new Color(43f / 255f, 94f / 255f, 216f / 255f, 175f / 255f),
            new Color(92f / 255f, 114f / 255f, 203f / 255f, 175f / 255f)
            );
        }
    }

    private Vector3 ValidDeployGridSize {
        get {
            return new Vector3(
                GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).battlefield_wid, 
                0f,
                GlobalConfig.GetBattlefieldReferenceByID(BattleSys.NowMapID).deployarea_len); 
        }
    }

    protected override Transform Root {
        get {
            return DeploySceneManager.Instance.transform;
        }
    }

    protected override int GridWidth {
        get {
            return 5;
        }
    }
    protected override void Init() {
        base.Init();

        GridInstance.renderGrid = false;
        GridInstance.hideGrid = true;

        GridCollider = GridInstance.gameObject.AddComponent<BoxCollider>();
        GridCollider.size = 2 * GridInstance.size;

        GameObject go = new GameObject("ValidDeployGridField");
        ValidDeployGrid = go.AddComponent<GFRectGrid>();

        ValidDeployGrid.renderLineWidth = GridWidth;
        ValidDeployGrid.size = ValidDeployGridSize;
        ValidDeployGrid.spacing = GridSpace;
        ValidDeployGrid.axisColors = GridColors;
        
        go.transform.SetParent(transform);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = AreaPosition;

        CreateDeployObstacle();
    }

    /// <summary>
    /// 生成布阵障碍
    /// </summary>
    private void CreateDeployObstacle() {
        float obstacle_size = 1000f;
        float obstacle_height = 50f;

        float offset = GridSpace.x/2f;//偏移，碰撞盒完全咬合，拖拽至最边界的时候就会被判断越界
        float valid_deploy_field_length = ValidDeployGridSize.x;
        float valid_deploy_field_width = ValidDeployGridSize.z;
       

        GameObject obsRoot = new GameObject("ObstacleRoot");
        obsRoot.transform.SetParent(transform);
        obsRoot.transform.localScale = Vector3.one;
        obsRoot.transform.localPosition = AreaPosition;

        GenerateObstacle("Obstacle_Left", obsRoot.transform,
            new Vector3((offset + valid_deploy_field_length + obstacle_size/2), 0f, 0f),
            new Vector3(obstacle_size, obstacle_height, 2*valid_deploy_field_width));

        GenerateObstacle("Obstacle_Right", obsRoot.transform,
            new Vector3(-(offset + valid_deploy_field_length + obstacle_size/2), 0f, 0f),
            new Vector3(obstacle_size, obstacle_height, 2*valid_deploy_field_width));

        GenerateObstacle("Obstacle_Forward", obsRoot.transform,
            new Vector3(0f, 0f, (offset + obstacle_size / 2 + valid_deploy_field_width)),
            new Vector3(2 * (obstacle_size + valid_deploy_field_length + offset), obstacle_height, obstacle_size));

        GenerateObstacle("Obstacle_Back", obsRoot.transform,
            new Vector3(0f, 0f, -(offset + obstacle_size / 2 + valid_deploy_field_width)),
            new Vector3(2 * (obstacle_size + valid_deploy_field_length + offset), obstacle_height, obstacle_size));

    }

    private void GenerateObstacle(string name, Transform root, Vector3 pos, Vector3 size, float offset = 2f) {
        GameObject go = new GameObject(name);
        go.transform.SetParent(root);
        go.transform.localPosition = pos;
        BoxCollider collider = go.AddComponent<BoxCollider>();
        collider.size = size;
        collider.center = Vector3.zero;
    }
}
