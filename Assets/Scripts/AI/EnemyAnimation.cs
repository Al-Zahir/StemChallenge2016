using UnityEngine;
using System.Collections;

public class EnemyAnimation : MonoBehaviour {

	public float deadZone = 5f;

	private Transform player;
	private EnemyAI enemyAI;
	private EnemyBattle enemyBattle;
	private EnemySight enemySight;
	private UnityEngine.AI.NavMeshAgent agent;
	private Animator anim;
	private HashIDs hash;
	private AnimatorSetup animSetup;

	void Awake(){

		player = GameObject.FindGameObjectWithTag (Tags.player).transform;
		enemyAI = GetComponent<EnemyAI> ();
		enemyBattle = GetComponent<EnemyBattle> ();
		enemySight = GetComponent<EnemySight> ();
		agent = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		anim = GetComponent<Animator> ();
		hash = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs> ();

		//agent.updateRotation = false;

		animSetup = new AnimatorSetup (anim, hash);

		deadZone *= Mathf.Deg2Rad;

	}

	void Update(){

		NavAnimSetup ();

	}

	void OnAnimatorMove(){

		//agent.velocity = anim.deltaPosition / Time.deltaTime;

		//transform.rotation = anim.rootRotation;

	}

	void NavAnimSetup(){

		float speed = agent.desiredVelocity.magnitude;
		float angle;

		if (enemySight.playerInSight) 
			angle = FindAngle (transform.forward, player.position - transform.position, transform.up);
		else {
			
			angle = FindAngle(transform.forward, agent.desiredVelocity, transform.up);

			if (Mathf.Abs(angle) < deadZone){

				transform.LookAt(transform.position + agent.desiredVelocity);
				angle = 0f;

			}
		
		}

		animSetup.Setup (speed, angle, (agent.speed == enemyAI.chaseSpeed));

	}

	float FindAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector){

		if (toVector == Vector3.zero)
			return 0f;

		float angle = Vector3.Angle (fromVector, toVector);

		Vector3 normal = Vector3.Cross (fromVector, toVector);

		angle *= Mathf.Sign (Vector3.Dot(normal, upVector));

		angle *= Mathf.Deg2Rad;

		return angle;

	}


}
