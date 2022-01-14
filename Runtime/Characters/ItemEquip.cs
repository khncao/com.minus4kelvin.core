

using UnityEngine;
using m4k.Items;

[CreateAssetMenu(fileName = "ItemEquip", menuName = "Data/Items/ItemEquip", order = 0)]
public class ItemEquip : Item {
    public static ItemTag[] equipTags = { ItemTag.Head, ItemTag.Body, ItemTag.Hairstyle, ItemTag.Outfit, ItemTag.Hat, ItemTag.Holdable };

    public override bool Primary(ItemSlot slot)
    {
        return base.Primary(slot);
    }
}