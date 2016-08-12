using UnityEngine;
using System.Collections;

public class FinalPuzzlePlate : MonoBehaviour {

    public Transform leftBlock, rightBlock;
    public FinalTempleDoor door;
    public bool endPuzzle, loweredRight;
    private IEnumerator leftAction, rightAction;
    public float lowerAmount = 4f;
    public float lowerTime = 2f;
    private float upY;

	// Use this for initialization
	void Start () {
        upY = rightBlock.position.y;
	}

    public void OnArrowHit()
    {
        if (loweredRight && !endPuzzle)
        {
            endPuzzle = true;
            StartCoroutine(LowerBlock(leftBlock, false));
            door.allowOpen = true;
            StartCoroutine(door.OpenClose(true));
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "PlayerBody")
        {
            if (rightAction != null)
                StopCoroutine(rightAction);

            rightAction = LowerBlock(rightBlock, false);
            loweredRight = true;
            StartCoroutine(rightAction);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag == "PlayerBody" && !endPuzzle)
        {
            if (rightAction != null)
                StopCoroutine(rightAction);

            rightAction = LowerBlock(rightBlock, true);
            loweredRight = false;
            StartCoroutine(rightAction);
            StartCoroutine(LowerBlock(leftBlock, true));
        }
    }

    private IEnumerator LowerBlock(Transform block, bool up)
    {
        
        float start = Time.time;
        Vector3 startPos = block.position;
        Vector3 upPos = startPos;
        upPos.y = up ? upY : upY - lowerAmount;

        while(Time.time < start + lowerTime)
        {
            block.position = Vector3.Lerp(startPos, upPos, Mathf.SmoothStep(0, 1, (Time.time - start) / lowerTime));
            yield return new WaitForFixedUpdate();
        }

        if(block == rightBlock)
            rightAction = null;
        else
            leftAction = null;
    }
}
