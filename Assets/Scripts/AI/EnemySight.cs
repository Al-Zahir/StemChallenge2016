using UnityEngine;
using System.Collections;

public class EnemySight : MonoBehaviour {

	public float fieldOfViewAngle = 110f;
	public bool playerInSight;
	public Vector3 personalLastSighting;

	private NavMeshAgent agent;
	private SphereCollider col;
	private LastPlayerSighting lastPlayerSighting;
	public GameObject player;
	private Animator playerAnim;
	private PlayerHealth playerHealth;
	private HashIDs hash;
	private PlayerLastSeen playerLastSeen;

	void Awake(){

		agent = GetComponent<NavMeshAgent> ();
		col = GetComponent<SphereCollider> ();
		lastPlayerSighting = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<LastPlayerSighting> ();
		player = GameObject.FindGameObjectWithTag ("Player").gameObject;
		playerAnim = player.GetComponent<Animator> ();
		playerHealth = player.GetComponent<PlayerHealth> ();
		hash = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs> ();
		personalLastSighting = lastPlayerSighting.resetPosition;
		playerLastSeen = player.GetComponent<PlayerLastSeen> ();

	}

	void Update(){

		if (playerHealth.health <= 0f)
			playerInSight = false;

		Debug.DrawRay(transform.position, col.radius * (transform.forward + Mathf.Tan (55 * Mathf.Deg2Rad) * transform.right).normalized, Color.blue);
		Debug.DrawRay(transform.position, col.radius * (transform.forward - Mathf.Tan (55 * Mathf.Deg2Rad) * transform.right).normalized, Color.blue);

	}

	void OnTriggerStay(Collider other){

		if (other.gameObject == player) {

			bool wasInSight = playerInSight;

			playerInSight = false;

			Vector3 direction = other.transform.position - transform.position;
			float angle = Vector3.Angle(direction, transform.forward);

			if (angle <= fieldOfViewAngle * 0.5f){

				RaycastHit hit;

				if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius)){

					if (hit.collider.gameObject == player){

						playerInSight = true;

						playerLastSeen.Appeared();

						personalLastSighting = player.transform.position;

					}

				}

			}

			if(wasInSight && !playerInSight)
				playerLastSeen.Disappeared();

			/*if (playerAnim.GetBool(hash.attractBool)){

				if (CalculatePathLength(player.transform.position) <= col.radius)
					personalLastSighting = player.transform.position;

			}*/
		
		}

	}

	void OnTriggerExit(Collider other){

		if (other.gameObject == player) {
			if(playerInSight)
				playerLastSeen.Disappeared();

			playerInSight = false;
		}

	}

	float CalculatePathLength(Vector3 targetPosition){

		NavMeshPath path = new NavMeshPath ();

		if (agent.enabled) 
			agent.CalculatePath (targetPosition, path);

		Vector3[] waypoints = new Vector3[path.corners.Length + 2];
		waypoints [0] = transform.position;
		waypoints [waypoints.Length - 1] = targetPosition;

		for (int i = 0; i < path.corners.Length; i++) 
			waypoints [i + 1] = path.corners [i];

		float pathLength = 0;

		for (int i = 0; i < waypoints.Length - 1; i++)
			pathLength += Vector3.Distance (waypoints[i], waypoints[i + 1]);

		return pathLength;

	}
}
