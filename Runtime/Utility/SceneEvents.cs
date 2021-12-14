// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace m4k {
[System.Serializable]
public class KeyAction {
    public string key;
    public UnityEvent action;

    public KeyAction() {}
    public KeyAction(string key, UnityAction action) {
        this.key = key;
        this.action = new UnityEvent();
        this.action.AddListener(action);
    }

    public bool Invoke() {
        if(action != null && action.GetPersistentEventCount() > 0) {
            action.Invoke();
            return true;
        }
        return false;
    }
}

public class SceneEvents : MonoBehaviour
{
    public List<KeyAction> sceneChoiceActions;

    private void Start() {
        // register scene controller manager for multiple scenes
    }
    private void OnDisable() {
        
    }

    public bool InvokeChoiceAction(string key) {
        int ind = sceneChoiceActions.FindIndex(x=>x.key == key);

        if(ind != -1){
            return sceneChoiceActions[ind].Invoke();
        }
        return false;
    }
}
}