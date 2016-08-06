using UnityEngine;
using System.Collections;

public class Mecanim : MonoBehaviour {

	public static bool inTrans(Animator anim, int layer)
	{
		return anim.GetAnimatorTransitionInfo(layer).fullPathHash != 0;
	}

	public static bool inAnim(Animator anim, string name, int layer)
	{
		return anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == Animator.StringToHash(name);
	}

	public static bool inAnyAnim(Animator anim, string[] names, int layer)
	{
		foreach (string name in names)
			if (anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == Animator.StringToHash(name))
				return true;
		return false;
	}

	public static bool soonInAnim(Animator anim, string name, int layer)
	{
		return anim.GetNextAnimatorStateInfo(layer).fullPathHash == Animator.StringToHash(name);
	}

	public static bool soonInAnyAnim(Animator anim, string[] names, int layer)
	{
		foreach (string name in names)
			if (anim.GetNextAnimatorStateInfo(layer).fullPathHash == Animator.StringToHash(name))
				return true;
		return false;
	}
}
