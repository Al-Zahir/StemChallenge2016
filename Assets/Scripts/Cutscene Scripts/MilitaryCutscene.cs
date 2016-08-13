using UnityEngine;
using System.Collections;

public class MilitaryCutscene : MonoBehaviour {

	public void Finished(){

		Application.LoadLevelAsync (Application.loadedLevel + 1);

	}
}
