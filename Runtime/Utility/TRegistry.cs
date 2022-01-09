using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TRegistry<T> 
{
    public List<T> instances = new List<T>();
    public System.Action<T> onRegistered, onUnregistered;

    public void RegisterInstance(T t) {
        if(instances.Contains(t)) {
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

    // static Dictionary<System.Type, MonoBehaviour> instanceDict = new Dictionary<System.Type, MonoBehaviour>();

    // public static void RegisterInstance<T>(T obj) {
    //     MonoBehaviour inst = null;
    //     instanceDict.TryGetValue(typeof(T), out inst);
    //     if(inst != null) {
    //         Debug.LogWarning($"{typeof(T).ToString()} instance exists");
    //         return;
    //     }
    //     instanceDict.Add(typeof(T), inst);
    // }

    // public static T GetInstance<T>() {
    //     MonoBehaviour inst = null;
    //     instanceDict.TryGetValue(typeof(T), out inst);

    //     if(inst == null) {
    //         // inst = GetComponentInChildren<T>() as MonoBehaviour;
    //         if(!inst) {
    //             Debug.LogWarning($"{typeof(T).ToString()} component not found");
    //             return default(T);
    //         }
    //         instanceDict.Add(typeof(T), inst);
    //     }
    //     if(inst != null)
    //         return (T)System.Convert.ChangeType(inst, typeof(T));
            
    //     Debug.LogWarning($"{typeof(T).ToString()} instance not found");;
    //     return default(T);
    // }
}

public interface IRegisterable {
    void Register();
    void Unregister();
}