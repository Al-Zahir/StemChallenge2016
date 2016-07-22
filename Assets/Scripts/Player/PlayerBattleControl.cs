using UnityEngine;
using System.Collections;

public class PlayerBattleControl : MonoBehaviour {

	public bool isInBattle;
	public GameObject attacker;
	public float maxDistance = 5;

	private Animator anim;
	private Rigidbody rigid;
	private PlayerMovement playerMovement;
	private PlayerHealth playerHealth;

	public Transform swordUnarmedPosition;
	public Transform swordArmedPosition;
	public Transform sword;

	public Transform shieldUnarmedPosition;
	public Transform shieldArmedPosition;
	public Transform shield;

	void Awake(){

		isInBattle = false;
		attacker = null;

		anim = GetComponent<Animator> ();
		rigid = GetComponent<Rigidbody> ();
		playerMovement = GetComponent<PlayerMovement> ();
		playerHealth = GetComponent<PlayerHealth> ();

		playerMovement.isDisabledByBattle = isInBattle;

	}

	void Update(){

		//remove later
		if (Input.GetKeyDown (KeyCode.M))
			Battle (!isInBattle);

		if (anim.GetAnimatorTransitionInfo (1).fullPathHash == 0
		    && anim.GetCurrentAnimatorStateInfo (1).fullPathHash == Animator.StringToHash ("Equip Dequip.draw_sword_2"))
			Equip ();
		else if (anim.GetAnimatorTransitionInfo (1).fullPathHash == 0
		         && anim.GetCurrentAnimatorStateInfo (1).fullPathHash == Animator.StringToHash ("Equip Dequip.sheath_sword_2"))
			Dequip ();
		else if (anim.GetAnimatorTransitionInfo (1).fullPathHash == 0
		         && (anim.GetCurrentAnimatorStateInfo (1).fullPathHash == Animator.StringToHash ("Equip Dequip.draw_sword_1")
		         || anim.GetCurrentAnimatorStateInfo (1).fullPathHash == Animator.StringToHash ("Equip Dequip.sheath_sword_1")))
			anim.SetLayerWeight (1, 1f);

		if (isInBattle) {

			if (playerMovement.isRunning)
				Battle (false);

			if (!attacker)
				attacker = playerMovement.GetNearestEnemy ();

			if (Input.GetMouseButtonDown (0))
				anim.SetTrigger ("Attack");

			if (Input.GetKeyDown (KeyCode.Z))
				anim.SetBool ("Blocking", true);
			else if (Input.GetKeyUp (KeyCode.Z))
				anim.SetBool ("Blocking", false);

			if (anim.GetBool ("Blocking")) {

				Vector3 lookAtDirection = transform.forward;

				if (attacker)
					lookAtDirection = attacker.transform.position;
				
				lookAtDirection.y = transform.position.y;

				transform.LookAt (lookAtDirection);

				float h = Input.GetAxis ("Horizontal");
				float v = Input.GetAxis ("Vertical");

				if (h != 0 || v != 0) {

					Vector3 worldDirection = playerMovement.mainCam.transform.TransformDirection (new Vector3 (h, 0, v).normalized);
					worldDirection.Scale (new Vector3 (1, 0, 1));

					float angle = 0;
					angle = Vector3.Angle (transform.forward, worldDirection);

					if (Vector3.Cross (transform.forward, worldDirection).y < 0)
						angle = -angle;

					anim.SetFloat ("SpeedX", Mathf.Sin (angle * Mathf.Deg2Rad));
					anim.SetFloat ("SpeedZ", Mathf.Cos (angle * Mathf.Deg2Rad));

					rigid.velocity = worldDirection;
				
				} else if (anim.GetFloat ("SpeedX") != 0 || anim.GetFloat ("SpeedZ") != 0) {
				
					anim.SetFloat ("SpeedX", 0);
					anim.SetFloat ("SpeedZ", 0);

					rigid.velocity = new Vector3 (0, 0, 0);
				
				}
			}

		}
			
	}

	public void Battle(bool b){

		isInBattle = b;
		//playerMovement.isDisabledByBattle = isInBattle;
		anim.SetBool ("isInBattle", isInBattle);

		if (b)
			anim.SetTrigger ("Equip");
		else
			anim.SetTrigger ("Dequip");

	}

	public void Equip(){

		sword.parent = swordArmedPosition;
		sword.localPosition = new Vector3 (0, 0, 0);
		sword.localRotation = Quaternion.identity;

		shield.parent = shieldArmedPosition;
		shield.localPosition = new Vector3 (0, 0, 0);
		shield.localRotation = Quaternion.identity;

	}

	public void Dequip(){

		sword.parent = swordUnarmedPosition;
		sword.localPosition = new Vector3 (0, 0, 0);
		sword.localRotation = Quaternion.identity;

		shield.parent = shieldUnarmedPosition;
		shield.localPosition = new Vector3 (0, 0, 0);
		shield.localRotation = Quaternion.identity;

	}
}
