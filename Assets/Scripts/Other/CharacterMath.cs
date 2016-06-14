using UnityEngine;
using System.Collections;
using System;

public class CharacterMath {
	
	private static float g = -Physics.gravity.y;
	
	public static float[] JumpInitialVelocity(float z, float y) {
		float minVelocity = 100000000;
		float angle = (Mathf.Atan2 (y, z));
		
		for (float a = angle; a < 90; a += 5f) {
			float numerator = g * Mathf.Pow (z, 2);
			float denominator1 = 2 * Mathf.Pow (Mathf.Cos (a * Mathf.Deg2Rad), 2);
			float denominator2 = z * Mathf.Tan (a * Mathf.Deg2Rad) - y;
			
			float velocity = Mathf.Sqrt(numerator / (denominator1 * denominator2));
			
			float time = z / (velocity * Mathf.Cos(a * Mathf.Deg2Rad));
			float dydt = velocity * Mathf.Sin(a * Mathf.Deg2Rad) - g * time;
			float dxdt = velocity * Mathf.Cos(a * Mathf.Deg2Rad);
			float dydx = dydt / dxdt;
			
			if(velocity < minVelocity && dydx < -1){
				minVelocity = velocity;
				angle = a;
			}
		}
		return new float[] {minVelocity, angle};
	}
}
