using UnityEngine;

public class TaskObject : MonoBehaviour
{
    private const float taskInteractionDistance = 7;

    public TaskID taskID;

    [HideInInspector]
    public bool isActive;
    [HideInInspector]
    public Task assignedTask;
    [HideInInspector]
    public Transform player;
    [HideInInspector]
    public GameObject gameTaskUI;

    private TaskLogic taskLogic;
    private TaskHandler taskHandler;
    private UnityEngine.UI.Button interactButton;

    private void Start()
    {
        taskHandler = FindObjectOfType<TaskHandler>();
        //interactButton = GameObject.FindGameObjectWithTag("InteractButton").GetComponent<UnityEngine.UI.Button>();
    }

    public void Activate(Task task, GameObject _gameTaskUI, UnityEngine.UI.Button _interactButton, Transform localPlayer)
    {
        interactButton = _interactButton;
        assignedTask = task;
        gameTaskUI = _gameTaskUI;
        gameTaskUI.SetActive(false);
        player = localPlayer;

        _gameTaskUI.transform.Find("StateButton").GetComponent<UnityEngine.UI.Button>()?.onClick.AddListener( () => { SetTaskUIActive(_gameTaskUI); } );
        _gameTaskUI.transform.Find("FinishButton").GetComponent<UnityEngine.UI.Button>()?.onClick.AddListener( () => { Finish(); } );
        taskLogic = _gameTaskUI.GetComponent<TaskLogic>();
        taskLogic.taskObject = this;
        isActive = true;
    }

    private void OnMouseDown()
    {
        if (isActive && player != null)
        {
            if (Vector3.Distance(player.position, transform.position) <= taskInteractionDistance)
            {
                Debug.Log("Allowing interaction with " + assignedTask.taskID.ToString());
                SetTaskUIActive(gameTaskUI);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int localID = LobbyManager.current.LocalPhotonView().ViewID;
        Photon.Pun.PhotonView pv = other.GetComponent<Photon.Pun.PhotonView>();
        if (pv == null)
            return;

        if (other.isTrigger)
            return;

        if (pv.ViewID == localID && isActive)
        {
            if (Vector3.Distance(player.position, transform.position) <= taskInteractionDistance)
            {
                interactButton.onClick.RemoveAllListeners();
                interactButton.onClick.AddListener(()=> { SetTaskUIActive(gameTaskUI); });
                interactButton.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        interactButton.onClick.RemoveAllListeners();
        interactButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameTaskUI)
            if (gameTaskUI.activeSelf)
                if (Vector3.Distance(player.position, transform.position) > taskInteractionDistance)
                    gameTaskUI.SetActive(false);
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void SetTaskUIActive(GameObject obj)
    {
        Debug.Log("Trying to activate the gameobject");
        gameTaskUI?.SetActive(!obj.activeSelf);
        if (gameTaskUI.activeSelf)
            taskLogic?.SetLogicState(true);
        else
            taskLogic?.SetLogicState(false);
    }

    public void Finish()
    {
        isActive = false;
        FindObjectOfType<TaskHandler>().FinishTask(taskID, this);
    }
}
