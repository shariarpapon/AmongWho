using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    public void SetupCamera()
    {
        transform.parent = null;
        transform.rotation = Quaternion.Euler(65, 0, 0); ;
        player = LobbyManager.current.LocalPhotonView().transform;
    }

    public void OnLevelWasLoaded(int level)
    {
        if (level == 0)
            Destroy(gameObject);
    }

    public void Update()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}
