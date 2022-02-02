using UnityEngine;
using System.Collections.Generic;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
using m4k.Items;
using m4k.Characters;
using m4k.Progression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m4k {
public class AssetRegistry : Singleton<AssetRegistry> {
    public static HashSet<RuntimeScriptableObject> scriptableObjects = new HashSet<RuntimeScriptableObject>();

    public DatabaseSO database;

    [System.NonSerialized]
    Dictionary<string, Item> itemNameDict;
    [System.NonSerialized]
    Dictionary<ItemTag, List<Item>> itemTagLists;
    [System.NonSerialized]
    Dictionary<ItemType, List<Item>> itemTypeLists;
    [System.NonSerialized]
    Dictionary<System.Type, List<Item>> itemTypeListDict;

    [System.NonSerialized]
    Dictionary<string, Character> charDict;

    [System.NonSerialized]
    Dictionary<string, Convo> idConvosDict;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
#if UNITY_EDITOR
        if(!_database) {
            UpdateDatabase();
        }
#endif
        itemNameDict = new Dictionary<string, Item>();
        itemTagLists = new Dictionary<ItemTag, List<Item>>();
        itemTypeListDict = new Dictionary<System.Type, List<Item>>();

        charDict = new Dictionary<string, Character>();

        idConvosDict = new Dictionary<string, Convo>();

        for(int i = 0; i < database.items.Count; ++i) {
            Item item = database.items[i];

            if(itemNameDict.ContainsKey(item.name)) {
                Debug.Log("Already contains: " + item.name);
            }
            itemNameDict.Add(item.name, item);

            for(int j = 0; j < item.itemTags.Count; ++j) {
                if(itemTagLists.ContainsKey(item.itemTags[j]))
                    itemTagLists[item.itemTags[j]].Add(item);
                else 
                    itemTagLists.Add(item.itemTags[j], new List<Item>(){ item });
            }

            if(itemTypeListDict.ContainsKey(item.GetType()))
                itemTypeListDict[item.GetType()].Add(item);
            else
                itemTypeListDict.Add(item.GetType(), new List<Item>(){ item });
        }

        for(int i = 0; i < database.characters.Count; ++i) {
            if(charDict.ContainsKey(database.characters[i].name)) {
                Debug.Log("Already contains: " + database.characters[i].name);
            }
            charDict.Add(database.characters[i].name, database.characters[i]);
        }

        for(int i = 0; i < database.convos.Count; ++i) {
            if(!idConvosDict.ContainsKey(database.convos[i].id)) {
                idConvosDict.Add(database.convos[i].id, database.convos[i]);
            }
        }
    }

    private void OnEnable() {
        foreach(var s in scriptableObjects) {
            s.OnEnable();
        }
    }

    private void OnDisable() {
        foreach(var s in scriptableObjects) {
            s.OnDisable();
        }
    }

    public Item GetItemFromName(string name) {
        Item item;
        itemNameDict.TryGetValue(name, out item);
        if(!item) {
            Debug.LogError(string.Format("//{0}// not found in db", name));
        }
        return item;
    }

    public List<Item> GetItemListByTag(ItemTag tag) {
        List<Item> get;
        itemTagLists.TryGetValue(tag, out get);
        return get;
    }

    public List<Item> GetItemListByType(System.Type type) {
        List<Item> get;
        itemTypeListDict.TryGetValue(type, out get);
        return get;
    }


    public Character GetCharacterFromName(string name) {
        Character chara;
        charDict.TryGetValue(name, out chara);
        if(!chara) {
            Debug.LogWarning($"//{name}// character not found");
        }
        return chara;
    }


    public Convo GetConvoById(string id) {
        Convo convo;
        idConvosDict.TryGetValue(id, out convo);
        return convo;
    }

#if UNITY_EDITOR
    public static DatabaseSO Database { 
        get { 
            Initialize(); 
            return _database; 
        }}
    static DatabaseSO _database;
    static string pathToDb = "Assets/Data/DatabaseSO.asset";
    static string[] searchFolders = new string[] { "Assets/Data" };

    static void Initialize() {
        if(_database) return;
        _database = (DatabaseSO)AssetDatabase.LoadAssetAtPath(pathToDb, typeof(DatabaseSO));
        if(!_database) {
            string[] result = AssetDatabase.FindAssets("t:DatabaseSO");
            if(result.Length > 0)
                _database = (DatabaseSO)AssetDatabase.LoadAssetAtPath<DatabaseSO>(AssetDatabase.GUIDToAssetPath(result[0]));
        }
        if(!_database) {
            Debug.LogWarning("DatabaseSO not found. Expected location: 'Assets/Data/DatabaseSO.asset'");
        }

        EditorApplication.playModeStateChanged -= OnPlayModeChange;
        EditorApplication.playModeStateChanged += OnPlayModeChange;
    }

    static void OnPlayModeChange(PlayModeStateChange state) {
        switch(state) {
            case PlayModeStateChange.EnteredPlayMode: {
                foreach(var i in scriptableObjects) {
                    i.OnEnable();
                }
                break;
            }
            case PlayModeStateChange.ExitingPlayMode: {
                foreach(var i in scriptableObjects) {
                    i.OnDisable();
                }
                break;
            }
        }
    }

    [MenuItem("Tools/Update Database")]
    // [InitializeOnLoadMethod]
    static void UpdateDatabase() {
        Initialize();
        
        if(!string.IsNullOrEmpty(_database.dataPathOverride))
            searchFolders[0] = _database.dataPathOverride;

        // scriptableObjects = FindAll<RuntimeScriptableObject>("t:RuntimeScriptableObject", searchFolders);

        _database.items = FindAll<Item>("t:Item", searchFolders);
        _database.characters = FindAll<Character>("t:Character", searchFolders);
        _database.convos = FindAll<Convo>("t:Convo", searchFolders);
    }

    static List<T> FindAll<T>(string query, string[] searchFolders) where T : UnityEngine.Object
    {
        var l = new List<T>();

        string[] result = AssetDatabase.FindAssets(query, searchFolders);

        for(int i = 0; i < result.Length; i++)
        {
            T obj = (T)AssetDatabase.LoadAssetAtPath<T>(
                AssetDatabase.GUIDToAssetPath(result[i])
            );
            l.Add(obj);
        }
        Debug.Log($"Updated query {query} asset registry, total: " + result.Length);
        return l;
    }

#endif
}
// }
}