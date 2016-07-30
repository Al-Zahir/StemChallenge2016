using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ArcheryCanvas : MonoBehaviour {

	public Text cursorText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		RaycastHit hit;

		if (Physics.Raycast (transform.position, transform.forward, out hit, 100))
			cursorText.text = "" + (hit.distance + 0.5056f);
		else
			cursorText.text = ">100";
	
	}
}
