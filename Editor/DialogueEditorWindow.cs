using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace m4k.Progression {
#if UNITY_EDITOR
// TODO: recurse subassets during import/export
public class DialogueEditorWindow : EditorWindow {

    [MenuItem("Tools/Dialogues/Dialogues Editor Window")]
    private static void ShowWindow() {
        var window = GetWindow<DialogueEditorWindow>();
        window.titleContent = new GUIContent("Dialogues Editor");
        window.Show();
    }

    // DialogueSO dialogue;
    // Vector2 scrollPos = Vector2.zero;
    // List<List<bool>> foldoutLines = new List<List<bool>>(2);
    // bool addConvo;

    // private void OnGUI() {
    //     GUILayout.BeginHorizontal();
    //     addConvo = GUILayout.Button("+", GUILayout.MaxWidth(20));
    //     dialogue = EditorGUILayout.ObjectField(dialogue, typeof(DialogueSO), false) as DialogueSO;
    //     GUILayout.EndHorizontal();
        
    //     if(!dialogue) 
    //         return;
    //     if(addConvo) {
    //         dialogue.dialogues.Add(new Dialogue.Convo());
    //     }
    //     scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
    //     for(int i = 0; i < dialogue.dialogues.Count; ++i) {
    //         var d = dialogue.dialogues[i];
    //         GUILayout.BeginHorizontal();
    //         if(GUILayout.Button("+", GUILayout.MaxWidth(20)))
    //             d.lines.Add(new Dialogue.Line());
    //         if(GUILayout.Button("x", GUILayout.MaxWidth(20))) {
    //             if(foldoutLines.Count >= dialogue.dialogues.Count)
    //                 foldoutLines.RemoveAt(i);
    //             dialogue.dialogues.RemoveAt(i);
    //             GUILayout.EndHorizontal();
    //             GUILayout.EndScrollView();
    //             continue;
    //         }
    //         d.id = EditorGUILayout.TextField(d.id);
    //         GUILayout.EndHorizontal();

    //         if(dialogue.dialogues[i].lines == null)
    //             dialogue.dialogues[i].lines = new Dialogue.LinesList();

    //         if(dialogue.dialogues[i].lines.Count < 1) 
    //             continue;
    //         if(foldoutLines.Count <= i) {
    //             foldoutLines.Add(new List<bool>());
    //         }

    //         for(int j = 0; j < d.lines.Count; ++j) {
    //             GUILayout.BeginHorizontal();
    //             if(GUILayout.Button("^", GUILayout.MaxWidth(20))) 
    //                 Swap<Dialogue.Line>(d.lines, j, Mathf.Max(j - 1, 0));
    //             if(GUILayout.Button("v", GUILayout.MaxWidth(20)))
    //                 Swap<Dialogue.Line>(d.lines, j, Mathf.Min(j + 1, d.lines.Count - 1));
    //             if(GUILayout.Button("x", GUILayout.MaxWidth(20))) {
    //                 d.lines.RemoveAt(j);
    //                 foldoutLines[i].RemoveAt(j);
    //                 GUILayout.EndHorizontal();
    //                 continue;
    //             }
    //             d.lines[j].character = EditorGUILayout.ObjectField(d.lines[j].character, typeof(Character), false, GUILayout.MaxWidth(100)) as Character;

    //             GUILayout.BeginVertical();
    //             if(foldoutLines[i].Count <= j)
    //                 foldoutLines[i].Add(false);
    //             foldoutLines[i][j] = EditorGUILayout.Foldout(foldoutLines[i][j], d.lines[j].text?.Substring(0, Mathf.Min(d.lines[j].text.Length, 50)));
    //             if(foldoutLines[i][j]) {
    //                 EditorGUILayout.LabelField("Text");
    //                 d.lines[j].text = EditorGUILayout.TextArea(d.lines[j].text);
    //                 d.lines[j].nextConvoId = EditorGUILayout.TextField("NextId", d.lines[j].nextConvoId);
    //             }
    //             GUILayout.EndVertical();
                
    //             GUILayout.EndHorizontal();
    //         }
    //     }
    //     EditorGUILayout.EndScrollView();
    // }

    static List<Convo> dialogues = new List<Convo>();
    static List<TextAsset> jsonDialogues = new List<TextAsset>();
    static string[] soPaths = new string[] {"Assets/Data/Dialogue/_import"};
    static string[] jsonPaths = new string[] {"Assets/Data/Dialogue/_export"};

    // Find and update convos at soPaths and cache
    static void UpdateDialogues() {
        dialogues.Clear();
        string[] soGuids = AssetDatabase.FindAssets("t:Convo", soPaths);

        for(int i = 0; i < soGuids.Length; ++i) {
            var dialogueAsset = (Convo)AssetDatabase.
                        LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(soGuids[i]), typeof(Convo));

            dialogues.Add(dialogueAsset);
        }
    }
    // Find and update jsons at jsonPaths and cache
    static void UpdateDialogueJsons() {
        jsonDialogues.Clear();
        string[] jsonGuids = AssetDatabase.FindAssets("t:TextAsset", jsonPaths);

        for(int i = 0; i < jsonGuids.Length; ++i) {
            var jsonDialogue = (TextAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(jsonGuids[i]), typeof(TextAsset));
            
            jsonDialogues.Add(jsonDialogue);
        }
    }

    // WIP TODO: serialize references by guid; serialize subassets
    // Currently can only be used to export/import main convo assets within editor session; instanceID will not survive editor reload

    [MenuItem("Tools/Dialogues/Export Convos to JSON")]
    static void ExportDialogues() {
        UpdateDialogues();
        for(int i = 0; i < dialogues.Count; ++i) {
            string json = JsonUtility.ToJson(dialogues[i], true);
            string path = Path.Combine(jsonPaths[0], dialogues[i].name);
            File.WriteAllText(path + ".json", json);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Dialogues/Sync JSON to Convos")]
    static void SyncDialogues() {
        UpdateDialogues();
        UpdateDialogueJsons();
        if(jsonDialogues.Count != dialogues.Count) {
            Debug.LogError("Differing number of json and convos");
            return;
        }
        for(int i = 0; i < jsonDialogues.Count; ++i) {
            JsonUtility.FromJsonOverwrite(jsonDialogues[i].text, dialogues[i]);
        }
    }

    public static void Swap<T>(IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }
}
#endif
}