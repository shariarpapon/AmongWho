using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI clientNameInput;
    public TextMeshProUGUI roomCodeInput;
    public TextMeshProUGUI roomCodeTitle;
    public TextMeshProUGUI roomClientList;
    public TextMeshProUGUI clientName;
    public TextMeshProUGUI newNameInput;
    public AudioSource musicSource;
    public Slider musicSlider;
    public HSVPicker.ColorPicker colorPicker;
    public GameObject clientListUI;
    [Space]
    public Slider imposterCountSlider;
    public Slider tasksPerPlayerSlider;
    public Slider moveSpeedSlider;
    public Slider killCooldownSlider;
    public Slider discussionTimeSlider;
    public TextMeshProUGUI mapTitle;

    [Space]
    public GameObject roomSettingsButton;
    public GameObject startGameButton;

    public List<Menu> menus = new List<Menu>();
    public List<Menu> customizableMenues = new List<Menu>();

    private void Start()
    {
        if (musicSlider != null)
            musicSlider.value = musicSource.volume;

        if (colorPicker != null)
            colorPicker.CurrentColor = RoomManager.current.myColor;
    }

    public void ShowClientList()
    {
        clientListUI.SetActive(!clientListUI.activeSelf);
    }

    public void InitiateRoomSettingsUI()
    {
        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            if (GameManager.current.IsGameTimerRunning == false)
            {
                imposterCountSlider.value = RoomManager.current.roomSettings.imposterCount;
                tasksPerPlayerSlider.value = RoomManager.current.roomSettings.tasksPerPlayer;
                moveSpeedSlider.value = RoomManager.current.roomSettings.moveSpeed;
                killCooldownSlider.value = RoomManager.current.roomSettings.killCooldown;
                discussionTimeSlider.value = RoomManager.current.roomSettings.discussionTime;
                mapTitle.text = RoomManager.current.roomSettings.map.ToString();
            }
        }
    }

    public void ChangeMap(int mapIndex)
    {
        RoomManager.current.roomSettings.map = (MapType)mapIndex;
        mapTitle.text = ((MapType)mapIndex).ToString();
    }

    public void ChangeRoomSettings(string settingsToChange)
    {
        Transform  target = null;
        switch (settingsToChange)
        {
            case "ImposterCount":
                RoomManager.current.roomSettings.imposterCount = (int)imposterCountSlider.value;
                target = imposterCountSlider.transform;
                break;
            case "TasksPerPlayer":
                RoomManager.current.roomSettings.tasksPerPlayer = (int)tasksPerPlayerSlider.value;
                target = tasksPerPlayerSlider.transform;
                break;
            case "MoveSpeed":
                RoomManager.current.roomSettings.moveSpeed = moveSpeedSlider.value;
                target = moveSpeedSlider.transform;
                break;
            case "KillCooldown":
                RoomManager.current.roomSettings.killCooldown = killCooldownSlider.value;
                target = killCooldownSlider.transform;
                break;
            case "DiscussionTime":
                RoomManager.current.roomSettings.discussionTime = discussionTimeSlider.value;
                target = discussionTimeSlider.transform;
                break;
        }
        target.parent.Find("Count").GetComponent<TextMeshProUGUI>().text = target.GetComponent<Slider>().value.ToString();
        RoomManager.current.UpdateRoomSettings();
    }

    public void OpenMenu(string name)
    {
        foreach (Menu menu in menus)
        {
            if (menu.UI != null)
            {
                if (menu.name == name)
                    menu.UI.SetActive(true);
                else
                    menu.UI.SetActive(false);
            }
        }
    }

    public void SetRoomSettingsActive(bool state)
    {
        roomSettingsButton?.SetActive(state);
    }

    public void OpenCustomizableMenu(string menuName)
    {
        foreach (Menu menu in customizableMenues)
        {
            if (menu.UI != null)
            {
                if (menu.name == menuName)
                    menu.UI.SetActive(true);
                else
                    menu.UI.SetActive(false);
            }
        }
    }

    public void ChangeColor()
    {
        Color color = colorPicker.CurrentColor;
        RoomManager.current.ChangePlayerColor(color);
        RoomManager.current.myColor = color;
    }

    public void EquipPlayerCosmetic(string cmtName)
    {
        RoomManager.current.EquipCosmetic(cmtName);
    }

    public void ChangeMusicVolume()
    {
        musicSource.volume = musicSlider.value;
    }

    public void ChangeName()
    {
        string nName = newNameInput.text;
        LobbyManager.current.ChangeNickName(nName);
        clientName.text = nName;
    }

    public void StartGame()
    {
        LobbyManager.current.StartGame();
    }

    public void SetName()
    {
        LobbyManager.clientName = Photon.Pun.PhotonNetwork.NickName;
        clientName.text = Photon.Pun.PhotonNetwork.NickName;
    }

    public void Connect()
    {
        if (string.IsNullOrEmpty(clientNameInput.text) == false)
        {
            LobbyManager.current.EstablishConnection(clientNameInput.text);
        }
        else
        {
            Debug.LogError("Name field is empty.");
        }
    }

    public void CreateRoom()
    {
        LobbyManager.current.CreateRoom();
    }

    public void JoinRoom()
    {
        string code = roomCodeInput.text;
        LobbyManager.current.JoinRoom(code);
    }

    public void UpdateClientList(Photon.Realtime.Player[] players)
    {
        roomClientList.text = string.Empty;
        for (int i = 0; i < players.Length; i++)
            roomClientList.text += $"\n>> " + players[i].NickName;
    }

    public void SetRoomTitle(string title)
    { 
        roomCodeTitle.text = title;
    }

    public void LoadScene(int index)
    {
        LobbyManager.current.LoadScene(index);
    }

    public void Leave()
    {
        LobbyManager.current.LeaveRoom();
    }

    public void QuitGame()
    {
        LobbyManager.current.DisconnectServer();
        Application.Quit();
    }

    [System.Serializable]
    public struct Menu
    {
        public string name;
        public GameObject UI;
    }
}
