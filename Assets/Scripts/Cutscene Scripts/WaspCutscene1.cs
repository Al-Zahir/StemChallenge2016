using UnityEngine;
using System.Collections;

public class WaspCutscene1 : MonoBehaviour {

	public float clipSpeed;
	public Transform leftRot;
	public Transform rightRot;
	private Quaternion targetRotation;

	public GameObject camera1;
	public GameObject camera2;

	private bool isFinished;
	public Light pointLight;
	private float time;
	private float totalTime;

	public Transform hologramPlanet;

	void Awake(){

		isFinished = false;
		time = 0;
		totalTime = 0;

	}

	void Update () {
	
		//transform.Translate (transform.forward * clipSpeed * 2 * Time.deltaTime);

		transform.rotation = Quaternion.Slerp (leftRot.rotation, rightRot.rotation, Mathf.SmoothStep (0, 1, Mathf.PingPong (clipSpeed * Time.time, 1)));

		/*if (transform.position.z < 15){

		}else if (transform.position.z > 15 && transform.position.z < 50) {

			transform.Rotate (Time.deltaTime * clipSpeed * (-transform.forward + transform.up));
		
		} else if (transform.position.z > 50 && transform.position.z < 85) {
		
			transform.Rotate (Time.deltaTime * clipSpeed * transform.forward);

		}*/

		if (isFinished) {

			clipSpeed = 5;

			hologramPlanet.Rotate(transform.up * 0.25f * 60 * Time.deltaTime, Space.World);

			transform.BroadcastMessage ("Move", SendMessageOptions.DontRequireReceiver);

			time += Time.deltaTime;
			totalTime += Time.deltaTime;

			if (time < 1) {
			
				pointLight.color = new Color (1.0f, time - 0.5f, time - 0.5f);
			
			} else if (time < 2) {
			
				pointLight.color = new Color (1.0f, 1.5f - time, 1.5f - time);
			
			} else {
			
				time = 0;

				if (totalTime >= 3) {
					camera2.GetComponent<Animation> ().Play ();
					totalTime = -5;
				}

				if (totalTime < 0 && totalTime > -1)
					Application.LoadLevelAsync (Application.loadedLevel + 1);
			
			}

		
		}

	}

	public void FinishDialog(){

		camera1.SetActive (false);
		camera2.SetActive (true);

		isFinished = true;

	}




}
