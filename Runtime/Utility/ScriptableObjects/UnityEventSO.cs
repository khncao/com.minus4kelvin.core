using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace m4k {
[CreateAssetMenu(fileName = "UnityEventSO", menuName = "Data/Events/UnityEventSO", order = 0)]
public class UnityEventSO : ScriptableObject {
    [SerializeField]
    UnityEvent unityEvent;
    
    Dictionary<object, HashSet<UnityAction>> objectActions = new Dictionary<object, HashSet<UnityAction>>();

    private void Awake() {
        Reset();
    }

    public void Reset() {
        unityEvent.RemoveAllListeners();
        objectActions = new Dictionary<object, HashSet<UnityAction>>();
    }
    
    public void CleanupObj(object obj) {
        if(objectActions.TryGetValue(obj, out var map)) {
            foreach(var a in map) 
                unityEvent.RemoveListener(a);
        }
    }

    public void CleanupNullObjects() {
        foreach(var m in objectActions) {
            if(m.Key != null) continue;
            foreach(var a in m.Value) {
                unityEvent.RemoveListener(a);
            }
        }
    }
    
    public void Invoke() {
        unityEvent?.Invoke();
    }

    public void AddListener(UnityAction action) {
        if(!objectActions.TryGetValue(action.Target, out var map)) {
            map = new HashSet<UnityAction>();
            objectActions.Add(action.Target, map);
        }
        if(map.Contains(action))
            return;
        map.Add(action);
        unityEvent.AddListener(action);
    }

    public void RemoveListener(UnityAction action) {
        if(objectActions.TryGetValue(action.Target, out var map)) {
            if(map.Contains(action))
                map.Remove(action);
        }
        else
            return;
        unityEvent.RemoveListener(action);
    }
}
}