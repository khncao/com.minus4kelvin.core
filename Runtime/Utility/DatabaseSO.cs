using UnityEngine;
using System.Collections.Generic;
using m4k.Characters;
using m4k.Items;
using m4k.Progression;

namespace m4k {
[System.Serializable]
public class AddressDatabase {
    public string name;
    public string guid;
    public string address;
}

[CreateAssetMenu(fileName = "DatabaseSO", menuName = "Data/Singular/DatabaseSO", order = 0)]
public class DatabaseSO : ScriptableObject {
    [Tooltip("Default: Assets/Data")]
    public string dataPathOverride;
    
    public List<Item> items;
    public List<Character> characters;
    public List<Convo> convos;
}
}