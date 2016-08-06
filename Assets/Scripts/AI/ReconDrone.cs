using UnityEngine;
using System.Collections;

public class ReconDrone : MonoBehaviour {

    public Transform player;
    private NavMeshAgent agent;
    private Vector3 lastDir = Vector3.forward;
    public float closestDistance = 2f;
    public float moveSpeed = 3f;

	// Use this for initialization
	void Start () {
        agent = GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
	void Update () {
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

    void Explosion()
    {
        Destroy(GetComponent<NavMeshAgent>());
        Destroy(this);
    }
}
