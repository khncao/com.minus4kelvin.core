using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace m4k {
[System.Serializable]
public class SceneDependency {
    public SceneReference scene;
    public List<SceneReference> required;
}
public class SceneHandler : Singleton<SceneHandler>
{
    public SceneReference mainMenuScene, newGameScene;
    public Action onSceneChanged, onSceneLoaded, onSceneUnloaded, onStartSceneChange, onFinishLoadAsync, onReturnToTitle;
    public Action<float> onSceneLoadProgress;
    public Scene currScene;
    public int prevSceneIndex = -1;
    public List<SceneDependency> sceneDependencies;
    
    public int latestLoadedSceneIndex { get { return currSceneIndex; }}
    public Scene activeScene { get { return SceneManager.GetActiveScene(); }}
    public bool isMainMenu { get { return activeScene.name == mainMenuScene.SceneName; }}

    int currSceneIndex;
    HashSet<string> loadedScenes = new HashSet<string>();
    AsyncOperation loadSceneAsync;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;

        OnDisable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnSceneChanged;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        // BuildSceneDatabase();
    }
    private void Start() {
        for(int i = 0; i < SceneManager.sceneCount; ++i) {
            var gs = SceneManager.GetSceneAt(i).name;
            if(!loadedScenes.Contains(gs))
                loadedScenes.Add(gs);
        }
    }
    
    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    void OnSceneChanged(Scene a, Scene newScene) {
        // Debug.Log($"OnChangeScene({newScene.buildIndex}: {newScene.name})");
        prevSceneIndex = activeScene.buildIndex;
        currScene = newScene;
        currSceneIndex = newScene.buildIndex;

        if(isMainMenu) {
            onReturnToTitle?.Invoke();
        }
        onSceneChanged?.Invoke();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if(!loadedScenes.Contains(scene.name))
            loadedScenes.Add(scene.name);

        onSceneLoaded?.Invoke();
        // Debug.Log($"OnSceneLoaded: {scene.name}; Loaded: {loadedScenes.Count}");
    }
    void OnSceneUnloaded(Scene scene) {
        onSceneUnloaded?.Invoke();
        loadedScenes.Remove(scene.name);
        // Debug.Log($"OnSceneUnloaded: {scene.name}; Loaded: {loadedScenes.Count}");
    }

    public void ReturnToMainMenu() {
        LoadScene(mainMenuScene.SceneName, false, false);
    }
    public void NewGameStart() {
        LoadScene(newGameScene.SceneName, false, true);
    }
    
    // public void UnloadScene(int index, Action<AsyncOperation> callback) {
    //     var unload = SceneManager.UnloadSceneAsync(index);
    //     unload.completed += callback;
    // }
    public void UnloadScene(string sceneName, Action<AsyncOperation> callback = null) {
        // Debug.Log($"Unload {sceneName}");
        if(!SceneManager.GetSceneByName(sceneName).IsValid())
            return;
        var unload = SceneManager.UnloadSceneAsync(sceneName);
        unload.completed += callback;
    }

    Coroutine loadSceneRoutine = null;

    public void LoadSceneByName(string sceneName, bool additive, bool setActiveScene) {
        LoadScene(sceneName, additive, setActiveScene);
    }
    public void LoadScene(string sceneName, bool additive, bool setActiveScene) {
        if(sceneName == mainMenuScene.SceneName) {
            SceneManager.LoadScene(sceneName);
            return;
        }
        if(SceneManager.GetSceneByName(sceneName).IsValid()) {
            Debug.LogWarning($"tried to load existing: {sceneName}");
            return;
        }
        if(loadSceneRoutine != null) {
            Debug.LogWarning("already loading a scene: " + sceneName);
            return;
        }
        onStartSceneChange?.Invoke();
        // Debug.Log(string.Format("LoadScene: {0}", sceneName));
        loadSceneRoutine = StartCoroutine(LoadSceneAsync(sceneName, additive, setActiveScene));
    }

    IEnumerator LoadSceneAsync(string sceneName, bool additive, bool setActiveScene) 
    {
        List<SceneReference> dependencies = GetSceneDependencies(sceneName);
        for(int i = 0; i < dependencies.Count; ++i) {
            if(loadedScenes.Contains(dependencies[i].SceneName))
                continue;
            yield return SceneManager.LoadSceneAsync(dependencies[i].SceneName, LoadSceneMode.Additive);
            loadedScenes.Add(dependencies[i].SceneName);
        }
        loadSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        // loadSceneAsync.allowSceneActivation = false;
        while(!loadSceneAsync.isDone) {
            onSceneLoadProgress?.Invoke(loadSceneAsync.progress);
            yield return null;
        }

        if(setActiveScene)
            SetActiveScene(sceneName);
        loadSceneRoutine = null;
        onFinishLoadAsync?.Invoke();
    }
    public void TriggerAllowSceneActivation() {
        if(loadSceneAsync == null) return;
        loadSceneAsync.allowSceneActivation = true;
    }
    public void SetActiveScene(string name) {
        SetActiveScene(SceneManager.GetSceneByName(name));
    }

    public void SetActiveScene(Scene scene) {
        // Debug.Log(string.Format("SetActiveScene: {0}", scene.name));
        SceneManager.SetActiveScene(scene);
    }

    // public GameScene GetSceneByName(string sceneName) {
    //     return sceneDB.scenes.Find(x=>x.sceneName == sceneName);
    // }
    List<SceneReference> GetSceneDependencies(string sceneName) {
        return sceneDependencies.Find(x=>x.scene.SceneName == sceneName).required;
    }
}
}