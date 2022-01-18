
using UnityEngine;
using System.Collections.Generic;

namespace m4k {
// TODO: bool operators for conditions
[System.Serializable]
public class Conditions  
{
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public List<Condition> conditions;
    [Tooltip("Finalization such as removing condition items")]
    public bool autoFinalize;
    
    [System.NonSerialized]
    public System.Action<Conditions> onChange;
    [System.NonSerialized]
    public System.Action onComplete;

    [System.NonSerialized]
    bool _alreadyFinalized;
    

    public void Init() {
        _alreadyFinalized = false;
        foreach(var c in conditions) {
            c?.BeforeCheck(this);
        }
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
            if(conditions[i] == null) continue;
            if(!conditions[i].CheckConditionMet()) {
                return false;
            }
        }
        if(autoFinalize)
            FinalizeConditions();

        onComplete?.Invoke();
        return true;
    }

    public void FinalizeConditions() {
        if(_alreadyFinalized) 
            return;

        _alreadyFinalized = true;
        foreach(var c in conditions) {
            c?.AfterComplete();
        }
        UnregisterChangeListener();
    }
}}