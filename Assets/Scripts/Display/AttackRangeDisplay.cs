using UnityEngine;
using System.Collections;

[RequireComponent( typeof( MeshRenderer ), typeof( MeshFilter ) )]
public class AttackRangeDisplay : MonoBehaviour {

    MeshFilter          MeshFilter_;
    MeshRenderer        MeshRenderer_;

    float               SectorRate_ = 0.15f;
    float               RectangleRate_ = 0.05f;

    // 为了让弧度足够圆滑,使用100个间隔
    int                 Segments_ = 100;

    /// <summary>
    /// 生成扇形攻击范围
    /// </summary>
    /// <param name="parentTrans">船体</param>
    /// <param name="minZ">最小攻击距离</param>
    /// <param name="maxZ">最大攻击距离</param>
    /// <param name="angle">角度</param>
    public static AttackRangeDisplay InitSectorAttackRange( bool bPlayer, Transform parentTrans, float minRadius, float maxRadius, float angle, Color color ) {
        GameObject go = new GameObject( "SectorAttackRange" );
        go.transform.parent = parentTrans;
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = new Vector3( 0, bPlayer ? 0 : 180, 0 );
        go.transform.localScale = Vector3.one;
        AttackRangeDisplay display = go.AddComponent<AttackRangeDisplay>();
        display.InitSectorAttackRange( minRadius, maxRadius, angle, color );
        return display;
    }

    /// <summary>
    /// 生成圆形攻击范围
    /// </summary>
    /// <param name="parentTrans">船体</param>
    /// <param name="radius">半径</param>
    /// <param name="color">颜色</param>
    /// <returns></returns>
    public static AttackRangeDisplay InitCircleAttackRange( Transform parentTrans, float radius, Color color ) {
        GameObject go = new GameObject( "CircleAttackRange" );
        go.transform.parent = parentTrans;
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        AttackRangeDisplay display = go.AddComponent<AttackRangeDisplay>();
        display.InitCircleAttackRange( radius, color );
        return display;
    }

    public static AttackRangeDisplay InitCircleAttackRange( Vector3 position, float radius, Color color ) {
        GameObject go = new GameObject( "CircleAttackRange" );
        go.transform.position = position;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        AttackRangeDisplay display = go.AddComponent<AttackRangeDisplay>();
        display.InitCircleAttackRange( radius, color );
        return display;
    }

    /// <summary>
    /// 生成矩形攻击范围
    /// </summary>
    /// <param name="parentTrans">船体</param>
    /// <param name="radiateLen">长度</param>
    /// <param name="radiateWid">宽度</param>
    /// <param name="color">颜色</param>
    /// <returns></returns>
    public static AttackRangeDisplay InitRectangleAttackRange( Transform parentTrans, float radiateLen, float radiateWid, Color color ) {
        GameObject go = new GameObject( "RectangleAttackRange" );
        go.transform.parent = parentTrans;
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        AttackRangeDisplay display = go.AddComponent<AttackRangeDisplay>();
        display.InitRectangleAttackRange( radiateLen, radiateWid, color );
        return display;
    }

    private void Awake() {
        MeshFilter_ = GetComponent<MeshFilter>();
        MeshRenderer_ = GetComponent<MeshRenderer>();
    }

    private void SetColor( Color color ) {
        if( color == Color.red )
            MeshRenderer_.material = Resources.Load( "Battle/AttackRange/AttackRangeMaterial_red" ) as Material;
        else if( color == Color.green )
            MeshRenderer_.material = Resources.Load( "Battle/AttackRange/AttackRangeMaterial_green" ) as Material;
        else if(color == Color.blue )
            MeshRenderer_.material = Resources.Load( "Battle/AttackRange/AttackRangeMaterial_blue" ) as Material;
        else if( color == Color.gray )
            MeshRenderer_.material = Resources.Load( "Battle/AttackRange/AttackRangeMaterial_gray" ) as Material;
    }

