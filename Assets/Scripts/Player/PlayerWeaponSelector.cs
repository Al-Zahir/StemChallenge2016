using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerWeaponSelector : MonoBehaviour {

	private Image[] slots;
	private PlayerMovement playerMovement;
	public Transform canvas;
    public bool swordAvail = true, bowAvail = true;

	private KeyCode[] keyCodes = {KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
		KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9};

	public int slotNumber;

	void Start(){

		playerMovement = GetComponent<PlayerMovement> ();

		slots = new Image[canvas.childCount];

		for (int i = 0; i < canvas.childCount; i++)
			slots [i] = canvas.GetChild (i).GetComponent<Image>();

		slotNumber = 1;

		for (int i = 0; i < slots.Length; i++) {

			if (slotNumber - 1 == i)
				slots [i].color = new Color (1f, 1f, 1f, 0.75f);
			else
				slots [i].color = new Color (0.65f, 0.65f, 0.65f, 0.75f);

		}

	}

	void Update(){

		for (int i = 1; i <= slots.Length; i++)
            if (Input.GetKeyDown(keyCodes[i]) && playerMovement.isAbleToMove)
            {
                bool clickingNotAllowed = !swordAvail && i == 2 ||
                                          !bowAvail && i == 3;

                if (!clickingNotAllowed)
                    ChangeSelected(i);
            }

        if (!swordAvail)
            slots[1].color = Color.clear;
        else if(slots[1].color == Color.clear && swordAvail)
            slots[1].color = new Color(0.65f, 0.65f, 0.65f, 0.75f);

        if (!bowAvail)
            slots[2].color = Color.clear;
        else if (slots[2].color == Color.clear && bowAvail)
            slots[2].color = new Color(0.65f, 0.65f, 0.65f, 0.75f);
	}

	public void ChangeSelected(int num){

		slotNumber = num;

		for (int i = 0; i < slots.Length; i++) {

			if (slotNumber - 1 == i)
				slots [i].color = new Color (1f, 1f, 1f, 0.75f);
			else
				slots [i].color = new Color (0.65f, 0.65f, 0.65f, 0.75f);

		}

	}
}
