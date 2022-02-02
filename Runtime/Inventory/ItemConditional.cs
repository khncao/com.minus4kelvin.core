using UnityEngine;

namespace m4k.Items {
// for achievements, uiItems, etc
[CreateAssetMenu(menuName="Data/Items/ItemConditional")]
[System.Serializable]
public class ItemConditional : Item
{
    public Conditions conditions;
    public bool hideIfUnmet;
    
    public bool CheckConditions() {
        return conditions.CheckCompleteReqs();
    }
}
}