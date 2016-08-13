using UnityEngine;
using System.Collections;

public class HologramRobot : MonoBehaviour {

	public void Move(){

		transform.localPosition = new Vector3 (
			Mathf.Lerp (transform.localPosition.x, 0, Time.deltaTime * 0.15f), 
			Mathf.Lerp (transform.localPosition.y, 0, Time.deltaTime * 0.15f), 
			transform.localPosition.z);

	}
}
