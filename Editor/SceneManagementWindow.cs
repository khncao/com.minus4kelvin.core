
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class SceneManagementWindow : EditorWindow {

    [MenuItem("Tools/Scene Management")]
    private static void ShowWindow() {
        var window = GetWindow<SceneManagementWindow>(false, "Scene Management Window", true);
    }

    [System.Serializable]
    public class SceneSession {
        public string label;
        public SceneSetup[] setup;
    }
    public List<SceneSession> sessions;
    const int entries = 3;

    public SceneManagementWindow() {
        // sessions = new List<SceneSetup[]>(3);
        Init();
    }

    private void Awake() {
        Init();
    }

    void Init() {
        if(sessions == null) {
            sessions = new List<SceneSession>();
            for(int i = 0; i < entries; ++i) {
                sessions.Add(new SceneSession());
            }
        }
    }

    private void OnGUI() {
        for(int i = 0; i < sessions.Count; ++i) {
            EditorGUILayout.BeginHorizontal();
            // GUILayout.Label(i.ToString());
            if(GUILayout.Button("S")) {
                StoreSceneSession(i);
            }
            if(GUILayout.Button("L")) {
                LoadSceneSession(i);
            }
            sessions[i].label = EditorGUILayout.TextField(sessions[i].label);
            EditorGUILayout.EndHorizontal();
        }
    }

    void StoreSceneSession(int id) {
        sessions[id].setup = EditorSceneManager.GetSceneManagerSetup();
    }

    void LoadSceneSession(int id) {
        if(sessions[id] == null)
            return;
        EditorSceneManager.RestoreSceneManagerSetup(sessions[id].setup);
    }

    void StoreSceneCamera(int id) {
        // SceneView.
    }
}