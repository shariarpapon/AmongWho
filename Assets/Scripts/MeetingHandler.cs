using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class MeetingHandler : MonoBehaviour
{
    private const int maxEmergencyCalls = 2;
    private const string skipVoteCode = "cc4a5ce1b3dAos902hf98G1h2Q89dha0b894eC";

    public GameObject meetingPanel;
    public GameObject playerVotingSlot;
    public GameObject voteIndicator;

    public bool allowEmergencyMeeting = false;
    public bool hasVoted = false;
    public int emergencyCalls = 0;

    //public int skippedVotes;

    [HideInInspector]
    public List<MatchPlayerData> allPlayerList = new List<MatchPlayerData>();

    public Dictionary<string, int> voteRecords = new Dictionary<string, int>();

    public float cooldownTime = 15;
    public float currentCDTime = 0;
    public bool inCD = false;

    private PhotonView gameManager_PhotonView;

    private void Awake()
    {
        voteRecords.Add(skipVoteCode, 0);
    }

    private void Update()
    {
        if(emergencyCalls < maxEmergencyCalls)
            if (allowEmergencyMeeting == false) RequestMeetingEligibility();
    }

    private void RequestMeetingEligibility()
    {
        if (!inCD)
        {
            currentCDTime = 0;
            inCD = true;
        }
        currentCDTime += Time.deltaTime;
        if (currentCDTime >= cooldownTime)
        {
            allowEmergencyMeeting = true;
            inCD = false;
            currentCDTime = 0;
        }
    }

    public void InitMeetingCooldownAction()
    {
        GameManager.current.InitCooldownAction(DiscussionCooldownTime, null, OnMeeting, OnEndMeeting);
    }

    public void InitMeeting(string caller, bool isEmergency)
    {
        gameManager_PhotonView = GameManager.current.GetComponent<PhotonView>();

        for (int i = 0; i < allPlayerList.Count; i++)
        {
            if (allPlayerList[i].playerName == PhotonNetwork.LocalPlayer.NickName && allPlayerList[i].isDead) return;
            else break;
        }

        //Player[] players = PhotonNetwork.PlayerList;
        //for (int i = 0; i < players.Length; i++)
        //{
        //    MatchPlayerData mpd = allPlayerList.Find(c => c.playerName == players[i].NickName);
        //    if (mpd != null)
        //        if (mpd.isDead == false)
        //            PhotonNetwork.SetMasterClient(players[i]);
        //}

        if (isEmergency)
        {
            if (allowEmergencyMeeting == false) return;
            else if (PhotonNetwork.LocalPlayer.NickName == caller) emergencyCalls++;    
        }
        GameManager.current.PauseGame();  //GAME IS PAUSED

        //skippedVotes = 0;
        Transform tf = meetingPanel.transform.Find("Player Voter Grid");

        for (int i = 0; i < tf.childCount; i++)
            Destroy(tf.GetChild(i).gameObject);

        List<string> keys = voteRecords.Keys.ToList();
        int playerCount = keys.Count - 1; // Excluding the skip vote key therefore minus 1
        for (int i = 0; i < playerCount; i++)
        {
            voteRecords[keys[i]] = 0;
            AddPlayerVotingSlot(allPlayerList[i], tf);
        }

        //for (int i = 0; i < allPlayerList.Count; i++)
        //    AddPlayerVotingSlot(allPlayerList[i], tf);

        AddSkipVotingSlot(tf);
        meetingPanel.SetActive(true);
        gameManager_PhotonView.RPC("RPC_InitMeetingCooldownAction", PhotonNetwork.MasterClient);
    }

    public void SetPlayerDeathStatus(string nickname)
    {
        for (int i = 0; i < allPlayerList.Count; i++)
        {
            if (allPlayerList[i].playerName == nickname)
            {
                allPlayerList[i].isDead = true;
                break;
            }
        }
    }

    private void AddSkipVotingSlot(Transform tf)
    {
        GameObject votingSlot = Instantiate(playerVotingSlot, tf);
        votingSlot.transform.Find("Player Name").GetComponent<TMPro.TextMeshProUGUI>().text = "SKIP";
        votingSlot.transform.Find("Vote Button").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { Vote(skipVoteCode); });
        votingSlot.transform.Find("Player Image").GetComponent<UnityEngine.UI.Image>().gameObject.SetActive(false);
    }

    private void AddPlayerVotingSlot(MatchPlayerData data, Transform tf)
    {
        GameObject votingSlot = Instantiate(playerVotingSlot, tf);
        TMPro.TextMeshProUGUI tmp = votingSlot.transform.Find("Player Name").GetComponent<TMPro.TextMeshProUGUI>();
        tmp.text = data.playerName;
        if (data.isDead)
        {
            tmp.color = Color.yellow;
            votingSlot.transform.Find("Vote Button").gameObject.SetActive(false);
        }
        else
            votingSlot.transform.Find("Vote Button").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { Vote(data.playerName); });

        votingSlot.transform.Find("Player Image").GetComponent<UnityEngine.UI.Image>().color = data.color;
    }

    public void Vote(string playerName)
    {
        if (hasVoted) return;
        //if (playerName == skipVoteCode) skippedVotes++;
        //else
        //{
        //    for (int i = 0; i < allPlayerList.Count; i++)
        //    {
        //        if (allPlayerList[i].playerName == playerName)
        //        {
        //            allPlayerList[i].votes++;
        //            break;
        //        }
        //    }
        //}
        foreach (Transform tf in meetingPanel.transform.Find("Player Voter Grid"))
        {
            tf.Find("Vote Button").gameObject.SetActive(false);
        }
        gameManager_PhotonView.RPC("RPC_UpdateVoteStats", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, playerName);
        hasVoted = true;
    }


    public void UpdateVotes(string voterName, string votedPlayerName)
    {
        Color voterColor = Color.white;
        for (int i = 0; i < allPlayerList.Count; i++)
            if (voterName == allPlayerList[i].playerName)
            {
                voterColor = allPlayerList[i].color;
                break;
            }

        Transform parent = meetingPanel.transform.Find("Player Voter Grid");
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform tf = parent.GetChild(i);
            TMPro.TextMeshProUGUI tmp = tf.Find("Player Name").GetComponent<TMPro.TextMeshProUGUI>();
            if (tmp.text == voterName)
                tmp.color = Color.green;

            if (tmp.text == votedPlayerName)
            {
                GameObject vig = tf.Find("Vote Indicator Grid").gameObject;
                GameObject voteIndicatorInstance = Instantiate(voteIndicator, vig.transform);
                voteIndicatorInstance.GetComponent<UnityEngine.UI.Image>().color = voterColor;
            }
        }

        //foreach (Transform tf in meetingPanel.transform.Find("Player Voter Grid"))
        //{
        //    TMPro.TextMeshProUGUI tmp = tf.Find("Player Name").GetComponent<TMPro.TextMeshProUGUI>();
        //    if (tmp.text == voterName)
        //    {
        //        GameObject vig = tf.Find("Vote Indicator Grid").gameObject;
        //        vig.SetActive(false);
        //        GameObject voteIndicatorInstance = Instantiate(voteIndicator, vig.transform);
        //        voteIndicatorInstance.GetComponent<UnityEngine.UI.Image>().color = voterColor;
        //        tmp.color = Color.green;
        //    }
        //}
        voteRecords[votedPlayerName]++;
    }

    float discussionTimerCount = RoomManager.current.roomSettings.discussionTime * 60;

    public void OnMeeting()
    {
        gameManager_PhotonView.RPC("RPC_OnMeetingProtocol", RpcTarget.All);
    }

    public void OnMeetingAction()
    {
        discussionTimerCount--;
        meetingPanel.transform.Find("Cooldown Timer").GetComponent<TMPro.TextMeshProUGUI>().text = discussionTimerCount.ToString();
    }

    public void OnEndMeeting()
    {
        VoteData data = GetVoteResults();

        gameManager_PhotonView.RPC("RPC_EndMeetingProtocol", PhotonNetwork.MasterClient);
        gameManager_PhotonView.RPC("RPC_EndMeeting", RpcTarget.All);  //FINAL END
    }

    public void InitVoteCooldownAction()
    {
        GameManager.current.InitCooldownAction(10, VoteResultInitAction, null, VoteResultEndAction);
    }

    public void OnMeetingEndAction()
    {
        DisableEmergencyMeeting();
    }

    private void VoteResultInitAction()
    {
        gameManager_PhotonView.RPC("RPC_VoteResultInitProtocol", RpcTarget.All);
    }

    private void VoteResultEndAction()
    {
        gameManager_PhotonView.RPC("RPC_VoteResultEndProtocol", RpcTarget.All);
    }

    public void OnVoteResultInit()
    {
        var recUI = meetingPanel.transform.GetComponentsInChildren<VoteRecordUI>();
        for (int i = 0; i < recUI.Length; i++)
        {
            recUI[i].gameObject.SetActive(true);
        }
    }

    public void OnVoteResultEnd()
    {
        VoteRecordUI[] recUI = meetingPanel.transform.GetComponentsInChildren<VoteRecordUI>();

        for (int i = 0; i < recUI.Length; i++)
        {
            for (int k = 0; k < recUI[i].transform.childCount; k++)
                Destroy(recUI[i].transform.GetChild(k).gameObject);
        }

        discussionTimerCount = DiscussionCooldownTime;
        meetingPanel.SetActive(false);
        GameManager.current.ResumeGame();
    }

    private float DiscussionCooldownTime { get { return RoomManager.current.roomSettings.discussionTime * 60; } }

    private VoteData GetVoteResults()
    {
        int playerCount = allPlayerList.Count;
        int skipped = voteRecords[skipVoteCode];
        if (skipped >= Mathf.CeilToInt(playerCount * 0.5f) ) return null; //ON VOTING END
        VoteData maxVotedData = new VoteData(" ", int.MinValue);
        int totalVoteCount = 0;
        List<string> keys = voteRecords.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            totalVoteCount += voteRecords[keys[i]];

            if (keys[i] != skipVoteCode)
            {
                int votes = voteRecords[keys[i]];
                if (votes > maxVotedData.voteCount)
                    maxVotedData = new VoteData(keys[i], votes);
            }
        }
        totalVoteCount -= maxVotedData.voteCount;
        if (maxVotedData.voteCount <= totalVoteCount) return null;  //ON VOTING END

        return maxVotedData;
        //VOTERESULT PROTOCOL
    }


    public void EnableEmergencyMeeting()
    {
        allowEmergencyMeeting = true;
    }

    public void DisableEmergencyMeeting()
    {
        allowEmergencyMeeting = false;
    }

    private class VoteData
    {
        public string playerName;
        public int voteCount;

        public VoteData(string playerName, int voteCount)
        {
            this.playerName = playerName;
            this.voteCount = voteCount;
        }
    }
}
