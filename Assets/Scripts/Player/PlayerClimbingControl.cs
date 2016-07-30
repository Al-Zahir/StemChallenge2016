using UnityEngine;
using System.Collections;

public class PlayerClimbingControl : MonoBehaviour
{

    private Animator anim;
    private PlayerIK playerIK;
    private PlayerMovement playerMovement;
    private PlayerBattleControl playerBattleControl;
    private Rigidbody rigid;
    private CapsuleCollider col;
    public bool debug = true;
    public bool isClimbing;
    private bool startingClimb = false;

    private Vector3 smoothingPos;
    public float smoothingTime = 1f;
    private float originalSmoothingTime;
    private Vector3 smoothingVelocity = Vector3.zero;

    public AnimationCurve climbStartY;
    public AnimationCurve jumpY;
    public AnimationCurve jumpX;
    public float jumpTimeout = 2f;
    private float jumpTimer;
    private float climbStartTimer;
    private float climbStartDistance;
    public float climbIKEnableTime = 0.6f;
    public float jumpIKDisableTime = 0.14f;
    public float jumpIKEnableTime = 0.48f;
    public float climbStartSmoothingTime = 0.01f;
    public float jumpGridSizeX = 2f;
    public float jumpGridSizeY = 3f;
    public float jumpGridStart = 2f;

    private bool disableInputInternal; // Used for assumptions about player state, dangerous if touched outside script
    public bool disableInputExternal; // Script behaves identical to if player let go of keyboard

    private RaycastHit hit;
    private Vector3 lastPos;
    private int lag = 0;
    private float lagCtr = 0;
    private float smoothMult = 1;

    private float shimmyFailCtr = 0;
    public float bodyNoiseLoop = 3f;
    public float bodyNoiseMagnitude = 0.5f;
    private float bodyNoiseTimer;

    public float hangWeight = 0.5f;

    private bool jumpRequested;
    private bool jumping;
    private float sign;
    private bool shimmy;

    void Start()
    {
        originalSmoothingTime = smoothingTime;
    }

    void Awake()
    {

        anim = GetComponent<Animator>();
        playerIK = GetComponent<PlayerIK>();
        playerMovement = GetComponent<PlayerMovement>();
        playerBattleControl = GetComponent<PlayerBattleControl>();
        rigid = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        isClimbing = false;
    }

