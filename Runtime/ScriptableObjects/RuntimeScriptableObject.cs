
using UnityEngine;

namespace m4k {
/// <summary>
/// Currently used for no domain reload workaround in AssetRegistry to manually call OnEnable and OnDisable in editor for ScriptableObject lifecycle
/// </summary>
public abstract class RuntimeScriptableObject : ScriptableObject {
    public virtual void Awake() {

    }

    public virtual void OnEnable() {
        if(!AssetRegistry.scriptableObjects.Contains(this))
            AssetRegistry.scriptableObjects.Add(this);
    }

    public virtual void OnDisable() {
        
    }

    public virtual void OnDestroy() {
        AssetRegistry.scriptableObjects.Remove(this);
    }

    public virtual void OnValidate() {

    }

    public virtual void Reset() {
        
    }
}
}