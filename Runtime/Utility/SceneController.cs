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

[System.Serializable]
public class KeyPoints {
    public string key;
    public List<Transform> points;
}

public class SceneController : MonoBehaviour
{
    [Header("Keyed unity events with scene references")]
    public List<KeyAction> sceneKeyEvents;
    [Header("Keyed scene point sets(spawn, patrol, etc)")]
    public List<KeyPoints> sceneKeyPoints;
    public SerializableDictionary<string, GameObject> sceneKeyObjects;

    public UnityEngine.SceneManagement.Scene scene {
        get {
            return gameObject.scene;
        }
    }

    public static Dictionary<string, SceneController> SceneControllers { get; private set; } = new Dictionary<string, SceneController>();


    public static bool TryGetSceneController(string sceneName, out SceneController result) {
        return SceneControllers.TryGetValue(sceneName, out result);
    }

    private void Awake() {
        if(!SceneControllers.ContainsKey(gameObject.scene.name)) {
            SceneControllers.Add(gameObject.scene.name, this);
        }
    }
    private void OnDestroy() {
        SceneControllers.Remove(gameObject.scene.name);
    }

    public bool InvokeKeyEvent(string key) {
        int ind = sceneKeyEvents.FindIndex(x=>x.key == key);

        if(ind != -1){
            return sceneKeyEvents[ind].Invoke();
        }
        return false;
    }

    public List<Transform> GetKeyPoints(string key) {
        return sceneKeyPoints.Find(x=>x.key == key)?.points;
    }

    public GameObject GetKeyObject(string key) {
        sceneKeyObjects.TryGetValue(key, out GameObject obj);
        return obj;
    }
}
}