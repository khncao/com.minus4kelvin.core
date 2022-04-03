using System;
using UnityEngine;
using m4k.Progression;

namespace m4k {
[Serializable]
public class ConditionStringState : Condition {
    public string key;
    public bool isNot;

    public override bool CheckConditionMet() {
        if(string.IsNullOrEmpty(key)) {
            Debug.LogError("Key empty");
            return false;
        }
        if(isNot)
            return !ProgressionManager.I.CheckKeyState(key);
        else 
            return ProgressionManager.I.CheckKeyState(key);
    }

    public override string ToString() {
        if(string.IsNullOrEmpty(key)) {
            Debug.LogError("Key empty");
            return "";
        }
            
        string col;
        if(isNot) {
            col = !ProgressionManager.I.CheckKeyState(key) ? "green" : "white";
            return $"<color={col}>- !{key}</color>";
        }
        else {
            col = ProgressionManager.I.CheckKeyState(key) ? "green" : "white";
            return $"<color={col}>- {key}</color>";
        }
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