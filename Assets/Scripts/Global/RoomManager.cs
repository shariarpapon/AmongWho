using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviour
{
    private MenuManager menuManager;

    public Color[] colors;
    public Transform spawnPointHolder;

    [HideInInspector]
    public Vector3[] spawnPoints;

    public static RoomManager current;

    public List<Cosmetic> cosmetics = new List<Cosmetic>();

    [HideInInspector]
    public Color myColor;
    [HideInInspector]
    public List<string> equipedCosmetic = new List<string>();

    public RoomSettings roomSettings;

    public ServerData taskData;

    public int spawnIndex = 0;

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

    private void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        spawnPoints = new Vector3[spawnPointHolder.childCount];
        for (int i = 0; i < spawnPointHolder.childCount; i++)
            spawnPoints[i] = spawnPointHolder.GetChild(i).position;
    }

    public void OnLevelWasLoaded(int level)
    {
        if (level != 0)
        {
            PhotonView p = LobbyManager.current.LocalPhotonView();
            if (p.IsMine == false) return;

            Debug.Log("assigning tasks");
            FindObjectOfType<TaskHandler>().AssignTasks(roomSettings.tasksPerPlayer, taskData);
            p.RPC("RPC_SetSpawn", RpcTarget.All, p.ViewID, spawnIndex);
            FindObjectOfType<CooldownTimer>()?.Initiate();
        }
        else
        {
            spawnIndex = 0;
        }
    }

    public void SpawnPlayersInRoom()
    {
        int count = PhotonNetwork.CurrentRoom.PlayerCount;
        Vector3 pos = spawnPoints[count - 1];

        Debug.Log("IS INSTANTING");
        GameObject player = PhotonNetwork.Instantiate("Player_Prototype", pos, Quaternion.identity);
        player.name = Random.Range(0, 100000).ToString();
        player.transform.Find("PlayerCamera").gameObject?.SetActive(false);
        PhotonView.DontDestroyOnLoad(player);
        SetupRoomSettings();
    }

    public void UpdateRoomSettings()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            LobbyManager.current.LocalPhotonView().RPC("RPC_SetRoomSettingsData", RpcTarget.All, (int)roomSettings.map, roomSettings.imposterCount, roomSettings.tasksPerPlayer,
            roomSettings.moveSpeed, roomSettings.killCooldown, roomSettings.discussionTime);
        }
    }

    public void SetupRoomSettings()
    {
        roomSettings = new RoomSettings();
        menuManager.InitiateRoomSettingsUI();
    }

    public void ChangePlayerColor(Color color)
    {
        PhotonView pv = LobbyManager.current.LocalPhotonView();
        if (pv == null)
            return;

        pv.RPC("RPC_ChangeColor", RpcTarget.AllBuffered, pv.ViewID, color.r, color.g, color.b);
    }

    public void EquipCosmetic(string cmtName)
    {
        PhotonView pv = LobbyManager.current.LocalPhotonView();
        Cosmetic selectedCosmetic = System.Array.Find(cosmetics.ToArray(), a => a.cosmeticName == cmtName);
        Transform spawnHolder = pv.transform.Find(selectedCosmetic.spawnPointName);

        if (spawnHolder != null)
        {
            string path = @"cosmetics\" + selectedCosmetic.cosmeticName;
            GameObject cmt = PhotonNetwork.Instantiate(path, Vector3.zero, spawnHolder.rotation);
            pv.RPC("RPC_EquipCosmetic", RpcTarget.AllBuffered, pv.ViewID, cmt.GetComponent<PhotonView>().ViewID, selectedCosmetic.spawnPointName);
        }
    }
}


