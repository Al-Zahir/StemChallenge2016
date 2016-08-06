using UnityEngine;
using System.Collections;

public class ShockDrone : MonoBehaviour
{

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

    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalEngineEuler = engine.localRotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(player.position);
        Vector3 noYPos = shockers[0].transform.position;
        noYPos.y = player.transform.position.y;

        smoothDesiredVelocity = Vector3.SmoothDamp(smoothDesiredVelocity, agent.desiredVelocity, ref dampVel1, 0.2f);

        if (Vector3.Distance(noYPos, player.position) < closestDistance)
        {
            agent.speed = 0;
            agent.acceleration = 1000;
            agent.destination = transform.position;
        }
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

        SetShockers(Vector3.Distance(noYPos, player.position) < closestDistance + 2);

        engine.localRotation = Quaternion.Euler(originalEngineEuler + new Vector3(smoothDesiredVelocity.magnitude / moveSpeed * engineTilt, 0, 0));
    }

    void Explosion()
    {
        Destroy(this);
        Destroy(GetComponent<NavMeshAgent>());
    }

    private void SetShockers(bool active)
    {
        foreach (ParticleSystem g in shockers)
            g.enableEmission = active;
    }
}
