using System;

namespace m4k {
[Serializable]
public abstract class Condition {
    public Conditions Conditions { get; set; }
    // public virtual void BeforeCheck(Conditions conditions) {}
    public virtual void Init() {}
    public virtual void AfterComplete() {}

    public abstract bool CheckConditionMet();
    
    public abstract void RegisterListener(Conditions conditions);
    public abstract void UnregisterListener(Conditions conditions);
}
}