    void Update()
    {

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool inFall = anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Falling.falling_idle");

        if (disableInputExternal || disableInputInternal)
        {
            h = 0;
            v = 0;
        }
        else if (isClimbing && Input.GetKeyDown(KeyCode.Space)) //KeyDown only works in update
            StartCoroutine(JumpRequest());

        if (shimmy)
        {
            playerIK.rightHandPos += playerIK.handDirectionRight * anim.GetFloat("RightHandShimmy");
            playerIK.leftHandPos += playerIK.handDirectionRight * anim.GetFloat("LeftHandShimmy");
            playerIK.NoSmoothing();
            playerIK.overrideIK = true;
        }

        //If not transitioning, and in the locomotion state, and is climbing, set is climbing to false
        if (anim.GetAnimatorTransitionInfo(0).fullPathHash == 0
            && (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Locomotion") ||
            anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Falling.falling_idle") ||
            anim.GetNextAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Falling.falling_idle"))
            && isClimbing && !startingClimb)
        {
            isClimbing = false;
            playerMovement.isDisabledByClimb = false;
            rigid.isKinematic = false;
        }

        //If the player can move, and is not climbing, and not transitioning, and is in the locomtion state, and is moving, start climbing rays
        if ((playerMovement.canMove()
            && !isClimbing
            && anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Locomotion")
            && v != 0
            && Mathf.Abs(playerMovement.angle) < 0.1f
            || inFall)
            && anim.GetAnimatorTransitionInfo(0).fullPathHash == 0
            && Physics.Raycast(transform.position, transform.forward, 2f))
        {
            for (float y = 1; y < 3; y += 0.1f)
            {
                DrawRay(transform.position + transform.up * y - transform.forward * 0.1f, transform.forward, Color.green);
                if (Physics.Raycast(transform.position + transform.up * y - transform.forward * 0.1f, transform.forward, out hit, 1.0f))
                    if (hit.transform.gameObject.tag == "Can Climb")
                        StartCoroutine(StartClimb(hit, inFall));
            }
        }

        if (isClimbing
            && anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_idle")
            && anim.GetAnimatorTransitionInfo(0).fullPathHash == 0
            && v != 0)
            ClimbUpDown(v);
    }

    private void UpdateHit()
    {
        RaycastHit tempHit;
        float closestZ = 100000f;
        for (float y = 0; y < 0.5; y += 0.01f)
        {
            Physics.Raycast(transform.position + transform.up * (0.9f + y), transform.forward, out tempHit, 1.0f);
            Vector3 localHit = transform.InverseTransformPoint(tempHit.point);
            DrawRay(transform.position + transform.up * (0.9f + y), transform.forward, Color.red);
            if (localHit.z < closestZ && localHit.z > 0.01f && tempHit.transform != null && tempHit.transform.tag == "Can Climb")
            {
                closestZ = localHit.z;
                hit = tempHit;
            }
        }
    }

    private void UpdateRot()
    {
        UpdateHit();
        DrawRay(hit.point, hit.normal * 0.2f, Color.red);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-hit.normal, Vector3.up), 0.2f);
        if ((anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_idle")
            || anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_shimmy")
            || anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_shimmy_1"))
            && (anim.GetAnimatorTransitionInfo(0).fullPathHash == 0 || anim.GetCurrentAnimatorStateInfo(0).fullPathHash != Animator.StringToHash("Base Layer.Climbing.braced_hang_idle"))
            && !disableInputInternal)
        {
            smoothingPos = hit.point - transform.up + hit.normal * col.radius;
            playerIK.hitPoint = hit.point;
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (disableInputExternal || disableInputInternal)
        {
            h = 0;
            v = 0;
        }

        // Had to put these two ifs into fixed as they interfered with moving platforms if updated every frame
        if (isClimbing)
            UpdateRot();

        if (isClimbing
            && v == 0
            && h != 0
            && !(anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.idle_to_braced_hang")
            || anim.GetAnimatorTransitionInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Locomotion -> Base Layer.Climbing.idle_to_braced_hang")))
            ClimbLeftRight(h);
        else if (anim.GetFloat("ClimbShimmy") != 0)
            EndShimmy();

        rigid.angularVelocity = Vector3.zero;
        Vector3 tempSmoothPos = smoothingPos;

        // If starting to climb can offset the target position to create a natural jump
        if (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.idle_to_braced_hang") ||
           anim.GetAnimatorTransitionInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Locomotion -> Base Layer.Climbing.idle_to_braced_hang"))
        {
            if (climbStartTimer < 0.0001) //Just started
                climbStartDistance = smoothingPos.y - transform.position.y;
            climbStartTimer += Time.deltaTime;
            tempSmoothPos.y += climbStartDistance * (climbStartY.Evaluate(climbStartTimer) - 1); //0 on curve shifts player back to ground
            smoothingTime = climbStartSmoothingTime;
        }
        // Animates the leap left/right by changing smoothing time and the Y position of the player
        else if (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_hop_left") ||
                 anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_hop_right"))
        {
            if (jumpTimer < 0.0001)
            {
                playerIK.IKGlobalWait(false, jumpIKDisableTime);
                playerIK.IKGlobalWait(true, jumpIKEnableTime);
            }

            tempSmoothPos.y += jumpY.Evaluate(jumpTimer);
            smoothingTime = jumpX.Evaluate(jumpTimer);
            jumpTimer += Time.deltaTime;
        }
        else
        {
            if (!jumping || !disableInputInternal)
            {
                jumping = false;
                climbStartTimer = 0;
                jumpTimer = 0;
                smoothingTime = originalSmoothingTime;
            }
        }

        // Deal with moving platforms / lag only if not in these states
        if (!(anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_hop_left") ||
            anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_hop_right")))
        {
            // React to sharp local Z change
            Vector3 localDelta = transform.InverseTransformVector(hit.point - lastPos);
            MoreInfo info = hit.transform != null ? hit.transform.GetComponent<MoreInfo>() : null;
            bool lagging = false;
            if (info == null)
                lagging = Mathf.Abs(localDelta.z) > 0.005f;
            else
                lagging = info.climbStickZ && Mathf.Abs(localDelta.z) > 0.005f || info.climbStickY && (Mathf.Abs(localDelta.y) > 0.001f && (anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_idle")
                || anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_shimmy")
                || anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.Climbing.braced_hang_shimmy_1"))
                && (anim.GetAnimatorTransitionInfo(0).fullPathHash == 0 || anim.GetCurrentAnimatorStateInfo(0).fullPathHash != Animator.StringToHash("Base Layer.Climbing.braced_hang_idle")));

            if (lagging)
                lag++;
            else
                lag -= 5;
            if (lag < 0) lag = 0;
            if (lag > 4)
            {
                smoothMult = 1f / (1f + lag);
                playerIK.hitPoint = hit.point;
                playerIK.NoSmoothing();
                playerIK.overrideIK = true;
            }
            else
            {
                smoothMult = 1;
                playerIK.overrideIK = false;
            }
        }

        bodyNoiseTimer += Time.deltaTime;
        if (bodyNoiseTimer > bodyNoiseLoop)
            bodyNoiseTimer = 0;


        if (lag < 5)
        {
            tempSmoothPos += -transform.up * playerIK.hanging * hangWeight;
            tempSmoothPos += transform.up * bodyNoiseMagnitude * Mathf.Sin(2 * Mathf.PI * bodyNoiseTimer / bodyNoiseLoop);
            tempSmoothPos += transform.forward * bodyNoiseMagnitude * Mathf.Cos(2 * Mathf.PI * bodyNoiseTimer / bodyNoiseLoop);
        }

        if (isClimbing)
            transform.position = Vector3.SmoothDamp(transform.position, tempSmoothPos, ref smoothingVelocity, smoothingTime * smoothMult);

        lastPos = hit.point;
    }

    void ClimbUpDown(float v)
    {
        if (v > 0.001f)
        {
            int numHit = 0;
            for (float y = 2; y < 3; y += 0.1f)
            {
                DrawRay(transform.position + transform.up * y, transform.forward, Color.green, 5f);
                if (Physics.Raycast(transform.position + transform.up * y, transform.forward, out hit, 1.5f))
                {
                    numHit++;
                    if (hit.transform.gameObject.tag == "Can Climb")
                    {
                        playerIK.ResetHandSpacingWait(0.3f);
                        anim.SetTrigger("ClimbUp");
                        lag = 0;
                        smoothMult = 1;
                        Vector3 targetPos = hit.point - transform.up + hit.normal * col.radius;
                        smoothingPos = targetPos;
                        playerIK.hitPoint = hit.point;
                        StartCoroutine(ClimbTimeout(1));
                        break;
                    }
                }
            }
            if (numHit == 0)
                EndClimb(v);
        }
        else if (v < -0.001f)
        {
            RaycastHit hit;

            bool breakedout = false;
            for (float y = -1; y <= 0; y += 0.1f)
            {
                DrawRay(transform.position + transform.up * y, transform.forward, Color.green);
                if (Physics.Raycast(transform.position + transform.up * y, transform.forward, out hit, 1.0f))
                {
                    if (hit.transform.gameObject.tag == "Can Climb")
                    {
                        playerIK.ResetHandSpacingWait(0.3f);
                        anim.SetTrigger("ClimbDown");
                        lag = 0;
                        smoothMult = 1;
                        Vector3 targetPos = hit.point - transform.up + hit.normal * col.radius;
                        smoothingPos = targetPos;
                        playerIK.hitPoint = hit.point;
                        StartCoroutine(ClimbTimeout(1));
                        breakedout = true;
                        break;
                    }
                }
            }

            if (!breakedout)
            {
                if (Physics.Raycast(transform.position, -transform.up, out hit, 2f))
                    smoothingPos = hit.point;
                else if (Input.GetKeyDown(KeyCode.Space))
                    smoothingPos = transform.position - 2 * transform.up;
                else
                    return;

                EndClimb(v);
            }
        }
    }

    void ClimbLeftRight(float h)
    {
        RaycastHit hit;
        sign = (h > 0) ? 1f : -1f;
        hit = sign < 0 ? playerIK.leftShimHit : playerIK.rightShimHit;

        //Jump Detection
        if (jumpRequested)
        {
            jumpRequested = false;
            float previousZ = -1f;
            RaycastHit detection;
            //Loop to draw the debug grid as it will exit raycasting early and not do most of them
            if (debug)
                for (float x = jumpGridSizeX; x >= 0; x -= 0.1f, previousZ = -1f)
                    for (float y = 2 - jumpGridSizeY; y < jumpGridSizeY; y += 0.1f)
                        DrawRay(transform.position - transform.forward + transform.right * sign * (x + jumpGridStart) + transform.up * y, transform.forward * 2, Color.blue, 5);

            for (float x = jumpGridSizeX; x >= 0; x -= 0.1f, previousZ = -1f)
                for (float y = 2 - jumpGridSizeY; y < jumpGridSizeY; y += 0.1f)
                {
                    if (Physics.Raycast(transform.position - transform.forward + transform.right * sign * (x + jumpGridStart) + transform.up * y, transform.forward, out detection, 2f))
                    {
                        float currentZ = transform.InverseTransformPoint(detection.point).z;
                        if (detection.transform.tag != "Can Climb")
                        {
                            previousZ = currentZ;
                            continue;
                        }

                        if (previousZ != -1f && currentZ < previousZ) // Implies a player-directed indent to hang on
                        {
                            playerIK.ResetHandSpacingWait(0.3f);
                            anim.SetTrigger(sign > 0 ? "ClimbJumpRight" : "ClimbJumpLeft");
                            lag = 0;
                            smoothMult = 1;
                            Vector3 targetPos = detection.point - transform.up + detection.normal * col.radius;
                            smoothingPos = targetPos;
                            smoothingTime = jumpX.Evaluate(0);
                            jumping = true;
                            playerIK.hitPoint = detection.point;
                            disableInputInternal = true;
                            StartCoroutine(ClimbTimeout(jumpTimeout));
                            playerIK.ResetHandSpacingWait(jumpTimeout);
                            return;
                        }

                        previousZ = currentZ;
                    }
                    else
                        previousZ = 1000f;
                }
        }

        if (!disableInputInternal && (sign < 0 && playerIK.leftShimHit.transform != null || sign > 0 && playerIK.rightShimHit.transform != null))
        {
            if (hit.transform.gameObject.tag == "Can Climb")
            {
                anim.SetFloat("ClimbShimmy", sign);

                playerIK.ResetHandSpacingImmediate();
                if (playerIK.handDirectionRight == Vector3.zero)
                {
                    if (shimmyFailCtr > 1f)
                    {
                        EndShimmy();
                        return;
                    }
                    shimmyFailCtr += Time.deltaTime;
                }
                else
                    shimmyFailCtr = 0;

                shimmy = true;

                smoothingPos = this.hit.point - transform.up + this.hit.normal * col.radius + playerIK.handDirectionRight * sign * Mathf.Pow(smoothMult, 0.667f);
            }
            else
                EndShimmy();
        }
        else
            EndShimmy();
    }

    private void EndShimmy()
    {
        anim.SetFloat("ClimbShimmy", 0);
        shimmyFailCtr = 0;
        playerIK.overrideIK = false;
        shimmy = false;
    }

    private IEnumerator ClimbTimeout(float seconds)
    {
        disableInputInternal = true;
        yield return new WaitForSeconds(seconds);
        disableInputInternal = false;
    }

    private IEnumerator StartClimb(RaycastHit hit, bool inFall)
    {
        if (!isClimbing)
        {
            if (playerMovement.isDisabledByGround)
                yield break;

            isClimbing = true;
            startingClimb = true;
            playerIK.ResetHandSpacingImmediate();

            playerMovement.isDisabledByClimb = true;
            smoothingPos = transform.position;
            if (playerBattleControl.isInBattle)
            {
                if (inFall)
                {
                    playerBattleControl.Dequip();
                }
                else
                {
                    playerBattleControl.GracefulDequip();
                    while (playerBattleControl.isTransitioning)
                        yield return new WaitForSeconds(0.1f);
                }
            }

            transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);

            anim.SetBool("Climbing", true);

            rigid.velocity = new Vector3(0, rigid.velocity.y, 0);

            rigid.useGravity = true;
            rigid.isKinematic = true;

            playerIK.IKGlobalWait(true, climbIKEnableTime);
            playerIK.ClearIK();
            playerIK.IKFootLate();

            Vector3 targetPos = hit.point - transform.up + hit.normal * col.radius;
            DrawRay(hit.point, hit.normal * 2, Color.cyan, 10f);
            playerIK.hitPoint = hit.point;
            smoothingPos = targetPos;
            startingClimb = false;
        }
    }

    void EndClimb(float v)
    {
        anim.SetFloat("FinishClimbing", v);
        anim.SetBool("Climbing", false);
        playerIK.ResetHandSpacingWait(0.3f);
        rigid.velocity = Vector3.zero;
        playerIK.SetIK(false);
        rigid.useGravity = true;

        bool foundHit = false;
        if (v > 0)
        {
            RaycastHit hit;

            for (float y2 = 0; y2 < 3; y2 += 0.1f)
            {
                DrawRay(transform.position + transform.up * y2, transform.forward, Color.green);

                if (Physics.Raycast(transform.position + transform.up * y2, transform.forward, out hit, 1.0f))
                {
                    smoothingPos = hit.point + transform.forward * col.radius;
                    foundHit = true;
                }
            }
            
            if(foundHit)
                return;
        }

        GetComponent<PlayerFallingControl>().StartFall();
    }

    private IEnumerator JumpRequest()
    {
        jumpRequested = true;
        yield return new WaitForSeconds(0.1f);
        jumpRequested = false;
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