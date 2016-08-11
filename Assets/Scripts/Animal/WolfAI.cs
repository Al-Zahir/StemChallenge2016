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
    public string[] blockingAnims;
    public float shieldAngle = 30;

    public bool busySurvival;
    private Transform playerTransform;
    private bool playerInCone;

    public float escapeRadius = 20;

    private bool startedRunningAway = false;

    private bool spottedPlayerDelayed = false;
    private float timeLostPlayer;

    public float removeTime = 0;

    private bool dead;

    private float waterStartTime;
    public float biteDistance = 2;
    public float biteDamage = 30;

	// Use this for initialization
	void Start () {

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        control = GetComponent<WolfBase>();

		waterSpots = GameObject.FindGameObjectsWithTag ("Water");
		isWalkingToWater = false;

		spottedPlayer = false;
        scareFactor = Random.Range(1, 11);

        waterLevel = Random.Range(50f, 100f);

        StartCoroutine(AttackPlayer());
	}

    private IEnumerator AttackPlayer()
    {
        while(!dead)
        {
            if (playerTransform != null && spottedPlayer && scareFactor <= 5)
            {
                Vector3 noYPos = transform.position;
                noYPos.y = playerTransform.position.y;
                if (Vector3.Distance(noYPos, playerTransform.position) < biteDistance && Mathf.Abs(transform.position.y - playerTransform.position.y) < 3)
                {
                    float angle = Vector3.Angle(-transform.forward, playerTransform.forward);
                    if (!Mecanim.inAnyAnim(playerTransform.GetComponent<Animator>(), blockingAnims, 1) || angle > shieldAngle)
                        playerTransform.GetComponent<PlayerHealth>().TakeDamage(biteDamage);
                }
            }

            yield return new WaitForSeconds(1);
        }
    }
	
    void FixedUpdate()
    {
        if (playerInCone)
        {
            spottedPlayer = CanSee(playerTransform);
        }

        if (spottedPlayer)
        {
            spottedPlayerDelayed = true;
            timeLostPlayer = -1;
        }
        else if (spottedPlayerDelayed && timeLostPlayer == -1)
        {
            timeLostPlayer = Time.time;
        }
        else if (Time.time > timeLostPlayer + 5 && timeLostPlayer != -1)
        {
            spottedPlayerDelayed = false;
            timeLostPlayer = -1;
        }
    }

	// Update is called once per frame
	void Update () 
    {
        if (dead)
            return;

        UpdateSurvival();
        UpdateLeisure();
	}

    void UpdateSurvival()
    {
        busySurvival = false;

        if (waterLevel < 0 && !dead)
        {
            dead = true;
            GetComponent<DroneHealth>().health = 0;
            GetComponent<DroneHealth>().OnHit(null);
        }

        if (startedRunningAway)
        {
            if (control.AtDestination())
            {
                busySurvival = false;
                startedRunningAway = false;
            }
            else
            {
                busySurvival = true;
            }
        }

        control.lookAtPos = Vector3.zero;
        
        if (anim.GetBool("isDrinking") || Mecanim.inAnyAnim(anim, drinkingAnims, 0))
        {
            busySurvival = true;
            waterLevel += waterLevelFillRate * Time.deltaTime;
            if (waterLevel >= 100)
            {
                control.CancelAction();
                isWalkingToWater = false;
            }
            return;
        }
        else
            waterLevel -= waterLevelDrainRate * Time.deltaTime;

        if (waterLevel < waterCriticalLevel)
        {
            busySurvival = true;
            if (!isWalkingToWater)
                WalkToWater();
            else if (control.AtDestination() && isWalkingToWater && Time.time > waterStartTime + 0.2f)
            {
                anim.SetBool("isDrinking", true);
                control.SetAgentSpeed(0);
            }
        }
        else if (scareFactor > 5 && (!startedRunningAway && spottedPlayerDelayed))
        {
            RaycastHit[] hits;
            bool hitWater = true;
            bool hitAnything = false;
            int numTried = 0;
            Vector3 targetPos = (transform.position - playerTransform.position).normalized * escapeRadius;
            targetPos.y = transform.position.y;
            //Debug.DrawRay(targetPos, Vector3.down * 100, Color.red, 10f);

            while (numTried < 20 && (!hitAnything || hitWater))
            {
                // Nothing in front, start to change direction
                float randAngle = Random.Range(0, 2 * Mathf.PI);
                targetPos = transform.TransformVector(Vector3.RotateTowards(transform.InverseTransformVector(targetPos), -transform.InverseTransformVector(targetPos), randAngle, 0));
                //Debug.DrawRay(targetPos + Vector3.up * 100f, Vector3.down * 100, Color.red, 10f);
                targetPos.y = transform.position.y;
                hits = Physics.RaycastAll(targetPos + Vector3.up * 100f, Vector3.down, 1000f);
                hitAnything = hits.Length > 0;
                hitWater = false;
                foreach (RaycastHit hit in hits)
                    if (hit.transform.tag == "Water")
                    {
                        hitWater = true;
                        break;
                    }

                numTried++;
            }

            if (numTried < 20)
            {
                startedRunningAway = true;
                control.SetDestination(targetPos, true);
                control.SetAgentSpeed(control.velRun);
            }
        }
        else if (scareFactor <= 5 && spottedPlayerDelayed)
        {
            busySurvival = true;
            control.SetDestination(playerTransform.position, true);
            control.SetAgentSpeed(control.velRun);
            if (Vector3.Distance(transform.position, playerTransform.position) < 2f)
            {
                control.SetAgentSpeed(0);
            }
            control.SetBite(true);
            control.lookAtPos = playerTransform.position;
        }

        if (!spottedPlayerDelayed)
        {
            control.SetBite(false);
        }
        //Debug.DrawRay(agent.destination + Vector3.up * 100f, Vector3.down * 100, Color.red, 10f);
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
            Vector3 targetPos = new Vector3(0, 0, 2);

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
                hitForward = false;
                while (numTried < 20 && (forwardHit.transform != null && forwardHit.transform.tag == "Water" || !hitForward))
                {
                    // Nothing in front, start to change direction
                    float randAngle = Random.Range(0, 2 * Mathf.PI);
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
        startedRunningAway = false;

        GameObject closestWater = FindCloseWater();
        if (closestWater == null)
            return;

        Vector3 waterEdge = transform.position;
        Vector3 inc = (closestWater.transform.position - transform.position).normalized * 0.5f;
        inc.y = 0;

		RaycastHit hit;
		while (Physics.Raycast (waterEdge + 100*Vector3.up, Vector3.down, out hit, 1000) && hit.transform.tag != "Water") {
            waterEdge += inc;
            DrawRay(waterEdge + 100*Vector3.up, 1000*Vector3.down, Color.red, 5f);
		}

		//waterEdge += (position - waterEdge).normalized * agent.radius / 2;

        /*if (!Physics.Raycast(waterEdge + Vector3.up, Vector3.down, out hit, 2))
            return;
        */

        isWalkingToWater = true;
        control.SetAgentSpeed(control.velRun);
        control.SetDestination(waterEdge, true);
        waterStartTime = Time.time;
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

    void Explosion()
    {
        dead = true;
        Destroy(GetComponent<NavMeshAgent>());
        Destroy(gameObject, removeTime);
        Destroy(control);
        Destroy(this);
    }

    private void DrawRay(Vector3 position, Vector3 direction, Color color)
    {
        if (debug)
            Debug.DrawRay(position, direction, color);
    }

    private void DrawRay(Vector3 position, Vector3 direction, Color color, float duration)
    {
        if (debug)
            Debug.DrawRay(position, direction, color, duration);
    }

}
