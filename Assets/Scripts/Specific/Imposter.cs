using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;

public class Imposter : MonoBehaviour
{
    public GameObject PFB_imposterControlUI;
    public GameObject PFB_bloodSplatterParticle;

    private Transform canvas;
    public Task[] sabotageTasks;

    private float distanceToTarget;

    private PlayerController target;

    private Button killButton;
    private Button sabotageButton;
    private Button ventButton;

    private List<PlayerController> inRangePlayers = new List<PlayerController>();

    public void InitiateImposterSettings()
    {
        if(GetComponent<Photon.Pun.PhotonView>().IsMine == false)return;

        PFB_bloodSplatterParticle = GameManager.current.references.PFB_bloodSplatterParticle;
        PFB_imposterControlUI = GameManager.current.references.PFB_imposterControlUI;

        canvas = FindObjectOfType<Canvas>().transform;
        Transform t = Instantiate(PFB_imposterControlUI, canvas).transform;
        t.SetAsFirstSibling();
        killButton = t.Find("Kill").GetComponent<Button>();
        sabotageButton = t.Find("Sabotage").GetComponent<Button>();
        ventButton = t.Find("Vent").GetComponent<Button>();

        killButton.onClick.AddListener(() => { OnKill(); });
        sabotageButton.onClick.AddListener(() => { OnSabotage(); });
        ventButton.onClick.AddListener(() => { OnVent(); });
    }

    private void Update()
    {
        if (target != null)
        {
            Debug.Log("target found");
            OnTarget();
        }
        else
        {
            OnTargetNull();
            if (inRangePlayers.Count > 0)
                CheckForTarget();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || player.isImposter) return;
        inRangePlayers.Add(player);

        if (target != null) target = GetTarget(target, player);
        else target = player;
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        else if (pc == target) target = null;
        inRangePlayers.Remove(pc);
    }

    private void CheckForTarget()
    {
        float shortestDist = float.MaxValue;
        PlayerController pc = null;

        if (inRangePlayers == null || inRangePlayers.Count <= 0) return;

        for (int i = 0; i < inRangePlayers.Count; i++)
        {
            if (inRangePlayers[i] == null)
            {
                inRangePlayers.RemoveAt(i);
                return;
            }

            float dist = Vector3.Distance(inRangePlayers[i].transform.position, transform.position);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                pc = inRangePlayers[i];
            }
        }

        if (pc != null)
            target = pc;
    }

    private PlayerController GetTarget(PlayerController current, PlayerController newTarget)
    {
        Vector3 pos = transform.position;
        float distCurrent = Vector3.Distance(current.transform.position, pos);
        float distNew = Vector3.Distance(newTarget.transform.position, pos);
        if (distCurrent <= distNew) return current;
        else return newTarget;
    }

    private void OnKill()
    {
        Kill();
    }

    private void OnSabotage() { }

    private void OnVent() { }

    private void OnTarget()
    {
        Debug.Log("Target is " + target.name);
        Highlight(target.gameObject);
        killButton.gameObject.SetActive(true);
    }

    private void OnTargetNull()
    {
        killButton.gameObject.SetActive(false);
    }

    public void Kill()
    {
        if (target == null) return;
        Vector3 pos = target.transform.position;

        string choppedBodyPath = "Body_Chopped";
        string bloodSplatterPath = "Blood_Splatter";

        PhotonView cpv = target.GetComponent<PhotonView>();

        Color color = cpv.GetComponentInChildren<SkinnedMeshRenderer>().material.color;
        Vector3 color3 = new Vector3(color.r, color.g, color.b);

        GameObject choppedBody = PhotonNetwork.Instantiate(choppedBodyPath, new Vector3(pos.x, 0, pos.z), Quaternion.identity);
        int cbID = choppedBody.GetComponent<PhotonView>().ViewID;

        GetComponent<PhotonView>().RPC("RPC_KillCrewmate", RpcTarget.AllViaServer, cbID, cpv.ViewID, target.playerName, color3);

        PhotonNetwork.Instantiate(bloodSplatterPath, pos, Quaternion.identity);
        target = null;
    }

    public void Sabotage(Task task)
    {
        Debug.Log($"Sbotaging {task.name}");
    }

    private void Highlight(GameObject crewmate)
    {
        Debug.Log("Highlighting " + crewmate.name);
    }

    private void Unhighlight(GameObject crewmate)
    {
        Debug.Log("Un-highlighting " + crewmate.name);
    }
}
