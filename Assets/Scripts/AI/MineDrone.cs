using UnityEngine;
using System.Collections;

public class MineDrone : MonoBehaviour
{

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

        Vector3 dir = Vector3.Scale(player.position - transform.position, new Vector3(1, 0, 1));
        if (dir.magnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            lastDir = dir;
        }
        else
            transform.rotation = Quaternion.LookRotation(lastDir, Vector3.up);

        engine.localRotation = Quaternion.Euler(originalEngineEuler + new Vector3(0, 0, smoothDesiredVelocity.magnitude / moveSpeed * engineTilt));
    }

    void Explosion()
    {
        Destroy(GetComponent<NavMeshAgent>());
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

        GetComponent<Explode>().explode(0, true, true, false);
    }
}
