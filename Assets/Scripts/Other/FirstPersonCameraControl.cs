using UnityEngine;
using System.Collections;

public class FirstPersonCameraControl : MonoBehaviour {

	public float firstPersonRotation;
	public GameObject player;
	public float lookSpeed = 1;

	void Start(){

		firstPersonRotation = 0;

	}

	// Update is called once per frame
	void Update () {
		//set firstPersonRotation back to 0 when transitioning, do later
		float h = lookSpeed * Input.GetAxis("Mouse X");
		float v = lookSpeed * Input.GetAxis("Mouse Y");
		float clampNeg = -40;
		float clampPos = 40;
		firstPersonRotation = Mathf.Clamp(firstPersonRotation - v, clampNeg,
			clampPos);
		// Set player rotation to camera rotation :)
		player.transform.Rotate(0,h,0);
		transform.rotation = player.transform.rotation * Quaternion.AngleAxis(firstPersonRotation, Vector3.right);
	}
}
