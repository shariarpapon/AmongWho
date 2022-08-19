using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager current;

    public static bool isGamePaused = false;

    public event System.Action OnGameStartedCallbacks;
    public event System.Action OnGamePausedCallbacks;
    public event System.Action OnGameResumeCallbacks;
    public event System.Action OnGameEndCallbacks;

    private bool isMeetingOnGoing = false;

    [HideInInspector]
    public Timer localTimer = null;

    public ReferenceStorage references;

    private MeetingHandler meetingHandler;

    public void OnEnable()
    {
        OnGameStartedCallbacks += OnGameStarted;
        OnGamePausedCallbacks += OnGamePaused;
        OnGameResumeCallbacks += OnGameResumed;
        OnGameEndCallbacks += OnGameEnd;
    }

    public void OnDisable()
    {
        OnGameStartedCallbacks -= OnGameStarted;
        OnGamePausedCallbacks -= OnGamePaused;
        OnGameResumeCallbacks -= OnGameResumed;
        OnGameEndCallbacks -= OnGameEnd;
    }

    public void Awake()
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

    private void Update()
    {
        if (localTimer != null && localTimer.isTimerRunning == true)
        {
            localTimer.currentTime += Time.deltaTime;
        }
    }

    [PunRPC]
    void RPC_InitMeetingCooldownAction()
    {
        if (isMeetingOnGoing == false)
        {
            isMeetingOnGoing = true;
            FindObjectOfType<MeetingHandler>().InitMeetingCooldownAction();
        }
    }

    [PunRPC]
    void RPC_EndMeeting()
    {
        meetingHandler.OnMeetingEndAction();
    }

    [PunRPC]
    void RPC_CallMeeting(string caller, int bodyID, bool isReport)
    {
        meetingHandler = FindObjectOfType<MeetingHandler>();
        meetingHandler.InitMeeting(caller, !isReport);
        if (isReport) Destroy(PhotonNetwork.GetPhotonView(bodyID).gameObject);
    }

    [PunRPC]
    void RPC_UpdateVoteStats(string voterName, string votedPlayerName)
    {
        meetingHandler.UpdateVotes(voterName, votedPlayerName);
    }

    [PunRPC]
    void RPC_EndMeetingProtocol()
    {
        isMeetingOnGoing = false;
        meetingHandler.InitVoteCooldownAction();
    }

    [PunRPC]
    void RPC_VoteResultInitProtocol()
    {
        meetingHandler.OnVoteResultInit();
    }

    [PunRPC]
    void RPC_VoteResultEndProtocol()
    {
        meetingHandler.OnVoteResultEnd();
    }

    [PunRPC]
    void RPC_OnMeetingProtocol()
    {
        meetingHandler.OnMeetingAction();
    }

    public void InitCooldownAction(float durationInSeconds, System.Action initAction, System.Action iterationAction, System.Action finalAction)
    {
        StartCoroutine(StartClock(durationInSeconds, initAction, iterationAction, finalAction));
    }

    private IEnumerator StartClock(float duration, System.Action initAction, System.Action iterationAction, System.Action finalAction)
    {
        float secPassed = 0;
        initAction?.Invoke();
        do
        {
            yield return new WaitForSeconds(1);
            iterationAction?.Invoke();
            secPassed++;
        }
        while (secPassed < duration);

        if (secPassed >= duration)
            finalAction?.Invoke();
    }

    public void OnGameStarted()
    {
        if (AssignImposters(RoomManager.current.roomSettings.imposterCount))
        {
            PhotonView[] pvs = FindObjectsOfType<PhotonView>();

            for (int i = 0; i < pvs.Length; i++)
            {
                if (pvs[i].GetComponent<PlayerController>() == true)
                    pvs[i].RPC("RPC_MakePersistent", RpcTarget.All, pvs[i].ViewID);
            }

            PhotonNetwork.LoadLevel((int)RoomManager.current.roomSettings.map);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            Debug.Log("Game has started");
        }
        else
            Debug.LogError("Not enough players or too many imposters");
    }

    public void OnGamePaused()
    {
        PhotonView pv = LobbyManager.current.LocalPhotonView();
        if (pv == null) return;
        pv.RPC("RPC_PauseGame", RpcTarget.AllViaServer, pv.ViewID);
        Debug.Log("Game has been paused"); 
    }
    public void OnGameResumed()
    {
        LobbyManager.current.LocalPhotonView().RPC("RPC_ResumeGame", RpcTarget.AllViaServer);
        Debug.Log("Game has been resumed");
    }
    public void OnGameEnd()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;

            if(SceneManager.GetActiveScene().buildIndex != 0)
                SceneManager.LoadScene(0);
        }
        Debug.Log("Game has ended");
    }

    public bool AssignImposters(int imposterCount)
    {
        int threshold = 1; //imposterCount * 2 + 2;                 //**TESTING PURPOSE ONLY**

        if (PhotonNetwork.CurrentRoom.PlayerCount < threshold || imposterCount <= 0)
            return false;

        PhotonView[] pvs = FindObjectsOfType<PhotonView>();
        List<PhotonView> plyPV = new List<PhotonView>();

        foreach (PhotonView p in pvs)
        {
            PlayerController pc = p.GetComponent<PlayerController>();
            if (pc)
            {
                p.RPC("RPC_NeutralizeImposter", RpcTarget.All, p.ViewID);
                plyPV.Add(p);
            }
        }

        for (int i = 0; i < imposterCount; i++)
        {
            int rand = Random.Range(0, plyPV.Count);
            PhotonView pv = plyPV[rand];
            pv.RPC("RPC_MakeImposter", RpcTarget.All, pv.ViewID);
            plyPV.Remove(plyPV[rand]);
        }
        return true;
    }

    public void StartGame()
    {
        OnGameStartedCallbacks?.Invoke();
    }

    public void PauseGame()
    {
        OnGamePausedCallbacks?.Invoke();
    }

    public void ResumeGame()
    {
        OnGameResumeCallbacks?.Invoke();
    }

    public void EndGame()
    {
        OnGameEndCallbacks?.Invoke();
    }

    public bool IsGameTimerRunning
    {
        get { return localTimer.isTimerRunning; }
        set { localTimer.isTimerRunning = value; }
    }
}

[System.Serializable]
public struct ReferenceStorage
{
    public GameObject PFB_imposterControlUI;
    public GameObject PFB_bloodSplatterParticle;
}

[System.Serializable]
public class Timer
{
    public bool isTimerRunning;
    public float currentTime;

    public void Stop() {
        isTimerRunning = false;
    }
    public void Start() {
        isTimerRunning = true;
    }
    public void Reset() {
        currentTime = 0;
    }
}
