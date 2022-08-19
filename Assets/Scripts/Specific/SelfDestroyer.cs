using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroyer : MonoBehaviour
{
    public float lifetime;
    private float time = 0;

    private void Update()
    {
        time += Time.deltaTime;
        if (time > lifetime)
            Destroy(gameObject);
    }
}
