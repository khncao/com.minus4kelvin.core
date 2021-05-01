using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRegistry<T> 
{
    public List<T> instances = new List<T>();
    public System.Action<T> onRegistered, onUnregistered;

    public void RegisterInstance(T t) {
        if(instances.Contains(t)) {
            Debug.LogWarning("Registry already contains item");
            return;
        }
        instances.Add(t);
        onRegistered?.Invoke(t);
    }

    public void UnregisterInstance(T t) {
        if(!instances.Contains(t)) {
            Debug.LogWarning("Registry does not contain item");
            return;
        }
        instances.Remove(t);
        onUnregistered?.Invoke(t);
    }

    public bool Contains(T t) {
        return instances.Contains(t);
    }
}

public interface IRegisterable {
    void Register();
    void Unregister();
}