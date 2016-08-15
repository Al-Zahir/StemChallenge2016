using UnityEngine;
using System.Collections.Generic;

namespace RogoDigital.Lipsync {
	public class LipSyncProject : ScriptableObject {
		[SerializeField]
		public string[] emotions;
		[SerializeField]
		public Color[] emotionColors;
		[SerializeField]
		public List<string> gestures;
	}
}