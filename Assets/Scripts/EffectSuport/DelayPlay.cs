using UnityEngine;
using System.Collections;

public class DelayPlay : MonoBehaviour {

    public float delayTime = 1f;


    // Use this for initialization
    void Start() {
        Restart();

    }

    public void Restart() {
        CancelInvoke();
        Invoke( "DelayFunc", delayTime );
        gameObject.SetActive( false );
    }

    void DelayFunc() {
        gameObject.SetActive( true );
    }

}
