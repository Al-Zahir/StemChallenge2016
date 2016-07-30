using UnityEngine;
using System.Collections;

public class WolfAI : MonoBehaviour {

	private Animator anim;
	private NavMeshAgent agent;

	public GameObject[] waterSpots;
	private bool isWalkingToWater;

	public bool spottedPlayer;

	public float scareFactor; //1-10, where 1 is least scared

	// Use this for initialization
	void Start () {

		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();

		waterSpots = GameObject.FindGameObjectsWithTag ("Water");
		isWalkingToWater = false;

		spottedPlayer = false;
		scareFactor = Random.Range (1, 10);
	
	}
	
	// Update is called once per frame
	void Update () {

		if (!spottedPlayer && !isWalkingToWater) {
		
			WalkToWater ();
		
		}

	
		if (agent.velocity.magnitude > 0 && anim.GetFloat ("Speed") < 0.01f)
			anim.SetFloat ("Speed", 1.0f);
		else if(agent.velocity.magnitude == 0 && anim.GetFloat ("Speed") > 0.01f)
			anim.SetFloat ("Speed", 0.0f);

		if (!agent.pathPending)
		{
			if (agent.remainingDistance <= agent.stoppingDistance)
			{
				if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
				{

					if (isWalkingToWater) {
					
						anim.SetBool ("isDrinking", true);
					
					}

				}
			}
		}
	}
		
	void WalkToWater(){

		isWalkingToWater = true;
		Vector3 position = Vector3.zero;

		Vector3 waterEdge = FindCloseWater ().transform.position;

		RaycastHit hit;
		while (Physics.Raycast (waterEdge + Vector3.up, Vector3.down, out hit, 2) && hit.transform.tag == "Water") {

			position = transform.position;
			position.y = waterEdge.y;

			waterEdge += (position - waterEdge).normalized * 0.1f;
		
		}

		agent.speed = 1;
		waterEdge += (position - waterEdge).normalized * agent.radius / 2;

		Physics.Raycast (waterEdge + Vector3.up, Vector3.down, out hit, 2);

		agent.SetDestination (hit.point);

	}

	GameObject FindCloseWater(){

		if (waterSpots.Length < 1)
			return null;

		GameObject closeWater = waterSpots[0];
		float minDistance = Vector3.Distance(transform.position, waterSpots[0].transform.position);

		foreach (GameObject g in waterSpots) {

			float newDistance = Vector3.Distance (transform.position, g.transform.position);

			if (newDistance < minDistance) {
			
				minDistance = newDistance;
				closeWater = g;
			
			}
		
		}

		return closeWater;

	}

}
