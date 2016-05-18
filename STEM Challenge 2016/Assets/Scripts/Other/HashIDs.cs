using UnityEngine;
using System.Collections;

public class HashIDs : MonoBehaviour {

	public int locomotionState;
	public int crouch2Stand;
	public int stand2Crouch;
	public int crouch2Sprint;
	public int sneakingState;
	public int speedFloat;
	public int sprintTriggerFloat;
	public int sneakBool;
	public int attractBool;
	public int velocityXFloat;
	public int velocityZFloat;
	public int isAimingBool;

	void Awake(){

		locomotionState = Animator.StringToHash ("Base Layer.Locomotion");
		crouch2Stand = Animator.StringToHash ("Base Layer.Crouch2Stand");
		stand2Crouch = Animator.StringToHash ("Base Layer.Stand2Crouch");
		crouch2Sprint = Animator.StringToHash ("Base Layer.Crouch2Sprint");
		sneakingState = Animator.StringToHash ("Base Layer.Sneaking");
		speedFloat = Animator.StringToHash ("Speed");
		sprintTriggerFloat = Animator.StringToHash ("Sprint Trigger");
		sneakBool = Animator.StringToHash ("Sneak");
		attractBool = Animator.StringToHash ("Attract");
		velocityXFloat = Animator.StringToHash ("VelocityX");
		velocityZFloat = Animator.StringToHash ("VelocityZ");
		isAimingBool = Animator.StringToHash ("isAiming");

	}

}
