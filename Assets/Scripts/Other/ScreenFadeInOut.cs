using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScreenFadeInOut : MonoBehaviour {
		
	public float fadeSpeed = 1.5f;

	private bool sceneStarting = true;
	private Image image;

	void Awake(){

		image = GetComponent<Image> ();
		image.rectTransform.anchorMax = new Vector2 (1.0f, 1.0f);

	}

	void Update(){

		if (sceneStarting) {
		
			StartScene();
		
		}

	}

	void FadeToClear(){

		image.color = Color.Lerp (image.color, Color.clear, fadeSpeed * Time.deltaTime);

	}

	void FadeToBlack(){
		
		image.color = Color.Lerp (image.color, Color.black, fadeSpeed * Time.deltaTime);
		
	}

	void StartScene(){

		FadeToClear ();

		if (image.color.a <= 0.05f) {
		
			image.color = Color.clear;
			image.enabled = false;
			sceneStarting = false;
		
		}

	}

	public void EndScene(){

		image.enabled = true;
		FadeToBlack ();

		if (image.color.a >= 0.95f) {
		
			Debug.Log("Reload Level");
		
		}

	}

	public IEnumerator EndScene(int level){

		image.enabled = true;

		while (image.color.a < 0.95f) {
			FadeToBlack ();
			yield return new WaitForEndOfFrame ();
		}

		Application.LoadLevelAsync (level);

	}
	
}
