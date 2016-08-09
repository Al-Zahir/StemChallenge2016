using UnityEngine;
using System.Collections;

public class MineDrone : MonoBehaviour
{
    public bool debug = false;
    public Transform player;
    public Transform cannon, engine;
    private NavMeshAgent agent;
    private Vector3 lastDir = Vector3.forward;
    public float closestDistance = 2f;
    public float moveSpeed = 3f;
    public float blowUpTime = 3f;
    public float engineTilt = 20f;

    private Vector3 originalEngineEuler;
    private Vector3 smoothDesiredVelocity;

    private Vector3 dampVel1;

    public float patrolRadius = 20;
    public float viewRadius = 4;
    private Vector3 startPos;
    private Quaternion startRot;

    public float fov = 70;

    public bool droneFoundPlayer = false;
    public bool chasePlayer = false;

    public float blowupDamage = 40;

    public float removeTime = 10f;
    private bool dead;

    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalEngineEuler = engine.localRotation.eulerAngles;
        startPos = transform.position;
        startRot = transform.rotation;

        if (player == null)
            player = GameObject.Find("Player").transform;

        StartCoroutine(FindPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) return;
        if (Vector3.Distance(Vector3.Scale(startPos, new Vector3(1, 0, 1)), Vector3.Scale(transform.position, new Vector3(1, 0, 1))) < 0.1f && !chasePlayer)
            transform.rotation = startRot;

        if (!chasePlayer || Mathf.Abs(transform.position.y - player.position.y) > 6)
        {
            agent.SetDestination(startPos);
            agent.speed = moveSpeed;
            return;
        }

        agent.SetDestination(player.position);
        Vector3 noYPos = transform.position;
        noYPos.y = player.transform.position.y;

        smoothDesiredVelocity = Vector3.SmoothDamp(smoothDesiredVelocity, agent.desiredVelocity, ref dampVel1, 0.2f);

        if (Vector3.Distance(noYPos, player.position) < closestDistance)
        {
            agent.speed = 0;
            if(GetComponent<DroneHealth>())
                Destroy(GetComponent<DroneHealth>());
            StartCoroutine(BlowUp());
        }
        else
            agent.speed = moveSpeed;

        Vector3 dir2 = Vector3.Scale(player.position - transform.position, new Vector3(1, 0, 1));
        if (dir2.magnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir2, Vector3.up);
            lastDir = dir2;
        }
        else
            transform.rotation = Quaternion.LookRotation(lastDir, Vector3.up);

        engine.localRotation = Quaternion.Euler(originalEngineEuler + new Vector3(0, 0, smoothDesiredVelocity.magnitude / moveSpeed * engineTilt));
    }

    private IEnumerator FindPlayer()
    {
        while (!dead)
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
                Debug.DrawRay(transform.position, Vector3.RotateTowards(dir, -dir, -fov / 2 * Mathf.Deg2Rad, 0) * viewRadius, Color.red, 0.5f);
                Debug.DrawRay(transform.position, Vector3.RotateTowards(dir, -dir, fov / 2 * Mathf.Deg2Rad, 0) * viewRadius, Color.red, 0.5f);
            }

            if (playerDistance < viewRadius && Mathf.Abs(angle) < fov / 2)
                chasePlayer = true;

            if (playerDistance > viewRadius || Vector3.Distance(adjAgentPos, startPos) > patrolRadius)
                chasePlayer = false;

            if (droneFoundPlayer)
                chasePlayer = true;

            yield return new WaitForSeconds(0.5f);
        }
    }

    void HitByPlayer()
    {
        chasePlayer = true;
    }

    void FoundPlayer()
    {
        droneFoundPlayer = true;
        chasePlayer = true;
    }

    void LostPlayer()
    {
        Vector3 adjPlayer = player.position;
        adjPlayer.y = transform.position.y;

        float playerDistance = Vector3.Distance(adjPlayer, transform.position);
        float angle = Vector3.Angle(Vector3.Scale(transform.forward, new Vector3(1, 0, 1)), adjPlayer - transform.position);
        Vector3 cross = Vector3.Cross(Vector3.Scale(transform.forward, new Vector3(1, 0, 1)), adjPlayer - transform.position);
        if (cross.y < 0) angle = -angle;

        droneFoundPlayer = false;

        if (!(playerDistance < viewRadius && Mathf.Abs(angle) < fov / 2))
            chasePlayer = false;
    }

    void Explosion()
    {
        dead = true;
        Destroy(GetComponent<NavMeshAgent>());
        Destroy(gameObject, removeTime);
        Destroy(this);
    }

    private IEnumerator BlowUp()
    {
        float startTime = Time.time;
        float accel = 18;
        float vel = 0;
        while (Time.time < startTime + blowUpTime)
        {
            vel += accel * Time.deltaTime;
            cannon.Rotate(Vector3.forward * vel * Time.deltaTime, Space.Self);
            yield return new WaitForEndOfFrame();
        }

        Vector3 noYPos = transform.position;
        noYPos.y = player.transform.position.y;
        float dist = Vector3.Distance(noYPos, player.position);
        GetComponent<Explode>().explode(0, true, true, false);
        player.GetComponent<PlayerHealth>().TakeDamage(blowupDamage / Mathf.Clamp(dist - closestDistance, 1, Mathf.Infinity));
    }
}
