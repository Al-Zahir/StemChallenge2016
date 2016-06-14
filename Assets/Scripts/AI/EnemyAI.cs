using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour {

	public float chaseWaitTime = 5f;
	public float patrolWaitTime = 1f;
	public float shotWaitTime = 1f;
	public float chaseSpeed = 3.5f;
	public float patrolSpeed = 1f;
	public Transform[] patrolWayPoints;
	public bool isDisabled;

	private Animator anim;
	private HashIDs hash;
	private EnemySight enemySight;
	private EnemyShooting enemyShooting;
	private NavMeshAgent agent;
	private Transform player;
	private PlayerHealth playerHealth;
	private LastPlayerSighting lastPlayerSighting;
	private PlayerLastSeen playerLastSeen;
	private float chaseTimer;
	private float patrolTimer;
	private float shotTimer;
	private int wayPointIndex;

	void Awake(){

		anim = GetComponent<Animator> ();
		hash = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs> ();
		enemySight = GetComponent<EnemySight> ();
		enemyShooting = GetComponent<EnemyShooting> ();
		agent = GetComponent<NavMeshAgent> ();
		player = GameObject.FindGameObjectWithTag (Tags.player).transform;
		playerHealth = player.GetComponent<PlayerHealth> ();
		lastPlayerSighting = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<LastPlayerSighting> ();
		playerLastSeen = player.GetComponent<PlayerLastSeen> ();

		isDisabled = false;

	}

	void Update(){

		if (!isDisabled) {
			if (enemySight.playerInSight && playerHealth.health > 0f) 
				Shooting ();
			else if (enemySight.personalLastSighting != lastPlayerSighting.resetPosition && playerHealth.health > 0f)
				Chasing ();
			else
				Patrolling ();
		} else 
			agent.Stop ();

	}

	void Shooting(){

		agent.Stop ();

		shotTimer += Time.deltaTime;

		if (shotTimer >= shotWaitTime) {
			
			shotTimer = 0f;
			enemyShooting.Shoot ();

		}

	}

	void Chasing(){

		agent.speed = chaseSpeed;

		Vector3 sightingDeltaPos = enemySight.personalLastSighting - transform.position;

		if (sightingDeltaPos.sqrMagnitude > 4f) {
			agent.Resume();
			agent.SetDestination (enemySight.personalLastSighting);
		}

		if (agent.remainingDistance < agent.stoppingDistance) {
		
			chaseTimer += Time.deltaTime;

			if (chaseTimer >= chaseWaitTime) {

				enemySight.personalLastSighting = lastPlayerSighting.resetPosition;
				playerLastSeen.Appeared();
				chaseTimer = 0f;

			}
		
		} else 
			chaseTimer = 0f;
	}

	void Patrolling(){

		agent.speed = patrolSpeed;

		if (agent.destination == lastPlayerSighting.resetPosition || agent.remainingDistance < agent.stoppingDistance) {
		
			patrolTimer += Time.deltaTime;

			if (patrolTimer >= patrolWaitTime) {

				if (wayPointIndex == patrolWayPoints.Length - 1)
					wayPointIndex = 0;
				else
					wayPointIndex++;

				patrolTimer = 0f;

			}
		
		} else
			patrolTimer = 0f;

		agent.Resume();
		agent.SetDestination(patrolWayPoints [wayPointIndex].position);

	}
}
