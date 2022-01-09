
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Applies theme preset to all prefabs found with UIObject component in "Assets/Prefabs" directory
/// </summary>
[CreateAssetMenu(fileName = "UIThemeSO", menuName = "Data/UIThemeSO", order = 0)]
public class UIThemeSO : ScriptableObject {
    [Header("Text")]
    public TMPro.TMP_FontAsset font1;
    public float fontSize1;
    public Color fontCol1;

    [Header("Sprites")]
    public Sprite panel1;
    public Sprite panel2;
    public Sprite slider1;
    public Sprite sliderKnob1;
    public Sprite scrollbar1;
    public Sprite button1;

    [Header("Colors")]
    public Color bg1;
    public Color accent1;
    public Color tint1;

#if UNITY_EDITOR
    [ContextMenu("Apply Theme")]
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

            component.ApplyTheme(this);

            // EditorUtility.SetDirty(asset);
            PrefabUtility.SaveAsPrefabAsset(asset, path);
            
            PrefabUtility.UnloadPrefabContents(asset);
            count++;
        }
        Debug.Log($"Applied theme to {count} UI obj prefabs");
        
        AssetDatabase.SaveAssets();
    }
#endif

}