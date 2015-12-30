using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoroutineJoin {
    List<bool> _subTasks = new List<bool>();

    private readonly MonoBehaviour OwningComponent_;

    public CoroutineJoin( MonoBehaviour owningComponent ) {
        OwningComponent_ = owningComponent;
    }

    public void StartSubtask( IEnumerator routine ) {
        _subTasks.Add( false );
        OwningComponent_.StartCoroutine( StartJoinableCoroutine( _subTasks.Count - 1, routine ) );
    }

    public Coroutine WaitForAll() {
        return OwningComponent_.StartCoroutine( WaitForAllSubtasks() );
    }

    private IEnumerator WaitForAllSubtasks() {
        while ( true ) {
            bool completedCheck = true;
            for ( int i = 0; i < _subTasks.Count; i++ ) {
                if ( !_subTasks[ i ] ) {
                    completedCheck = false;
                    break;
                }
            }

            if ( completedCheck ) {
                break;
            }
            else {
                yield return null;
            }
        }
    }

    private IEnumerator StartJoinableCoroutine( int index, IEnumerator coroutine ) {
        yield return OwningComponent_.StartCoroutine( coroutine );
        _subTasks[ index ] = true;
    }
}