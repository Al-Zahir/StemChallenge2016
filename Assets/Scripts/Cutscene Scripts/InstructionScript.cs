using UnityEngine;
using System.Collections;

public class InstructionScript : MonoBehaviour {

	public void ClickedNext(){

		Application.LoadLevelAsync (Application.loadedLevel + 1);

	}
}
