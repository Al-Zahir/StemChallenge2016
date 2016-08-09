using UnityEngine;
using System.Collections;

public class ShockDrone : MonoBehaviour
{
    public bool debug = false;
    public Transform player;
    public Transform engine;
    private NavMeshAgent agent;
    private Vector3 lastDir = Vector3.forward;
    public float closestDistance = 2f;
    public float moveSpeed = 3f;
    public float engineTilt = 20f;
    public ParticleSystem[] shockers;

    private Vector3 originalEngineEuler;
    private Vector3 smoothDesiredVelocity;

    private Vector3 dampVel1;

    public float patrolRadius = 20;
    public float viewRadius = 4;
    private Vector3 startPos;
    private Quaternion startRot;

    public float fov = 70;

    public bool chasePlayer = false;
    private bool droneFoundPlayer = false;

    public float zapDamage = 10;

    private bool backingUp = false;
    public float fallBackTime = 1f;
    public float fallBackLength = 0.5f;

    public string[] blockingAnims;

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
        StartCoroutine(ZapPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) return;

        if (Vector3.Distance(Vector3.Scale(startPos, new Vector3(1, 0, 1)), Vector3.Scale(transform.position, new Vector3(1, 0, 1))) < 0.1f && !chasePlayer)
            transform.rotation = startRot;

        if ((!chasePlayer || Mathf.Abs(transform.position.y - player.position.y) > 6) && !backingUp)
        {
            agent.SetDestination(startPos);
            agent.speed = moveSpeed;
            SetShockers(false);
            return;
        }

        Vector3 noYPos = shockers[0].transform.position;
        noYPos.y = player.transform.position.y;
        smoothDesiredVelocity = Vector3.SmoothDamp(smoothDesiredVelocity, agent.desiredVelocity, ref dampVel1, 0.2f);

        if (Vector3.Distance(noYPos, player.position) < closestDistance && !backingUp)
        {
            agent.speed = 0;
            agent.acceleration = 1000;
            agent.destination = transform.position;
        }
        else if (!backingUp)
        {
            agent.speed = moveSpeed;
            agent.SetDestination(player.position);
        }
        
        Vector3 dir2 = Vector3.Scale(player.position - transform.position, new Vector3(1, 0, 1));
        if (dir2.magnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir2, Vector3.up);
            lastDir = dir2;
        }
        else
            transform.rotation = Quaternion.LookRotation(lastDir, Vector3.up);

        // Setting enabled is expensive apparently
        bool enableShockers = Vector3.Distance(noYPos, player.position) < closestDistance + 2;
        if (shockers[0].emission.enabled && !enableShockers)
            SetShockers(enableShockers);
        else if (!shockers[0].emission.enabled && enableShockers)
            SetShockers(enableShockers);

        engine.localRotation = Quaternion.Euler(originalEngineEuler + new Vector3(smoothDesiredVelocity.magnitude / moveSpeed * engineTilt, 0, 0));
    }

    private IEnumerator FallBack()
    {
        yield return new WaitForSeconds(0.2f);
        agent.destination = transform.position - transform.forward * fallBackLength;
        agent.speed = moveSpeed;
        backingUp = true;
        yield return new WaitForSeconds(fallBackTime);
        backingUp = false;
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

    private IEnumerator ZapPlayer()
    {
        while(!dead)
        {
            Vector3 noYPos = shockers[0].transform.position;
            noYPos.y = player.transform.position.y;
            if (shockers[0].emission.enabled && Vector3.Distance(noYPos, player.position) < closestDistance)
            {
                if (Mecanim.inAnyAnim(player.GetComponent<Animator>(), blockingAnims, 1))
                    StartCoroutine(FallBack());
                else
                    player.GetComponent<PlayerHealth>().TakeDamage(zapDamage);
            }

            yield return new WaitForSeconds(1);
        }
    }

    void HitByPlayer()
    {
        chasePlayer = true;
        StartCoroutine(FallBack());
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

        if(!(playerDistance < viewRadius && Mathf.Abs(angle) < fov / 2))
            chasePlayer = false;
    }

    void Explosion()
    {
        dead = true;
        Destroy(GetComponent<NavMeshAgent>());
        Destroy(gameObject, removeTime);
        Destroy(this);
    }

    private void SetShockers(bool active)
    {
        foreach (ParticleSystem g in shockers)
            g.enableEmission = active;
    }
}
