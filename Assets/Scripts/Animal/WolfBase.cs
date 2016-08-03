using UnityEngine;
using System.Collections;

public class WolfBase : MonoBehaviour
{
    public bool debug = true;
    public Animator anim { get; private set; }
    public NavMeshAgent agent { get; private set; }

    public float orientationCastRadius = 0.5f;
    public float rotSpeed = 0.05f;
    private Vector3 normalVel, dampedNormal;

    public bool linkAnimAndAgentSpeed = true;
    public float velAnimScalar = 2;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        UpdateRotation();

        if (linkAnimAndAgentSpeed)
        {
            float vel = agent.desiredVelocity.magnitude;
            if (vel <= 1)
            {
                SetAnimSpeed(vel);
                SetAnimSprint(0);
            }
            else
            {
                SetAnimSpeed(1);
                SetAnimSprint(vel - 1);
            }
        }
    }

    void UpdateRotation()
    {
        RaycastHit[] surroundings = new RaycastHit[4];
        Vector3 upOffset = 1f * transform.up;
        float length = 3f;
        Physics.Raycast(transform.position + orientationCastRadius * transform.forward + upOffset, Vector3.down, out surroundings[0], length);
        Physics.Raycast(transform.position - orientationCastRadius * transform.forward + upOffset, Vector3.down, out surroundings[1], length);
        Physics.Raycast(transform.position + orientationCastRadius * transform.right + upOffset, Vector3.down, out surroundings[2], length);
        Physics.Raycast(transform.position - orientationCastRadius * transform.right + upOffset, Vector3.down, out surroundings[3], length);
        DrawRay(transform.position + orientationCastRadius * transform.forward + upOffset, length * Vector3.down, Color.red);
        DrawRay(transform.position - orientationCastRadius * transform.forward + upOffset, length * Vector3.down, Color.red);
        DrawRay(transform.position + orientationCastRadius * transform.right + upOffset, length * Vector3.down, Color.red);
        DrawRay(transform.position - orientationCastRadius * transform.right + upOffset, length * Vector3.down, Color.red);
        Vector3 averageNormal = Vector3.zero;
        foreach (RaycastHit hit in surroundings)
            averageNormal += hit.normal;
        averageNormal /= surroundings.Length;
        dampedNormal = Vector3.SmoothDamp(dampedNormal, averageNormal, ref normalVel, rotSpeed);

        Vector3 direction = Vector3.Scale(agent.velocity, new Vector3(1, 0, 1));
        Vector3 adjDirection = Vector3.ProjectOnPlane(direction, dampedNormal);
        DrawRay(transform.position, dampedNormal, Color.blue);

        if (Vector3.Magnitude(direction) > 0.001f)
            transform.rotation = Quaternion.LookRotation(adjDirection, dampedNormal);
    }

    public void SetAgentSpeed(float speed)
    {
        agent.speed = speed;
    }

    public void SetAnimSpeed(float speed)
    {
        anim.SetFloat("Speed", speed);
    }

    public void SetAnimSprint(float sprint)
    {
        anim.SetFloat("SprintTrigger", sprint);
    }

    public void CancelAction()
    {
        StartCoroutine(CancelActionRoutine());
    }

    private IEnumerator CancelActionRoutine()
    {
        anim.SetBool("cancelAction", true);
        anim.SetBool("isDrinking", false);

        while(!Mecanim.inAnim(anim, "Wolf Locomotion", 0))
            yield return new WaitForSeconds(0.05f);
        anim.SetBool("cancelAction", false);
    }

    public bool SetDestination(Vector3 target, bool overrideCurrent)
    {
        if (overrideCurrent || AtDestination())
        {
            agent.SetDestination(target);
            return true;
        }

        return false;
    }

    public bool AtDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    private void DrawRay(Vector3 position, Vector3 direction, Color color)
    {
        if (debug)
            Debug.DrawRay(position, direction, color);
    }

}
