using UnityEngine;
using System.Collections;

public class PlayerArcheryControl : MonoBehaviour {

	private Animator anim;
	private Rigidbody rigid;
	private PlayerMovement playerMovement;
	private PlayerHealth playerHealth;
	private CameraScript cameraScript;

	public bool isHoldingBow;
	public bool isAiming;
	public float speedDampTime = 0.1f;

	public float minVel = 20;
	public float maxVel = 40;
	private float vel;

	public GameObject bow;
	private GameObject holdingBow;

	private Transform bowString;
	private Vector3 bowStringStartPos;

	public GameObject arrow;
	private GameObject holdingArrow;

	private Transform rightArm;
	private Transform leftArm;

	void Awake(){

		isHoldingBow = false;
		isAiming = false; 

		anim = GetComponent<Animator> ();
		rigid = GetComponent<Rigidbody> ();
		playerMovement = GetComponent<PlayerMovement> ();
		playerHealth = GetComponent<PlayerHealth> ();
		cameraScript = playerMovement.mainCam.GetComponent<CameraScript> ();

		playerMovement.isDisabledByArchery = isHoldingBow;

		rightArm = FindDeepChild (transform, "mixamorig:RightHand");
		leftArm = FindDeepChild (transform, "mixamorig:LeftHand");

	}

	void Update(){

		anim.SetBool ("FireBowReadOnly", anim.GetBool("FireBow"));
	
		if (Input.GetKeyDown (KeyCode.F) && !Mecanim.inTrans(anim, 0) && 
			(Mecanim.inAnim(anim, "Base Layer.Locomotion", 0) || Mecanim.inAnim(anim, "Base Layer.Archery Locomotion", 0))) {

			if (!isHoldingBow)
				Equip ();
			else
				Dequip ();
		
		}
	
		if (isHoldingBow) {
		
			UpdateMovement ();
				
			if (Input.GetMouseButton (1) && (!isAiming || !holdingArrow))
				Aim (true);
			else if (!Input.GetMouseButton (1) && (isAiming || holdingArrow))
				Aim (false);

			if (Input.GetKeyDown (KeyCode.Z))
				MorePower ();

			if (Input.GetKeyUp (KeyCode.Z))
				Fire ();

			if (Mecanim.inAnim (anim, "Archey Aiming Locomtion", 0) || Mecanim.inAnim (anim, "Draw Recoil.New State 0", 1) ||
				Mecanim.inAnim (anim, "Draw Recoil.aim_overdraw", 1))
				bowString.position = rightArm.position;
			else if (bowString && bowString.localPosition != bowStringStartPos)
				bowString.localPosition = bowStringStartPos;

			if (isAiming && holdingArrow) {
			
				holdingArrow.transform.position = bowString.position;
				Vector3 parentPos = bowString.parent.position + bowString.parent.transform.right * 0.06f - bowString.parent.transform.forward * 0.02f;
				holdingArrow.transform.rotation = Quaternion.FromToRotation(Vector3.up,  parentPos - bowString.position);
				holdingArrow.transform.position += holdingArrow.transform.up * holdingArrow.transform.lossyScale.y;
			
			}
			
		}

		if (!Mecanim.inTrans (anim, 1) && Mecanim.inAnim (anim, "Draw Recoil.aim_overdraw", 1)) {

			float percentageComplete = anim.GetCurrentAnimatorStateInfo (1).normalizedTime;

			vel = (maxVel - minVel) * percentageComplete + minVel;

		}

	}

	void UpdateMovement(){

		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");

		if (h != 0 || v != 0) {

			Vector3 worldDirection = playerMovement.mainCam.transform.TransformDirection (new Vector3 (h, 0, v).normalized);
			worldDirection.Scale (new Vector3 (1, 0, 1));

			float angle = 0;
			angle = Vector3.Angle (transform.forward, worldDirection);

			if (Vector3.Cross (transform.forward, worldDirection).y < 0)
				angle = -angle;

			anim.SetFloat ("SpeedX", Mathf.Sin (angle * Mathf.Deg2Rad), speedDampTime, Time.deltaTime);
			anim.SetFloat ("SpeedZ", Mathf.Cos (angle * Mathf.Deg2Rad), speedDampTime, Time.deltaTime);

			rigid.velocity = worldDirection;

		} else if (anim.GetFloat ("SpeedX") != 0 || anim.GetFloat ("SpeedZ") != 0) {

			anim.SetFloat ("SpeedX", 0, speedDampTime, Time.deltaTime);
			anim.SetFloat ("SpeedZ", 0, speedDampTime, Time.deltaTime);

			rigid.velocity = new Vector3 (0, 0, 0);

		}

		rigid.rotation = Quaternion.LookRotation(Vector3.Scale(playerMovement.mainCam.transform.forward, new Vector3(1, 0, 1)), Vector3.up);

	}

	void Equip(){

		isHoldingBow = true;
		anim.SetTrigger ("EquipBow");
		playerMovement.isDisabledByArchery = true;

		holdingBow = (GameObject)Instantiate (bow, leftArm.position, leftArm.rotation);
		holdingBow.transform.parent = leftArm;
		holdingBow.transform.localPosition = new Vector3(0, 0.12f, 0.035f);
		holdingBow.transform.localRotation = Quaternion.Euler (new Vector3(0, 0, 90));
		bowString = FindDeepChild (holdingBow.transform, "string");
		bowStringStartPos = bowString.localPosition;
	
	}

	void Dequip(){

		isHoldingBow = false;
		anim.SetTrigger ("DequipBow");
		playerMovement.isDisabledByArchery = false;

		if (holdingBow)
			Destroy (holdingBow);

	}

	void Aim(bool a){

		bool before = isAiming;
		isAiming = a;
		anim.SetBool ("isAimingBow", a);
		cameraScript.isAiming = a;

		if(before != isAiming)
			cameraScript.StartCoroutine ("TransitionCamera");

		if (isAiming) {
		
			holdingArrow = (GameObject)Instantiate (arrow, holdingBow.transform.position, holdingBow.transform.rotation);
			holdingArrow.transform.parent = rightArm;
			holdingArrow.transform.localPosition = Vector3.zero;
			//holdingArrow.transform.Translate (-holdingArrow.transform.forward * holdingArrow.transform.lossyScale.y / 2);
		
		} else {
		
			if (holdingArrow)
				Destroy (holdingArrow);
		
		}

	}

	void MorePower(){

		anim.SetTrigger ("MorePowerBow");

	}

	void Fire(){

		if(!isAiming){

			StartCoroutine ("QuickShot");

		}

		anim.SetTrigger ("FireBow");

		if (isAiming) {
			
			Destroy (holdingArrow);

			GameObject a = (GameObject)Instantiate (arrow, 
				playerMovement.mainCam.transform.position, 
				Quaternion.Euler (playerMovement.mainCam.transform.forward));// * Quaternion.Euler (playerMovement.mainCam.transform.right * 90));
			a.GetComponent<Rigidbody>().isKinematic = false;
			a.GetComponent<Arrow> ().enabled = true;
			a.GetComponent<Rigidbody> ().velocity = playerMovement.mainCam.transform.forward * vel;
		
		}

	}

	IEnumerator QuickShot(){

		anim.SetBool ("isAimingBow", true);
		yield return new WaitForEndOfFrame ();
		anim.SetBool ("isAimingBow", false);

	}

	Transform FindDeepChild(Transform aParent, string aName){
		var result = aParent.Find(aName);
		if (result != null)
			return result;
		foreach(Transform child in aParent)
		{
			result = FindDeepChild(child, aName);
			if (result != null)
				return result;
		}
		return null;
	}
}
