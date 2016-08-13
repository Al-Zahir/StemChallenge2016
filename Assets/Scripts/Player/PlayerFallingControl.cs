using UnityEngine;
using System.Collections;

public class PlayerFallingControl : MonoBehaviour {

	private Animator anim;
	private CapsuleCollider col;
	private Rigidbody rigid;
	private PlayerMovement playerMovement;
    public LayerMask playerLayer;

	private float distToGround;

	private RaycastHit jumpTarget;
	private Vector3 jumpDirection;
	private Vector3 jumpTopStartPoint;
	private int numRays;

	public float jumpSpeed = 2.0f;
	public float jumpAngleOffsetDegrees = 10;
	public float jumpTransitionDisplacementZ = 2.5f;
	public float jumpTimeAllowance = 1;
	public float maxJumpDistanceX = 6;
	public float maxJumpDistanceY = 3;
	public float maxJumpDistanceYDown = 3;
	public float minJumpTargetSize = 2;
	public float jumpTargetOffsetY = 0.4f;
	public bool drawJumpDebug = true;

    private bool jumpTransitionAllowed = true;
    public bool cameFromFallingOffOnAccident = false;
    public Vector3 accidentVelocity = Vector3.zero;
    private Vector3 lastPos;
    private int failPosCheckCtr;
    private bool canCheckFail = false;

    private Vector3[] lastVelocities = new Vector3[5];
    private int lastVelocityIndex = -1;
    public float failCheckDistance = 0.01f;

    public Vector3 minVel = new Vector3(0, 2, 2);

    public AudioClip[] jumpSounds;
    public AudioClip landSound;

	void Awake(){

		anim = GetComponent<Animator> ();
		col = GetComponent<CapsuleCollider> ();
		rigid = GetComponent<Rigidbody> ();
		playerMovement = GetComponent<PlayerMovement> ();

		distToGround = col.bounds.extents.y;

	}

    void FixedUpdate()
    {
        if (lastVelocityIndex == -1)
        { 
            for (int i = 0; i < lastVelocities.Length; i++)
                lastVelocities[i] = rigid.velocity;
            lastVelocityIndex++;
        }
        else
            lastVelocities[lastVelocityIndex] = rigid.velocity;

        lastVelocityIndex++;
        if (lastVelocityIndex >= lastVelocities.Length)
            lastVelocityIndex = 0;

        if (playerMovement.isDisabledByGround && jumpTransitionAllowed && Vector3.Magnitude(transform.position - lastPos) < failCheckDistance * Time.deltaTime && canCheckFail)
            failPosCheckCtr++;

        lastPos = transform.position;
    }

	void Update(){
        playerMovement.rootMotionFall = Mecanim.inAnim(anim, "Base Layer.Falling.falling_to_roll", 0) ||
                                        Mecanim.soonInAnim(anim, "Base Layer.Falling.falling_to_roll", 0);

        if (playerMovement.isDisabledByBattle || playerMovement.isDisabledByClimb)
            return;

		HandleJumpInput ();

        if (!playerMovement.isDisabledByGround && jumpTransitionAllowed && (!IsGrounded() || (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !playerMovement.isHoldingBow)))
        {
            jumpTransitionAllowed = false;
            cameFromFallingOffOnAccident = !(Input.GetKeyDown(KeyCode.Space) && IsGrounded());
            accidentVelocity = rigid.velocity;
            StartFall();
        }
        else if (playerMovement.isDisabledByGround && jumpTransitionAllowed && (IsNearGround(0.1f) || failPosCheckCtr > 20))
        {
            jumpTransitionAllowed = false; 
            //Debug.Log(failPosCheckCtr);
            EndFall();
        }

	}

