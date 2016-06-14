using UnityEngine;
using System.Collections;

public class EnemyShooting : MonoBehaviour {

	public float maximumDamage = 120f;
	public float minimumDamage = 45f;
	public AudioClip shotClip;
	public float flashIntensity = 3f;
	public float fadeSpeed = 10f;

	private Animator anim;
	private LineRenderer laserShotLine;
	private Light laserShotLight;
	private SphereCollider col;
	private Transform player;
	private PlayerHealth playerHealth;
	private bool shooting;
	private float scaledDamage;

	void Awake(){

		anim = GetComponent<Animator> ();
		laserShotLine = GetComponentInChildren<LineRenderer> ();
		laserShotLight = laserShotLine.gameObject.GetComponent<Light> ();
		col = GetComponent<SphereCollider> ();
		player = GameObject.FindGameObjectWithTag (Tags.player).transform;
		playerHealth = player.gameObject.GetComponent<PlayerHealth> ();

		laserShotLine.enabled = false;
		laserShotLight.intensity = 0f;

		scaledDamage = maximumDamage - minimumDamage;

	}

	void Update(){

		if (laserShotLine.enabled && !shooting)
			laserShotLine.enabled = false;

		laserShotLight.intensity = Mathf.Lerp (laserShotLight.intensity, 0f, fadeSpeed * Time.deltaTime);

	}

	void OnAnimatorIK(int layerIndex){

		anim.SetIKPosition (AvatarIKGoal.RightHand, player.position + Vector3.up);

		anim.SetIKPositionWeight (AvatarIKGoal.RightHand, 1.0f);

	}

	public void Shoot(){

		shooting = true;

		StartCoroutine ("ResetShot");

		float fractionalDistance = (col.radius - Vector3.Distance (transform.position, player.position)) / col.radius;

		float damage = scaledDamage * fractionalDistance + minimumDamage;

		playerHealth.TakeDamage (damage);

		ShotEffects ();

	}

	void ShotEffects(){

		laserShotLine.SetPosition (0, laserShotLine.transform.position);

		laserShotLine.SetPosition (1, player.transform.position + Vector3.up * 1.5f);

		laserShotLine.enabled = true;

		laserShotLight.intensity = flashIntensity;

		AudioSource.PlayClipAtPoint (shotClip, laserShotLight.transform.position);

	}

	IEnumerator ResetShot(){

		for (int i  = 0; i < 5; i++)
			yield return new WaitForEndOfFrame();

		shooting = false;

	}

}
