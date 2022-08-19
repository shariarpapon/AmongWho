using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TaskHandler : MonoBehaviour
{
    [HideInInspector]
    public List<Task> myTasks = null;

    public GameObject taskUIPrefab;
    public Transform taskUIParent;
    public Transform gameTaskUIParent;
    public TaskObject[] taskObjects;

    private GameObject interactButton;

    public void AssignTasks(int taskCount, ServerData data)
    {
        List<Task> tasks = data.crewTasks.ToList();

        taskCount = (taskCount > tasks.Count) ? tasks.Count : taskCount;
        interactButton = GameObject.FindWithTag("InteractButton");

        taskObjects = FindObjectsOfType<TaskObject>();

        for (int i = 0; i < taskCount; i++)
        {
            Debug.Log("Is assigning tasks");

            int rand = Random.Range(0, tasks.Count);
            Task task = tasks[rand];
            myTasks.Add(task);
            tasks.Remove(task);
            task.taskStatusUI = Instantiate(taskUIPrefab, taskUIParent).GetComponent<TMPro.TextMeshProUGUI>();
            GameObject gTaskUI = Instantiate(task.gameTaskUIPrefab, gameTaskUIParent);

            TaskObject to = GetTaskObject(task.taskID);
            to.Activate(task, gTaskUI, interactButton.GetComponent<UnityEngine.UI.Button>(), LobbyManager.current.LocalPhotonView().transform);
            UpdateTaskUI(i, " ");
        }

        interactButton.SetActive(false);
    }

    public TaskObject GetTaskObject(TaskID taskID)
    {
        TaskObject to = System.Array.Find(taskObjects, t => t.taskID == taskID);
        return to;
    }

    public void FinishTask(TaskID ID, TaskObject to)
    {
        Task tsk = System.Array.Find(myTasks.ToArray(), t => t.taskID == ID);
        to.Deactivate();

        if (AllTasksFinished())
            OnTasksFinished();

        if (interactButton.activeSelf)
            interactButton.SetActive(false);

        Destroy(to.gameTaskUI);
        UpdateTaskUI(tsk, "DONE");
    }

    public void UpdateTaskUI(int idx, string status)
    {
        Task tsk = myTasks[idx];
        tsk.taskStatusUI.text = $"{tsk.location} : {tsk._name} -- {status}";
        tsk.taskStatusUI.color = (status  == "DONE") ? Color.green : Color.white;
    }

    public void UpdateTaskUI(Task task, string status)
    {
        Task tsk = task;
        tsk.taskStatusUI.text = $"{tsk.location} : {tsk._name} -- {status}";
        tsk.taskStatusUI.color = (status == "DONE") ? Color.green : Color.white;
    }

    private bool AllTasksFinished()
    {
        var gtu = FindObjectsOfType<TaskLogic>();

        foreach (Task task in myTasks)
        {
            foreach (TaskLogic t in gtu)
            {
                if (task.taskID == t.taskObject.taskID)
                    return true;
            }
        }
        return false;
    }

    public void OnTasksFinished()
    {
        Debug.Log("All tasks finished");
    }
}
