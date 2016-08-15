using UnityEngine;
using System.Collections;

public class RobotAttackCutscene : MonoBehaviour {

	public Transform camera;

	public void StartCamera(){

		camera.GetComponent<Animation> ().Play();

	}

	public void OnFinished(){

		Application.LoadLevelAsync (Application.loadedLevel + 1);

	}

}
