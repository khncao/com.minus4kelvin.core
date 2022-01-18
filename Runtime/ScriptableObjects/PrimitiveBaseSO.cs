using System;
using UnityEngine;

namespace m4k {
public abstract class PrimitiveBaseSO<T> : ScriptableObject, IComparable<T> where T : IComparable {
    [SerializeField]
    T _value;

    public T value {
        get { return _value; }
        set {
            _value = value;
            onValueChange?.Invoke(value);
            onChange?.Invoke();
        }
    }

    public System.Action<T> onValueChange;
    public System.Action onChange;

    private void OnValidate() {
        onChange?.Invoke();
    }

    public int CompareTo(T obj) {
        return _value.CompareTo(obj);
    }
}
}