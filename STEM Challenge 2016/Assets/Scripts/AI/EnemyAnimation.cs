using UnityEngine;
using System.Collections;

public class EnemyAnimation : MonoBehaviour {

	public float deadZone = 5f;

	private Transform player;
	private EnemySight enemySight;
	private NavMeshAgent agent;
	private Animator anim;
	private HashIDs hash;
	private AnimatorSetup animSetup;

	void Awake(){

		player = GameObject.FindGameObjectWithTag (Tags.player).transform;
		enemySight = GetComponent<EnemySight> ();
		agent = GetComponent<NavMeshAgent> ();
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

		float speed;
		float angle;

		if (enemySight.playerInSight) {
		
			speed = 0f;
			angle = FindAngle (transform.forward, player.position - transform.position, transform.up);
		
		} else {
		
			speed = Vector3.Project(agent.desiredVelocity, transform.forward).magnitude;
			angle = FindAngle(transform.forward, agent.desiredVelocity, transform.up);

			if (Mathf.Abs(angle) < deadZone){

				transform.LookAt(transform.position + agent.desiredVelocity);
				angle = 0f;

			}
		
		}

		animSetup.Setup (speed * Mathf.Sin(angle * Mathf.Rad2Deg), speed * Mathf.Cos(angle * Mathf.Rad2Deg));

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
