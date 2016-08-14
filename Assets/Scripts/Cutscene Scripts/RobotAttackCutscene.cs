using UnityEngine;
using System.Collections;

public class RobotAttackCutscene : MonoBehaviour {

	public Transform camera;

	public void OnFinishedMovingDrones(){

		camera.GetComponent<Animation> ().Play();

	}


}
