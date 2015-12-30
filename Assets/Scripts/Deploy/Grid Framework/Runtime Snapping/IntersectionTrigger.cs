using UnityEngine;
using System.Collections;

public class IntersectionTrigger : MonoBehaviour {
	
	private SnappingUnits snappingScript;
	
	public void SetSnappingScript(SnappingUnits script){
		snappingScript = script;
	}

	void OnTriggerEnter(Collider other){
        if( other.name == "Grid" ) return;
        //Debugger.LogError("OnTriggerEnter: " + other.transform.name);
		snappingScript.SyncIntersectAmount(true);
	}
	
	void OnTriggerExit(Collider other){
        if( other.name == "Grid" ) return;
        //Debugger.LogError("OnTriggerExit: " + other.transform.name);
		snappingScript.SyncIntersectAmount(false);
	}

    void OnTriggerStay(Collider other) {
        //Debugger.LogError(string.Format("{0} collide with {1}", transform.parent.name, other.transform.parent.name));
        if (other.name == "__PlayerFormationGrid__") return;
        if (other.name == transform.parent.name) return;
        if (other.name != "IntersectionTrigger") return;

      

        SnappingUnits.InvalidClickDeploy = true;
    }
}