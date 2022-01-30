// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using m4k.Progression;

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

public class SceneController : MonoBehaviour
{
    public List<KeyAction> sceneKeyEvents;
    public SerializableDictionary<string, GameObject> sceneKeyObjects;

    public UnityEngine.SceneManagement.Scene scene {
        get {
            return gameObject.scene;
        }
    }

    private void OnEnable() {
        ProgressionManager.I?.RegisterSceneController(this);
    }
    private void OnDisable() {
        ProgressionManager.I?.UnregisterSceneController(this);
    }

    public bool InvokeKeyEvent(string key) {
        int ind = sceneKeyEvents.FindIndex(x=>x.key == key);

        if(ind != -1){
            return sceneKeyEvents[ind].Invoke();
        }
        return false;
    }

    public GameObject GetKeyObject(string key) {
        sceneKeyObjects.TryGetValue(key, out GameObject obj);
        return obj;
    }
}
}