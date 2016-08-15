using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingScript : MonoBehaviour {

	public float rotSpeed;
	public Text loadingText;

	private float numDotsF;

	// Use this for initialization
	void Start () {

		numDotsF = 0.0f;
		loadingText.text = "Loading";
		StartCoroutine (LoadNewScene());

	}
	
	// Update is called once per frame
	void Update () {

		transform.Rotate(Vector3.forward * rotSpeed * Time.deltaTime);

		string dots = "";

		int numDots = Mathf.FloorToInt (numDotsF);

		for(int i = 0; i < numDots; i++)
			dots += ".";

		loadingText.text = "Loading" + dots;

		numDotsF += Time.deltaTime;

		if (numDotsF > 4)
			numDotsF = 0;
	
	}

	IEnumerator LoadNewScene() {

		AsyncOperation async = Application.LoadLevelAsync (Application.loadedLevel + 1);

		while (!async.isDone) {
			yield return null;
		}

	}
}
