using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Utility {
    public class ObjectPooler<T> where T : UnityEngine.MonoBehaviour, IPooled<T> {
        public T[] instances;
        public int activeCount;
        protected Stack<int> m_FreeIdx;
        Transform container;

        public void Initialize(int count, T prefab) 
        {
            instances = new T[count];
            m_FreeIdx = new Stack<int>(count);
            activeCount = 0;
            container = new GameObject(prefab.ToString() + " pool").transform;

            for (int i = 0; i < count; ++i)
            {
                instances[i] = Object.Instantiate(prefab);
                instances[i].gameObject.SetActive(false);
                instances[i].poolID = i;
                instances[i].pool = this;
                instances[i].transform.SetParent(container);

                m_FreeIdx.Push(i);
            }
        }

        public T GetNew() 
        {
            int idx = m_FreeIdx.Pop();
            instances[idx].gameObject.SetActive(true);
            activeCount++;

            return instances[idx];
        }

        public void Free(T obj) 
        {
            instances[obj.poolID].transform.SetParent(container);
            m_FreeIdx.Push(obj.poolID);
            instances[obj.poolID].gameObject.SetActive(false);
            activeCount--;
        }
    }

    public interface IPooled<T> where T : MonoBehaviour, IPooled<T> {
        int poolID { get; set; }
        ObjectPooler<T> pool { get; set; }
    } 
}