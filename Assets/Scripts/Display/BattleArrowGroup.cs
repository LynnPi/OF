using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleArrowGroup : MonoBehaviour {

    private readonly Vector3[] SkillTargetFaceDirections = new Vector3[] {
        Vector3.left, 
        Vector3.right
    };

    private readonly Vector3[] SelectShipFaceDirections = new Vector3[] {
        Vector3.left, 
        Vector3.right,
        Vector3.forward,
        Vector3.back 
    };

    private Transform               ParentTrans_;
    private float                   Vol_;
    private List<BattleArrowCell>   ArrowGroup_;

    public static BattleArrowGroup InitArrowGroup( Transform parentTrans, float size, bool bSkillTarget ) {
        GameObject go = new GameObject( "_ArrowGroup_" );        
        BattleArrowGroup group = go.AddComponent<BattleArrowGroup>();
        group.Init( parentTrans, size, bSkillTarget );
        return group;
    }

    public Transform GetParent() {
        return ParentTrans_;
    }

    public float GetVol() {
        return Vol_;
    }

    public void ChangeParent( Transform parentTrans, float size ) {
        ParentTrans_ = parentTrans;
        Vol_ = size;
        if( transform == null )
            return;
        transform.SetParent( parentTrans );
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        foreach( var iter in ArrowGroup_ ) {
            iter.ResetPosition( size );
        }

        if( gameObject.activeSelf ) {
            foreach( var cell in ArrowGroup_ )
                cell.ArrowAnimator.SetTrigger( size != 0 ? "open" : "close" );
        }

        gameObject.SetActive( size != 0 );

        if( gameObject.activeSelf ) {
            foreach( var cell in ArrowGroup_ )
                cell.ArrowAnimator.SetTrigger( size != 0 ? "open" : "close" );
        }
    }

    private void Init( Transform parentTrans, float size, bool bSkillTarget ) {
        ArrowGroup_ = new List<BattleArrowCell>();
        Vector3 [] list = bSkillTarget ? SkillTargetFaceDirections : SelectShipFaceDirections;
        foreach( var dir in list ) {
            BattleArrowCell cell = BattleArrowCell.CreateInstance( gameObject.transform, dir, size, bSkillTarget );
            ArrowGroup_.Add( cell );
        }
        ChangeParent( parentTrans, size );
    }
}
