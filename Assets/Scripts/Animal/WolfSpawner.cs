using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WolfSpawner : MonoBehaviour {

    public bool debug = false;
    public GameObject wolfPrefab;
    public float numWolves = 10;
    private List<GameObject> spawned = new List<GameObject>();
    public float randSpawnSquare = 200;
    public Transform wolfParent;

    void Start()
    {
        SpawnWolves();
        StartCoroutine(Checker());
    }

    private IEnumerator Checker()
    {
        while (this)
        {
            SpawnWolves();
            yield return new WaitForSeconds(5);
        }
    }

    private void SpawnWolves()
    {
        for(int i=0; i<numWolves; i++)
        {
            if(i >= spawned.Count || spawned[i] == null)
            {
                Vector3 randPos = transform.position + new Vector3(Random.Range(-randSpawnSquare, randSpawnSquare), 0, Random.Range(-randSpawnSquare, randSpawnSquare));
                UnityEngine.AI.NavMeshHit myNavHit;
                if (UnityEngine.AI.NavMesh.SamplePosition(randPos, out myNavHit, 100, -1))
                {
                    GameObject spawn = (GameObject)Instantiate(wolfPrefab, myNavHit.position, Quaternion.identity);
                    spawn.transform.parent = wolfParent;
                    if (i >= spawned.Count)
                        spawned.Add(spawn);
                    else
                        spawned[i] = spawn;
                    if (debug)
                        Debug.Log("Spawned wolf at (" + randPos.x + " " + randPos.z + ")");
                }
            }
        }
    }
}
