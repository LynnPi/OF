using UnityEngine;
using System.Collections;

public class ExampleDescription : MonoBehaviour {

	private string message = "Click and drag a block over the grid and observe how it snaps to the grid's cells. " +
		"If you try to go off the grid's plane movement will stop because the cursor input can't be handled." +
		"\n" +
		"If you click-drag on the grid itself you will see the point itself being aligned without any Transform (turn on gizmos to see)." +
		"\n\n" +
		"The mouse input is handled by casting a ray from the cursor through the camera into the grid and seing where it hits the grid's collider. " +
		"Note that the polar grid's mesh collider has been created through code and fits the grid exactly, no matter what settings you use.";

	void OnGUI () {
		GUI.TextArea (new Rect (10, 10, 400, 200), message);
	}
}
