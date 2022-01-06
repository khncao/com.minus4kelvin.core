/// <summary>
/// Adapted from Unity's 3D Game Kit
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
    public class MonoBehaviourPooler<T> where T : UnityEngine.MonoBehaviour, IPooledMonoBehaviour<T> {
        public int activeCount;
        Stack<T> _stack;
        // Transform container;
        T _prefab;

        public MonoBehaviourPooler(int count, T prefab) 
        {
            _stack = new Stack<T>(count);
            _prefab = prefab;
            activeCount = 0;
            // container = new GameObject(prefab.ToString() + " pool").transform;

            for (int i = 0; i < count; ++i)
            {
                T instance = Object.Instantiate(_prefab);
                instance.pool = this;
                instance.gameObject.SetActive(false);
                // instance.transform.SetParent(container);

                _stack.Push(instance);
            }
        }

        public T GetNew() 
        {
            T instance;
            if(_stack.Count < 1) {
                instance = Object.Instantiate(_prefab);
                instance.pool = this;
                // instance.transform.SetParent(container);
            }
            else {
                instance = _stack.Pop();
            }
            instance.gameObject.SetActive(true);
            activeCount++;

            return instance;
        }

        public void Free(T obj) 
        {
            // obj.transform.SetParent(container);
            obj.gameObject.SetActive(false);
            _stack.Push(obj);
            
            activeCount--;
        }
    }

    public interface IPooledMonoBehaviour<T> where T : MonoBehaviour, IPooledMonoBehaviour<T> {
        MonoBehaviourPooler<T> pool { get; set; }
    } 
}