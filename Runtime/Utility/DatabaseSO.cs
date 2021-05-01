﻿using UnityEngine;
using System.Collections.Generic;
using m4k.Characters;
using m4k.InventorySystem;

namespace m4k {
[System.Serializable]
public class AddressDatabase {
    public string name;
    public string guid;
    public string address;
}

[CreateAssetMenu(fileName = "DatabaseSO", menuName = "ScriptableObjects/Singular/DatabaseSO", order = 0)]
public class DatabaseSO : ScriptableObject {
    public List<Item> items;
    public List<Character> characters;
    public List<GameScene> scenes;
}
}