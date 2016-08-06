using UnityEngine;
using System.Collections;

public class PlayerBattleControl : MonoBehaviour {

	public bool isInBattle;
	public float maxDistance = 5;

	private Animator anim;
	private Rigidbody rigid;
	private PlayerMovement playerMovement;
	private PlayerWeaponSelector playerWeaponSelector;
	private PlayerHealth playerHealth;

	public Transform swordUnarmedPosition;
	public Transform swordArmedPosition;
	public Transform sword;

	public Transform shieldUnarmedPosition;
	public Transform shieldArmedPosition;
    public Transform shield;
    public Transform shieldAnimLeft;
    public Transform shieldAnimRight;

    public float blockingRotTime = 0.01f;

    public string[] swordRootMotionAnimations = {"Base Layer.sword_slash_1"};
    public int numAttacks = 4;

    public float speedDampTime = 0.1f;
    private Quaternion swordAnimRot;

    private float swordClickStartTime;
    private float swordHoldStartTime;
    private bool swordClick;
    public float swordClickMax = 0.2f;
    public float swordHoldMax = 0.5f;
    private bool lastClickWasDouble = false;
    private bool lastClickWasHeld = false;
    private bool stillHolding = false;

    public float shieldSwingPeriod = 1f;
    public float shieldSwingOffset = 0.2f;

    // Allows other scripts to sheath the sword
    public bool isTransitioning = false;

	void Awake(){

		isInBattle = false;

		anim = GetComponent<Animator> ();
		rigid = GetComponent<Rigidbody> ();
		playerMovement = GetComponent<PlayerMovement> ();
		playerWeaponSelector = GetComponent<PlayerWeaponSelector> ();
		playerHealth = GetComponent<PlayerHealth> ();

		playerMovement.isDisabledByBattle = isInBattle;

	}
    
