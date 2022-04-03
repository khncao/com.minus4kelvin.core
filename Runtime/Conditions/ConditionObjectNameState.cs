using System;
using UnityEngine;
using m4k.Progression;

namespace m4k {
[Serializable]
public class ConditionObjectNameState : Condition {
    public UnityEngine.Object keyObject;
    public bool isNot;

    public override bool CheckConditionMet() {
        if(!keyObject) {
            Debug.LogError("No keyObject");
            return false;
        }
        if(isNot)
            return !ProgressionManager.I.CheckKeyState(keyObject.name);
        else
            return ProgressionManager.I.CheckKeyState(keyObject.name);
    }

    public override string ToString() {
        if(!keyObject) {
            Debug.LogError("No keyObject");
            return "";
        }
            
        string col;
        if(isNot) {
            col = !ProgressionManager.I.CheckKeyState(keyObject.name) ? "green" : "white";
            return $"<color={col}>- !{keyObject.name}</color>";
        }
        else {
            col = ProgressionManager.I.CheckKeyState(keyObject.name) ? "green" : "white";
            return $"<color={col}>- {keyObject.name}</color>";
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