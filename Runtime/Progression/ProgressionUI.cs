
using UnityEngine;
using m4k.Items;

namespace m4k.Progression {
public class ProgressionUI : MonoBehaviour {
    public GameObject journalWindow, achievesWindow;
    public Transform objectiveLogParent, objectiveTrackerGUIParent;
    public GameObject objectiveTxtPrefab;
    public ItemSlotHandler achieveSlotManager;

    ProgressionManager progressionManager;

    public void Init(ProgressionManager pm) {
        progressionManager = pm;
    }
    
    public void ToggleJournal() {
        journalWindow.SetActive(!journalWindow.activeInHierarchy);
    }
    public void ToggleAchievements() {
        if(!achievesWindow.activeInHierarchy)
            progressionManager.CheckAchievements();
        achievesWindow.SetActive(!achievesWindow.activeInHierarchy);
    }
    public GameObject InstantiateGetObjectiveTracker() {
        // var instance = Instantiate(objectiveTxtPrefab, objectiveLogParent, false);
        var instance = Instantiate(objectiveTxtPrefab, objectiveTrackerGUIParent, false);
        return instance;
    }
}}