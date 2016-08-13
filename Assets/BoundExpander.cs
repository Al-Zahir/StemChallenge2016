using UnityEngine;
using System.Collections;

public class BoundExpander : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Bounds bounds = new Bounds(GetComponent<MeshFilter>().mesh.bounds.center, GetComponent<MeshFilter>().mesh.bounds.size + new Vector3(100, 100, 100));
        GetComponent<MeshFilter>().mesh.bounds = bounds;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
