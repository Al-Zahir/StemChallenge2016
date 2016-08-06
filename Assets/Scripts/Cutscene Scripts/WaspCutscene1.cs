using UnityEngine;
using System.Collections;

public class WaspCutscene1 : MonoBehaviour {

	public float clipSpeed;
	public Transform leftRot;
	public Transform rightRot;
	private Quaternion targetRotation;

	void Awake(){


	}

	void Update () {
	
		transform.Translate (transform.forward * clipSpeed * 2 * Time.deltaTime);

		if (transform.position.z < 15){
			
			transform.rotation = Quaternion.Slerp (leftRot.rotation, rightRot.rotation, Mathf.SmoothStep (0, 1, Mathf.PingPong (clipSpeed * Time.time, 1)));

		}else if (transform.position.z > 15 && transform.position.z < 50) {

			transform.Rotate (Time.deltaTime * clipSpeed * (-transform.forward + transform.up));
		
		} else if (transform.position.z > 50 && transform.position.z < 85) {
		
			transform.Rotate (Time.deltaTime * clipSpeed * transform.forward);

		}

	}
}
