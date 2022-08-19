using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadPlayer : MonoBehaviour
{
    private void OnMouseDown()
    {
        ReportBody();
    }

    private void ReportBody()
    {
        Photon.Pun.PhotonView pv = LobbyManager.current.LocalPhotonView();
        GameManager.current.GetComponent<Photon.Pun.PhotonView>().RPC("RPC_CallMeeting", Photon.Pun.RpcTarget.All, Photon.Pun.PhotonNetwork.LocalPlayer.NickName, GetComponent<Photon.Pun.PhotonView>().ViewID, true);
    }
}
