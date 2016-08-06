using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	public Canvas canvas;
	private Text[] text;

	public float minAlpha = 0.5f;
	public float maxAlpha = 1.0f;

	private float targetAlpha;
	private float startAlpha;
	private float startTime;

	public float smoothTime = 0.1f;

	public bool flyIn = false;

	void Awake(){

		text = canvas.GetComponentsInChildren<Text> ();

		flyIn = false;

		targetAlpha = 1.0f;
		startTime = Time.time;
		startAlpha = text[0].color.a;

	}

	void Update(){

		foreach(Text t in text)
			t.color = new Color(t.color.r, t.color.g, t.color.b, Mathf.Lerp(startAlpha, targetAlpha, (Time.time - startTime)));

		if (text[0].color.a == maxAlpha) {
			targetAlpha = minAlpha;
			startTime = Time.time;
			startAlpha = text[0].color.a;
		} else if (text[0].color.a == minAlpha) {
			targetAlpha = maxAlpha;
			startTime = Time.time;
			startAlpha = text[0].color.a;
		}


		if (flyIn) {
		
			transform.Translate (transform.forward * 2 * Time.deltaTime);

			if (transform.position.z > 10)
				FlyInComplete ();
		}

	}

	public void StartGame(){

		canvas.gameObject.SetActive (false);
		flyIn = true;

	}

	public void FlyInComplete(){

		Application.LoadLevelAsync (Application.loadedLevel + 1);

	}


}
