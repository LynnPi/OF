using UnityEngine;
using System.Collections;

/// <summary>
/// 布阵单元位置校正器
/// </summary>
public class DeployUnitPositionCorrector : MonoBehaviour {
    // makes the object snap to the bottom of the grid, respecting the grid's rotation
    public static Vector3 CalculateOffsetY(GFGrid grid, Transform targetTrans) {
        //first store the objects position in grid coordinates
        Vector3 gridPosition = grid.WorldToGrid(targetTrans.position);
        //then change only the Y coordinate
        gridPosition.y = 0.5f * targetTrans.lossyScale.y;
        //convert the result back to world coordinates
        return grid.GridToWorld(gridPosition);
    }

    /*
    //根据封套半径纠正X和Z坐标
    public static void CorrectOffsetXZ(GFGrid grid, Transform targetTrans, float ShipWrapRadius) {
        GFRectGrid RectGrid = grid as GFRectGrid;
        Vector3 posUncorrected = grid.WorldToGrid(targetTrans.position);

        float ShipWarpRadiusToGrid_ = ShipWrapRadius / RectGrid.spacing.x;//舰船单位封套半径在格子世界里面的长度
        int HalfUnitCountX = (int)(grid.size.x / RectGrid.spacing.x); ;//x方向的单位个数的一半
        int HalfUnitCountZ = (int)(grid.size.z / RectGrid.spacing.z); ;//z方向的单位个数的一半

        if ((posUncorrected.z - ShipWarpRadiusToGrid_) < -HalfUnitCountZ) {
            Vector3 correct = new Vector3(posUncorrected.x, posUncorrected.y, ShipWarpRadiusToGrid_ - HalfUnitCountZ);
            targetTrans.position = new Vector3(targetTrans.position.x, targetTrans.position.y, grid.GridToWorld(correct).z);
        }

        if ((posUncorrected.z + ShipWarpRadiusToGrid_) > HalfUnitCountZ) {
            Vector3 correct = new Vector3(posUncorrected.x, posUncorrected.y, HalfUnitCountZ - ShipWarpRadiusToGrid_);
            targetTrans.position = new Vector3(targetTrans.position.x, targetTrans.position.y, grid.GridToWorld(correct).z);
        }

        if ((posUncorrected.x - ShipWarpRadiusToGrid_) < -HalfUnitCountX) {

            Vector3 correct = new Vector3(ShipWarpRadiusToGrid_ - HalfUnitCountX, posUncorrected.y, posUncorrected.z);
            targetTrans.position = new Vector3(grid.GridToWorld(correct).x, targetTrans.position.y, targetTrans.position.z);
        }

        if ((posUncorrected.x + ShipWarpRadiusToGrid_) > HalfUnitCountX) {
            Vector3 correct = new Vector3(HalfUnitCountX - ShipWarpRadiusToGrid_, posUncorrected.y, posUncorrected.z);
            targetTrans.position = new Vector3(grid.GridToWorld(correct).x, targetTrans.position.y, targetTrans.position.z);
        }
    }
     */
    
}
