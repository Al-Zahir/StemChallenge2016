using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour {

	public float health = 100f;
	public float resetAfterDeathTime = 5f;
	public AudioClip deathClip;

	private Animator anim;
	private HashIDs hash;
	private PlayerMovement playerMovement;
	private ScreenFadeInOut screenFadeInOut;
	private AudioSource footstepsAudio;
	private float timer;
	private bool playerDead;

	void Awake(){

		anim = GetComponent<Animator> ();
		hash = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs> ();
		playerMovement = GetComponent<PlayerMovement> ();
		screenFadeInOut = GameObject.FindGameObjectWithTag (Tags.fader).GetComponent<ScreenFadeInOut> ();
		footstepsAudio = GetComponent<AudioSource> ();
		timer = 0f;
		playerDead = false;

	}

	void Update(){

		if (health <= 0f) {
		
			if (!playerDead)
				PlayerDying();
			else{

				PlayerDead();
				LevelReset();

			}

		}

	}

	void PlayerDying(){

		playerDead = true;

		//anim.SetBool (hash.deadBool, playerDead);

		AudioSource.PlayClipAtPoint (deathClip, transform.position);

	}

	void PlayerDead(){

		//if (anim.GetCurrentAnimatorStateInfo (0).nameHash == hash.dyingState) 
		//	anim.SetBool (hash.deadBool, false);

		anim.SetFloat (hash.speedFloat, 0.0f);
		playerMovement.enabled = false;

		footstepsAudio.Stop ();
	}

	void LevelReset(){

		timer += Time.deltaTime;

		if (timer >= resetAfterDeathTime) 
			screenFadeInOut.EndScene ();
	}

	public void TakeDamage(float amount){

		health -= amount;

	}

}
