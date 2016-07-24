using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	public Transform player;
	public Camera cameraA;
	public float cameraSpeed = 2.0f;
    private float thirdPersonRotationY, thirdPersonRotationX;
    public float thirdPersonClampNegative = -40, 
                 thirdPersonClampPositive = 45,
                 wallHitOffset = 0.3f;
    public LayerMask playerLayer;
    public Transform thirdPersonCameraLocation;
    public Transform thirdPersonCameraPivot;
    private Vector3 thirdPersonCameraOriginalLocation;
    public bool debug = true;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
        thirdPersonCameraOriginalLocation = thirdPersonCameraLocation.localPosition;
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

        float h = cameraSpeed * Input.GetAxis("Mouse X");
        float v = cameraSpeed * Input.GetAxis("Mouse Y");

        thirdPersonRotationY += h;
        thirdPersonRotationX = Mathf.Clamp(thirdPersonRotationX - v, thirdPersonClampNegative, thirdPersonClampPositive);
        Vector3 targetPosition = player.transform.position + ControllerTools.RotatePointAroundPivot(thirdPersonCameraOriginalLocation,
                                                                 thirdPersonCameraPivot.localPosition, new Vector3(thirdPersonRotationX, thirdPersonRotationY, 0));
        RaycastHit hit;
        Vector3 direction = targetPosition - thirdPersonCameraPivot.position;
        if (Physics.Raycast(thirdPersonCameraPivot.position,
                            direction.normalized, out hit,
                            Vector3.Distance(thirdPersonCameraPivot.position, targetPosition), ~playerLayer))
            cameraA.transform.position = hit.point - direction.normalized * wallHitOffset;
        else
            cameraA.transform.position = targetPosition;
        if (debug)
            Debug.DrawRay(thirdPersonCameraPivot.position, direction, Color.red);
        cameraA.transform.LookAt(thirdPersonCameraPivot);

        /*
		transform.position = player.position + Vector3.up;
        cameraA.transform.LookAt (transform);
		
		//transform.Rotate(Vector3.down, Input.GetAxis("PS4_RightStickX") * cameraSpeed);
		//transform.Rotate(Vector3.right, Input.GetAxis("PS4_RightStickY") * cameraSpeed);
		
		transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * cameraSpeed);
         */
	}
}
