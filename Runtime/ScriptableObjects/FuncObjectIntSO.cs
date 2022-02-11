using System;
using UnityEngine;

namespace m4k {
[CreateAssetMenu(fileName = "FuncObjectIntSO", menuName = "Data/Events/FuncObjectIntSO", order = 0)]
public class FuncObjectIntSO : ScriptableObject {
    Func<object, int> func;

    public int FuncCount { get { return func.GetInvocationList().Length; }}


    private void OnDisable() {
        func = null;
    }

    public void AddFunction(Func<object, int> func) {
        this.func -= func;
        this.func += func;
    }

    public void RemoveFunction(Func<object, int> func) {
        this.func -= func;
    }

    /// <summary>
    /// Returns sum total of all listener function returned values.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int InvokeTotal(object obj) {
        if(func == null) {
            Debug.LogWarning("Invoked null func");
            return -1;
        }
        int total = 0;
        foreach(Func<object, int> i in func.GetInvocationList())
            total += i.Invoke(obj);
        return total;
    }

    /// <summary>
    /// Returns return value of most recently added function
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int InvokeLast(object obj) {
        if(func == null) {
            Debug.LogWarning("Invoked null func");
            return -1;
        }
        return func.Invoke(obj);
    }
}}