using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskLogic : MonoBehaviour
{
    public bool runLogic;
    public TaskObject taskObject;

    public void SetLogicState(bool state)
    {
        runLogic = state;
    }

    public void Update()
    {
        if (runLogic == true)
            if (IsAttemptSuccess())
                EndTask();
    }

    protected virtual bool IsAttemptSuccess()
    {
        return false;
    }

    public void EndTask()
    {
        taskObject.Finish();
    }
}
