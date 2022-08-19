using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "New Task", menuName = "Custom/Task")]
public class Task : ScriptableObject
{
    public string _name;
    public TaskID taskID;
    public string location;
    public GameObject gameTaskUIPrefab;

    //[System.NonSerialized]
    //public bool isFinished;

    [HideInInspector]
    public TextMeshProUGUI taskStatusUI; 
}

