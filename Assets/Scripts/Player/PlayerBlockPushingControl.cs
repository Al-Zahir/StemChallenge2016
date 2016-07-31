using UnityEngine;
using System.Collections;

public class PlayerBlockPushingControl : MonoBehaviour {

	private Animator anim;
	private PlayerMovement playerMovement;
	private Rigidbody rigid;

	public float speedDampTime = 0.1f;
	public Vector3 offset = new Vector3(0, 0, 0.1f);

	public bool isPushing;
	private Transform block;

	private Transform rightArm;
	private Transform leftArm;

	private Vector3 refVelocity;

	void Awake(){

		anim = GetComponent<Animator> ();
		playerMovement = GetComponent<PlayerMovement> ();
		rigid = GetComponent<Rigidbody> ();

		isPushing = false;
		playerMovement.isDisabledByPushing = isPushing;

		rightArm = FindDeepChild (transform, "mixamorig:RightHand");
		leftArm = FindDeepChild (transform, "mixamorig:LeftHand");

	}

	void Update(){

		if (Input.GetMouseButtonDown (0) && !isPushing) {
		
			RaycastHit hit;

			if (Physics.Raycast (transform.position + 1 * transform.up, transform.forward, out hit, 1)) {
			
				if (hit.transform.tag == "PushingBlock") {
				
					block = hit.transform;
					StartPushing ();
				
				}
			
			}

		}

		if (isPushing && !Input.GetMouseButton (0))
			StopPushing ();

		if (isPushing) {
		
			float h = Input.GetAxis ("Horizontal");
			float v = Input.GetAxis ("Vertical");

			if (h != 0 || v != 0) {

				Vector3 direction = new Vector3 (h, 0, v).normalized;
				anim.SetFloat ("Speed", 1.0f, speedDampTime, Time.deltaTime);

				rigid.velocity = transform.forward * 1.5f + transform.up * rigid.velocity.y;

			} else {
				anim.SetFloat ("Speed", 0.0f, speedDampTime, Time.deltaTime);
				rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
			}
		
			if (block) {
			
				//Physics.IgnoreCollision (transform.GetComponent<Collider>(), block.transform.GetComponent<Collider>());
				Vector3 average = transform.InverseTransformPoint(rightArm.position + leftArm.position) / 2 + offset;
				block.transform.localPosition = average;
			
			}
		}

	}

	void StartPushing(){

		isPushing = true;
		block.parent = transform;
		playerMovement.isDisabledByPushing = isPushing;
		anim.SetBool ("isPushing", true);
	
	}

	void StopPushing(){
	
		isPushing = false;
		block.parent = null;
		block = null;
		playerMovement.isDisabledByPushing = isPushing;
		anim.SetBool ("isPushing", false);

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
