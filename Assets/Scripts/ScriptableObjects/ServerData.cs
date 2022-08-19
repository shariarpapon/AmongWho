using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Custom/Server Data")]
public class ServerData : ScriptableObject
{
    public Task[] crewTasks;
}
