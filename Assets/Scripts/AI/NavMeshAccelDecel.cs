using UnityEngine;
using System.Collections;

public class NavMeshAccelDecel : MonoBehaviour {

    public float acceleration = 4f;
    public float deceleration = 60f;
    public float closeEnoughMeters = 2f;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    void Start()
    {
        navMeshAgent = gameObject.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
    }

    void Update()
    {
        if (navMeshAgent)
        {

            // speed up slowly, but stop quickly
            if (navMeshAgent.hasPath)
                navMeshAgent.acceleration = (navMeshAgent.remainingDistance < closeEnoughMeters) ? deceleration : acceleration;

        }
    }
}