    /// <summary>
    /// 初始化扇形攻击范围
    /// </summary>
    /// <param name="minZ"></param>
    /// <param name="maxZ"></param>
    /// <param name="angleDegree"></param>
    private void InitSectorAttackRange( float minRadius, float maxRadius, float angle, Color color ) {
        MeshFilter_.mesh = CreateSectorMesh( minRadius, maxRadius, angle );
        SetColor( color );
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 初始化圆形攻击范围
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    private void InitCircleAttackRange( float radius, Color color ) {
        MeshFilter_.mesh = CreateSectorMesh( 0, radius, 360 );
        SetColor( color );
        gameObject.SetActive( false );
    }

    /// <summary>
    /// 初始化矩形攻击范围
    /// </summary>
    /// <param name="radiateLen"></param>
    /// <param name="radiateWid"></param>
    /// <param name="color"></param>
    private void InitRectangleAttackRange( float radiateLen, float radiateWid, Color color ) {
        MeshFilter_.mesh = CreateRectangleMesh( radiateLen, radiateWid );
        SetColor( color );
        gameObject.SetActive( false );
    }

    private Vector3[] GetVertices( Vector3[] list1, Vector3[] list2, Vector3[] list3, Vector3[] list4 ) {
        int first = 0;
        int second = list1.Length;
        int third = second + list2.Length;
        int fourth = third + list3.Length;
        // 所有顶点
        Vector3[] vertices = new Vector3[fourth + list4.Length];
        list1.CopyTo( vertices, first );
        list2.CopyTo( vertices, second );
        list3.CopyTo( vertices, third );
        list4.CopyTo( vertices, fourth );
        return vertices;
    }

    private int[] GetTriangles( Vector3[] list1, Vector3[] list2, Vector3[] list3, Vector3[] list4 ) {
        int first = 0;
        int second = list1.Length;
        int third = second + list2.Length;
        int fourth = third + list3.Length;
        // 三角形对应的顶点index
        int[] triangles = new int[Segments_ * 6 * 3];
        for( int i=0; i < triangles.Length; i += 18 ) {
            triangles[i] = first;
            triangles[i + 1] = second;
            triangles[i + 2] = first + 1;

            triangles[i + 3] = second;
            triangles[i + 4] = second + 1;
            triangles[i + 5] = ++first;

            triangles[i + 6] = second;
            triangles[i + 7] = third;
            triangles[i + 8] = second + 1;

            triangles[i + 9] = third;
            triangles[i + 10] = third + 1;
            triangles[i + 11] = ++second;

            triangles[i + 12] = third;
            triangles[i + 13] = fourth;
            triangles[i + 14] = third + 1;

            triangles[i + 15] = fourth;
            triangles[i + 16] = fourth + 1;
            triangles[i + 17] = ++third;
            fourth++;
        }
        return triangles;
    }

    private Vector2[] GetUvs( Vector3[] list1, Vector3[] list2, Vector3[] list3, Vector3[] list4, float rate ) {
        int second = list1.Length;
        int third = second + list2.Length;
        int fourth = third + list3.Length;
        Vector2[] uvs = new Vector2[fourth + list4.Length];
        for( int i=0; i < uvs.Length; i++ ) {
            int x = i % list1.Length;
            float u = (float)x / (float)(list1.Length - 1);
            float v = 0;
            if( i >= second && i < third )
                v = rate;
            else if( i >= third && i < fourth )
                v = 1 - rate;
            else if( i >= fourth ) {
                v = 1;
            }
            uvs[i] = new Vector2( u, v );
        }
        return uvs;
    }

    /// <summary>
    /// 创建矩形范围mesh
    /// </summary>
    /// <param name="radiateLen"></param>
    /// <param name="radiateWid"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    private Mesh CreateRectangleMesh( float radiateLen, float radiateWid ) {
        Segments_ = 5;
        Mesh mesh = new Mesh();
        mesh.name = "RectangleMesh";
        float offset = radiateLen * RectangleRate_;
        Vector3[] vertices1 = GetRectangleVertices( 0, radiateWid, Segments_ );
        Vector3[] vertices2 = GetRectangleVertices( offset, radiateWid, Segments_ );
        Vector3[] vertices3 = GetRectangleVertices( radiateLen - offset, radiateWid, Segments_ );
        Vector3[] vertices4 = GetRectangleVertices( radiateLen, radiateWid, Segments_ );        
        mesh.vertices = GetVertices( vertices1, vertices2, vertices3, vertices4 );
        mesh.triangles = GetTriangles( vertices1, vertices2, vertices3, vertices4 );
        mesh.uv = GetUvs( vertices1, vertices2, vertices3, vertices4, RectangleRate_ );        
        return mesh;
    }

