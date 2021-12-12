using UnityEngine;
using System.Collections.Generic;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
using m4k.InventorySystem;
using m4k.Characters;
using m4k.Progression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m4k {
public class AssetRegistry : Singleton<AssetRegistry> {
    public DatabaseSO database;
    [Tooltip("Default: Assets/Data")]
    public string dataPathOverride;

    [System.NonSerialized]
    Dictionary<string, Item> itemNameDict;
    // Dictionary<string, Item> itemGuidDict = new Dictionary<string, Item>();
    [System.NonSerialized]
    Dictionary<string, Character> charDict;
    [System.NonSerialized]
    Dictionary<ItemTag, List<Item>> itemTagLists;
    [System.NonSerialized]
    Dictionary<ItemType, List<Item>> itemTypeLists;
    [System.NonSerialized]
    Dictionary<string, Convo> idConvosDict;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
        if(!database || database.items == null) {
            UpdateDatabase();
        }
        itemNameDict = new Dictionary<string, Item>();
        charDict = new Dictionary<string, Character>();
        itemTagLists = new Dictionary<ItemTag, List<Item>>();
        itemTypeLists = new Dictionary<ItemType, List<Item>>();
        idConvosDict = new Dictionary<string, Convo>();

        for(int i = 0; i < database.items.Count; ++i) {
            Item item = database.items[i];

            if(itemNameDict.ContainsKey(item.name)) {
                Debug.Log("Already contains: " + item.name);
            }
            itemNameDict.Add(item.name, item);
            // itemGuidDict.Add(item.guid, item);

            for(int j = 0; j < item.itemTags.Count; ++j) {
                if(itemTagLists.ContainsKey(item.itemTags[j]))
                    itemTagLists[item.itemTags[j]].Add(item);
                else 
                    itemTagLists.Add(item.itemTags[j], new List<Item>(){ item });
            }
            if(itemTypeLists.ContainsKey(item.itemType))
                itemTypeLists[item.itemType].Add(item);
            else
                itemTypeLists.Add(item.itemType, new List<Item>(){ item });
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

    // public AsyncOperationHandle LoadAsset(string key) {
    //     var handle = Addressables.LoadAssetAsync<Item>(name);
    //     nameItemDict.Add(handle.Result.name, handle.Result);
    //     return handle;
    // }

    // public Item GetItemFromGuid(string guid) {
    //     Item item;
    //     itemGuidDict.TryGetValue(guid, out item);
    //     if(!item) {
    //         Debug.LogWarning($"//{guid}// item guid not found");
    //     }
    //     return item;
    // }
    public Item GetItemFromName(string name) {
        Item item;
        itemNameDict.TryGetValue(name, out item);
        if(!item) {
            Debug.LogError(string.Format("//{0}// not found in db", name));
        }
        return item;
    }

    public Character GetCharacterFromName(string name) {
        Character chara;
        charDict.TryGetValue(name, out chara);
        if(!chara) {
            Debug.LogWarning($"//{name}// character not found");
        }
        return chara;
    }

    public List<Item> GetItemListByTag(ItemTag tag) {
        List<Item> get;
        itemTagLists.TryGetValue(tag, out get);
        return get;
    }
    public List<Item> GetItemListByType(ItemType type) {
        List<Item> get;
        itemTypeLists.TryGetValue(type, out get);
        return get;
    }

    public Convo GetConvoById(string id) {
        Convo convo;
        idConvosDict.TryGetValue(id, out convo);
        return convo;
    }

#if UNITY_EDITOR
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
    }

    private void OnValidate() {
        if(!string.IsNullOrEmpty(dataPathOverride))
            searchFolders[0] = dataPathOverride;
    }

    [MenuItem("Tools/Update Database")]
    // [InitializeOnLoadMethod]
    static void UpdateDatabase() {
        Initialize();
        UpdateTypeRegistry<Item>("t:Item", searchFolders, ref _database.items);
        UpdateTypeRegistry<Character>("t:Character", searchFolders, ref _database.characters);
        UpdateTypeRegistry<Convo>("t:Convo", searchFolders, ref _database.convos);
    }

    static void UpdateTypeRegistry<T>(string query, string[] searchFolders, ref List<T> l) where T : UnityEngine.Object
    {
        l = new List<T>();

        string[] result = AssetDatabase.FindAssets(query, searchFolders);

        for(int i = 0; i < result.Length; i++)
        {
            T obj = (T)AssetDatabase.LoadAssetAtPath<T>(
                AssetDatabase.GUIDToAssetPath(result[i])
            );
            l.Add(obj);
        }
        Debug.Log($"Updated query {query} asset registry, total: " + result.Length);
    }

#endif
}
// }
}