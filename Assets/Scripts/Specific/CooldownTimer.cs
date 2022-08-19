using UnityEngine;
using TMPro;

public class CooldownTimer : MonoBehaviour
{
    public float cooldown = 10;

    public GameObject timerUI;
    public TextMeshProUGUI timerText;
    public Timer timer;

    private bool init = false;
    private bool playersLoaded = false;
    private float initTime = 0;

    public void Initiate()
    {
        init = true;
        GameManager.isGamePaused = true;
    }

    public void Update()
    {
        if (playersLoaded == false)
        {
            if (PlayersLoaded)
            {
                playersLoaded = true;
                initTime = GameManager.current.localTimer.currentTime;
            }
            else
                return;
        }

        if (IsTimerRunning && GameManager.current && init == true)
        {
            float decrement = GameManager.current.localTimer.currentTime - initTime;
            float cdTime = cooldown - decrement;
            
            timerText.text = Mathf.CeilToInt(cdTime).ToString();

            if (cdTime < 0)
                OnCooldownEnd();
        }
    }

    private bool PlayersLoaded
    {
        get
        {
            var players = FindObjectsOfType<PlayerController>();
            return players.Length == Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount;
        }
    }

    public void OnCooldownEnd()
    {
        GameManager.isGamePaused = false;
        Destroy(gameObject);
    }

    public bool IsTimerRunning
    {
        get { return GameManager.current.localTimer.isTimerRunning; }
    }
}
