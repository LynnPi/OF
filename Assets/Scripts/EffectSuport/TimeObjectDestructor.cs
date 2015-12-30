using UnityEngine;
using System.Collections;

public class TimeObjectDestructor : MonoBehaviour {

    public float TimeOut = 1.0f;

    void Awake() {
        Invoke( "DestroyNow", TimeOut );
    }

    void DestroyNow() {
        DestroyObject( this.gameObject );
    }
}
