using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReconDrone : MonoBehaviour {

    public bool debug = false;
    public Transform player;
    private NavMeshAgent agent;
    private Vector3 lastDir = Vector3.forward;
    public float closestDistance = 2f;
    public float moveSpeed = 3f;

    public float patrolRadius = 10;
    private Vector3 startPos;
    private Quaternion startRot;

    public GameObject[] nearbyDrones;

    public float fov = 70;
    private bool noticedPlayer = false, canNoticePlayer = true, hitNoticedPlayer = false;

    public float removeTime = 10f;

    private bool dead;
    public bool overrideSeePlayer;

	// Use this for initialization
	void Start () {
        agent = GetComponent<NavMeshAgent>();
        startPos = transform.position;
        startRot = transform.rotation;

        if (player == null)
            player = GameObject.Find("Player").transform;

        Collider[] nearbyDronesCol = Physics.OverlapSphere(startPos, patrolRadius);
        List<GameObject> found = new List<GameObject>();
        foreach (Collider c in nearbyDronesCol)
            if (c.GetComponent<OnHitObject>() != null && c.GetComponent<OnHitObject>().parent.tag == "Enemy" &&
                !found.Contains(c.GetComponent<OnHitObject>().parent) && c.GetComponent<OnHitObject>().parent != gameObject)
                found.Add(c.GetComponent<OnHitObject>().parent);

        nearbyDrones = found.ToArray();

        StartCoroutine(NoticePlayer());
	}
	
    void OnEnable()
    {
        if (player == null) return;
        StartCoroutine(NoticePlayer());
    }

    private IEnumerator NoticePlayer()
    {
        while (!dead)
        {
            if (canNoticePlayer && GetComponent<NavMeshAgent>().enabled)
            {
                Vector3 adjPlayer = player.position;
                adjPlayer.y = transform.position.y;
                Vector3 adjAgentPos = transform.position;
                adjAgentPos.y = startPos.y;

                float playerDistance = Vector3.Distance(adjPlayer, transform.position);
                float angle = Vector3.Angle(Vector3.Scale(transform.forward, new Vector3(1, 0, 1)), adjPlayer - transform.position);
                Vector3 cross = Vector3.Cross(Vector3.Scale(transform.forward, new Vector3(1, 0, 1)), adjPlayer - transform.position);
                if (cross.y < 0) angle = -angle;

                if (debug)
                {
                    Vector3 dir = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
                    Debug.DrawRay(transform.position, Vector3.RotateTowards(dir, -dir, -fov / 2 * Mathf.Deg2Rad, 0) * patrolRadius, Color.red, 0.5f);
                    Debug.DrawRay(transform.position, Vector3.RotateTowards(dir, -dir, fov / 2 * Mathf.Deg2Rad, 0) * patrolRadius, Color.red, 0.5f);
                }

                if (playerDistance < patrolRadius && Mathf.Abs(angle) < fov / 2 && (!noticedPlayer || hitNoticedPlayer))
                {
                    noticedPlayer = true;
                    hitNoticedPlayer = false;

                    foreach (GameObject drone in nearbyDrones)
                        if(drone != null)
                            drone.SendMessage("FoundPlayer", SendMessageOptions.DontRequireReceiver);
                }
                else if (Vector3.Distance(adjAgentPos, startPos) > patrolRadius && noticedPlayer)
                {
                    StartCoroutine(EndChase());
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

	// Update is called once per frame
    void Update()
    {
        if (dead || !GetComponent<NavMeshAgent>().enabled) return;

        if (Vector3.Distance(Vector3.Scale(startPos, new Vector3(1, 0, 1)), Vector3.Scale(transform.position, new Vector3(1, 0, 1))) < 0.1f && !noticedPlayer && !hitNoticedPlayer && !overrideSeePlayer)
            transform.rotation = startRot;

        if (!noticedPlayer && !hitNoticedPlayer || Mathf.Abs(transform.position.y - player.position.y) > 10 && !overrideSeePlayer)
        {
            agent.destination = startPos - Vector3.up * 2;
            agent.speed = moveSpeed;
            return;
        }


        Vector3 adjAgentPos = transform.position;
        adjAgentPos.y = startPos.y;
        if (Vector3.Distance(adjAgentPos, startPos) > patrolRadius && hitNoticedPlayer && !overrideSeePlayer)
        {
            StartCoroutine(EndChase());
            return;
        }

        agent.SetDestination(player.position);
        Vector3 noYPos = transform.position;
        noYPos.y = player.transform.position.y;
        if (Vector3.Distance(noYPos, player.position) < closestDistance)
            agent.speed = 0;
        else
            agent.speed = moveSpeed;

        Vector3 dir = Vector3.Scale(player.position - transform.position, new Vector3(1, 0, 1));
        if (dir.magnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            lastDir = dir;
        }
        else
            transform.rotation = Quaternion.LookRotation(lastDir, Vector3.up);
	}

    void HitByPlayer()
    {
        hitNoticedPlayer = true;
    }

    void FoundPlayer()
    {
        // If another drone found player then we found him too
        if (!noticedPlayer && canNoticePlayer)
        {
            noticedPlayer = true;

            foreach (GameObject drone in nearbyDrones)
                if (drone != null)
                    drone.SendMessage("FoundPlayer", SendMessageOptions.DontRequireReceiver);
        }
    }

    void LostPlayer()
    {
        // If another drone lost player but we still see him, send to nearby drones that we see him
        if (noticedPlayer)
        {
            //Debug.Log("C" + Time.time);
            foreach (GameObject drone in nearbyDrones)
                if (drone != null)
                    drone.SendMessage("FoundPlayer", SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator EndChase()
    {
        canNoticePlayer = false;
        hitNoticedPlayer = false;
        // Send a lost message for non drone objects
        foreach (GameObject drone in nearbyDrones)
            if (drone != null && !drone.name.ToLower().Contains("recon"))
                    drone.SendMessage("LostPlayer", SendMessageOptions.DontRequireReceiver);

        // Send a lost message for drone objects, which allows them to rebroadcast their foundPlayer
        foreach (GameObject drone in nearbyDrones)
            if (drone != null && drone.name.ToLower().Contains("recon"))
                    drone.SendMessage("LostPlayer", SendMessageOptions.DontRequireReceiver);

        noticedPlayer = false; 
        yield return new WaitForSeconds(1);
        canNoticePlayer = true;
    }

    void Explosion()
    {
        dead = true;
        Destroy(GetComponent<NavMeshAgent>());
        // Send a lost message for non drone objects
        foreach (GameObject drone in nearbyDrones)
                if (drone != null && !drone.name.ToLower().Contains("recon"))
                    drone.SendMessage("LostPlayer", SendMessageOptions.DontRequireReceiver);

        // Send a lost message for drone objects, which allows them to rebroadcast their foundPlayer
        foreach (GameObject drone in nearbyDrones)
            if (drone != null && drone.name.ToLower().Contains("recon"))
                    drone.SendMessage("LostPlayer", SendMessageOptions.DontRequireReceiver);

        Destroy(gameObject, removeTime);
        Destroy(this);
    }
}
