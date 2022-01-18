using System;

namespace m4k {
[Serializable]
public abstract class Condition {
    public virtual void BeforeCheck(Conditions conditions) {}
    public abstract bool CheckConditionMet();
    public virtual void AfterComplete() {}
    public virtual void RegisterListener(Conditions conditions) {}
    public virtual void UnregisterListener(Conditions conditions) {}
}
}