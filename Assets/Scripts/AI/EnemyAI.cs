using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour {

	public float chaseWaitTime = 5f;
	public float patrolWaitTime = 1f;
	public float shotWaitTime = 1f;

	public float chaseSpeed = 3.5f;
	public float suspisionSpeed = 1f;
	public float patrolSpeed = 1f;

	public Transform[] patrolWayPoints;
	public bool isDisabled;

	private Animator anim;
	private HashIDs hash;
	private EnemySight enemySight;
	private EnemyBattle enemyBattle;
	//private EnemyShooting enemyShooting;
	private NavMeshAgent agent;
	private Transform player;
	private PlayerHealth playerHealth;
	private LastPlayerSighting lastPlayerSighting;
	private PlayerLastSeen playerLastSeen;
	private PlayerBattleControl playerBattle;

	private float chaseTimer;
	private float patrolTimer;
	private float shotTimer;
	private int wayPointIndex;

	public AudioClip aware;
	public AudioClip spotted;
	public AudioClip blending;
	public AudioClip escape;

	private bool didPlayAware;
	private bool didPlaySpotted;
	private bool didPlayBlending;

	public float suspision = 0f;

	void Awake(){

		anim = GetComponent<Animator> ();
		hash = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs> ();
		enemySight = GetComponent<EnemySight> ();
		enemyBattle = GetComponent<EnemyBattle> ();
		//enemyShooting = GetComponent<EnemyShooting> ();
		agent = GetComponent<NavMeshAgent> ();
		player = GameObject.FindGameObjectWithTag (Tags.player).transform;
		playerHealth = player.GetComponent<PlayerHealth> ();
		lastPlayerSighting = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<LastPlayerSighting> ();
		playerLastSeen = player.GetComponent<PlayerLastSeen> ();
		playerBattle = player.GetComponent<PlayerBattleControl> ();

		isDisabled = false;

		didPlayAware = false;
		didPlaySpotted = false;
		didPlayBlending = false;
	}

	void Update(){

		if (!isDisabled) {
			if (enemySight.playerInSight && playerHealth.health > 0f) 
				Battling ();
			else if (enemySight.personalLastSighting != lastPlayerSighting.resetPosition && playerHealth.health > 0f && suspision > 0)
				Chasing ();
			else
				Patrolling ();
		} else 
			agent.Stop ();

		if (!enemySight.playerInSight && (didPlayAware || didPlaySpotted)) {
		
			AudioSource.PlayClipAtPoint (blending, player.position);
			didPlayAware = false;
			didPlaySpotted = false;
			didPlayBlending = true;
		
		}

	}

	void Shooting(){

		agent.Stop ();

		shotTimer += Time.deltaTime;

		if (shotTimer >= shotWaitTime) {
			
			shotTimer = 0f;
//			enemyShooting.Shoot ();

		}

	}

	void Battling(){

		if (suspision == 10)
			enemyBattle.Battle ();
		else if (suspision > 10 || playerBattle.isInBattle)
			suspision = 10;
		else {
			if (!didPlayAware) {
			
				AudioSource.PlayClipAtPoint (aware, player.position);
				didPlayAware = true;
			
			}

			suspision += playerHealth.notority * Time.deltaTime;
		}

		if (suspision < 10)
			agent.speed = suspisionSpeed;
		else {

			if(!didPlaySpotted){

				AudioSource.PlayClipAtPoint (spotted, player.position);
				didPlaySpotted = true;

			}

			agent.speed = chaseSpeed;

		}

		if (Vector3.Distance (transform.position, player.position) > 5 && suspision > 0)
			agent.SetDestination (player.position);

	}

	void Chasing(){

		if (suspision < 10)
			agent.speed = suspisionSpeed;
		else
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
				suspision = 0f;

			}
		
		} else 
			chaseTimer = 0f;
	}

	void Patrolling(){

		if (didPlayBlending) {
		
			AudioSource.PlayClipAtPoint (escape, player.position);
			didPlayBlending = false;
		
		}

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