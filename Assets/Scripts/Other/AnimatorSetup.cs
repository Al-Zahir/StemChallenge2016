using UnityEngine;
using System.Collections;

public class AnimatorSetup{

	public float speedDampTime = 0.1f;

	private Animator anim;
	private HashIDs hash;

	public AnimatorSetup(Animator animator, HashIDs hashIDs){
	
		anim = animator;
		hash = hashIDs;
	
	}

	public void Setup(float speed, float angle, bool isRunning){

		anim.SetFloat ("Speed", speed, speedDampTime, Time.deltaTime);
		anim.SetFloat ("SpeedX", speed * Mathf.Sin(angle * Mathf.Rad2Deg), speedDampTime, Time.deltaTime);
		anim.SetFloat ("SpeedZ", speed * Mathf.Cos(angle * Mathf.Rad2Deg), speedDampTime, Time.deltaTime);

		if (isRunning)
			anim.SetFloat ("SprintTrigger", 1, speedDampTime, Time.deltaTime);
		else
			anim.SetFloat ("SprintTrigger", 0, speedDampTime, Time.deltaTime);

	}

}
