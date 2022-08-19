using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager current;
    private  MenuManager menuManager;

    public static string clientName;

    public void Awake()
    {
        if (current == null)
            current = this;
        else {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        clientName = LoadClientName();
        menuManager = FindObjectOfType<MenuManager>();
    }

    private void Start()
    {
        Debug.Log("Establishing connection to master-server...");
        EstablishConnection(clientName);
        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 12;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void OnLevelWasLoaded(int level)
    {
        if (level == 0)
        {
            menuManager = FindObjectOfType<MenuManager>();
            PhotonNetwork.ConnectUsingSettings();

            if (PhotonNetwork.InRoom)
                menuManager.OpenMenu("RoomMenu");
        }
        else
        {
            LobbyManager.current.LocalPhotonView().RPC("RPC_StartGame", RpcTarget.All);
        }
    }

    public void LoadScene(int index)
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(index);
    }

    public void ChangeNickName(string newName)
    {
        PhotonNetwork.NickName = newName;
        clientName = newName;

        string path = Application.persistentDataPath + "/clientdata.txt";
        File.Delete(path);
        File.WriteAllText(path, newName);
    }

    public void StartGame()
    {
        Debug.Log("Starting game...");
        GameManager.current.StartGame();
    }

    public void EstablishConnection(string nickname)
    {
        if (string.IsNullOrEmpty(nickname) == true)
        {
            menuManager.OpenMenu("NameMenu");
            return;
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.NickName = nickname;
            clientName = nickname;

            string path = Application.persistentDataPath + "/clientdata.txt";
            File.Delete(path);
            File.WriteAllText(path, nickname);
        }
    }

    public PhotonView PhotonViewByID(int viewID)
    {
        var pvs = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pview in pvs)
        {
            if (pview.ViewID == viewID)
                return pview;
        }
        return null;
    }

    public PhotonView LocalPhotonView()
    {
        var pvs = FindObjectsOfType<PhotonView>();
        return System.Array.Find(pvs, c => c.IsMine == true && c.GetComponent<PlayerController>() != null);
    }

    public string LoadClientName()
    {
        string path = Application.persistentDataPath + "/clientdata.txt";
        if (File.Exists(path))
            return File.ReadAllText(path);
        else
            return string.Empty;
    }

    public string CurrentRoomName()
    {
        return PhotonNetwork.CurrentRoom.Name;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public string ClientName()
    {
        return PhotonNetwork.NickName;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master-server");
        menuManager.OpenMenu("TitleMenu");
        menuManager.SetName();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void CreateRoom()
    {
        string code = Random.Range(1000, 99999).ToString();
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 10 };
        PhotonNetwork.CreateRoom(code, roomOps);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room Created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Unable to create room. Try again.");
    }

    public void JoinRoom(string code)
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room [Code : "+ PhotonNetwork.CurrentRoom.Name +"]");

        menuManager.OpenMenu("RoomMenu");
        menuManager.SetRoomTitle(PhotonNetwork.CurrentRoom.Name);
        CheckMasterAuthority();
        menuManager.UpdateClientList(PhotonNetwork.PlayerList);

        RoomManager rm = FindObjectOfType<RoomManager>();
        rm.SpawnPlayersInRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        menuManager.UpdateClientList(PhotonNetwork.PlayerList);

        var pvs = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in pvs)
        {
            if (pv.IsMine == false)
            {
                Debug.Log("has entered and dontdestroy on load applied");
                PhotonView.DontDestroyOnLoad(pv.gameObject);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            CheckMasterAuthority();
            menuManager.UpdateClientList(PhotonNetwork.PlayerList);
        }
    }

    public void CheckMasterAuthority()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            menuManager.SetRoomSettingsActive(true);
            menuManager.startGameButton.SetActive(true);
        }
        else
        {
            menuManager.SetRoomSettingsActive(false);
            menuManager.startGameButton.SetActive(false);
        }
    }

    public void LeaveRoom()
    {
        if(PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            PhotonNetwork.LoadLevel(0);
        }
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            menuManager.UpdateClientList(PhotonNetwork.PlayerList);
            menuManager.OpenMenu("TitleMenu");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Unable to join room. Try again.");
    }

    public void DisconnectServer()
    {
        Debug.Log("Disconnecting from server");
        PhotonNetwork.Disconnect();
    }

    public void OnApplicationQuit()
    {
        DisconnectServer();
    }
      
}
