using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    public GameObject taskUI;

    public void LeaveGame()
    {
        LobbyManager.current.LeaveRoom();
    }

    public void ShowTasks()
    {
        taskUI.SetActive(!taskUI.activeSelf);
    }
}
