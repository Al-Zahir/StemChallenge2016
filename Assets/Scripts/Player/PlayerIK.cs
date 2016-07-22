using UnityEngine;
using System.Collections;

public class PlayerIK : MonoBehaviour
{

    public bool debug = true;
    public bool useIK;

    public bool leftHandIK;
    public bool rightHandIK;
    public bool leftFootIK = true;
    public bool rightFootIK = true;

    public Vector3 leftHandOffset;
    public Vector3 rightHandOffset;
    public Vector3 handDirectionRight;
    public Vector3 hitPoint;

    public Transform leftOrigin;
    public Transform rightOrigin;

    public Transform rightShimmy, leftShimmy;

    public Vector3 leftOriginOriginalPos;
    public Vector3 rightOriginOriginalPos;

    public Vector3 leftHandPos;
    public Vector3 rightHandPos;
    public Vector3 leftHandActual;
    public Vector3 rightHandActual;

    private Vector3 leftHandVelocity;
    private Vector3 rightHandVelocity;
    public float handSmoothing = 0.1f;
    public float handRaycastMult = 1;

    public Quaternion leftHandRot = Quaternion.identity;
    public Quaternion rightHandRot = Quaternion.identity;

    public float leftWeight = 1f;
    public float rightWeight = 1f;

    public RaycastHit leftShimHit;
    public RaycastHit rightShimHit;

    public bool overrideIK = false;

    private Animator anim;
    private PlayerClimbingControl playerClimbingControl;

    public Transform leftFoot;
    public Transform rightFoot;
    public Transform leftHand;
    public Transform rightHand;

    public RaycastHit LFootHit;
    public RaycastHit RFootHit;

    private Quaternion leftFootRot = Quaternion.identity;
    private Quaternion rightFootRot = Quaternion.identity;
    private Vector3 leftFootPos;
    private Vector3 rightFootPos;
    private Vector3 leftFootActual;
    private Vector3 rightFootActual;
    private Vector3 leftFootVelocity;
    private Vector3 rightFootVelocity;
    public float footSmoothing = 0.1f;

    public float footIKWeight = 0.5f;
    public float leftFootRestZ = 0.5f;
    public float footHangDistance = 1f;

    public float legNoiseLoop = 5f;
    public float legNoiseMagnitude = 2f;
    private float legNoiseTimer;

    public float hanging;

    private Vector3 localLeftHandPosOverride;
    private Vector3 localRightHandPosOverride;
    private Vector3 beforeOverrideLeftPos;
    private Vector3 beforeOverrideRightPos;

    public Transform leftToe;
    public Transform rightToe;
    private bool adjLeft, adjRight;
    private Quaternion leftToeStart;
    private Quaternion rightToeStart;

    void Start()
    {
        leftToeStart = leftToe.localRotation;
        rightToeStart = rightToe.localRotation;
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        playerClimbingControl = GetComponent<PlayerClimbingControl>();

        SetIK(false);

        leftOriginOriginalPos = leftOrigin.localPosition;
        rightOriginOriginalPos = rightOrigin.localPosition;
    }

    void Update()
    {
        SetLocalHandPositions();
    }

