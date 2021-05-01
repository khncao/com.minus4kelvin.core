using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace m4k {
public class SceneHandler : Singleton<SceneHandler>
{
    public GameScene mainMenuScene, newGameScene;
    public Action onSceneChanged, onSceneLoaded, onSceneUnloaded, onStartSceneChange, onReturnToTitle;
    public Action<float> onSceneLoadProgress;
    public Scene currScene;
    public int prevSceneIndex = -1;
    public AsyncOperation loadSceneAsync;
    public List<GameScene> gameScenes;

    public int latestLoadedSceneIndex { get { return currSceneIndex; }}
    public Scene activeScene { get { return SceneManager.GetActiveScene(); }}
    public bool isMainMenu { get { return activeScene.name == mainMenuScene.sceneName; }}

    int currSceneIndex;
    HashSet<GameScene> loadedScenes = new HashSet<GameScene>();
    // Dictionary<string, GameScene> nameSceneDict; 

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
            var gs = AssetRegistry.I.GetSceneByName(SceneManager.GetSceneAt(i).name);
            if(!loadedScenes.Contains(gs))
                loadedScenes.Add(gs);
        }
    }
    
    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    // void BuildSceneDatabase() {
    //     foreach(var s in gameScenes) {
    //         if(nameSceneDict.ContainsKey(s.sceneName)) continue;
    //             nameSceneDict.Add(s.sceneName, s);
    //     }
    //     // for(int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i) {
    //     //     var scene = SceneManager.GetSceneByBuildIndex(i);
    //     //     // nameSceneDict.Add(scene.name, )
    //     // }
    // }

    void OnSceneChanged(Scene a, Scene newScene) {
        Debug.Log($"OnChangeScene({newScene.buildIndex}: {newScene.name})");
        prevSceneIndex = activeScene.buildIndex;
        currScene = newScene;
        currSceneIndex = newScene.buildIndex;

        if(isMainMenu) {
            onReturnToTitle?.Invoke();
        }
        onSceneChanged?.Invoke();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        loadedScenes.Add(AssetRegistry.I.GetSceneByName(scene.name));
        onSceneLoaded?.Invoke();
        if(loadedScenes.Count > 1 && isMainMenu) {
            SetActiveScene(scene);
        }
        Debug.Log($"OnSceneLoaded: {scene.name}; Loaded: {loadedScenes.Count}");
    }
    void OnSceneUnloaded(Scene scene) {
        onSceneUnloaded?.Invoke();
        loadedScenes.Remove(AssetRegistry.I.GetSceneByName(scene.name));
        Debug.Log($"OnSceneUnloaded: {scene.name}; Loaded: {loadedScenes.Count}");
    }

    public void ReturnToMainMenu() {
        LoadScene(mainMenuScene, false);
    }
    
    // public void UnloadScene(int index, Action<AsyncOperation> callback) {
    //     var unload = SceneManager.UnloadSceneAsync(index);
    //     unload.completed += callback;
    // }
    public void UnloadScene(GameScene scene, Action<AsyncOperation> callback = null) {
        Debug.Log($"Unload {scene.sceneName}");
        if(!SceneManager.GetSceneByName(scene.sceneName).IsValid())
            return;
        var unload = SceneManager.UnloadSceneAsync(scene.sceneName);
        unload.completed += callback;
    }

    Coroutine loadSceneRoutine = null;

    public void LoadSceneByName(string sceneName, bool additive) {
        LoadScene(AssetRegistry.I.GetSceneByName(sceneName), additive);
    }
    public void LoadScene(GameScene scene, bool additive) {
        if(scene == mainMenuScene) {
            SceneManager.LoadScene(scene.sceneName);
            return;
        }
        if(SceneManager.GetSceneByName(scene.sceneName).IsValid()) {
            Debug.LogWarning($"tried to load existing: {scene.sceneName}");
            return;
        }
        if(loadSceneRoutine != null) {
            Debug.LogWarning("already loading a scene: " + scene.sceneName);
            return;
        }
        if(!additive) {
            foreach(var s in loadedScenes) {
                if(s == mainMenuScene || scene.requiredScenes.Contains(s))
                    continue;
                UnloadScene(s);
            }
        }
        Debug.Log(string.Format("LoadScene: {0}", scene.sceneName));
        loadSceneRoutine = StartCoroutine(LoadSceneAsync(scene, additive));
    }

    IEnumerator LoadSceneAsync(GameScene scene, bool additive) 
    {
        onStartSceneChange?.Invoke();

        for(int i = 0; i < scene.requiredScenes.Count; ++i) {
            if(loadedScenes.Contains(scene.requiredScenes[i]))
                continue;
            yield return SceneManager.LoadSceneAsync(scene.requiredScenes[i].sceneName, LoadSceneMode.Additive);
            loadedScenes.Add(scene.requiredScenes[i]);
        }
        loadSceneAsync = SceneManager.LoadSceneAsync(scene.sceneName, LoadSceneMode.Additive);
        
        // loadSceneAsync.allowSceneActivation = false;
        while(!loadSceneAsync.isDone) {
            onSceneLoadProgress?.Invoke(loadSceneAsync.progress);
            yield return null;
        }

        SetActiveScene(scene.sceneName);
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
}
}