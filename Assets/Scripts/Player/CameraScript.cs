using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	public Transform player;
	public Camera cameraA;
	public float cameraSpeed = 2.0f;
	
	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)){

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

		}
		else if (Input.GetMouseButtonDown(0)){

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

		}


		transform.position = player.position + Vector3.up;
        cameraA.transform.LookAt (transform);
		
		//transform.Rotate(Vector3.down, Input.GetAxis("PS4_RightStickX") * cameraSpeed);
		//transform.Rotate(Vector3.right, Input.GetAxis("PS4_RightStickY") * cameraSpeed);
		
		transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * cameraSpeed);
	}
}
