// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class KeyAction {
    public string key;
    public UnityEvent action;
}

public class SceneController : MonoBehaviour
{
    public List<KeyAction> sceneChoiceActions;
    public List<KeyAction> objectiveActions;

    private void Start() {
        // register scene controller manager for multiple scenes
    }

    public void InvokeChoiceAction(string key) {
        int ind = sceneChoiceActions.FindIndex(x=>x.key == key);

        if(ind != -1)
            sceneChoiceActions[ind].action?.Invoke();
    }

    public void InvokeObjectiveAction(string key) {
        int ind = objectiveActions.FindIndex(x=>x.key == key);

        if(ind != -1)
            objectiveActions[ind].action?.Invoke();
    }
}
