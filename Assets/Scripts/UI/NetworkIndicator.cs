using UnityEngine;
using System.Collections;

public class NetworkIndicator : MonoBehaviour {

    public static NetworkIndicator Instance;
    void Awake () {
        Instance = this;
        this.gameObject.SetActive ( false );
        //DontDestroyOnLoad ( this );
    }

    public void StartActivityIndicator () {
        this.gameObject.SetActive ( true );
    }

    public void StopActivityIndicator () {
        this.gameObject.SetActive ( false );
    }
}
