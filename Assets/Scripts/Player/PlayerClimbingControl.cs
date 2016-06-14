using UnityEngine;
using System.Collections;

public class PlayerClimbingControl : MonoBehaviour{

	private Animator anim;
	private PlayerIK playerIK;
	private PlayerMovement playerMovement;
	private Rigidbody rigid;
	private CapsuleCollider col;
	public bool isClimbing;

	private Vector3 smoothingPos;
	public float smoothingTime = 1f;
	private Vector3 smoothingVelocity = Vector3.zero;

    public bool disableInput = false;

	void Awake (){

		anim = GetComponent<Animator> ();
		playerIK = GetComponent<PlayerIK> ();
		playerMovement = GetComponent<PlayerMovement> ();
		rigid = GetComponent<Rigidbody> ();
		col = GetComponent<CapsuleCollider> ();
		isClimbing = false;

	}

	void Update (){

		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");
        if (disableInput)
        {
            h = 0; 
            v = 0;
        }

		//If not transitioning, and in the locomotion state, and is climbing, set is climbing to false
		if (anim.GetAnimatorTransitionInfo (0).fullPathHash == 0 
			&& anim.GetCurrentAnimatorStateInfo (0).fullPathHash == Animator.StringToHash ("Base Layer.Locomotion") 
			&& isClimbing) {
			isClimbing = false;
			playerMovement.isDisabledByClimb = false;
			rigid.isKinematic = false;
		}

		//If the player can move, and is not climbing, and not transitioning, and is in the locomtion state, and is moving, start climbing rays
		if (playerMovement.canMove()
			&& !isClimbing 
			&& anim.GetAnimatorTransitionInfo (0).fullPathHash == 0 
			&& anim.GetCurrentAnimatorStateInfo (0).fullPathHash == Animator.StringToHash ("Base Layer.Locomotion") 
		    && v != 0
			&& Mathf.Abs(playerMovement.angle) < 0.1f && 
			Physics.Raycast(transform.position, transform.forward, 2f)) {

			RaycastHit hit;
			
			for (float y = 1; y < 3; y += 0.1f) {
				
				Debug.DrawRay (transform.position + transform.up * y, transform.forward, Color.green);
				
				if (Physics.Raycast (transform.position + transform.up * y, transform.forward, out hit, 1.0f))
					if (hit.transform.gameObject.tag == "Can Climb")
						StartClimb(hit);
				
			}

		}

		if (isClimbing 
		    && anim.GetCurrentAnimatorStateInfo (0).fullPathHash == Animator.StringToHash ("Base Layer.Climbing.braced_hang_idle") 
		    && anim.GetAnimatorTransitionInfo (0).fullPathHash == 0 
		    && v != 0)
			ClimbUpDown(v);

		if (isClimbing 
		    && v == 0
			&& h != 0)
			ClimbLeftRight (h);
		else if(anim.GetFloat("ClimbShimmy") != 0)
			anim.SetFloat ("ClimbShimmy", 0);

	}

	void FixedUpdate (){

		rigid.angularVelocity = Vector3.zero;

		if (isClimbing)
			transform.position = Vector3.SmoothDamp (transform.position, smoothingPos, ref smoothingVelocity, smoothingTime);

	}

	void StartClimb(RaycastHit hit){

		playerIK.ResetHandSpacing ();

		Vector3 targetLookAt = transform.position - hit.normal;
		targetLookAt.y = transform.position.y;
		transform.LookAt (targetLookAt);

		playerMovement.isDisabledByClimb = true;
		isClimbing = true;
		
		anim.SetBool ("Climbing", true);
		
		rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
		
		rigid.useGravity = true;							
		rigid.isKinematic = true;
		
		playerIK.SetIK (true);
		
		Vector3 targetPos = hit.point - transform.up - transform.forward * col.radius;
		playerIK.hitPoint = hit.point;
		smoothingPos = targetPos;
	}

	void ClimbUpDown(float v){

		RaycastHit hit;

		if (v > 0) {
		
			for (float y = 2; y < 3; y += 0.1f) {
				Debug.DrawRay (transform.position + transform.up * y, transform.forward, Color.green);
				
				if (Physics.Raycast (transform.position + transform.up * y, transform.forward, out hit, 1.0f)) {
					
					if (hit.transform.gameObject.tag == "Can Climb") {

						playerIK.ResetHandSpacing();

						anim.SetTrigger ("ClimbUp");
						
						Vector3 targetPos = hit.point - transform.up - transform.forward * col.radius;

						smoothingPos = targetPos;
						
						playerIK.hitPoint = hit.point;
                        StartCoroutine(ClimbTimeout(1));
						
						break;
						
					}
					
				}else
					EndClimb(v);
				
			}
		
		} else {

			for (float y = -1; y <= 0; y += 0.1f) {
				Debug.DrawRay (transform.position + transform.up * y, transform.forward, Color.green);
				
				if (Physics.Raycast (transform.position + transform.up * y, transform.forward, out hit, 1.0f)) {
					
					if (hit.transform.gameObject.tag == "Can Climb") {

						playerIK.ResetHandSpacing();
						
						anim.SetTrigger ("ClimbDown");
						
						Vector3 targetPos = hit.point - transform.up - transform.forward * col.radius;
						
						smoothingPos = targetPos;

                        playerIK.hitPoint = hit.point;
                        StartCoroutine(ClimbTimeout(1));
						
						break;
						
					}else if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f)){
						smoothingPos = hit.point;
						EndClimb(v);
					}
					
				}
				
			}
		
		}



	}

	void ClimbLeftRight(float h){

		RaycastHit hit;

		float sign = (h > 0) ? 1f : -1f;

		//if (Physics.Raycast (transform.position + playerIK.handDirectionRight * sign * 2 + transform.up, transform.forward, out hit, 1.0f)) {
        if (sign < 0 && Physics.Raycast(playerIK.leftShimmy.position, -transform.up * 2, out hit, 2f) || sign > 0 && Physics.Raycast(playerIK.rightShimmy.position, -transform.up * 2, out hit, 2f))
        {
			if (hit.transform.gameObject.tag == "Can Climb") {
				anim.SetFloat ("ClimbShimmy", sign);

				playerIK.ResetHandSpacing();

				playerIK.rightHandPos += playerIK.handDirectionRight * anim.GetFloat("RightHandShimmy");
				playerIK.leftHandPos += playerIK.handDirectionRight * anim.GetFloat("LeftHandShimmy");

				smoothingPos = transform.position + playerIK.handDirectionRight * sign;
			} else
				anim.SetFloat ("ClimbShimmy", 0);
		} else 
			anim.SetFloat ("ClimbShimmy", 0);

	}

    private IEnumerator ClimbTimeout(float seconds)
    {
        disableInput = true;
        yield return new WaitForSeconds(seconds);
        disableInput = false;
    }

	void EndClimb(float v){

		anim.SetFloat ("FinishClimbing", v);

		anim.SetBool ("Climbing", false);

		playerIK.ResetHandSpacing ();

		rigid.velocity = Vector3.zero;

		playerIK.SetIK (false);

        rigid.useGravity = true;

		if (v > 0) {
			
			RaycastHit hit;
			
			for (float y2 = 0; y2 < 3; y2 += 0.1f) {
				Debug.DrawRay (transform.position + transform.up * y2, transform.forward, Color.green);
				
				if (Physics.Raycast (transform.position + transform.up * y2, transform.forward, out hit, 1.0f))
					smoothingPos = hit.point + transform.forward * col.radius;
			}
			
		}

	}

}
