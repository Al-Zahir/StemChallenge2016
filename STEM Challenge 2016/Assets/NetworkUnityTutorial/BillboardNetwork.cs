using UnityEngine;
using System.Collections;

public class BillboardNetwork : MonoBehaviour {

	void Update(){

		transform.LookAt (Camera.main.transform);

	}

}
