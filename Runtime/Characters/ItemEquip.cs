

using UnityEngine;
using m4k.InventorySystem;

[CreateAssetMenu(fileName = "ItemEquip", menuName = "ScriptableObjects/Items/ItemEquip", order = 0)]
public class ItemEquip : Item {
    public static ItemTag[] equipTags = { ItemTag.Head, ItemTag.Body, ItemTag.Hairstyle, ItemTag.Outfit, ItemTag.Hat, ItemTag.Holdable };
    public override void SingleClick(ItemSlot slot)
    {
        base.SingleClick(slot);
    }
}