using UnityEngine;
using System.Collections;

public class DelayPlayRepeat : MonoBehaviour {

	// Use this for initialization
    public GameObject TargetDelayObject;
    public float delayTime = 1f;
    private bool NeedDeplayPlayCall = false;

    void OnEnable() {
        NeedDeplayPlayCall = true;
    }
	
	// Update is called once per frame
    public void Restart() {
        CancelInvoke();
        Invoke( "DelayFunc", delayTime );
        TargetDelayObject.SetActive( false );
    }

    void DelayFunc() {
        TargetDelayObject.SetActive( true );
    }

    void Update() {
        if( NeedDeplayPlayCall ) {
            NeedDeplayPlayCall = false;
            Restart();
        }
    }
}
