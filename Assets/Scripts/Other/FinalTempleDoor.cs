using UnityEngine;
using System.Collections;

public class FinalTempleDoor : MonoBehaviour {

    public float openCloseTime = 2f;
    private IEnumerator currentAction;
    public float lowerAmount = 10f;
    private Vector3 closedPos, openPos;
    public bool allowOpen = false;

    void Start()
    {
        closedPos = transform.parent.position;
        openPos = closedPos - lowerAmount * transform.up;
    }

    public IEnumerator OpenClose(bool open)
    {
        float start = Time.time;
        Vector3 startPos = transform.parent.position;
        while(Time.time < start + openCloseTime)
        {
            transform.parent.position = Vector3.Lerp(startPos, open ? openPos : closedPos, Mathf.SmoothStep(0, 1, (Time.time - start) / openCloseTime));
            yield return new WaitForFixedUpdate();
        }

        currentAction = null;
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "PlayerBody" && allowOpen)
        {
            //Debug.Log("ENTER:"+col.name);
            if (currentAction != null)
                StopCoroutine(currentAction);

            currentAction = OpenClose(true);
            StartCoroutine(currentAction);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag == "PlayerBody" && allowOpen)
        {
            if (currentAction != null)
                StopCoroutine(currentAction);

            currentAction = OpenClose(false);
            StartCoroutine(currentAction);
        }
    }
}
