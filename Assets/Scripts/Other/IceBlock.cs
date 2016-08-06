using UnityEngine;
using System.Collections;

public class IceBlock : MonoBehaviour {

	private Rigidbody rigid;
	private Vector3 direction;
	private bool isMoving;

	public bool allowPlayerMove;

    public LayerMask iceLayer;

	void Awake(){

		rigid = GetComponent<Rigidbody> ();
		isMoving = false;

		allowPlayerMove = true;

	}

	void Update(){

		if (isMoving) {
		
			RaycastHit hit;
			if (Physics.Raycast (transform.position - transform.up * ((transform.lossyScale.y / 2) - 0.1f), direction, out hit, (transform.lossyScale.y / 2), iceLayer)) {
			
				isMoving = false;
				rigid.isKinematic = true;
				rigid.velocity = Vector3.zero;

				transform.localPosition = new Vector3 (Mathf.RoundToInt(transform.localPosition.x), transform.localPosition.y,
					Mathf.RoundToInt(transform.localPosition.z));
			
			}

			Debug.DrawRay (transform.position - transform.up * ((transform.lossyScale.y / 2) - 0.1f), direction * ((transform.lossyScale.y / 2)), Color.blue, 6f);
		
		}

	}

	public void Push(Vector3 d){

		/*float angleF = Vector3.Angle (transform.forward, d);
		float angleB = Vector3.Angle (-transform.forward, d);
		float angleL = Vector3.Angle (-transform.right, d);
		float angleR = Vector3.Angle (transform.right, d);

		if (angleF < angleB && angleF < angleL && angleF < angleR)
			direction = transform.forward;
		else if (angleB < angleF && angleB < angleL && angleB < angleR)
			direction = -transform.forward;
		else if (angleL < angleB && angleL < angleF && angleL < angleR)
			direction = -transform.right;
		else
			direction = transform.right;*/

		direction = d;

		Debug.Log (direction + " " + transform.forward + " " + Time.time);

		rigid.isKinematic = false;

		isMoving = true;

		allowPlayerMove = false;

		rigid.velocity = d * 4;

		StartCoroutine (ReleasePlayer());

	}

	IEnumerator ReleasePlayer(){

		yield return new WaitForSeconds (0.5f);
		allowPlayerMove = true;

	}
}
