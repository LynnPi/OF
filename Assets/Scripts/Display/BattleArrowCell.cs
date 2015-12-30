using UnityEngine;
using System.Collections;

public class BattleArrowCell : MonoBehaviour {

    private Animator ArrowAnimator_;
    public Animator ArrowAnimator {
        get {
            if( ArrowAnimator_ == null ) {
                ArrowAnimator_ = GetComponentInChildren<Animator>();
            }
            return ArrowAnimator_;
        }
    }

    /// <summary>
    /// 资源
    /// </summary>
    private static GameObject PrefabTarget_;
    private static GameObject PrefabTarget {
        get {
            if( PrefabTarget_ == null ) {
                PrefabTarget_ = Resources.Load( "Battle/Arrow/selecttarget" ) as GameObject;
            }
            return PrefabTarget_;
        }
    }

    private static GameObject PrefabEnemy_;
    private static GameObject PrefabEnemy {
        get {
            if( PrefabEnemy_ == null ) {
                PrefabEnemy_ = Resources.Load( "Battle/Arrow/selectenemy" ) as GameObject;
            }
            return PrefabEnemy_;
        }
    }

    public Vector3 FaceDirction_;

    public static BattleArrowCell CreateInstance( Transform parent, Vector3 dir, float size, bool bSkillTarget ) {
        GameObject go;
        if( bSkillTarget )
            go = Instantiate( PrefabEnemy ) as GameObject;
        else
            go = Instantiate( PrefabTarget ) as GameObject;
        go.transform.SetParent( parent );
        //go.transform.localScale = Vector3.one;
        BattleArrowCell instance = go.AddComponent<BattleArrowCell>();
        instance.FaceDirction_ = dir;
        instance.ResetFaceDirection();
        instance.ResetPosition( size );
        return instance;
    }

    /// <summary>
    /// 设置朝向
    /// </summary>
    /// <param name="dir"></param>
    public void ResetFaceDirection() {
        if( FaceDirction_ == Vector3.forward )
            transform.eulerAngles = Vector3.zero;
        else if( FaceDirction_ == Vector3.back )
            transform.eulerAngles = -180f * Vector3.up;
        else if( FaceDirction_ == Vector3.right )
            transform.eulerAngles = 90f * Vector3.up;
        else if( FaceDirction_ == Vector3.left )
            transform.eulerAngles = -90f * Vector3.up;
    }

    /// <summary>
    /// 根据半径设置坐标
    /// </summary>
    /// <param name="size"></param>
    public void ResetPosition( float size ) {
        transform.localPosition = size * FaceDirction_;
    }
}
