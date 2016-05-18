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

	public void Setup(float speedX, float speedZ){

		anim.SetFloat (hash.velocityXFloat, speedX, speedDampTime, Time.deltaTime);
		anim.SetFloat (hash.velocityZFloat, speedZ, speedDampTime, Time.deltaTime);

	}

}
