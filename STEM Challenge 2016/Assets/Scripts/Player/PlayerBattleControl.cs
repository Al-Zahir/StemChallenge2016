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

		//Remove this later
		if (Input.GetKeyDown (KeyCode.M)) {
			if (!isInBattle)
				Battle (true);
			else
				Battle (false);
		}
		//End of Removing

		if (isInBattle) {

			if (Input.GetKey(KeyCode.LeftShift))
				Battle (false);

			if (!attacker || Vector3.Distance (attacker.transform.position, transform.position) > maxDistance) {
				attacker = playerMovement.GetNearestEnemy ();

				if (!attacker || Vector3.Distance(attacker.transform.position, transform.position) > maxDistance) {
					Battle (false);
					return;
				}

			}

			Vector3 lookAtDirection = attacker.transform.position;
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

				rigid.velocity = Vector3.forward * worldDirection.z + Vector3.right * worldDirection.x;
			
			} else if (anim.GetFloat ("SpeedX") != 0 || anim.GetFloat ("SpeedZ") != 0) {
			
				anim.SetFloat ("SpeedX", 0);
				anim.SetFloat ("SpeedZ", 0);

				rigid.velocity = new Vector3(0, 0, 0);
			
			}

		}
			
	}

	public void Battle(bool b){

		isInBattle = b;
		playerMovement.isDisabledByBattle = isInBattle;
		anim.SetBool ("isInBattle", isInBattle);

	}
}
