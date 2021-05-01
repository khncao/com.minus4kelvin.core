using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
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
        return obj;
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