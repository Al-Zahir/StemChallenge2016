using UnityEngine;
using System.Collections;

public class WolfAI : MonoBehaviour {

    public bool debug = true;
	private Animator anim;
	private NavMeshAgent agent;
    private WolfBase control;

	public GameObject[] waterSpots;
	private bool isWalkingToWater;

	public bool spottedPlayer;
	public float scareFactor; //1-10, where 1 is least scared
    public float waterLevel = 100, waterCriticalLevel = 20, waterLevelFillRate = 5, waterLevelDrainRate = 1;
    public float leisureActionChangeTime = 10f;
    private float leisureActionStart = float.NegativeInfinity;
    private int leisureActionID;
    private bool leisureInitialized = false;

    public string[] drinkingAnims;

    public bool busySurvival;
    private Transform playerTransform;
    private bool playerInCone;

	// Use this for initialization
	void Start () {

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        control = GetComponent<WolfBase>();

		waterSpots = GameObject.FindGameObjectsWithTag ("Water");
		isWalkingToWater = false;

		spottedPlayer = false;
		scareFactor = Random.Range (1, 11);

        waterLevel = Random.Range(50f, 100f);
	}
	
    void FixedUpdate()
    {
        if (playerInCone)
        {
            spottedPlayer = CanSee(playerTransform);
        }
    }

	// Update is called once per frame
	void Update () 
    {
        UpdateSurvival();
        UpdateLeisure();
	}

    void UpdateSurvival()
    {
        busySurvival = false;

        if (anim.GetBool("isDrinking") || Mecanim.inAnyAnim(anim, drinkingAnims, 0))
        {
            busySurvival = true;
            waterLevel += waterLevelFillRate * Time.deltaTime;
            if (waterLevel >= 100)
            {
                control.CancelAction();
                isWalkingToWater = false;
            }
        }
        else
            waterLevel -= waterLevelDrainRate * Time.deltaTime;

        if (waterLevel < waterCriticalLevel)
        {
            busySurvival = true;
            if (!spottedPlayer && !isWalkingToWater)
                WalkToWater();
            else if (control.AtDestination() && isWalkingToWater)
            {
                anim.SetBool("isDrinking", true);
                control.SetAgentSpeed(0);
            }
        }
    }

    void UpdateLeisure()
    {
        if (busySurvival)
            return;

        float leisureTime = Time.time - leisureActionStart;

        if (leisureTime > leisureActionChangeTime)
        {
            leisureActionID = Random.Range(0, 2);
            leisureInitialized = false;
            switch (leisureActionID)
            {
                case 0:
                    // Idle
                    control.CancelAction();
                    control.SetAgentSpeed(0);
                    break;
                case 1:
                    // Random walk or run
                    control.SetAgentSpeed(Random.Range(0,1f) > 0.5 ? control.velWalk : control.velRun);
                    break;
            }

            leisureActionStart = Time.time;
        }

        // Random walk or run
        if(leisureActionID == 1)
        {
            RaycastHit forwardHit, destHit;
            Vector3 targetPos = new Vector3(0, 0, 3);

            int numTried = 0;
            bool hitForward = Physics.Raycast(transform.TransformPoint(targetPos) + 5f * Vector3.up, Vector3.down, out forwardHit, 12f);
            bool hitDest = Physics.Raycast(agent.destination + 5f * Vector3.up, Vector3.down, out destHit, 12f);

            if(!leisureInitialized)
            {
                hitForward = false;
                hitDest = false;
            }

            if (!hitDest || Vector3.Distance(destHit.point, transform.position) < 1f)
            {
                while (numTried < 20 && (!hitForward || forwardHit.transform != null && forwardHit.transform.tag == "Water"))
                {
                    // Nothing in front, start to change direction
                    float randAngle = Random.Range(0, 360);
                    targetPos = Vector3.RotateTowards(targetPos, -targetPos, randAngle, 0);
                    hitForward = Physics.Raycast(transform.TransformPoint(targetPos) + 5f * Vector3.up, Vector3.down, out forwardHit, 12f);
                    numTried++;
                }

                control.SetDestination(transform.TransformPoint(targetPos), true);
            }

            if (numTried >= 20)
            {
                control.SetAgentSpeed(0);
                leisureActionID = 0;
                return;
            }

            DrawRay(transform.TransformPoint(targetPos) + 5f * Vector3.up, Vector3.down * 12f, Color.green);

            leisureInitialized = true;
        }
    }

    void ForwardVisionTriggerEnter(Collider other)
    {
        if (other.transform.name == "Player")
        {
            playerInCone = true;
            playerTransform = other.transform;
        }
    }

    void ForwardVisionTriggerExit(Collider other)
    {
        if (other.transform.name == "Player")
        {
            playerInCone = false;
            spottedPlayer = false;
        }
    }
		
	void WalkToWater(){
		Vector3 position = Vector3.zero;

        GameObject closestWater = FindCloseWater();
        if (closestWater == null)
            return;

        Vector3 waterEdge = closestWater.transform.position;

		RaycastHit hit;
		while (Physics.Raycast (waterEdge + Vector3.up, Vector3.down, out hit, 2) && hit.transform.tag == "Water") {

			position = transform.position;
			position.y = waterEdge.y;

			waterEdge += (position - waterEdge).normalized * 0.1f;
		
		}

		//waterEdge += (position - waterEdge).normalized * agent.radius / 2;

        /*if (!Physics.Raycast(waterEdge + Vector3.up, Vector3.down, out hit, 2))
            return;
        */

        isWalkingToWater = true;
        control.SetAgentSpeed(control.velRun);
        control.SetDestination(waterEdge, true);

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

    public bool CanSee(Transform t)
    {
        NavMeshHit hit;

        return !NavMesh.Raycast(transform.position + 2 * transform.up, t.position + 2 * transform.up, out hit, NavMesh.AllAreas)
            && Vector3.Angle(transform.forward, t.position - transform.position) < 80;
    }

    private void DrawRay(Vector3 position, Vector3 direction, Color color)
    {
        if (debug)
            Debug.DrawRay(position, direction, color);
    }

}