	void Update()
    {
        if (!mecInTrans(1) && mecInAnim("Equip Dequip.sword_base", 1))
            isTransitioning = false;

        playerMovement.rootMotionBattle = mecInAnyAnim(swordRootMotionAnimations, 0);

        if ((playerMovement.isDisabledByClimb || playerMovement.isDisabledByArchery || playerMovement.isHoldingBow) && !isTransitioning)
        {
            if (isInBattle || sword.parent == swordArmedPosition || shield.parent == shieldArmedPosition)
            {
				//playerWeaponSelector.ChangeSelected ((playerMovement.isDisabledByArchery || playerMovement.isHoldingBow) ? 2 : 1);
                isInBattle = false;
                Dequip();
            }

            return;
        }

        if (playerMovement.isDisabledByGround)
        {
			if (!isInBattle && (sword.parent == swordArmedPosition || shield.parent == shieldArmedPosition))
				Dequip();
            else if (isInBattle && (sword.parent != swordArmedPosition || shield.parent != shieldArmedPosition))
				Equip();
            return;
        }

        bool singleClicked = false;
        bool doubleClicked = false;

        HandleClicks(ref singleClicked, ref doubleClicked);

        if (mecSoonInAnim("Equip Dequip.draw_sword_2", 1))
			Equip ();
        else if (mecSoonInAnim("Equip Dequip.sheath_sword_2", 1))
			Dequip ();
        else if (!mecInTrans(0) && (mecInAnim("Equip Dequip.draw_sword_1", 1) || mecInAnim("Equip Dequip.sheath_sword_1", 1)))
			anim.SetLayerWeight (1, 1f);

        if (isTransitioning)
            return;

		if (isInBattle) {

			//if (playerMovement.isRunning)
			//	Battle (false);

            bool goingToSwing = false;
            bool notInAnyAttack = !mecInAnyAnim(swordRootMotionAnimations, 0) && !mecSoonInAnyAnim(swordRootMotionAnimations, 0);

            if (singleClicked && !anim.GetBool("Blocking") && notInAnyAttack)
            {
                int currentAttack;
                if (anim.GetFloat("SprintTrigger") > 0.05f)
                    currentAttack = 5;
                else
                    currentAttack = Random.Range(0, numAttacks);
                anim.SetInteger("AttackNumber", currentAttack);
                anim.SetTrigger("Attack");
                goingToSwing = true;
            }

            if (Input.GetMouseButton(0) && !mecInTrans(0) && mecInAnyAnim(swordRootMotionAnimations, 0))
                anim.SetTrigger("ContinueAttack");
            
            anim.SetBool("ContinueAttackReadOnly", anim.GetBool("ContinueAttack"));


            if (Input.GetMouseButton(1) && !goingToSwing && notInAnyAttack)
                anim.SetBool("Blocking", true);
            else
                anim.SetBool("Blocking", false);

            if (doubleClicked && notInAnyAttack)
            {
                anim.SetInteger("AttackNumber", 4);
                anim.SetTrigger("Attack");
            }

            if(Input.GetKeyDown(KeyCode.Q) && notInAnyAttack)
            {
                anim.SetInteger("AttackNumber", 6);
                anim.SetTrigger("Attack");
            }

            if (mecSoonInAnyAnim(swordRootMotionAnimations, 0))
                swordAnimRot = Quaternion.LookRotation(Vector3.Scale(playerMovement.mainCam.transform.forward, new Vector3(1, 0, 1)), Vector3.up);

            if (mecInAnyAnim(swordRootMotionAnimations, 0))
                transform.rotation = Quaternion.Slerp(transform.rotation, swordAnimRot, Time.deltaTime / 0.1f);

			if (anim.GetBool ("Blocking")) {

				float h = Input.GetAxis ("Horizontal");
				float v = Input.GetAxis ("Vertical");

				if (h != 0 || v != 0) {

					Vector3 worldDirection = playerMovement.mainCam.transform.TransformDirection (new Vector3 (h, 0, v).normalized);
                    worldDirection.Scale(new Vector3(1, 0, 1));
                    worldDirection.Normalize();

					float angle = 0;
					angle = Vector3.Angle (transform.forward, worldDirection);

					if (Vector3.Cross (transform.forward, worldDirection).y < 0)
						angle = -angle;

					anim.SetFloat ("SpeedX", Mathf.Sin (angle * Mathf.Deg2Rad), speedDampTime, Time.deltaTime);
					anim.SetFloat ("SpeedZ", Mathf.Cos (angle * Mathf.Deg2Rad), speedDampTime, Time.deltaTime);

					rigid.velocity = worldDirection;
				
				} else if (anim.GetFloat ("SpeedX") != 0 || anim.GetFloat ("SpeedZ") != 0) {
				
					anim.SetFloat ("SpeedX", 0, speedDampTime, Time.deltaTime);
					anim.SetFloat ("SpeedZ", 0, speedDampTime, Time.deltaTime);

					rigid.velocity = new Vector3 (0, 0, 0);
				
				}

                rigid.rotation = Quaternion.LookRotation(Vector3.Scale(playerMovement.mainCam.transform.forward, new Vector3(1, 0, 1)), Vector3.up);

            }

		}
        else
        {
            if (anim.GetFloat("Speed") > 0.5f && anim.GetFloat("SprintTrigger") > 0.5f)
            {
                float lerpNum = 0.5f + 0.5f * Mathf.Sin(shieldSwingOffset + 2 * Mathf.PI / shieldSwingPeriod * (anim.GetCurrentAnimatorStateInfo(0).normalizedTime % shieldSwingPeriod));
                shield.position = Vector3.Lerp(shieldAnimLeft.position, shieldAnimRight.position, lerpNum);
                shield.rotation = Quaternion.Slerp(shieldAnimLeft.rotation, shieldAnimRight.rotation, lerpNum);
            }
            else
            {
                shield.parent = shieldUnarmedPosition;
                shield.localPosition = new Vector3(0, 0, 0);
                shield.localRotation = Quaternion.identity;
                shield.localScale = Vector3.one;
            }
        }
			
        /*if (Input.GetKeyDown(KeyCode.R) || !isInBattle && singleClicked)
        {
            Battle(!isInBattle);
        }*/


		if (playerWeaponSelector.slotNumber == 2) {

			if (!isInBattle) {
				isTransitioning = true;
				Battle (true);
			}

		} else if (isInBattle) {
            if (playerMovement.isAbleToMove && playerWeaponSelector.slotNumber == 1)
            {
                isTransitioning = true;
                Battle(false);
            }
            else
            {
                isInBattle = false;
                Dequip();
                anim.ResetTrigger("Dequip");
            }
		}

	}

