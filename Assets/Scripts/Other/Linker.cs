using UnityEngine;
using System.Collections;

public class Linker : MonoBehaviour {

    public bool linkXPosition;
    public bool linkYPosition;
    public bool linkZPosition;
    public Transform positionObject;
    private Vector3 positionOffset;
    public bool linkRotation;
    public bool inverseRotation = false;
    public bool linkPositionToRotation = false;
    public Transform rotationObject;
    private Quaternion rotationOffset;

    void Start()
    {
        if(positionObject != null)
            positionOffset = transform.position - positionObject.position;
        if (rotationObject != null)
            rotationOffset = Quaternion.Inverse(inverseRotation ? Quaternion.Inverse(rotationObject.rotation) : rotationObject.rotation) * transform.rotation;
    }

    void FixedUpdate()
    {
        Vector3 linkedPosition = new Vector3(0,0,0);
        if(positionObject != null)
            linkedPosition = positionObject.position + positionOffset;
        Quaternion linkedRotation = Quaternion.identity;
        if (rotationObject != null)
            linkedRotation = (inverseRotation ? Quaternion.Inverse(rotationObject.rotation) : rotationObject.rotation) * rotationOffset;

        if (linkXPosition)
            transform.position = new Vector3(linkedPosition.x, transform.position.y, transform.position.z);
        if (linkYPosition)
            transform.position = new Vector3(transform.position.x, linkedPosition.y, transform.position.z);
        if (linkZPosition)
            transform.position = new Vector3(transform.position.x, transform.position.y, linkedPosition.z);

        if (linkRotation)
            transform.rotation = linkedRotation;
    }
}
