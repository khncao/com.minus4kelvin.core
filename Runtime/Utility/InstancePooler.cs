/// <summary>
/// Adapted from Unity's 3D Game Kit
/// </summary>

using System.Collections;
using System.Collections.Generic;
// using UnityEngine;

namespace m4k {
    public class InstancePooler<T> where T : IPooled<T>, new() {
        Stack<T> _stack;

        public InstancePooler(int count) {
            _stack = new Stack<T>(count);

            for (int i = 0; i < count; ++i)
            {
                var instance = new T();
                instance.pool = this;

                _stack.Push(instance);
            }
        }

        public T GetNew() 
        {
            T instance;
            if(_stack.Count < 1) {
                instance = new T();
                instance.pool = this;
            }
            else {
                instance = _stack.Pop();
            }

            return instance;
        }

        public void Free(T obj) 
        {
            _stack.Push(obj);
        }
    }

    public interface IPooled<T> where T : IPooled<T>, new() {
        InstancePooler<T> pool { get; set; }
    } 
}