using UnityEngine;
using System.Collections;

public class Unparent : MonoBehaviour {

	// Use this for initialization
	void Start () {
        transform.parent = null;
	}
}