    /// <summary>
    /// 创建扇形范围mesh,包含圆形
    /// </summary>
    /// <param name="minZ"></param>
    /// <param name="maxZ"></param>
    /// <param name="angleDegree"></param>
    /// <returns></returns>
    private Mesh CreateSectorMesh( float minRadius, float maxRadius, float angle ) {
        Segments_ = (int)angle;
        Mesh mesh = new Mesh();
        mesh.name = "SectorMesh";
        float offset = 2;// (maxRadius - minRadius) * SectorRate_;
        // 内圈顶点
        Vector3[] vertices1 = GetSectorVertices( minRadius, angle, Segments_ );
        // 第2圈
        Vector3[] vertices2 = GetSectorVertices( minRadius + offset, angle, Segments_ );
        // 第3圈
        Vector3[] vertices3 = GetSectorVertices( maxRadius - offset, angle, Segments_ );
        // 外圈顶点
        Vector3[] vertices4 = GetSectorVertices( maxRadius, angle, Segments_ );
        mesh.vertices = GetVertices( vertices1, vertices2, vertices3, vertices4 );
        mesh.triangles = GetTriangles( vertices1, vertices2, vertices3, vertices4 );
        Vector2[] uvs = GetUvs( vertices1, vertices2, vertices3, vertices4, SectorRate_ );
                
        int start = 0;
        int end = start + vertices1.Length;
        int num = 0;
        while( num < 4 ) {
            for( int i=start; i < end; i++ ) {
                int index = i % vertices1.Length;
                if( index < 2 )
                    uvs[i] = new Vector2( 0f, uvs[i].y );
                else if( index >= 2 && index < 5 )
                    uvs[i] = new Vector2( 0.1f, uvs[i].y );
                else if( index >= (vertices1.Length - 2) )
                    uvs[i] = new Vector2( 1, uvs[i].y );
                else if( index > (vertices1.Length - 5) && index <= (vertices1.Length - 3) )
                    uvs[i] = new Vector2( 0.9f, uvs[i].y );
            }
            start += vertices1.Length;
            end = start + vertices1.Length;
            num++;
        }
        mesh.uv = uvs;
        
        return mesh;
    }

    /// <summary>
    /// 获得矩形顶点
    /// </summary>
    /// <param name="length"></param>
    /// <param name="width"></param>
    /// <param name="segments"></param>
    /// <returns></returns>
    private Vector3[] GetRectangleVertices( float length, float width, int segments ) {
        // y坐标
        float y = length;
        // 例:2段,则有3个点
        Vector3[] vertices = new Vector3[segments + 1];
        // 当前x
        float currX = -width / 2;        
        // 每段x
        float deltaX = width / segments;
        for( int i = 0; i < vertices.Length; i++ ) {
            vertices[i] = new Vector3( currX, 0, y );
            // 当前x递减
            currX += deltaX;
        }
        return vertices;
    }

    /// <summary>
    /// 获得扇形顶点
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="angleDegree"></param>
    /// <param name="segments"></param>
    /// <returns></returns>
    private Vector3[] GetSectorVertices( float radius, float angleDegree, int segments ) {
        // 顶点
        Vector3[] vertices = new Vector3[segments + 1];
        // 度数转换为弧度
        float angle = Mathf.Deg2Rad * angleDegree;
        // 当前角度
        float currAngle = angle / 2;
        // 每段角度
        float deltaAngle = angle / segments;
        // 算出除中心点外的其他坐标
        for( int i = 0; i < vertices.Length; i++ ) {
            // 根据半径用三角函数算出坐标
            vertices[i] = new Vector3( Mathf.Sin( currAngle ) * radius, 0, Mathf.Cos( currAngle ) * radius );
            // 当前角度递减
            currAngle -= deltaAngle;
        }
        return vertices;
    }
}