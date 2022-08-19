using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager current;
    public GameObject joystickUI;

    private void Awake()
    {
        if (current == null)
            current = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void DrawJoystick()
    {
        joystickUI.SetActive(true);
    }

    public void HideJoystick()
    {
        joystickUI.SetActive(false);
    }

}
