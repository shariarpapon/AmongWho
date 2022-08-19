using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_SCRIPT : MonoBehaviour
{
    public Material outline;

    private void Start()
    {
        Debug.Log("material change");
        var rend = GetComponent<MeshRenderer>();
        rend.material = outline;
        rend.UpdateGIMaterials();
    }
}
