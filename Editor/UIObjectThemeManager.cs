
using UnityEngine;
using UnityEditor;

public class UIObjectThemeManager : EditorWindow {

    [MenuItem("Tools/UI Object Manager")]
    private static void ShowWindow() {
        var window = GetWindow<UIObjectThemeManager>(false, "UI Object Manager", true);
    }

    public UIThemeSO uIThemeSO;

    private void OnGUI() {
        uIThemeSO = EditorGUILayout.ObjectField(uIThemeSO, typeof(UIThemeSO), false) as UIThemeSO;
        if(GUILayout.Button("Apply")) Apply();
    }

    void Apply() {
        // var objs = FindObjectsOfType<UIObject>();
        // foreach(var o in objs) {
        //     o.ApplyTheme(uIThemeSO);
        //     PrefabUtility.ApplyPrefabInstance(o.gameObject, InteractionMode.AutomatedAction);
        // }
        // Debug.Log($"Applied theme to {objs.Length} UI objects");

        var guids = AssetDatabase.FindAssets("t:Prefab", new string[] {"Assets/Prefabs"});
        int count = 0;
        for(int i = 0; i < guids.Length; ++i) {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);

            // using (var s = new PrefabUtility.EditPrefabContentsScope(path)) {
            //     var component = s.prefabContentsRoot.GetComponent<UIObject>();
            //     if(!component) {
            //         continue;
            //     }
            //     component.ApplyTheme(uIThemeSO);
            // }

            // var asset = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            var asset = PrefabUtility.LoadPrefabContents(path);

            var component = asset.GetComponent<UIObject>();
            if(!component) {
                PrefabUtility.UnloadPrefabContents(asset);
                continue;
            }

            component.ApplyTheme(uIThemeSO);

            // EditorUtility.SetDirty(asset);
            PrefabUtility.SaveAsPrefabAsset(asset, path);
            
            PrefabUtility.UnloadPrefabContents(asset);
            count++;
        }
        Debug.Log($"Applied theme to {count} UI obj prefabs");
        
        AssetDatabase.SaveAssets();
    }
}