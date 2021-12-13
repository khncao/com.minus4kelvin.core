using System;

namespace m4k.Progression {
[Serializable]
public class ConditionObjectNameState : Condition {
    public UnityEngine.Object keyObject;
    public bool isNot;

    string _lastCheckStatus = "";

    public override bool CheckConditionMet() {
        if(isNot)
            return !ProgressionManager.I.CheckCompletionState(keyObject.name);
        else
            return ProgressionManager.I.CheckCompletionState(keyObject.name);
    }

    public override string ToString() {
        if(keyObject == null) 
            return "";
            
        string col;
        if(isNot) {
            col = !ProgressionManager.I.CheckCompletionState(keyObject.name) ? "green" : "white";
            _lastCheckStatus = $"<color={col}>- !{keyObject.name}</color>";
        }
        else {
            col = ProgressionManager.I.CheckCompletionState(keyObject.name) ? "green" : "white";
            _lastCheckStatus = $"<color={col}>- {keyObject.name}</color>";
        }
        return _lastCheckStatus;
    }
}
}