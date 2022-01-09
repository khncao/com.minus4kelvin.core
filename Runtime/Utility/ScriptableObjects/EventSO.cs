using UnityEngine;
using UnityEngine.Events;

namespace m4k {
[CreateAssetMenu(fileName = "EventSO", menuName = "Data/Primitives/EventSO", order = 0)]
public class EventSO : ScriptableObject {
    UnityEvent unityEvent = new UnityEvent();

    // private void Awake() {
    //     unityEvent.RemoveAllListeners();
    // }
    
    public void Invoke() {
        unityEvent?.Invoke();
    }

    public void AddListener(UnityAction action) {
        unityEvent.AddListener(action);
    }

    public void RemoveListener(UnityAction action) {
        unityEvent.RemoveListener(action);
    }
}
}