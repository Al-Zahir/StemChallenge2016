using UnityEngine;
using System.Collections;

public class PlayerBlockPushingControl : MonoBehaviour {

	private Animator anim;
	private PlayerMovement playerMovement;
	private PlayerWeaponSelector playerWeaponSelector;
	private Rigidbody rigid;

	public float speedDampTime = 0.1f;
	public float handOffset = 0.1f;
	public Vector3 offset = new Vector3(0, 0, 0.1f);

	public bool isPushing;
	private bool allowBlockMove;
	private Transform block;

	private Transform rightArm;
	private Transform leftArm;

	private Vector3 refVelocity;

	void Awake(){

		anim = GetComponent<Animator> ();
		playerMovement = GetComponent<PlayerMovement> ();
		playerWeaponSelector = GetComponent<PlayerWeaponSelector> ();
		rigid = GetComponent<Rigidbody> ();

		isPushing = false;
		allowBlockMove = false;
		playerMovement.isDisabledByPushing = isPushing;

		rightArm = FindDeepChild (transform, "mixamorig:RightHand");
		leftArm = FindDeepChild (transform, "mixamorig:LeftHand");

	}

	void Update(){

		if (Input.GetMouseButtonDown (0) && !isPushing && playerWeaponSelector.slotNumber == 1) {
		
			RaycastHit hit;

			if (Physics.Raycast (transform.position + 1 * transform.up, transform.forward, out hit, 1)) {
			
				if (hit.transform.tag == "PushingBlock") {

					StartCoroutine (StartPushing (hit));
				
				} else if (hit.transform.tag == "IceBlock") {
				
					StartCoroutine (PushIce (hit));
				
				}
			
			}

		}

		if (isPushing && (!Input.GetMouseButton (0) || playerWeaponSelector.slotNumber != 1))
			StopPushing ();

		if (isPushing && allowBlockMove) {
		
			if (Mecanim.inAnim (anim, "Base Layer.Pushing.Pushing Locomotion", 0)) {

                RaycastHit hit;
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");

                Vector3 direction = new Vector3(h, 0, v).normalized;

                Vector3 worldDirection = playerMovement.mainCam.transform.TransformDirection(direction);


                if ((h != 0 || v != 0) && transform.InverseTransformDirection(worldDirection).z > 0 && !Physics.Raycast(block.position - block.up * ((block.lossyScale.y / 2) - 0.1f), transform.forward, out hit, (block.lossyScale.y / 2)))
                {

                    anim.SetFloat("Speed", 1.0f, speedDampTime, Time.deltaTime);

                    rigid.velocity = transform.forward * 1.5f + transform.up * rigid.velocity.y;

                }
                else
                {
                    anim.SetFloat("Speed", 0.0f, speedDampTime, Time.deltaTime);
                    rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
                }
					
			}

			if (block && !Mecanim.inTrans(anim, 0) && allowBlockMove) {

				Vector3 average = (rightArm.position + leftArm.position) / 2 + transform.TransformVector(offset);
				average.y = block.position.y;
				block.transform.position = Vector3.SmoothDamp(block.transform.position, average, ref refVelocity, speedDampTime);

			}
		}

	}

	IEnumerator StartPushing(RaycastHit hit){
		allowBlockMove = false;
		block = hit.transform;
		offset = new Vector3 (0, 0, (block.lossyScale.z / 2) + 0.15f);
		//Physics.IgnoreCollision (transform.GetComponent<Collider>(), block.transform.GetComponent<Collider>());
		isPushing = true;

		playerMovement.isDisabledByPushing = isPushing;
		anim.SetBool ("isPushing", true);

		transform.rotation = Quaternion.LookRotation (-hit.normal);

		Vector3 side1, side2;

		float angle = Vector3.Angle (hit.normal, block.forward);

		if (angle < 1 || angle > 179) {
			
			side1 = block.position - block.forward * (block.lossyScale.z + handOffset);
			side2 = block.position + block.forward * (block.lossyScale.z + handOffset);

		
		} else {
		
			//assuming cubes
			side1 = block.position - block.right * (block.lossyScale.z + handOffset);
			side2 = block.position + block.right * (block.lossyScale.z + handOffset);
		
		}


		float distance1 = Vector3.Distance (transform.position, side1);
		float distance2 = Vector3.Distance (transform.position, side2);

		Vector3 refVelPlayer = Vector3.zero;
		Vector3 targetPos = (distance1 > distance2) ? side2 : side1;
		targetPos.y = transform.position.y;

		while (Vector3.Distance (transform.position, targetPos) > 0.01f) {
			transform.position = Vector3.SmoothDamp (transform.position, targetPos, ref refVelPlayer, speedDampTime);
			yield return new WaitForEndOfFrame ();
			
		}

        if (!block)
            yield break;

		Vector3 blockPos = block.position;
		blockPos.y = transform.position.y;
		transform.LookAt (blockPos);

		block.GetComponent<Rigidbody> ().isKinematic = false; 
		allowBlockMove = true;
	}

	void StopPushing(){
	
		isPushing = false;
		//Physics.IgnoreCollision (transform.GetComponent<Collider>(), block.transform.GetComponent<Collider>(), false);

		block.GetComponent<Rigidbody> ().isKinematic = true;
		block = null;
		playerMovement.isDisabledByPushing = isPushing;
		anim.SetBool ("isPushing", false);

	}

	IEnumerator PushIce(RaycastHit hit){

		block = hit.transform;
		offset = new Vector3 (0, 0, (block.lossyScale.z / 2) + 0.15f);
		Physics.IgnoreCollision (transform.GetComponent<Collider>(), block.transform.GetComponent<Collider>());

		anim.SetTrigger ("PushIce");

		transform.rotation = Quaternion.LookRotation (-hit.normal);

		Vector3 side1, side2;

		float angle = Vector3.Angle (hit.normal, block.forward);

		if (angle < 1 || angle > 179) {

			side1 = block.position - block.forward * (block.lossyScale.z + handOffset);
			side2 = block.position + block.forward * (block.lossyScale.z + handOffset);


		} else {

			//assuming cubes
			side1 = block.position - block.right * (block.lossyScale.z + handOffset);
			side2 = block.position + block.right * (block.lossyScale.z + handOffset);

		}


		float distance1 = Vector3.Distance (transform.position, side1);
		float distance2 = Vector3.Distance (transform.position, side2);

		Vector3 refVelPlayer = Vector3.zero;

		Vector3 targetPos = (distance1 > distance2) ? side2 : side1;
		targetPos.y = transform.position.y;

		Vector3 blockPos = block.position;
		blockPos.y = transform.position.y;
		transform.LookAt (blockPos);

		IceBlock ib = block.GetComponent<IceBlock> ();
		ib.Push ((blockPos - targetPos).normalized);

		while (Vector3.Distance (transform.position, targetPos) > 0.01f && !ib.allowPlayerMove) {
			transform.position = Vector3.SmoothDamp (transform.position, targetPos, ref refVelPlayer, speedDampTime);
			yield return new WaitForEndOfFrame ();
		}

		Physics.IgnoreCollision (transform.GetComponent<Collider>(), block.transform.GetComponent<Collider>(), false);

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
