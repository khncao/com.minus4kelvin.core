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
            onChange?.Invoke(value);
        }
    }

    public System.Action<T> onChange;

    public int CompareTo(T obj) {
        return _value.CompareTo(obj);
    }
}
}