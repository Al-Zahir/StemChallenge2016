using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	public AudioClip whistleClip;
	public float turnSmoothing = 15f;
	public float speedDampTime = 0.1f;

	public bool isDisabledByClimb;
	public bool isDisabledByAttack;
	public bool isDisabledByGround;
	public bool isDisabledByBattle;
	public bool isDisabledByArchery;
	public bool isDisabledByPushing;

	public bool isRunning;

	public Camera mainCam;

	private Animator anim;
	private CapsuleCollider col;
	private PlayerClimbingControl playerClimbingControl;
	private PlayerFallingControl playerFallingControl;
	//private HashIDs hash;
	private Rigidbody rigid;
	private AudioSource footstepsAudio;

	private bool isAbleToMove;

	public Vector3 eulerAngles;
	public float eulerYTarget;
	public float eulerYVelocity;
	public Vector3 direction;
	public float angle;

	void Awake(){

		anim = GetComponent<Animator> ();
		col = GetComponent<CapsuleCollider> ();
		//hash = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs> ();
		rigid = GetComponent<Rigidbody> ();
		footstepsAudio = GetComponent<AudioSource> ();
		playerClimbingControl = GetComponent<PlayerClimbingControl> ();
		playerFallingControl = GetComponent<PlayerFallingControl> ();

		isDisabledByAttack = false;

	}	

	void FixedUpdate(){

		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");
		//bool sneak = anim.GetBool (hash.sneakBool);

		if (isAbleToMove && anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Locomotion"))
			MovementManagement (h, v, false);
		else {
			anim.SetFloat ("Speed", 0.0f, speedDampTime, Time.deltaTime);
			anim.SetFloat ("SprintTrigger", 0.0f, speedDampTime, Time.deltaTime);
		}

		/*if (!isAbleToMove) {
		
			if (!isDisabledByAttack && isDisabledByGround && !isDisabledByClimb) {
			
				Vector3 direction = new Vector3 (h, 0, v).normalized;	
				Vector3 worldDirection = mainCam.transform.TransformDirection (direction);

				worldDirection.Scale (new Vector3 (1, 0, 1));

				if (playerFallingControl.isGrounded (worldDirection)) {
				
					MovementManagement (h, v, false);
				
				}
			
			}
		
		}*/

	}

	void Update(){

		isRunning = anim.GetFloat ("SprintTrigger") > 0.1f;

		isAbleToMove = canMove ();

		MakeAngle ();

		//bool whistle = (Input.GetKeyDown (KeyCode.X) && anim.GetBool(hash.sneakBool));

		//AudioManagement (whistle);

	}

	void MovementManagement(float h, float v, bool sneak){
		if (h != 0 || v != 0) {
			
			direction = new Vector3 (h, 0, v).normalized;
			Rotating (direction);
			anim.SetFloat ("Speed", 1.0f, speedDampTime, Time.deltaTime);
			
			rigid.velocity = transform.forward * 1.58f + transform.up * rigid.velocity.y;

			if (Input.GetKey (KeyCode.LeftShift)) {
				anim.SetFloat ("SprintTrigger", 1.0f, speedDampTime, Time.deltaTime);

				rigid.velocity = transform.forward * 4.765f + transform.up * rigid.velocity.y;
			}else
				anim.SetFloat ("SprintTrigger", 0.0f, speedDampTime, Time.deltaTime);
			
		} else {
			anim.SetFloat ("Speed", 0.0f, speedDampTime, Time.deltaTime);
			rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
		}

		/*if (anim.GetCurrentAnimatorStateInfo (0).fullPathHash == hash.crouch2Sprint) {
		
			sneak = false;
			anim.SetBool(hash.sneakBool, sneak);
		
		}

		if (Input.GetKeyDown (KeyCode.C))
			anim.SetBool(hash.sneakBool, !sneak);*/

		if (Input.GetKeyDown (KeyCode.Space)) {
		
			GameObject enemy = GetNearestEnemy();

			if (enemy != null && Vector3.Distance(enemy.transform.position, transform.position) < 2f){

				Attack(enemy);

			}
		
		}
	}

	void Rotating(Vector3 targetDirection){

		/*Quaternion targetRotation = Quaternion.LookRotation (targetDirection, Vector3.up);

		Quaternion newRotation = Quaternion.Lerp (rigid.rotation, targetRotation, turnSmoothing * Time.deltaTime);

		rigid.MoveRotation (newRotation);*/

        if (isDisabledByClimb)
            return;

		if (targetDirection != Vector3.zero && anim.GetCurrentAnimatorStateInfo (0).fullPathHash == Animator.StringToHash ("Base Layer.Locomotion"))
			eulerYTarget = eulerAngles.y + angle;
		
		eulerAngles.y = Mathf.SmoothDampAngle (eulerAngles.y, eulerYTarget, ref eulerYVelocity, 0.1f);
		transform.eulerAngles = eulerAngles;
	}

	void MakeAngle(){

		Vector3 worldDirection = mainCam.transform.TransformDirection (direction);

		worldDirection.Scale (new Vector3 (1, 0, 1));
		angle = 0;
		angle = Vector3.Angle (transform.forward, worldDirection);

		if (Vector3.Cross (transform.forward, worldDirection).y < 0)
			angle = -angle;

	}

	/*void AudioManagement(bool whistle){

		if (anim.GetFloat (hash.speedFloat) > 0.3f && anim.GetFloat(hash.sprintTriggerFloat) > 0.3f) {

			if (!footstepsAudio.isPlaying)
				footstepsAudio.Play ();

		} else 
			footstepsAudio.Stop ();

		if (whistle)
			AudioSource.PlayClipAtPoint (whistleClip, transform.position);

		anim.SetBool(hash.attractBool, whistle);

	}*/

	public GameObject GetNearestEnemy(){

		GameObject[] enemies = GameObject.FindGameObjectsWithTag (Tags.enemy);

		float distance = Mathf.Infinity;
		int index = 0;

		for (int i = 0; i < enemies.Length; i++) {
		
			float newDistance = Vector3.Distance(enemies[i].transform.position, transform.position);

			if (newDistance < distance){

				distance = newDistance;
				index = i;

			}
		
		}

		if (index < enemies.Length)
			return enemies [index];
		
		return null;
	}

	void Attack(GameObject enemy){

		isDisabledByAttack = true;
		enemy.GetComponent<EnemyAI> ().isDisabled = true;
		enemy.GetComponentInChildren<CapsuleCollider> ().enabled = false;

		transform.LookAt (enemy.transform.position);

		enemy.transform.position = transform.position + 0.5f * transform.forward;
		enemy.transform.LookAt (transform.position + 2f * transform.forward);

		anim.SetBool ("Attack", true);
		if (anim.GetFloat ("SprintTrigger") > 0.1f) {
			enemy.GetComponent<Animator> ().SetTrigger ("isAttackRun");
			StartCoroutine (ResetAttack(enemy, 4.6f));
		} else {
			enemy.GetComponent<Animator> ().SetTrigger ("isAttackStand");
			StartCoroutine (ResetAttack(enemy, 1.5f));
		}

	}

	IEnumerator ResetAttack(GameObject enemy, float time){
	
		yield return new WaitForSeconds(time);

		anim.SetBool ("Attack", false);

		enemy.GetComponent<EnemyHealth> ().TakeDamage (enemy.GetComponent<EnemyHealth>().health);

		isDisabledByAttack = false;
	
		Destroy (enemy);
	}

	public bool canMove(){

		bool flag = !isDisabledByAttack && 
					!isDisabledByGround && 
					!isDisabledByClimb && 
					!isDisabledByBattle && 
					!isDisabledByArchery && 
					!isDisabledByPushing;

		if(isAbleToMove != flag)
			rigid.velocity = new Vector3 (0, rigid.velocity.y, 0);

		return flag;

	}

}
