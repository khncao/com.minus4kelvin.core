/// <summary>
/// Adopted from Unity's ECS/DOTS Sample
/// </summary>

using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
public static class EnableInstancingOnAllMaterials
{
    [MenuItem("Tools/Enable Instancing on All Materials")]

    static void DoIt()
    {
        var materialGuids = AssetDatabase.FindAssets("t:Material");
        foreach (var materialGuid in materialGuids)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuid));
            material.enableInstancing = true;
        }
    }
}
#endif