	public void Battle(bool b){

		isInBattle = b;
		//playerMovement.isDisabledByBattle = isInBattle;
		anim.SetBool ("isInBattle", isInBattle);

		if (b)
			anim.SetTrigger ("Equip");
		else
			anim.SetTrigger ("Dequip");

	}

    public void GracefulDequip() 
    {
        isTransitioning = true;
        Battle(false);
    }

	public void Equip(){

		isInBattle = true;

		sword.parent = swordArmedPosition;
		sword.localPosition = new Vector3 (0, 0, 0);
		sword.localRotation = Quaternion.identity;

		shield.parent = shieldArmedPosition;
		shield.localPosition = new Vector3 (0, 0, 0);
        shield.localRotation = Quaternion.identity;
        shield.localScale = Vector3.one;
	}

	public void Dequip(){

		isInBattle = false;
		sword.parent = swordUnarmedPosition;
		sword.localPosition = new Vector3 (0, 0, 0);
		sword.localRotation = Quaternion.identity;

		shield.parent = shieldUnarmedPosition;
		shield.localPosition = new Vector3 (0, 0, 0);
		shield.localRotation = Quaternion.identity;
        shield.localScale = Vector3.one;
	}

    private void HandleClicks(ref bool singleClicked, ref bool doubleClicked)
    {
        singleClicked = false;
        doubleClicked = false;

        if (Input.GetMouseButtonDown(0))
        {
            swordHoldStartTime = Time.time;
            stillHolding = true;
        }

        if (Input.GetMouseButtonUp(0))
            stillHolding = false;

        if (Input.GetMouseButtonUp(0) && !swordClick || Input.GetMouseButtonDown(0) && swordClick)
        {
            if (!swordClick)
            {
                if (lastClickWasDouble)
                    lastClickWasDouble = false;
                else if (lastClickWasHeld)
                    lastClickWasHeld = false;
                else
                {
                    swordClick = true;
                    swordClickStartTime = Time.time;
                }
            }
            else
            {
                swordClick = false;
                doubleClicked = true;
                lastClickWasDouble = true;
            }
        }

        if (swordClick && Time.time - swordClickStartTime > swordClickMax)
        {
            swordClick = false;
            singleClicked = true;
        }

        if (stillHolding && Time.time - swordHoldStartTime > swordHoldMax)
        {
            stillHolding = false;
            lastClickWasHeld = true;
            singleClicked = true;
        }
    }

    private bool mecInTrans(int layer)
    {
        return anim.GetAnimatorTransitionInfo(layer).fullPathHash != 0;
    }

    private bool mecInAnim(string name, int layer)
    {
        return anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == Animator.StringToHash(name);
    }

    private bool mecInAnyAnim(string[] names, int layer)
    {
        foreach (string name in names)
            if (anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == Animator.StringToHash(name))
                return true;
        return false;
    }

    private bool mecSoonInAnim(string name, int layer)
    {
        return anim.GetNextAnimatorStateInfo(layer).fullPathHash == Animator.StringToHash(name);
    }

    private bool mecSoonInAnyAnim(string[] names, int layer)
    {
        foreach (string name in names)
            if (anim.GetNextAnimatorStateInfo(layer).fullPathHash == Animator.StringToHash(name))
                return true;
        return false;
    }
}
