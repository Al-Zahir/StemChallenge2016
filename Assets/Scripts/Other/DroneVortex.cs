using UnityEngine;
using System.Collections;

public class DroneVortex : MonoBehaviour {

    public GameObject drone;
    public float radiusStart = 2f;
    public float radiusEnd = 10;
    public float height = 50f;
    public int numLayers = 10;
    public float numDronesPerMeter = 0.2f;
    public float orbitVel = 1;

    private Transform[] rings;

    void Start()
    {
        if (!Application.isPlaying)
            return;

        if (rings != null)
            foreach (Transform t in rings)
                Destroy(t.gameObject);

        GenVortex();
    }

    public void BlowUp()
    {
        transform.Find("ExplosionBlue").gameObject.SetActive(true);
        transform.Find("ExplosionBlue").parent = null;
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            t.parent = null;
            t.gameObject.AddComponent<Rigidbody>();
            t.GetComponent<Rigidbody>().velocity = Random.onUnitSphere * 4;
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        // Option 1: Constant velocity
        /*
        float radInc = (radiusEnd - radiusStart) / numLayers;

	    for(int i=0; i<numLayers; i++)
        {
            float radius = radiusStart + i * radInc;
            rings[i].Rotate(Vector3.up * Mathf.Rad2Deg * orbitVel / 1 * Time.deltaTime);
        }
        */

        // Option 2: Constant angular velocity
        transform.Rotate(Vector3.up * Mathf.Rad2Deg * orbitVel * Time.deltaTime);
	}

    private void GenVortex()
    {
        rings = new Transform[numLayers];

        float radInc = (radiusEnd - radiusStart) / numLayers;
        float heightInc = height / numLayers;

        for (int i = 0; i < numLayers; i++)
        {
            float radius = radiusStart + i * radInc;
            float layerHeight = i * heightInc;
            float circ = 2 * Mathf.PI * radius;
            int numDrones = (int)(circ * numDronesPerMeter);
            float angleInc = 2 * Mathf.PI / numDrones;

            GameObject layer = new GameObject("layer" + i);
            rings[i] = layer.transform;
            layer.transform.position = transform.position + Vector3.up * layerHeight;
            layer.transform.parent = transform;

            for (int j = 0; j < numDrones; j++)
            {
                float angle = j * angleInc;
                Vector3 dronePos = transform.position + new Vector3(radius * Mathf.Cos(angle), layerHeight, radius * Mathf.Sin(angle));
                Vector3 adjPos = transform.position;
                adjPos.y = dronePos.y;
                GameObject g = (GameObject)Instantiate(
                    drone,
                    dronePos,
                    Quaternion.LookRotation(dronePos - adjPos));

                g.transform.parent = layer.transform;
            }
        }
    }
}
