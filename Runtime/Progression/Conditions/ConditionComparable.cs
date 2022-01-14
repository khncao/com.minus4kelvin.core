using System;
using UnityEngine;

namespace m4k.Progression {
[Serializable]
public class ConditionComparable<T> : Condition where T : IComparable {
    public string description;
    public PrimitiveBaseSO<T> obj;
    public ComparisonType op;
    public T val;
    
    string _lastCheckStatus = "";

    public override void InitializeCondition() {
    }

    public override bool CheckConditionMet() {
        if(!obj) {
            Debug.LogError("No obj in condition");
            return false;
        }
        return Comparisons.Compare(op, obj.value, val);
    }

    public override void FinalizeCondition() {
    }

    public override string ToString() {
        if(!obj) {
            Debug.LogError("No obj in condition");
            return "";
        }
        bool pass = Comparisons.Compare(op, obj.value, val);

        string col = pass ? "green" : "white";
        _lastCheckStatus = $"<color={col}>- {description}: {obj.value}/{val}</color>";
        return _lastCheckStatus;
    }
}

[Serializable]
public class ConditionIntComparable : ConditionComparable<int> {
}

[Serializable]
public class ConditionFloatComparable : ConditionComparable<float> { }

[Serializable]
public class ConditionBoolComparable : ConditionComparable<bool> { }
}