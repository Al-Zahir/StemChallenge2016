using UnityEngine;
using System.Collections;

public class StartTempleDoor : MonoBehaviour {

    public float openCloseTime = 2f;
    private IEnumerator currentAction;
    public float lowerAmount = 10f;
    private Vector3 closedPos, openPos;
    public bool allowOpen = true;
    private GameController gameController;

    void Start()
    {
        closedPos = transform.parent.position;
        openPos = closedPos - lowerAmount * transform.up;
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public IEnumerator OpenClose(bool open)
    {
        float start = Time.time;
        Vector3 startPos = transform.parent.position;
        gameController.GetComponent<AudioSource>().clip = gameController.doorSounds[Random.Range(0, gameController.doorSounds.Length)];
        gameController.GetComponent<AudioSource>().Play();
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
