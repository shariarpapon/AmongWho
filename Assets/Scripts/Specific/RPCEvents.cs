using Photon.Pun;
using TMPro;
using UnityEngine;

public class RPCEvents : MonoBehaviour
{
    [PunRPC]
    void RPC_SetMatchPlayerData(string nickname, Vector3 color)
    {
        MatchPlayerData data = new MatchPlayerData(nickname, new Color(color.x, color.y, color.z, 1));
        MeetingHandler mh = FindObjectOfType<MeetingHandler>();
        mh.allPlayerList.Add(data);
        mh.voteRecords.Add(nickname, 0);
    }

    [PunRPC]
    void RPC_KillCrewmate(int bodyID, int playerID, string playerName, Vector3 color)
    {
        FindObjectOfType<MeetingHandler>().SetPlayerDeathStatus(playerName);

        PhotonView playerPV = LobbyManager.current.PhotonViewByID(playerID);
        playerPV.gameObject.SetActive(false);

        PhotonView bodyPV = PhotonNetwork.GetPhotonView(bodyID);
        bodyPV.GetComponentInChildren<MeshRenderer>().material.color = new Color(color.x, color.y, color.z, 1);
    }

    [PunRPC]
    void RPC_NeutralizeImposter(int viewID)
    {
        LobbyManager.current.PhotonViewByID(viewID).GetComponent<PlayerController>().isImposter = false;
    }

    [PunRPC]
    void RPC_MakeImposter(int viewID)
    {
        LobbyManager.current.PhotonViewByID(viewID).GetComponent<PlayerController>().isImposter = true;
    }

    [PunRPC]
    void RPC_SetSpawn(int viewID, int spawnIndex)
    {
        var spawn = GameObject.FindWithTag("SpawnPointHolder");
        PhotonView pv = PhotonNetwork.GetPhotonView(viewID);
        pv.transform.position = spawn.transform.GetChild(spawnIndex).position;
        RoomManager.current.spawnIndex++;
    }

    [PunRPC]
    void RPC_MakePersistent(int viewID)
    {
        PhotonView.DontDestroyOnLoad(LobbyManager.current.PhotonViewByID(viewID).gameObject);
    }

    [PunRPC]
    void RPC_Set3DName(int viewID, string nickName, bool placeholder)
    {
        PhotonView pv = LobbyManager.current.PhotonViewByID(viewID);
        pv.GetComponentInChildren<TextMeshPro>().text = nickName;
    }

    [PunRPC]
    void RPC_SetMyColor(int viewID, float r, float g, float b)
    {
        Color clr = new Color(r, g, b, 255);
        PhotonView pv = LobbyManager.current.PhotonViewByID(viewID);
        pv.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material.color = clr;

        if (pv.IsMine)
            RoomManager.current.myColor = clr;
    }

    [PunRPC]
    void RPC_EquipCosmetic(int pID, int cID, string spawnPlace)
    {
        PhotonView pv = LobbyManager.current.PhotonViewByID(pID);
        PhotonView cpv = LobbyManager.current.PhotonViewByID(cID);

        Transform place = pv.transform.Find(spawnPlace);
        foreach (Transform t in place)
        {
            if (t != null)
            {
                if (pv.IsMine)
                    RoomManager.current.equipedCosmetic.Remove(t.name);

                Destroy(t.gameObject);
            }
        }
        cpv.transform.parent = place.transform;
        cpv.transform.localPosition = Vector3.zero;
        RoomManager.current.equipedCosmetic.Add(cpv.transform.name);
    }

    [PunRPC]
    void RPC_SetRoomSettingsData(int mapIndex, int imposterCount, int tasksPerPlayer, float moveSpeed, float killCooldown, float discussionTime)
    {
        RoomManager.current.roomSettings.map = (MapType)mapIndex;
        RoomManager.current.roomSettings.imposterCount = imposterCount;
        RoomManager.current.roomSettings.tasksPerPlayer = tasksPerPlayer;
        RoomManager.current.roomSettings.moveSpeed = moveSpeed;
        RoomManager.current.roomSettings.killCooldown = killCooldown;
        RoomManager.current.roomSettings.discussionTime = discussionTime;
    }

    [PunRPC]
    void RPC_ChangeColor(int viewID, float r, float g, float b)
    {
        PhotonView pvw = LobbyManager.current.PhotonViewByID(viewID);
        pvw.GetComponentInChildren<SkinnedMeshRenderer>().material.color = new Color(r, g, b, 255);
    }

    [PunRPC]
    void RPC_StartGame()
    {
        Transform cam = transform.Find("PlayerCamera");
        PhotonView pv = GetComponent<PhotonView>();

        if (pv.IsMine == false)
            cam.gameObject.SetActive(false);
        else
        {
            cam.gameObject.SetActive(true);
            cam.GetComponent<Camera>().tag = "MainCamera";
            cam.GetComponent<CameraController>().SetupCamera();
        }

        GameManager.current.localTimer = new Timer();
        GameManager.current.localTimer.Start();
        GameManager.current.IsGameTimerRunning = true;
    }

    [PunRPC]
    void RPC_PauseGame(int viewID)
    {
        Debug.Log("paused by " + viewID);
        GameManager.current.localTimer.Stop();
        GameManager.isGamePaused = true;
    }

    [PunRPC]
    void RPC_ResumeGame()
    {
        GameManager.current.localTimer.Start();
        GameManager.isGamePaused = false;
    }
}