	void HandleJumpInput() {
		jumpDirection = transform.forward;
		jumpTopStartPoint = transform.position + new Vector3(0, maxJumpDistanceY, 0);
		jumpTopStartPoint += transform.forward * jumpTransitionDisplacementZ;
		// An extra ray to determine if final cast is jumpable
		numRays = (int)(maxJumpDistanceX / minJumpTargetSize) + 1;
		if (drawJumpDebug) {
			Debug.DrawRay (jumpTopStartPoint, jumpDirection * maxJumpDistanceX, Color.red);
			Debug.DrawRay (jumpTopStartPoint + jumpDirection * maxJumpDistanceX, jumpDirection * minJumpTargetSize, Color.blue);
			
			for (int i=0; i<numRays; i++) {
				Debug.DrawRay (jumpTopStartPoint + jumpDirection * (i + 1) * minJumpTargetSize, 
				               Vector3.down * (maxJumpDistanceY + maxJumpDistanceYDown), 
				               Color.green);
			}
		}
	}

	public void StartFall(){
		// Data saved for future use if needed
		RaycastHit[] frontData = new RaycastHit[numRays];
		bool lastHitSuccess = false;
		int hitSuccessIndex = -1;
		
		for (int i=0; i<numRays; i++) {
			RaycastHit outHit;
			Physics.Raycast(jumpTopStartPoint + jumpDirection * (i + 1) * minJumpTargetSize, 
			                Vector3.down, out outHit, maxJumpDistanceY + maxJumpDistanceYDown);
			frontData[i] = outHit;
			if(lastHitSuccess &&  outHit.transform != null)
			{
				// Found a platform of adequate width
				hitSuccessIndex = i-1;
			}
			else if(outHit.transform != null)
			{
				lastHitSuccess = true;
			}
		}
		
		if(hitSuccessIndex != -1)
        {
            playerMovement.isDisabledByGround = true;
			// Target used inside OnJumpStart
			jumpTarget = frontData[hitSuccessIndex];
            anim.SetBool("FallingAccident", cameFromFallingOffOnAccident);
            anim.SetBool("Falling", true);
            anim.SetTrigger("FallingTrigger");
            failPosCheckCtr = 0;
		}
        else
        {
            playerMovement.isDisabledByGround = true;
            jumpTarget = new RaycastHit();
            jumpTarget.point = transform.position + transform.forward * 2f;
            anim.SetBool("FallingAccident", cameFromFallingOffOnAccident);
            anim.SetBool("Falling", true);
            anim.SetTrigger("FallingTrigger");
            failPosCheckCtr = 0;
        }

        if (cameFromFallingOffOnAccident)
        {
            StartCoroutine(AllowTransition());
            StartCoroutine(FixAccidentVel());
        }

        StartCoroutine(FixFail());
	}

	void OnJumpStart() {
		Vector3 targetDisplacement = transform.InverseTransformDirection(jumpTarget.point - transform.position) + 
			jumpTargetOffsetY * Vector3.up;
		float zDisplacement = targetDisplacement.z - jumpTransitionDisplacementZ;
		float yDisplacement = targetDisplacement.y;
		float[] math = CharacterMath.JumpInitialVelocity (zDisplacement, yDisplacement);
		float angle = math [1] * Mathf.Deg2Rad;//Mathf.Atan2 (yDisplacement, zDisplacement) + jumpAngleOffsetDegrees *  Mathf.PI / 180;
		float magnitude = math [0];//CharacterMath.JumpInitialVelocity(zDisplacement,
		//yDisplacement,
		// angle);
		/*if (magnitude < 0)
			return;
		Vector3 direction = transform.TransformDirection (new Vector3 (0, Mathf.Sin (angle), 
		                                                               Mathf.Cos (angle)));
		Vector3 force = magnitude * direction.normalized;
		if (force.magnitude > 0) {
			anim.applyRootMotion = false;
			r.AddForce (force, ForceMode.VelocityChange);
		}*/

		//Debug.Log ("Z: " + zDisplacement + " Y: " + yDisplacement + " V: " + magnitude + " ø: " + angle);

        if (Vector3.Magnitude(targetDisplacement) > 100f)
            cameFromFallingOffOnAccident = true;

        if (!cameFromFallingOffOnAccident)
        {
            rigid.velocity = transform.forward * magnitude * Mathf.Cos(angle) + transform.up * magnitude * Mathf.Sin(angle);
            PlayJump();
        }

        if (rigid.velocity.magnitude < minVel.magnitude)
        {
            rigid.velocity = transform.TransformVector(minVel);
        }

        StartCoroutine(AllowTransition());
        cameFromFallingOffOnAccident = false;
	}

