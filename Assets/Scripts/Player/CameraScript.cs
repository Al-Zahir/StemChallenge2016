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

	public bool isAiming;

	private Quaternion spineRotation;
	private Transform spine;

	//Cursor Stuff
	public Texture2D cursor;
	private Texture2D fill;
	private int mouseWidth = Screen.height / 30;
	private int mouseHeight = Screen.height / 30;
	private Vector2 mouseOffset = Vector2.zero; // Sets the offset of the mouse from the middle of the screen
	public float crosshairScale = 1; // Scale independent of other variables (drawing size of crosshair)
	public float crosshairAimMultiplier = 2; // Crosshair shrink when aiming
	public Vector2 crossHairTransition = new Vector2(4.9f, -5.8f);
	private int renderCounter = int.MaxValue;

	//Camera
	private bool isTransitioning = false;
	public Transform cameraAimPosition;
	public Transform thirdPersonCameraYLocation;
	public Transform firstPersonCameraPivotLocation;
	public float transitionTime = 0.5f; // General transition time variable
	public float firstPersonClampNegative = -80;
	public float firstPersonClampPositive = 80;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
        thirdPersonCameraOriginalLocation = thirdPersonCameraLocation.localPosition;

		spine = player.FindChild ("mixamorig:Hips").FindChild("mixamorig:Spine");
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

		if (!isAiming && !isTransitioning) {
			float h = cameraSpeed * Input.GetAxis ("Mouse X");
			float v = cameraSpeed * Input.GetAxis ("Mouse Y");

			thirdPersonRotationY += h;
			thirdPersonRotationX = Mathf.Clamp (thirdPersonRotationX - v, thirdPersonClampNegative, thirdPersonClampPositive);
			Vector3 targetPosition = player.transform.position + ControllerTools.RotatePointAroundPivot (thirdPersonCameraOriginalLocation,
				                         thirdPersonCameraPivot.localPosition, new Vector3 (thirdPersonRotationX, thirdPersonRotationY, 0));
			RaycastHit hit;
			Vector3 direction = targetPosition - thirdPersonCameraPivot.position;
			if (Physics.Raycast (thirdPersonCameraPivot.position,
				    direction.normalized, out hit,
				    Vector3.Distance (thirdPersonCameraPivot.position, targetPosition), ~playerLayer))
				cameraA.transform.position = hit.point - direction.normalized * wallHitOffset;
			else
				cameraA.transform.position = targetPosition;
			if (debug)
				Debug.DrawRay (thirdPersonCameraPivot.position, direction, Color.red);
			cameraA.transform.LookAt (thirdPersonCameraPivot);
		} else if (isAiming && !isTransitioning) {
		
			UpdateCameraAim ();
		
		}

        /*
		transform.position = player.position + Vector3.up;
        cameraA.transform.LookAt (transform);
		
		//transform.Rotate(Vector3.down, Input.GetAxis("PS4_RightStickX") * cameraSpeed);
		//transform.Rotate(Vector3.right, Input.GetAxis("PS4_RightStickY") * cameraSpeed);
		
		transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * cameraSpeed);
         */
	}





	public void OnGUI(){

		if (isAiming && !isTransitioning) {
			mouseWidth = (int)(Screen.height / 30 * crosshairScale);
			mouseHeight = (int)(Screen.height / 30 * crosshairScale);

			//Texture2D fill = (Texture2D)Instantiate (cursor);

			if (renderCounter > 4) {
				renderCounter = 0;
				fill = new Texture2D (cursor.width, cursor.height);
				fill.SetPixels (cursor.GetPixels ());

				Animator anim = player.GetComponent<Animator> ();
				if (!Mecanim.inTrans (anim, 1) && Mecanim.inAnim (anim, "Draw Recoil.aim_overdraw", 1)) {
			
					float percentageComplete = anim.GetCurrentAnimatorStateInfo (1).normalizedTime;
					float innerRadius = 217f * fill.width / 512, outerRadius = 236f * fill.height / 512;
					float center = 256f * ((fill.width + fill.height) / 2) / 512;

					for (int y = 0; y < fill.height; y++) {
						for (int x = 0; x < fill.width; x++) {
							float radius = Mathf.Sqrt ((y - center) * (y - center) + (x - center) * (x - center));
							float angle = 180 - Mathf.Atan2 (y - center, x - center) * Mathf.Rad2Deg;
							angle %= 360;

							if (radius > innerRadius && radius < outerRadius && angle < percentageComplete * 360) {
								fill.SetPixel (x, y, Color.blue);
							}
						}
					}

				
				}

				fill.Apply ();
			}

			// Drawn with consideration to mouseOffset and crosshair scaling
			GUI.DrawTexture (new Rect (Screen.width / 2 - (mouseWidth / 2) + (int)(mouseWidth * mouseOffset.x / crosshairScale), 
				Screen.height / 2 - (mouseHeight / 2) + (int)(mouseHeight * mouseOffset.y / crosshairScale), mouseWidth, mouseHeight), fill);
			renderCounter++;
		}
	}

	// Direction of player aim in world coordinates (To be used for shooting)
	private Vector3 aimDirection()
	{
		// Notice different +- than OnGUI() as ScreenPointToRay uses different (0,0) placement
		return Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2 + (int)(mouseWidth * mouseOffset.x / crosshairScale), Screen.height / 2 - (int)(mouseHeight * mouseOffset.y / crosshairScale))).direction;
	}

	// Transitions the camera to aim or normal mode
	private IEnumerator TransitionCamera()
	{
		/* Note about crosshair offset through transition:
         * The values set are only approximate locations and will work okay for most aiming situations
         * After trying many ways to find the actual angle needed I found it is not possible:
         * 
         * THIRDPERSONCAMERA - <-- where its aiming right now
         *                      -
         *                         -
         *                            - 
         * FIRSTPERSONCAMERA             -
         * 
         * Notice that you can draw the aiming from firstpersoncamera to any location along the line.
         * It is not possible to get the cursor placement perfect as it depends on the distance away you are aiming at.
         * If we were to use that distance (raycast) it would work but it would be inconsistent and unfair as players would not know where the cursor goes (pro players need full control)
         * The current settings work fine and experienced players should be able to adjust their aim while it's transitioning.
        */
		isTransitioning = true;
		// Angles carry through viewpoints
		Quaternion targetRot = isAiming ? (cameraAimPosition.rotation * Quaternion.AngleAxis(thirdPersonRotationX / 1.3f, Vector3.right)) : Quaternion.LookRotation(thirdPersonCameraYLocation.position - cameraA.transform.position);
		Quaternion initialRot = cameraA.transform.rotation;
		Vector3 initialPos = cameraA.transform.position;
		// Crosshair changes scale depending on zoom
		float targetScale = isAiming ? (crosshairScale / crosshairAimMultiplier) : (crosshairScale * crosshairAimMultiplier);
		float timePassed = 0;

		thirdPersonRotationX = isAiming ? 0 : Vector3.Angle(cameraA.transform.forward, Vector3.Scale(cameraA.transform.forward, new Vector3(1, 0, 1)));
		if (!isAiming && cameraA.transform.forward.y > 0) thirdPersonRotationX *= -1;
		if (!isAiming) thirdPersonRotationX *= 1.3f;
		thirdPersonRotationY = transform.rotation.eulerAngles.y;
		Vector2 targetMouse = isAiming ? Vector2.zero : crossHairTransition;
		Vector2 initialMouse = mouseOffset;

		while (timePassed / transitionTime < 1)
		{
			// Use SmoothStep to make it slow down at start and end of target, similar to SmoothDamp but will converge.
			cameraA.transform.position = Vector3.Lerp(initialPos, isAiming ? cameraAimPosition.position : thirdPersonCameraLocation.position, Mathf.SmoothStep(0, 1, timePassed / transitionTime));
			cameraA.transform.rotation = Quaternion.Slerp(initialRot, targetRot, timePassed / transitionTime);
			crosshairScale = Mathf.Lerp(crosshairScale, targetScale, timePassed / transitionTime);
			mouseOffset = Vector3.Lerp(initialMouse, targetMouse, timePassed / transitionTime);
			// Allow rotation while transitioning. Considers different rotation speeds based on current aim / not aim.
			if (isAiming)
			{
				float h = Input.GetAxis ("Mouse X") * cameraSpeed;
				float v = Input.GetAxis ("Mouse Y") * cameraSpeed;
				transform.rotation *= Quaternion.AngleAxis(h, Vector3.up);
				targetRot *= Quaternion.AngleAxis(h, Vector3.up);
				targetRot *= Quaternion.AngleAxis(v, Vector3.left);
				targetRot = Quaternion.Euler(targetRot.eulerAngles.x, targetRot.eulerAngles.y, 0);
			}
			else
			{
				float h = cameraSpeed * Input.GetAxis ("Mouse X");
				float v = cameraSpeed * Input.GetAxis ("Mouse Y");
				thirdPersonRotationX -= v;
				thirdPersonRotationY += h;
				transform.rotation *= Quaternion.AngleAxis(h, Vector3.up);
				targetRot = Quaternion.LookRotation(thirdPersonCameraYLocation.position - cameraA.transform.position);
			}

			timePassed += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		isTransitioning = false;
		cameraA.transform.parent = null;
	}

	// Split third person camera wall hit detection for usage from multiple areas.
	private Vector3 WallHitPosition()
	{
		Vector3 targetPosition = RotatePointAroundPivot(transform.position + thirdPersonCameraLocation.position, thirdPersonCameraYLocation.position,
			new Vector3(thirdPersonRotationX, thirdPersonRotationY, 0));
		RaycastHit hit;
		Vector3 direction = targetPosition - thirdPersonCameraYLocation.position;
		if (Physics.Raycast(thirdPersonCameraYLocation.position,
			direction.normalized, out hit,
			Vector3.Distance(thirdPersonCameraYLocation.position, targetPosition), ~playerLayer))
			return hit.point - direction.normalized * wallHitOffset;
		else
			return targetPosition;
	}

	// Returns a vector3 of rotation around a point, used for checking if camera is hitting a wall
	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles) * dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; // return it
	}

	// Sets the camera to aim mode
	private void UpdateCameraAim()
	{
		//mcameraA.transform.position = cameraAimPosition.position;
		cameraA.transform.parent = player.transform;

		float xRot = Input.GetAxis ("Mouse X");
		float yRot = Input.GetAxis ("Mouse Y");

		player.transform.rotation *= Quaternion.AngleAxis(xRot * cameraSpeed, Vector3.up);

		cameraA.transform.localRotation = Quaternion.Euler(cameraA.transform.localRotation.eulerAngles.x - yRot * cameraSpeed, 0, 0);
		// Annoying euler angle bypass for limiting the rotation of the camera. Working fine and is better than rewriting a lot of code for a firstPersonX variable.
		// LOCAL rotation so y:0 z:0
		if (cameraA.transform.localRotation.eulerAngles.x < 360 + firstPersonClampNegative && cameraA.transform.localRotation.eulerAngles.x > 270)
			cameraA.transform.localRotation = Quaternion.Euler(firstPersonClampNegative, 0, 0);
		if (cameraA.transform.localRotation.eulerAngles.x > firstPersonClampPositive && cameraA.transform.localRotation.eulerAngles.x < 90)
			cameraA.transform.localRotation = Quaternion.Euler(firstPersonClampPositive, 0, 0);

		float radius = 1.5f;
		cameraA.transform.localPosition = player.InverseTransformPoint(firstPersonCameraPivotLocation.position) + 
			RotatePointAroundPivot (Vector3.back * radius, Vector3.zero, new Vector3 (cameraA.transform.localEulerAngles.x, 0, 0));

		spineRotation = Quaternion.Euler(0,0,cameraA.transform.localEulerAngles.x);
	}

	void LateUpdate(){
	
		if(isAiming && !isTransitioning)
			spine.transform.localRotation = spineRotation;
	
	}
}
