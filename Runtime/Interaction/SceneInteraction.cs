using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace m4k.Interaction {
[System.Serializable]
public class SceneUnityEvent : UnityEvent<Scene> {}

public class SceneInteraction : MonoBehaviour
{
    public SceneReference toScene;
    public bool additive = true;
    // public SceneUnityEvent sceneUnityEvent;
    // public void LoadSceneAdditive(Scene scene) {
    //     SceneHandler.I.LoadSceneByName(sceneName, LoadSceneMode.Additive);
    // }
    public void LoadSceneAdditive() {
        SceneHandler.I.LoadScene(toScene.SceneName, additive);
    }

    public void UnloadScene() {
        SceneHandler.I.UnloadScene(toScene.SceneName, null);
    }
}
}