using UnityEngine;
using System.Collections;

public class IKSnap : MonoBehaviour {

	public bool useIK;

	public bool leftHandIK;
	public bool rightHandIK;

	public Transform leftOrigin;
	public Transform rightOrigin;

	private Vector3 leftHandPos;
	private Vector3 rightHandPos;

	private Quaternion leftHandRot;
	private Quaternion rightHandRot;

	public Vector3 leftHandOffset;
	public Vector3 rightHandOffset;

	private Animator anim;

	void Awake(){

		anim = GetComponent<Animator> ();

	}

	void FixedUpdate(){

		RaycastHit RHit;
		RaycastHit LHit;


		//if (Physics.Raycast (transform.position + new Vector3 (0.0f, 2.0f, 0.5f), -transform.up + new Vector3 (-0.5f, 0.0f, 0.0f), out LHit, 1f)) {
		if (Physics.Raycast (leftOrigin.position, -transform.up, out LHit, 1f)) {
			leftHandIK = true;
			leftHandPos = LHit.point + leftHandOffset;

			leftHandRot = Quaternion.FromToRotation(Vector3.forward, LHit.normal);
		
		} else {
		
			leftHandIK = false;
		
		}

		//if (Physics.Raycast (transform.position + new Vector3 (0.0f, 2.0f, 0.5f), -transform.up + new Vector3 (0.5f, 0.0f, 0.0f), out RHit, 1f)) {
		if (Physics.Raycast (rightOrigin.position, -transform.up, out RHit, 1f)) {	
			rightHandIK = true;
			rightHandPos = RHit.point + rightHandOffset;

			rightHandRot = Quaternion.FromToRotation(Vector3.forward, RHit.normal);
			
		} else {
			
			rightHandIK = false;
			
		}

	}

	void Update(){

		//Debug.DrawRay (transform.position + new Vector3 (0.0f, 2.0f, 0.5f), -transform.up + new Vector3 (-0.5f, 0.0f, 0.0f), Color.green);
		//Debug.DrawRay (transform.position + new Vector3 (0.0f, 2.0f, 0.5f), -transform.up + new Vector3 (0.5f, 0.0f, 0.0f), Color.green);
		Debug.DrawRay (leftOrigin.position, -transform.up, Color.green);
		Debug.DrawRay (rightOrigin.position, -transform.up, Color.green);

	}

	void OnAnimatorIK(){

		if (useIK) {
		
			if(leftHandIK){

				anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
				anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos);

				anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
				anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandRot);

			}

			if(rightHandIK){
				
				anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
				anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos);

				anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
				anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandRot);
				
			}
		
		}

	}
}
