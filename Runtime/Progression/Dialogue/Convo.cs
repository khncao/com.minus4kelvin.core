using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m4k.Progression {
[System.Serializable]
[CreateAssetMenu(menuName="ScriptableObjects/Progress/Convo")]
public class Convo : ScriptableObject
{
    public List<Line> lines;
    public Convo nextConvo;
    public LineSO nextLine;
    public bool autoSkipIfSeen;

    public List<LineSO> lineSos;
    public List<Convo> subConvos;

    // single instance to allow convo modification at runtime
    public Convo Instance { 
        get { 
            if(!_instance){
                _instance = Instantiate(this);
                // for name matching to keystates since instantiate appends suffix
                _instance.name = this.name;
            }
            return _instance;
        }
    }
    public string id { get { return name; }}

    [System.NonSerialized]
    Convo _instance;

#if UNITY_EDITOR
    // NaughtyAttributes added next commit with optional define
    // [NaughtyAttributes.Button]
    void UpdateLineSos() {
        for(int i = 0; i < lines.Count; ++i) {
            if(lineSos.Count < i + 1 || lineSos[i] == null) {
                var lineSO = ScriptableObject.CreateInstance<LineSO>();
                lineSO.name = $"Line_{i}";
                lineSO.line = lines[i];
                lineSos.Add(lineSO);
                AssetDatabase.AddObjectToAsset(lineSO, this);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
            }
            else if(lineSos[i] != null) {
                lineSos[i].line = lines[i];
            }
        }
    }

    // [NaughtyAttributes.Button]
    void CreateSubConvo() {
        var convo = ScriptableObject.CreateInstance<Convo>();
        convo.name = $"Subconvo_{subConvos.Count}";
        subConvos.Add(convo);
        AssetDatabase.AddObjectToAsset(convo, this);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
    }

    // [NaughtyAttributes.Button]
    void DeleteSelected() {
        string s = "";
        foreach(var i in Selection.objects) {
            s += $"{i.name}\n";
        }
        if(!EditorUtility.DisplayDialog("Delete selected subasset", $"Are you sure you want to delete:\n{s}", "Confirm", "Cancel"))
            return;
        foreach(var i in Selection.objects) {
            if(i == this) continue;
            DestroyImmediate(i, true);
        }
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
    }
    
    // [NaughtyAttributes.Button]
    void DeleteLineSos() {
        if(!EditorUtility.DisplayDialog("Delete LineSO subassets", "Are you sure you want to delete all LineSO subassets?", "Confirm", "Cancel"))
            return;
        DeleteSubAssets<LineSO>();
        lineSos.Clear();
    }

    // [NaughtyAttributes.Button]
    void DeleteSubConvos() {
        if(!EditorUtility.DisplayDialog("Delete Convo subassets", "Are you sure you want to delete all Convo subassets?", "Confirm", "Cancel"))
            return;
        DeleteSubAssets<Convo>();
        subConvos.Clear();
    }
    
    void DeleteSubAssets<T>() {
        var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(this));
        foreach(var i in subAssets) {
            if(i is T)
                DestroyImmediate(i, true);
        }
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
    }
#endif
}
}