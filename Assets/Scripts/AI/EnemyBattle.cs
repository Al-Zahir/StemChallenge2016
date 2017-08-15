using UnityEngine;
using System.Collections;

public class EnemyBattle : MonoBehaviour {

	public float maximumDamage = 120f;
	public float minimumDamage = 45f;

	private Animator anim;
	private UnityEngine.AI.NavMeshAgent agent;
	private SphereCollider col;
	private Transform player;
	private PlayerMovement playerMovement;
	private PlayerHealth playerHealth;
	private PlayerBattleControl playerBattleControl;
	public bool isInBattle;
	private float scaledDamage;

	public Transform swordUnarmedPosition;
	public Transform swordArmedPosition;
	public Transform sword;

	void Awake(){

		anim = GetComponent<Animator> ();
		agent = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		col = GetComponent<SphereCollider> ();
		player = GameObject.FindGameObjectWithTag (Tags.player).transform;
		playerMovement = player.gameObject.GetComponent<PlayerMovement> ();
		playerHealth = player.gameObject.GetComponent<PlayerHealth> ();
		playerBattleControl = player.GetComponent<PlayerBattleControl> ();

		scaledDamage = maximumDamage - minimumDamage;

	}

	void Update(){

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

	}

	public void Battle(){

		if (Vector3.Distance (transform.position, player.position) > 4 || playerMovement.isRunning) {

			if (anim.GetAnimatorTransitionInfo (0).fullPathHash == 0)
				agent.SetDestination (player.position);

		} else if (!isInBattle) {
			
			Debug.Log ("Start the Battle");
			
			isInBattle = true;
			playerBattleControl.Battle (true);
			anim.SetBool ("isInBattle", true);

			anim.SetTrigger ("Equip");
			
		
		} else {
		
			transform.LookAt (player.position);
			Debug.Log ("Battle");
		
		}

	}

	public void Equip(){

		sword.parent = swordArmedPosition;
		sword.localPosition = new Vector3 (0, 0, 0);
		sword.localRotation = Quaternion.identity;

	}

	public void Dequip(){

		sword.parent = swordUnarmedPosition;
		sword.localPosition = new Vector3 (0, 0, 0);
		sword.localRotation = Quaternion.identity;

	}
}
