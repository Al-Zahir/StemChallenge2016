using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossDrone : MonoBehaviour {

    public bool debug = false;
    public bool active = false;
    public Transform player;
    public Transform leftSpawner, rightSpawner;
    public GameObject explosionPrefab;
    //private NavMeshAgent agent;
    private Vector3 lastDir = Vector3.forward;
    public float closestDistance = 2f;
    public float moveSpeed = 3f;

    public float patrolRadius = 30;
    private Vector3 startPos;
    private Quaternion startRot;

    public float fov = 80;
    public float removeTime = 60f;

    public GameObject[] dronePrefabs;
    public float timeBetweenSpawns = 10f;

    public float droneStartDistance = 5;
    public Transform eyeCover;
    public Transform eyeOpenPos;
    private Vector3 eyeClosedPos;
    public float eyeOpenTime = 1f;
    public float eyeOpenDelay = 1f;
    public bool eyeOpen;

    private bool dead;

    public bool grounded = false;

    private Vector3 targetPos, dampVel1;
    public float posSmoothTime = 0.5f;

    public float downTime = 3f;

    private Quaternion targetRot;
    public float rotSpeed = 2;

    private GameController gameController;

	private List<GameObject> spawnedDrones;

	// Use this for initialization
	void Start () {
        //agent = GetComponent<NavMeshAgent>();
        startPos = transform.position;
        targetPos = startPos;
        targetRot = transform.rotation;

		spawnedDrones = new List<GameObject> ();

        if (player == null)
            player = GameObject.Find("Player").transform;

        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        eyeClosedPos = eyeCover.localPosition;

        StartCoroutine(AttackingRoutine());
	}

    private IEnumerator AttackingRoutine()
    {
        while(!dead)
        {
			if (grounded || !active || spawnedDrones.Count == dronePrefabs.Length)
            {
                yield return new WaitForSeconds(timeBetweenSpawns);
                continue;
            }

            // Open eye
            float lerpTime = 0;
            while (lerpTime < eyeOpenTime && !grounded)
            {
                eyeCover.localPosition = Vector3.Lerp(eyeClosedPos, eyeOpenPos.localPosition, Mathf.SmoothStep(0, 1, lerpTime));
                lerpTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            int randID = Random.Range(0, dronePrefabs.Length);

			while (AlreadySpawned (randID)) {
			
				if (randID == dronePrefabs.Length - 1)
					randID = 0;
				else
					randID++;
			
			}

            if (!grounded)
            {
                Vector3 pos = Random.Range(0, 1f) > 0.5 ? leftSpawner.position : rightSpawner.position;
                // transform.position + transform.forward * droneStartDistance
                GameObject instantiated = (GameObject)Instantiate(dronePrefabs[randID], pos, Quaternion.LookRotation(transform.forward));
                GameObject explosion = (GameObject)Instantiate(explosionPrefab, pos, Quaternion.identity);
                explosion.SetActive(true);
                Destroy(explosion, 10);

				spawnedDrones.Add (instantiated);

                if (instantiated.GetComponent<ShockDrone>() != null)
                    instantiated.GetComponent<ShockDrone>().overrideSeePlayer = true;
                else if (instantiated.GetComponent<MineDrone>() != null)
                    instantiated.GetComponent<MineDrone>().overrideSeePlayer = true;
                else if (instantiated.GetComponent<ReconDrone>() != null)
                    instantiated.GetComponent<ReconDrone>().overrideSeePlayer = true;
                eyeOpen = true;
                yield return new WaitForSeconds(eyeOpenDelay);
            }

            if(!grounded)
                eyeOpen = false;

            // Close eye
            lerpTime = 0;
            while (lerpTime < eyeOpenTime && !grounded)
            {
                eyeCover.localPosition = Vector3.Lerp(eyeOpenPos.localPosition, eyeClosedPos, Mathf.SmoothStep(0, 1, lerpTime));
                lerpTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private IEnumerator GroundRoutine()
    {
        grounded = true;
        eyeCover.localPosition = eyeClosedPos;
        eyeOpen = false;
        Rigidbody rigid = gameObject.AddComponent<Rigidbody>();
        rigid.mass = 1000;
        yield return new WaitForSeconds(2);
        Destroy(rigid);
        yield return new WaitForSeconds(downTime);
        grounded = false;
    }

	// Update is called once per frame
    void Update()
    {
        if (dead || !active) return;

        Vector3 noYPos = transform.position;
        noYPos.y = player.transform.position.y;

        Vector3 dir = Vector3.Scale(player.position - transform.position, new Vector3(1, 0, 1));

        if (!grounded)
        {
            if (dir.magnitude > 0.001f)
            {
                targetRot = Quaternion.LookRotation(dir, Vector3.up);
                lastDir = dir;
            }
            else
                targetRot = Quaternion.LookRotation(lastDir, Vector3.up);

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref dampVel1, posSmoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
        }
	}

    void HitByPlayer()
    {
        // Player successfully hit eye while in air
        if(!grounded)
        {
            StartCoroutine(GroundRoutine());
        }
    }

    void Explosion()
    {
        dead = true;
        Destroy(GetComponent<UnityEngine.AI.NavMeshAgent>());
        // Send a lost message for non drone objects
        /*foreach (GameObject drone in nearbyDrones)
                if (drone != null && !drone.name.ToLower().Contains("recon"))
                    drone.SendMessage("LostPlayer", SendMessageOptions.DontRequireReceiver);

        // Send a lost message for drone objects, which allows them to rebroadcast their foundPlayer
        foreach (GameObject drone in nearbyDrones)
            if (drone != null && drone.name.ToLower().Contains("recon"))
                    drone.SendMessage("LostPlayer", SendMessageOptions.DontRequireReceiver);
        */
        Destroy(gameObject, removeTime);
        Destroy(this);
        gameController.FinishedEndTemple();
    }

	public void DestroyAll(){

		foreach (GameObject g in spawnedDrones)
			Destroy (g);

	}

	public bool AlreadySpawned(int randID){

		foreach (GameObject g in spawnedDrones) {
		
			if (randID == 0) {
				if (g.GetComponent<MineDrone> ()) {
					return true;
				}
			} else if (randID == 1) {
				if (g.GetComponent<ReconDrone> ()) {
					return true;
				}
			} else if (randID == 2) {
				if (g.GetComponent<ShockDrone> ()) {
					return true;
				}
			}
			
		
		}

		return false;
	
	}
}