    public void PlayJump()
    {
        GetComponent<AudioSource>().clip = jumpSounds[Random.Range(0, jumpSounds.Length)];
        GetComponent<AudioSource>().Play();
    }

    private IEnumerator AllowTransition()
    {
        yield return new WaitForSeconds(0.1f);
        jumpTransitionAllowed = true;
    }

    private IEnumerator FixAccidentVel()
    {
        yield return new WaitForFixedUpdate();
        rigid.velocity = accidentVelocity;
    }

    private IEnumerator FixFail()
    {
        canCheckFail = false;
        yield return new WaitForSeconds(1f);
        canCheckFail = true;
    }
	
	public void EndFall(){

		anim.SetBool ("Falling", false);
        GetComponent<AudioSource>().clip = landSound;
        GetComponent<AudioSource>().Play();

        float yVel = rigid.velocity.y;
        foreach (Vector3 velocity in lastVelocities)
            if (Mathf.Abs(velocity.y) > Mathf.Abs(yVel))
                yVel = velocity.y;
        //Debug.Log(yVel);
		//float yVel = rigid.velocity.y;

		if (yVel > -9.8f)
			anim.SetTrigger ("FallingLand");
		else if (yVel > -12.6f)
			anim.SetTrigger ("FallingRoll");
		else
			anim.SetTrigger ("FallingHard");

		playerMovement.isDisabledByGround = false;
        StartCoroutine(AllowTransition());
	}

	public bool IsNotOnEdge() {
		int numberOfHits = 0;
		
		Vector3 pos = transform.position + distToGround * Vector3.up;
        float k = 0.5f;
		
		Debug.DrawRay (pos + col.radius * transform.right + (col.radius / 2) * transform.forward, -Vector3.up * (distToGround + k), Color.red);
		Debug.DrawRay (pos - col.radius * transform.right + (col.radius / 2) * transform.forward, -Vector3.up * (distToGround + k), Color.red);
		Debug.DrawRay (pos + col.radius * transform.forward, -Vector3.up * (distToGround + k), Color.red);
		Debug.DrawRay (pos - col.radius * transform.forward, -Vector3.up * (distToGround + k), Color.red);
		
		if (Physics.Raycast (pos + col.radius * transform.right + (col.radius / 1.5f) * transform.forward, -Vector3.up, distToGround + k))
			numberOfHits++;
		if (Physics.Raycast (pos - col.radius * transform.right + (col.radius / 1.5f) * transform.forward, -Vector3.up, distToGround + k))
			numberOfHits++;
		if (Physics.Raycast (pos + col.radius * transform.forward, -Vector3.up, distToGround + k))
			numberOfHits++;
		if (Physics.Raycast (pos - col.radius * transform.forward, -Vector3.up, distToGround + k))
			numberOfHits++;
		
		return (numberOfHits > 1);
	}

    public bool IsGrounded()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y - 0.1f, col.bounds.center.z), col.radius, ~playerLayer);
    }

    public bool IsNearGround(float height)
    {
        Debug.DrawRay(transform.position + 0.01f * transform.up, Vector3.down * height);
        return Physics.Raycast(transform.position + 0.01f * transform.up, Vector3.down, height, ~playerLayer);
    }

	public bool IsNotOnEdge(Vector3 direction){

		Vector3 pos = transform.position + distToGround * Vector3.up;

		return Physics.Raycast (pos + col.radius * direction, -Vector3.up, distToGround + 0.01f);

	}
	
}
