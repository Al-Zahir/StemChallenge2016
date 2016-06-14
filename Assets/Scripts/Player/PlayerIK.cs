using UnityEngine;
using System.Collections;

public class PlayerIK : MonoBehaviour {

	public bool useIK;
	
	public bool leftHandIK;
	public bool rightHandIK;

	public Vector3 leftHandOffset;
	public Vector3 rightHandOffset;
	public Vector3 handDirectionRight;
	public Vector3 hitPoint;
	
	public Transform leftOrigin;
	public Transform rightOrigin;

    public Transform rightShimmy, leftShimmy;

	public Vector3 leftOriginOriginalPos;
    public Vector3 rightOriginOriginalPos;
	
	public Vector3 leftHandPos;
	public Vector3 rightHandPos;
	public Vector3 leftHandActual;
	public Vector3 rightHandActual;
	
	private Vector3 leftHandVelocity;
	private Vector3 rightHandVelocity;
	public float handSmoothing = 0.1f;
	
	public Quaternion leftHandRot = Quaternion.identity;
    public Quaternion rightHandRot = Quaternion.identity;

	private float leftWeight = 1f;
	private float rightWeight = 1f;

	private Animator anim;
	private PlayerClimbingControl playerClimbingControl;

	void Awake(){

		anim = GetComponent<Animator> ();
		playerClimbingControl = GetComponent<PlayerClimbingControl> ();

		SetIK (false);

		leftOriginOriginalPos = leftOrigin.localPosition;
		rightOriginOriginalPos = rightOrigin.localPosition;

	}

	void Update(){

		SetLocalHandPositions ();

	}

	void FixedUpdate(){

		Debug.DrawRay (leftOrigin.position, -transform.up, Color.green);
		Debug.DrawRay (rightOrigin.position, -transform.up, Color.green);
		
		DoIKRays ();
		
		if (anim.GetCurrentAnimatorStateInfo (0).fullPathHash == Animator.StringToHash ("Base Layer.Climbing.idle_to_braced_hang") ||
		    anim.GetCurrentAnimatorStateInfo (0).fullPathHash == Animator.StringToHash ("Base Layer.Climbing.sprint_to_braced_hang")) {
			
			NoSmoothing();
			
		}

	}

	void SetLocalHandPositions(){
        /*(transform.right * leftOrigin.localPosition.x) +
           (transform.up * leftOrigin.localPosition.y) +
               (transform.forward * (localHit.z - 0.5f));*/
		Vector3 localHit = transform.InverseTransformPoint (hitPoint);
        leftOrigin.localPosition = new Vector3(leftOrigin.localPosition.x, leftOrigin.localPosition.y, localHit.z + 0.025f);
        rightOrigin.localPosition = new Vector3(rightOrigin.localPosition.x, rightOrigin.localPosition.y, localHit.z + 0.025f);
        leftShimmy.localPosition = new Vector3(leftShimmy.localPosition.x, leftShimmy.localPosition.y, localHit.z + 0.025f);
        rightShimmy.localPosition = new Vector3(rightShimmy.localPosition.x, rightShimmy.localPosition.y, localHit.z + 0.025f);
		
	}

	void OnAnimatorIK (){
		
		if (useIK) {
			
			if (leftHandIK) {
				
				leftHandActual = Vector3.SmoothDamp(leftHandActual, leftHandPos, ref leftHandVelocity, handSmoothing);
				
				anim.SetIKPositionWeight (AvatarIKGoal.LeftHand, leftWeight);
				anim.SetIKPosition (AvatarIKGoal.LeftHand, leftHandActual);
				
				anim.SetIKRotationWeight (AvatarIKGoal.LeftHand, leftWeight);
				anim.SetIKRotation (AvatarIKGoal.LeftHand, leftHandRot);
				
			}
			
			if (rightHandIK) {
				
				rightHandActual = Vector3.SmoothDamp(rightHandActual, rightHandPos, ref rightHandVelocity, handSmoothing);
				
				anim.SetIKPositionWeight (AvatarIKGoal.RightHand, rightWeight);
				anim.SetIKPosition (AvatarIKGoal.RightHand, rightHandActual);
				
				anim.SetIKRotationWeight (AvatarIKGoal.RightHand, rightWeight);
				anim.SetIKRotation (AvatarIKGoal.RightHand, rightHandRot);
				
			}
			
		}
		
	}

	void DoIKRays() {

		if (playerClimbingControl.isClimbing) {
			RaycastHit RHit;
			RaycastHit LHit;
			
			if (Physics.Raycast (leftOrigin.position, -transform.up * 2, out LHit, 2f)) {
				
				leftHandIK = true;
				leftHandPos = LHit.point + transform.TransformVector(leftHandOffset);
                Vector3 localForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
                leftHandRot = Quaternion.FromToRotation(localForward, LHit.normal) * Quaternion.LookRotation(localForward);
				
			} else {
				
				leftHandIK = false;
				
			}
			
			if (Physics.Raycast (rightOrigin.position, -transform.up * 2, out RHit, 2f)) {	
				
				rightHandIK = true;
				rightHandPos = RHit.point + transform.TransformVector(rightHandOffset);

                Vector3 localForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
                rightHandRot = Quaternion.FromToRotation(localForward, RHit.normal) * Quaternion.LookRotation(localForward);
                    /*Quaternion.FromToRotation(Vector3.forward, Vector3.Scale(transform.forward, new Vector3(1, 0, 1))) * 
                    Quaternion.FromToRotation(Vector3.forward, RHit.normal);*/
                //Debug.DrawRay(rightHandPos, Vector3.Scale(transform.forward, new Vector3(1, 0, 1)), Color.black,5);
                //temp.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.Scale(transform.forward, new Vector3(1, 0, 1))) * Quaternion.FromToRotation(Vector3.forward, RHit.normal);
				
			} else {
				
				rightHandIK = false;
				
			}
			
			if(rightHandIK != leftHandIK){
				
				if(rightHandIK)
					leftOrigin.position = Vector3.MoveTowards(leftOrigin.position, rightOrigin.position, 0.01f);
				else
					rightOrigin.position = Vector3.MoveTowards(rightOrigin.position, leftOrigin.position, 0.01f);
				
			}else if(rightHandIK && leftHandIK){
				handDirectionRight = RHit.point - LHit.point;
				Debug.DrawRay(transform.position + transform.up, handDirectionRight * 2, Color.cyan);
			}
			
		} else {
			
			leftHandIK = false;
			rightHandIK = false;
			
		}
		
	}

	public void ResetHandSpacing(){
		
		StartCoroutine (HandResetWait ());
		
	}
	
	IEnumerator HandResetWait() {
		
		yield return new WaitForSeconds (0.3f);
		leftOrigin.localPosition = new Vector3 (leftOriginOriginalPos.x, leftOrigin.localPosition.y, leftOrigin.localPosition.z);
		rightOrigin.localPosition = new Vector3 (rightOriginOriginalPos.x, rightOrigin.localPosition.y, rightOrigin.localPosition.z);
		
	}

	public void SetIK(bool b){
	
		useIK = b;
		leftHandIK = b;
		rightHandIK = b;
	
	}

	public void NoSmoothing(){

		rightHandActual = rightHandPos;
		leftHandActual = leftHandPos;
		
	}

}
