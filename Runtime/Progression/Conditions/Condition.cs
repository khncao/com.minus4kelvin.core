using System;

namespace m4k.Progression {
[Serializable]
public abstract class Condition {
    public abstract bool CheckConditionMet();
    public virtual void FinalizeCondition() {}
}
}