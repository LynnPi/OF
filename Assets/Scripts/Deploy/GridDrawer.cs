using UnityEngine;
using System.Collections;

/// <summary>
/// 格子生成器
/// </summary>
public class GridDrawer : MonoBehaviour {
    //格子的单位大小
    public const float GRID_SPACE_UNIT_SIZE = 10F;

    public GFRectGrid GridInstance;
    protected virtual Vector3 GridSize {
        get {
            return Vector3.zero;
        }
    }
    protected virtual Vector3 GridSpace {
        get {
            return new Vector3(GRID_SPACE_UNIT_SIZE, 1f, GRID_SPACE_UNIT_SIZE);
        }
    }

    protected virtual int GridWidth {
        get {
            return 6;
        }
    }
    protected virtual GFColorVector3 GridColors {
        get {
            return new GFColorVector3(Color.white, Color.white, Color.white);
        }
    }

    protected virtual Vector3 AreaPosition {
        get {
            return Vector3.zero;
        }
    }

    protected virtual Transform Root {
        get {
            return null;
        }
    }
    void Start() {
        Init();
    }

    protected virtual void Init() {
        transform.SetParent(Root);
        transform.localScale = Vector3.one;
        transform.eulerAngles = Vector3.zero;
        transform.localPosition = Vector3.zero;

        GameObject go = new GameObject("Grid");
        GridInstance = go.AddComponent<GFRectGrid>();

        GridInstance.renderLineWidth = GridWidth;       //设置格子线条的宽度
        GridInstance.size = GridSize;                   //设置格子的大小
        GridInstance.spacing = GridSpace;               //设置格子单位的大小
        GridInstance.axisColors = GridColors;           //设置格子线条的颜色
        
        go.transform.SetParent(transform);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = AreaPosition;
    }

    /// <summary>
    /// 成组的单元的位置算法
    /// </summary>
    public static Vector3 ResultPositionInFormation(int index, float radius) {
        //工字型
        float length = radius/2;
        if (index == 0) {
            return Vector3.zero + new Vector3(radius % GRID_SPACE_UNIT_SIZE, 0f, radius % GRID_SPACE_UNIT_SIZE);
        }
        else if (index == 1) {
            return new Vector3(length, 0f, length) + new Vector3(radius % GRID_SPACE_UNIT_SIZE, 0f, radius % GRID_SPACE_UNIT_SIZE);
        }
        else if (index == 2) {
            return new Vector3(-length, 0f, length) + new Vector3(radius % GRID_SPACE_UNIT_SIZE, 0f, radius % GRID_SPACE_UNIT_SIZE);
        }
        else if (index == 3) {
            return new Vector3(length, 0f, -length) + new Vector3(radius % GRID_SPACE_UNIT_SIZE, 0f, radius % GRID_SPACE_UNIT_SIZE);
        }
        else if (index == 4) {
            return new Vector3(-length, 0f, -length) + new Vector3(radius % GRID_SPACE_UNIT_SIZE, 0f, radius % GRID_SPACE_UNIT_SIZE);
        }
        else {
            return Vector3.zero;
        }
    }

    public static Vector3 ResultEnemyPositionInFormation(int index, float radius) {
        //工字型
        float length = radius / 2;
        if (index == 0) {
            return Vector3.zero;
        }
        else if (index == 1) {
            return new Vector3(length, 0f, length);
        }
        else if (index == 2) {
            return new Vector3(-length, 0f, length);
        }
        else if (index == 3) {
            return new Vector3(length, 0f, -length);
        }
        else if (index == 4) {
            return new Vector3(-length, 0f, -length);
        }
        else {
            return Vector3.zero;
        }
    }


    public static Vector3 OffsetByWrapRadius(Vector3 src, float radius) {
        return src + new Vector3(radius % GRID_SPACE_UNIT_SIZE, 0f, radius % GRID_SPACE_UNIT_SIZE);
    }

    public void EnableRenderGrid(bool able) {
        GridInstance.renderGrid = able;
    }
}
