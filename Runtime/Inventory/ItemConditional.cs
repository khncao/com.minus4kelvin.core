using UnityEngine;

namespace m4k.Items {
// for achievements, uiItems, etc
[CreateAssetMenu(menuName="ScriptableObjects/Items/ItemConditional")]
[System.Serializable]
public class ItemConditional : Item
{
    public Conditions conditions;
    
    public override bool Primary(ItemSlot slot) {
        return conditions.CheckCompleteReqs();
    }
}
}