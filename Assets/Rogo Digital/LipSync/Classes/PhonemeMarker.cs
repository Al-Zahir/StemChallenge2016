using UnityEngine;

namespace RogoDigital.Lipsync {
	[System.Serializable]
	public class PhonemeMarker : System.Object {
		[SerializeField]
		public Phoneme phoneme;
		[SerializeField]
		public float time;
		[SerializeField]
		public float intensity = 1;
		[SerializeField]
		public bool sustain = false;

		public PhonemeMarker (Phoneme phoneme, float time, float intensity, bool sustain) {
			this.phoneme = phoneme;
			this.time = time;
			this.intensity = intensity;
			this.sustain = sustain;
		}

		public PhonemeMarker (Phoneme phoneme, float time) {
			this.phoneme = phoneme;
			this.time = time;
		}
	}
}