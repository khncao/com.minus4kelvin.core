using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
/// <summary>
/// Fade out GUI, fade in cinematic bars via CanvasGroup global registry for timeline bind targets. Useful for references to persistent scene objects and instantiated gameobjects.
/// </summary>
public class PlayableManager : Singleton<PlayableManager>
{
    public CanvasGroup cinematicBars, gui;
    public GameObject[] objects;
    Dictionary<string, GameObject> globalBindTargets = new Dictionary<string, GameObject>();

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
        
        for(int i = 0; i < objects.Length; ++i) {
            globalBindTargets.Add(objects[i].name, objects[i]);
        }
    }
    public GameObject GetBindTarget(string query) {
        GameObject obj;
        globalBindTargets.TryGetValue(query, out obj);
        if(!obj) Debug.LogWarning($"Could not find binding for {query}");
        else Debug.Log($"Global bind found for {query}");
        return obj;
    }

    public void RegisterBindTarget(GameObject go) {
        // clean name in case of instantiated with (clone)suffix
        string goName = go.name.Replace("(Clone)", "");
        if(!globalBindTargets.ContainsKey(goName)) {
            globalBindTargets.Add(goName, go);
            Debug.Log($"Registered bindable: {goName}");
        }
    }

    public void ToggleCinematic(bool b) {
        if(b) {
            cinematicBars.FadeIn();
            gui.FadeOut();
        }
        else {
            cinematicBars.FadeOut();
            gui.FadeIn();
        }
    }
}
}