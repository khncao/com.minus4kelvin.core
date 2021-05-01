using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
using m4k.InventorySystem;
using m4k.Characters;

namespace m4k {
// [ExecuteInEditMode]
public class AssetRegistry : Singleton<AssetRegistry> {
    public static DatabaseSO database;

    Dictionary<string, Item> itemNameDict = new Dictionary<string, Item>();
    // Dictionary<string, Item> itemGuidDict = new Dictionary<string, Item>();
    Dictionary<string, Character> charDict = new Dictionary<string, Character>();
    Dictionary<ItemTag, List<Item>> itemTagLists = new Dictionary<ItemTag, List<Item>>();
    Dictionary<ItemType, List<Item>> itemTypeLists = new Dictionary<ItemType, List<Item>>();

    static string[] searchFolders = new string[] { "Assets/Data" };
    // static string dataPath = "Assets/Data/";
    // Dictionary<string, Item> nameItemDict;
    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
        // nameItemDict = new Dictionary<string, Item>();

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
    // public Item GetItemFromName(string name) {
    //     if(!nameItemDict.ContainsKey(name))
    //         LoadAsset(name);
    //     Item item;
    //     nameItemDict.TryGetValue(name, out item);
    //     return item;
    // }
    public Character GetCharacterFromName(string name) {
        Character chara;
        charDict.TryGetValue(name, out chara);
        if(!chara) {
            Debug.LogWarning($"//{name}// character not found");
        }
        return chara;
    }

    public GameScene GetSceneByName(string sceneName) {
        return database.scenes.Find(x=>x.sceneName == sceneName);
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

#if UNITY_EDITOR
    static void Initialize() {
        if(database) return;
        database = (DatabaseSO)AssetDatabase.LoadAssetAtPath("Assets/Data/DatabaseSO.asset", typeof(DatabaseSO));
    }

    [MenuItem("Tools/Update Database")]
    [InitializeOnLoadMethod]
    static void UpdateDatabase() {
        Initialize();
        // UpdateCharsRegistry();
        // UpdateItemsRegistry();
        UpdateTypeRegistry<Item>("t:Item", searchFolders, ref database.items);
        UpdateTypeRegistry<Character>("t:Character", searchFolders, ref database.characters);
        UpdateTypeRegistry<GameScene>("t:GameScene", searchFolders, ref database.scenes);
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

    // static void UpdateItemsRegistry()
    // {
    //     database.items = new List<Item>();

    //     string[] result = AssetDatabase.FindAssets("t:Item", searchFolders);

    //     for(int i = 0; i < result.Length; i++)
    //     {
    //         var itemAsset = (Item)AssetDatabase.
    //                     LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(result[i]), 
    //                     typeof(Item));
    //         itemAsset.guid = result[i];

    //         database.items.Add(itemAsset);

    //         // convert item-> itembuildable and itemequip
    //         // string p = "Assets/Data/output/";
    //         // if(itemAsset.itemType == ItemType.Buildable) {
    //         //     var ib = ScriptableObject.CreateInstance<ItemBuildable>();
    //         //     ib.Copy(itemAsset);
    //         //     AssetDatabase.CreateAsset(ib, p + $"buildables/{itemAsset.name}.asset");
    //         // }
    //         // if(itemAsset.itemType == ItemType.Equip) {
    //         //     var ie = ScriptableObject.CreateInstance<ItemEquip>();
    //         //     ie.Copy(itemAsset);
    //         //     AssetDatabase.CreateAsset(ie, p + $"equips/{itemAsset.name}.asset");
    //         // }
    //         // AssetDatabase.SaveAssets();

    //     }
    //     Debug.Log("Updated items asset registry, total: " + result.Length);
    // }

    // static void UpdateCharsRegistry()
    // {
    //     database.characters = new List<Character>();

    //     string[] result = AssetDatabase.FindAssets("t:Character", searchFolders);

    //     for(int i = 0; i < result.Length; i++)
    //     {
    //         var charAsset = (Character)AssetDatabase.
    //                     LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(result[i]), 
    //                     typeof(Character));
    //         database.characters.Add(charAsset);
    //     }
    //     Debug.Log("Updated character asset registry, total: " + result.Length);
    // }

#endif
}
// }
}