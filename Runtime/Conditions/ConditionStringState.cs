using System;
using UnityEngine;
using m4k.Progression;

namespace m4k {
[Serializable]
public class ConditionStringState : Condition {
    public string key;
    public bool isNot;

    string _lastCheckStatus = "";

    public override bool CheckConditionMet() {
        if(string.IsNullOrEmpty(key)) {
            Debug.LogError("Key empty");
            return false;
        }
        if(isNot)
            return !ProgressionManager.I.CheckCompletionState(key);
        else 
            return ProgressionManager.I.CheckCompletionState(key);
    }

    public override string ToString() {
        if(string.IsNullOrEmpty(key)) {
            Debug.LogError("Key empty");
            return "";
        }
            
        string col;
        if(isNot) {
            col = !ProgressionManager.I.CheckCompletionState(key) ? "green" : "white";
            _lastCheckStatus = $"<color={col}>- !{key}</color>";
        }
        else {
            col = ProgressionManager.I.CheckCompletionState(key) ? "green" : "white";
            _lastCheckStatus = $"<color={col}>- {key}</color>";
        }
        return _lastCheckStatus;
    }

    public override void RegisterListener(Conditions conditions) {
        ProgressionManager.I.onRegisterCompletionState -= conditions.OnChange;
        ProgressionManager.I.onRegisterCompletionState += conditions.OnChange;
    }

    public override void UnregisterListener(Conditions conditions) {
        ProgressionManager.I.onRegisterCompletionState -= conditions.OnChange;
    }
}
}