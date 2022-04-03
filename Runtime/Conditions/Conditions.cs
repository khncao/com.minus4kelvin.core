
using UnityEngine;
using System.Collections.Generic;

namespace m4k {
// TODO: bool operators for conditions
// TODO: parameters in for condition check(self/target GO, etc)
// TODO: require all instances to call Init before use for reliable state

/// <summary>
/// Container for conditions. Should not rely on field state of this class and Condition classes as they are used in ScriptableObjects
/// </summary>
[System.Serializable]
public class Conditions  
{
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public List<Condition> conditions;
    
    [System.NonSerialized]
    public System.Action<Conditions> onChange;
    [System.NonSerialized]
    public System.Action onComplete;

    public UnityEngine.Object self { get; set; }

    public void Init(UnityEngine.Object self) {
        this.self = self;
        foreach(var condition in conditions)
            condition.Init();
    }

    // Listens to relevant onChange events to update condition completion status
    public void RegisterChangeListener() {
        foreach(var c in conditions) {
            c?.RegisterListener(this);
        }

        OnChange();
    }

    public void UnregisterChangeListener() {
        foreach(var c in conditions) {
            c?.UnregisterListener(this);
        }
    }
    
    public void OnChange() {
        onChange?.Invoke(this);
    }

    public bool CheckCompleteReqs() {
        for(int i = 0; i < conditions.Count; ++i) {
            if(conditions[i] == null) 
                continue;
            conditions[i].Conditions = this;
            if(!conditions[i].CheckConditionMet()) {
                return false;
            }
        }

        onComplete?.Invoke();
        return true;
    }

    public void FinalizeConditions() {
        foreach(var c in conditions) {
            c?.AfterComplete();
        }
        UnregisterChangeListener();
    }
}}