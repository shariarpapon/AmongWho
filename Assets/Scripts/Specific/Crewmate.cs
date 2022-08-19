using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crewmate : MonoBehaviour
{
    public GameObject crewmateUI;
    public Transform canvas;

    public void ActivateCrewmate()
    {
        if (crewmateUI == null) // TEST
            return;

        canvas = FindObjectOfType<Canvas>().transform;
        Instantiate(crewmateUI, canvas);
    }
}
