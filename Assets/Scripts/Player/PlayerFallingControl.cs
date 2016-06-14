using UnityEngine;
using System.Collections;

public class PlayerFallingControl : MonoBehaviour {

	private Animator anim;
	private CapsuleCollider col;
	private Rigidbody rigid;
	private PlayerMovement playerMovement;

	private float distToGround;

	private RaycastHit jumpTarget;
	private Vector3 jumpDirection;
	private Vector3 jumpTopStartPoint;
	private int numRays;

	public float jumpSpeed = 2.0f;
	public float jumpAngleOffsetDegrees = 10;
	public float jumpTransitionDisplacementZ = 2.5f;
	public float jumpTimeAllowance = 1;
	public float maxJumpDistanceX = 6;
	public float maxJumpDistanceY = 3;
	public float maxJumpDistanceYDown = 3;
	public float minJumpTargetSize = 2;
	public float jumpTargetOffsetY = 0.4f;
	public bool drawJumpDebug = true;

	void Awake(){

		anim = GetComponent<Animator> ();
		col = GetComponent<CapsuleCollider> ();
		rigid = GetComponent<Rigidbody> ();
		playerMovement = GetComponent<PlayerMovement> ();

		distToGround = col.bounds.extents.y;

	}

	void Update(){

		HandleJumpInput ();

		if (!playerMovement.isDisabledByGround && !isGrounded ())
			StartFall ();
		else if (playerMovement.isDisabledByGround && isGrounded ())
			EndFall ();

	}

	void HandleJumpInput() {
		jumpDirection = transform.forward;
		jumpTopStartPoint = transform.position + new Vector3(0, maxJumpDistanceY, 0);
		jumpTopStartPoint += transform.forward * jumpTransitionDisplacementZ;
		// An extra ray to determine if final cast is jumpable
		numRays = (int)(maxJumpDistanceX / minJumpTargetSize) + 1;
		if (drawJumpDebug) {
			Debug.DrawRay (jumpTopStartPoint, jumpDirection * maxJumpDistanceX, Color.red);
			Debug.DrawRay (jumpTopStartPoint + jumpDirection * maxJumpDistanceX, jumpDirection * minJumpTargetSize, Color.blue);
			
			for (int i=0; i<numRays; i++) {
				Debug.DrawRay (jumpTopStartPoint + jumpDirection * (i + 1) * minJumpTargetSize, 
				               Vector3.down * (maxJumpDistanceY + maxJumpDistanceYDown), 
				               Color.green);
			}
		}
	}

	public void StartFall(){

		playerMovement.isDisabledByGround = true;

		// Data saved for future use if needed
		RaycastHit[] frontData = new RaycastHit[numRays];
		bool lastHitSuccess = false;
		int hitSuccessIndex = -1;
		
		for (int i=0; i<numRays; i++) {
			RaycastHit outHit;
			Physics.Raycast(jumpTopStartPoint + jumpDirection * (i + 1) * minJumpTargetSize, 
			                Vector3.down, out outHit, maxJumpDistanceY + maxJumpDistanceYDown);
			frontData[i] = outHit;
			if(lastHitSuccess &&  outHit.transform != null)
			{
				// Found a platform of adequate width
				hitSuccessIndex = i-1;
			}
			else if(outHit.transform != null)
			{
				lastHitSuccess = true;
			}
		}
		
		if(hitSuccessIndex != -1)
		{
			// Target used inside OnJumpStart
			jumpTarget = frontData[hitSuccessIndex];
			anim.SetBool ("Falling", true);
		}


	}

	void OnJumpStart() {
		Vector3 targetDisplacement = transform.InverseTransformDirection(jumpTarget.point - transform.position) + 
			jumpTargetOffsetY * Vector3.up;
		float zDisplacement = targetDisplacement.z - jumpTransitionDisplacementZ;
		float yDisplacement = targetDisplacement.y;
		float[] math = CharacterMath.JumpInitialVelocity (zDisplacement, yDisplacement);
		float angle = math [1] * Mathf.Deg2Rad;//Mathf.Atan2 (yDisplacement, zDisplacement) + jumpAngleOffsetDegrees *  Mathf.PI / 180;
		float magnitude = math [0];//CharacterMath.JumpInitialVelocity(zDisplacement,
		//yDisplacement,
		// angle);
		/*if (magnitude < 0)
			return;
		Vector3 direction = transform.TransformDirection (new Vector3 (0, Mathf.Sin (angle), 
		                                                               Mathf.Cos (angle)));
		Vector3 force = magnitude * direction.normalized;
		if (force.magnitude > 0) {
			anim.applyRootMotion = false;
			r.AddForce (force, ForceMode.VelocityChange);
		}*/

		Debug.Log ("Z: " + zDisplacement + " Y: " + yDisplacement + " V: " + magnitude + " ø: " + angle);

		rigid.velocity = transform.forward * magnitude * Mathf.Cos(angle) + transform.up * magnitude * Mathf.Sin(angle);
	}
	
	public void EndFall(){

		anim.SetBool ("Falling", false);

		float yVel = rigid.velocity.y;

		if (yVel > -9.8f)
			anim.SetTrigger ("FallingLand");
		else if (yVel > -19.6f)
			anim.SetTrigger ("FallingRoll");
		else
			anim.SetTrigger ("FallingHard");

		playerMovement.isDisabledByGround = false;
			
	}

	public bool isGrounded() {
		
		int numberOfHits = 0;
		
		Vector3 pos = transform.position + distToGround * Vector3.up;
		
		Debug.DrawRay (pos + col.radius * transform.right + (col.radius / 2) * transform.forward, -Vector3.up * (distToGround + 0.01f), Color.red);
		Debug.DrawRay (pos - col.radius * transform.right + (col.radius / 2) * transform.forward, -Vector3.up * (distToGround + 0.01f), Color.red);
		Debug.DrawRay (pos + col.radius * transform.forward, -Vector3.up * (distToGround + 0.01f), Color.red);
		Debug.DrawRay (pos - col.radius * transform.forward, -Vector3.up * (distToGround + 0.01f), Color.red);
		
		if (Physics.Raycast (pos + col.radius * transform.right + (col.radius / 1.5f) * transform.forward, -Vector3.up, distToGround + 0.01f))
			numberOfHits++;
		if (Physics.Raycast (pos - col.radius * transform.right + (col.radius / 1.5f) * transform.forward, -Vector3.up, distToGround + 0.01f))
			numberOfHits++;
		if (Physics.Raycast (pos + col.radius * transform.forward, -Vector3.up, distToGround + 0.01f))
			numberOfHits++;
		if (Physics.Raycast (pos - col.radius * transform.forward, -Vector3.up, distToGround + 0.01f))
			numberOfHits++;
		
		return (numberOfHits > 1);
		
	}

	public bool isGrounded(Vector3 direction){

		Vector3 pos = transform.position + distToGround * Vector3.up;

		return Physics.Raycast (pos + col.radius * direction, -Vector3.up, distToGround + 0.01f);

	}
	
}