    void FixedUpdate()
    {
        DrawRay(leftOrigin.position, -transform.up * handRaycastMult, Color.green);
        DrawRay(rightOrigin.position, -transform.up * handRaycastMult, Color.green);

        if (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.idle_to_braced_hang") ||
            anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.sprint_to_braced_hang") ||
            anim.GetAnimatorTransitionInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Locomotion -> Base Layer.Climbing.idle_to_braced_hang"))
        {
            NoSmoothing();
        }
    }

    void SetLocalHandPositions()
    {
        Vector3 localHit = transform.InverseTransformPoint(hitPoint);
        leftOrigin.localPosition = new Vector3(leftOrigin.localPosition.x, leftOrigin.localPosition.y, localHit.z + 0.03f);
        rightOrigin.localPosition = new Vector3(rightOrigin.localPosition.x, rightOrigin.localPosition.y, localHit.z + 0.03f);
        leftShimmy.localPosition = new Vector3(leftShimmy.localPosition.x, leftShimmy.localPosition.y, localHit.z + 0.2f);
        rightShimmy.localPosition = new Vector3(rightShimmy.localPosition.x, rightShimmy.localPosition.y, localHit.z + 0.2f);
    }

    void LateUpdate()
    {
        if(playerClimbingControl.isClimbing)
        {
            if (adjLeft)
                leftToe.localRotation = leftToeStart;
            if (adjRight)
                rightToe.localRotation = rightToeStart;
        }
    }

    void OnAnimatorIK()
    {
        if (useIK)
        {
            DoIKRays();
            if (leftHandIK || overrideIK)
            {
                if (leftHandIK)
                    localLeftHandPosOverride = transform.InverseTransformPoint(leftHandPos);
                else
                    leftHandActual = transform.TransformPoint(localLeftHandPosOverride);

                leftHandActual = Vector3.SmoothDamp(leftHandActual, !leftHandIK ? transform.TransformPoint(localLeftHandPosOverride) : leftHandPos, ref leftHandVelocity, handSmoothing);

                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftWeight);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandActual);

                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftWeight);
                anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandRot);
            }

            if (rightHandIK || overrideIK)
            {
                if (rightHandIK)
                {
                    localRightHandPosOverride = transform.InverseTransformPoint(rightHandPos);
                }
                else
                    rightHandActual = transform.TransformPoint(localRightHandPosOverride);

                rightHandActual = Vector3.SmoothDamp(rightHandActual, !rightHandIK ? transform.TransformPoint(localRightHandPosOverride) : rightHandPos, ref rightHandVelocity, handSmoothing);

                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightWeight);
                anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandActual);

                anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightWeight);
                anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandRot);
            }
            
            if (leftFootIK)
            {
                leftFootActual = Vector3.SmoothDamp(leftFootActual, leftFootPos, ref leftFootVelocity, footSmoothing);

                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, footIKWeight);
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootActual);

                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, footIKWeight);
                anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRot);
            }

            if (rightFootIK)
            {
                rightFootActual = Vector3.SmoothDamp(rightFootActual, rightFootPos, ref rightFootVelocity, footSmoothing);

                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, footIKWeight);
                anim.SetIKPosition(AvatarIKGoal.RightFoot, rightFootActual);

                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, footIKWeight);
                anim.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRot);
            }
        }
    }

    void DoIKRays()
    {
        if (playerClimbingControl.isClimbing)
        {
            RaycastHit RHit;
            RaycastHit LHit;

            Physics.Raycast(leftShimmy.position, -transform.up, out leftShimHit, 2f);
            Physics.Raycast(rightShimmy.position, -transform.up, out rightShimHit, 2f);
            if (Physics.Raycast(leftOrigin.position, -transform.up, out LHit, 2f) && LHit.transform.gameObject.tag == "Can Climb")
            {
                IKSetWait(true, true);
                leftHandPos = LHit.point + transform.TransformVector(leftHandOffset);

                Vector3 localForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
                leftHandRot = Quaternion.FromToRotation(localForward, LHit.normal) * Quaternion.LookRotation(localForward);
            }
            else
            {
                leftHandIK = false;
            }

            if (Physics.Raycast(rightOrigin.position, -transform.up, out RHit, 2f) && RHit.transform.gameObject.tag == "Can Climb")
            {
                IKSetWait(false, true);
                rightHandPos = RHit.point + transform.TransformVector(rightHandOffset);

                Vector3 localForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
                rightHandRot = Quaternion.FromToRotation(localForward, RHit.normal) * Quaternion.LookRotation(localForward);
            }
            else
            {
                rightHandIK = false;
            }

            if (rightHandIK != leftHandIK)
            {

                if (rightHandIK)
                    leftOrigin.position = Vector3.MoveTowards(leftOrigin.position, rightOrigin.position, 0.01f);
                else
                    rightOrigin.position = Vector3.MoveTowards(rightOrigin.position, leftOrigin.position, 0.01f);

            }
            else if (rightHandIK && leftHandIK)
            {
                handDirectionRight = (RHit.point - LHit.point).normalized * 0.5f;
                DrawRay(transform.position + transform.up, handDirectionRight * 2, Color.cyan);
            }
            else if(!overrideIK)
                handDirectionRight = Vector3.zero;

            Vector3 localLeft = transform.InverseTransformPoint(leftFoot.position);
            localLeft.z = 0.3f;
            Vector3 globalLeft = transform.TransformPoint(localLeft);
            DrawRay(globalLeft, transform.forward, Color.blue);
            legNoiseTimer += Time.deltaTime;
            if (legNoiseTimer > legNoiseLoop)
                legNoiseTimer = 0;
            hanging = 0;
            if(Physics.Raycast(globalLeft, transform.forward, out LFootHit, 0.8f)) //Foot can collide
            {
                leftFootPos = LFootHit.point - transform.forward * 0.05f;
                Vector3 localForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
                if (Vector3.Angle(LFootHit.normal, localForward) > 1)
                    leftFootRot = Quaternion.LookRotation(localForward, LFootHit.normal);
                else
                    leftFootRot = leftFoot.rotation;
                adjLeft = false;
            }
            else //Foot is hanging
            {
                leftFootPos = leftFoot.position - transform.up * footHangDistance + transform.forward * legNoiseMagnitude * Mathf.Sin(2 * Mathf.PI * legNoiseTimer / legNoiseLoop);
                hanging++;
                adjLeft = true;
            }

            Vector3 localRight = transform.InverseTransformPoint(rightFoot.position);
            localRight.z = 0.3f;
            Vector3 globalRight = transform.TransformPoint(localRight);
            DrawRay(globalRight, transform.forward, Color.blue);
            if (Physics.Raycast(globalRight, transform.forward, out RFootHit, 0.8f))
            {
                rightFootPos = RFootHit.point - transform.forward * 0.05f;
                Vector3 localForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
                if (Vector3.Angle(RFootHit.normal, localForward) > 1)
                    rightFootRot = Quaternion.LookRotation(localForward, RFootHit.normal);
                else
                    rightFootRot = rightFoot.rotation;
                adjRight = false;
            }
            else //Foot is hanging
            {
                rightFootPos = rightFoot.position - transform.up * footHangDistance + transform.forward * legNoiseMagnitude * Mathf.Sin(2 * Mathf.PI * legNoiseTimer / legNoiseLoop);
                hanging++;
                adjRight = true;
            }
        }
        else
        {
            leftHandIK = false;
            rightHandIK = false;
        }

    }

    public void ResetHandSpacingWait(float seconds)
    {
        StartCoroutine(HandResetWait(seconds));
    }

    public void ResetHandSpacingImmediate()
    {
        leftOrigin.localPosition = new Vector3(leftOriginOriginalPos.x, leftOrigin.localPosition.y, leftOrigin.localPosition.z);
        rightOrigin.localPosition = new Vector3(rightOriginOriginalPos.x, rightOrigin.localPosition.y, rightOrigin.localPosition.z);
    }

    IEnumerator HandResetWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ResetHandSpacingImmediate();
    }

    public void IKSetWait(bool leftHand, bool setting)
    {
        StartCoroutine(IKSetWaitC(leftHand, setting));
    }

    IEnumerator IKSetWaitC(bool leftHand, bool setting)
    {
        yield return new WaitForSeconds(0.1f);
        if (leftHand)
            leftHandIK = setting;
        else
            rightHandIK = setting;
    }

    public void IKGlobalWait(bool setting, float time)
    {
        StartCoroutine(IKGlobalWaitC(setting, time));
    }

    IEnumerator IKGlobalWaitC(bool setting, float time)
    {
        yield return new WaitForSeconds(time);
        ClearIK();
        useIK = setting;
    }

    public void IKFootLate()
    {
        StartCoroutine(IKFootLateC());
    }

    IEnumerator IKFootLateC()
    {
        leftFootIK = false;
        rightFootIK = false;
        yield return new WaitForSeconds(1f);
        leftFootActual = leftFoot.position;
        rightFootActual = rightFoot.position;
        leftFootIK = true;
        rightFootIK = true;
    }

    public void SetIK(bool b)
    {
        useIK = b;
    }

    public void NoSmoothing()
    {
        rightHandActual = rightHandPos;
        leftHandActual = leftHandPos;
    }

    public void ClearIK()
    {
        leftHandPos = leftHand.position;
        rightHandPos = rightHand.position;
        leftFootPos = leftFoot.position;
        rightFootPos = rightFoot.position;
        leftFootActual = leftFootPos;
        rightFootActual = rightFootPos;
        NoSmoothing();
    }

    // Used to allow the debug boolean to affect drawing rays
    private void DrawRay(Vector3 position, Vector3 direction, Color color, float duration)
    {
        if (debug)
            Debug.DrawRay(position, direction, color, duration);
    }

    private void DrawRay(Vector3 position, Vector3 direction, Color color)
    {
        if (debug)
            Debug.DrawRay(position, direction, color);
    }
}
