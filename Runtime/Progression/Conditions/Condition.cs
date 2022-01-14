using System;

namespace m4k.Progression {
[Serializable]
public abstract class Condition {
    public virtual void InitializeCondition() {}
    public abstract bool CheckConditionMet();
    public virtual void FinalizeCondition() {}
}
}