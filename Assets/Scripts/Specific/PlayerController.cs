using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float angularSpeed;

    public Animator animator;

    private Joystick joystick;

    public bool isImposter = false;

    private delegate Vector2 InputMethod();
    private InputMethod movementInput;

    public string playerName;

    private void OnEnable()
    {
        Transform cam = transform.Find("PlayerCamera");
        PhotonView pv = GetComponent<PhotonView>();
        if (pv.IsMine == false) {
            cam.gameObject.SetActive(false);
            enabled = false;
        }
        else {
            cam.gameObject.SetActive(true);
            cam.GetComponent<Camera>().tag = "MainCamera";
            GetComponentInChildren<TextMeshPro>().text = PhotonNetwork.NickName;
        }
        joystick = FindObjectOfType<FixedJoystick>();

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
            movementInput = new InputMethod(KeyboardInput);
        else if (Application.platform == RuntimePlatform.Android)
            movementInput = new InputMethod(MobileInput);
    }

    private void Awake()
    {
        if(GetComponent<PhotonView>().IsMine)
            playerName = PhotonNetwork.LocalPlayer.NickName;
        Debug.Log(playerName);
    }

    private void OnLevelWasLoaded(int level)
    {
        PhotonView pv = GetComponent<PhotonView>();
        if (pv.IsMine == false)
            return;

        joystick = FindObjectOfType<Joystick>();
        if (level != 0)
        {
            if (isImposter)
                SetupImposter();
            else
                SetupCrewmate();

            string nickname = PhotonNetwork.LocalPlayer.NickName;
            Color color = GetComponentInChildren<SkinnedMeshRenderer>().material.color;
            Vector3 color3 = new Vector3(color.r, color.g, color.b);
            pv.RPC("RPC_SetMatchPlayerData", RpcTarget.All, nickname, color3);
        }
        else if(level == 0)
            isImposter = false;
    }

    private void Start()
    {
        PhotonView phv = GetComponent<PhotonView>();
        Color color = Random.ColorHSV();
        phv.RPC("RPC_SetMyColor", RpcTarget.AllBuffered, phv.ViewID, color.r, color.g, color.b);
        phv.RPC("RPC_Set3DName", RpcTarget.OthersBuffered, phv.ViewID, PhotonNetwork.NickName, true);
    }

    private void SetupImposter()
    {
        var titleParent = GameObject.FindWithTag("PlayerRoleTitle");
        titleParent.transform.Find("ImposterTitle").gameObject.SetActive(true);
        titleParent.transform.Find("CrewmateTitle").gameObject.SetActive(false);

        titleParent.SetActive(true);
        titleParent.GetComponent<UIFadeTransition>().InitTransition();
        GetComponentInChildren<TextMeshPro>().color = Color.red;
        gameObject.AddComponent<Imposter>().InitiateImposterSettings();
    }

    private void SetupCrewmate()
    {
        var titleParent = GameObject.FindWithTag("PlayerRoleTitle");
        titleParent.transform.Find("CrewmateTitle").gameObject.SetActive(true);
        titleParent.transform.Find("ImposterTitle").gameObject.SetActive(false);

        titleParent.SetActive(true);
        titleParent.GetComponent<UIFadeTransition>().InitTransition();
        GetComponentInChildren<TextMeshPro>().color = Color.white;
        gameObject.AddComponent<Crewmate>().ActivateCrewmate();
    }

    public void Update()
    {
        if (GameManager.isGamePaused)
            return;

        Vector2 input = movementInput();
        Move(input);
    }

    private Vector2 KeyboardInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    private Vector2 MobileInput()
    {
        return new Vector2(joystick.Horizontal, joystick.Vertical);
    }

    public void Move(Vector2 input)
    {
        if (input.magnitude > 0)
        {
            transform.Translate(new Vector3(input.x, 0, input.y).normalized * RoomManager.current.roomSettings.moveSpeed * Time.deltaTime, Space.World);
            Rotate(input);
            animator.SetBool("Walk", true);
            animator.SetBool("Idle", false);
        }
        else if(input.magnitude == 0)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Idle", true);
        }
    }

    public void Rotate(Vector2 input)
    {
        Quaternion rot = transform.rotation;
        Quaternion newRot = Quaternion.LookRotation(new Vector3(input.x, 0, input.y).normalized, Vector3.up);
        Quaternion smoothRot = Quaternion.Lerp(rot, newRot, angularSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, smoothRot.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
}
