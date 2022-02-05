using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;
using m4k.Interaction;
using UnityEngine.Events;

namespace m4k.Progression {
public class DialogueManager : Singleton<DialogueManager>
{
    public DialogueUI UI;
    public DatabaseSO database;
    public AudioSource dialogueAudioSource;
    public AudioClip nextLineSfx;
    public int audioLineLengthBuffer = 1;
    public bool inChoice, inDialogue, inChoiceLoop;

    public System.Action onStartDialogue, onNextLine, onEndDialogue;
    public System.Action<Convo> onCompleteConvo;

    Dialogue _currDialogue;
    Character _currChar;
    bool _skipLine, _replaceCurr;
    int _currLineIndex;
    Coroutine _nextLineCr;
    Convo _prevConvo, _currConvo, _replacedConvo;
    List<Choice> _currChoices;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;

        if(!UI) UI = GetComponentInChildren<DialogueUI>();
        UI.Init(this);
    }

    public void StopDialogue() {
        UI.ToggleDialogueUI(false);
        UI.DisableChoices();
        dialogueAudioSource.Stop();
        inChoice = false;
        _prevConvo = _currConvo;
        _replacedConvo = null;
        _currDialogue = null;
        _currConvo = null;
        _currChar = null;
        _currChoices = null;
        inDialogue = false;
        inChoiceLoop = false;
        onEndDialogue?.Invoke();
        InteractionManager.I.ToggleHideBlockInteractables(false);
    }
    public void ReplaceDialogue(string id) {
        Convo convo = GetConvo(id);
        ReplaceDialogue(convo, 0);
    }
    public void ReplaceDialogue(Convo convo) {
        ReplaceDialogue(convo, 0);
    }
    public void ReplaceDialogue(Convo convo, int startLineIndex) {
        _replaceCurr = true;
        _replacedConvo = _currConvo;
        AssignDialogue(_currDialogue, convo, startLineIndex);
    }

    public void AssignDialogue(Dialogue dialogue, Convo convo, int startLineIndex) {
        if(!dialogue) {
            Debug.LogWarning("No dialogue assigned");
            return;
        }
        if(!convo) {
            Debug.Log("No convo assigned");
            return;
        }
        if(inDialogue && !_replaceCurr){
            return;
        }
        InteractionManager.I.ToggleHideBlockInteractables(true);
        _replaceCurr = false;
        inChoice = false;
        _currDialogue = dialogue;
        _currConvo = convo.Instance;
        _currLineIndex = startLineIndex;
        if(!_currChar) {
            _currChar = _currDialogue.character;
        }
        UI.ToggleDialogueUI(true);
        inDialogue = true;
        onStartDialogue?.Invoke();

        if(convo.autoSkipIfSeen && ProgressionManager.I.CheckKeyState(convo.id)) {
            StartChoicePrompt();
        }
        else
            NextLine();
    }

    public void InputNextLine() {
        if(!_currDialogue || !_currDialogue.allowInteract || !inDialogue) return;
        if(inChoice) {
            UI.SelectChoice();
        }
        else {
            NextLine();
        }
    }

    void NextLine() {
        if(_nextLineCr != null) {
            dialogueAudioSource.Stop();
            StopCoroutine(_nextLineCr);
        }

        if(_currLineIndex >= _currConvo.lines.Count) // end of currConvo
        {
            onCompleteConvo?.Invoke(_currConvo);
            
            if(_currConvo.nextConvo) {
                int ind = 0;
                if(_currConvo.nextLine) {
                    ind = GetLineSOIndex(_currConvo.nextLine);
                    Debug.Log(ind);
                }
                ReplaceDialogue(_currConvo.nextConvo, ind);
            }
            else if(_currDialogue.choicesOnConvoEnd) {
                StartChoicePrompt();
            }
            else {
                StopDialogue();
            }
            return;
        }
        Line line = _currConvo.lines[_currLineIndex];
        ParseDialogueLine(line);
        
        int lineDuration = _currDialogue.autopassSeconds;
        if(line.audio) {
            dialogueAudioSource.PlayOneShot(line.audio);
            lineDuration = (int)line.audio.length + audioLineLengthBuffer;
        }
        else if(nextLineSfx) {
            Feedback.I.PlayAudio(nextLineSfx);
        }
        if(lineDuration == 0 && !_currDialogue.allowInteract) {
            Debug.LogError("Line does not have way to progress, or dialogue mistakenly disallows input");
        }
        _currLineIndex++;
        onNextLine?.Invoke();

        if(_skipLine) {
            _skipLine = false;
            NextLine();
        }
        else if(lineDuration > 0) 
            _nextLineCr = StartCoroutine(PauseAndNextLine(lineDuration));
    }

    void ParseDialogueLine(Line line) {
        if(line.character) {
            _currChar = line.character;
        }
        if(line.choices.Count > 0) {
            StartInlineChoicePrompt(line.choices);
        }
        UI.UpdateDialogueUI(_currChar, line.text, _currDialogue.useCharPortrait);
    }

    bool StartChoicePrompt() {
        inChoice = inChoiceLoop = true;
        _currDialogue.UpdateChoices();
        if(_currDialogue.choices.Count < 1)
            return false;

        _currChoices = _currDialogue.choices;
        UI.ProcessChoices(_currChoices);
        UI.UpdateDialogueUI(_currDialogue.character, _currDialogue.choiceLine, _currDialogue.useCharPortrait);
        return true;
    }
    void StartInlineChoicePrompt(List<Choice> choices) {
        inChoice = true;
        _currChoices = choices;
        // skip seen subconvos if flagged autoSkipIfSeen
        for(int i = 0; i < choices.Count; ++i) {
            if(choices[i].nextConvo && choices[i].nextConvo.autoSkipIfSeen && ProgressionManager.I.CheckKeyState(choices[i].nextConvo.id)) {
                choices.RemoveAt(i);
            }
        }
        UI.ProcessChoices(choices);
    }

    public void SelectChoice(int val) {
        var choice = _currChoices[val];
        var preChoiceConvo = _currConvo;

        if(!string.IsNullOrEmpty(choice.key)) {
            _currDialogue?.InvokeChoiceAction(choice.key);
            ProgressionManager.I.InvokeKeyActions(choice.key);
        }

        if(choice.nextConvo) {
            ReplaceDialogue(choice.nextConvo);
        }
        else if(choice.text == _currDialogue.exitChoiceLine 
        || choice.key == "exit" 
        || (inChoiceLoop && _currConvo == preChoiceConvo)) 
        {
            StopDialogue();
        }
        else if(!inChoiceLoop) {
            NextLine();
        }

        inChoice = false;
        inChoiceLoop = false;
    }

    public void SetPaused(bool b) => inChoice = b;
    public void SetSkipLine(bool b) => _skipLine = b;

    IEnumerator PauseAndNextLine(int seconds) {
        while(inChoice) {
            yield return null;
        }
        yield return new WaitForSeconds(seconds);
        InputNextLine();
    }

    // util lineSO ref to line index in convo; hacky way to have assignable lines
    int GetLineSOIndex(LineSO lineSO) {
        if(!_currConvo) return -1;
        return _currConvo.lineSos.FindIndex(x=>x == lineSO);
    }

    public Convo GetConvo(string id) {
        Convo c = database.convos.Find(x=>x.name == id);
        if(!c) {
            Debug.LogError("Convo not found");
        }
        return c;
    }

    // Dictionary<string, DialogueState> dialogueStatesDict = new Dictionary<string, DialogueState>();
    // DialogueState[] _dialogueStates;
    // public void RegisterDialogue(Dialogue dialogue) {
    //     if(!dialogueStatesDict.ContainsKey(dialogue.dialogueId)) {
    //         var newState = new DialogueState();
    //         newState.dialogue = dialogue;
    //         newState.dialogueId = dialogue.dialogueId;
    //         newState.persistConvoId = dialogue.persistConvoId;

    //         dialogueStatesDict[dialogue.dialogueId] = newState;
    //     }
    //     else {
    //         var state = dialogueStatesDict[dialogue.dialogueId];
    //         state.dialogue = dialogue;
    //         dialogue.persistConvoId = state.persistConvoId;
    //         dialogueStatesDict[dialogue.dialogueId] = state;
    //     }
    // }
    
    // public void Serialize() {
    //     // _dialogueStates = dialogueStatesDict.Values.ToArray();
    //     for(int i = 0; i < _dialogueStates.Length; ++i) {
    //         if(string.IsNullOrEmpty(_dialogueStates[i].dialogueId)) continue;
    //         var state = _dialogueStates[i];
    //         state.persistConvoId = _dialogueStates[i].dialogue.persistConvoId;
    //         _dialogueStates[i] = state;
    //     }
    // }

    // public void Deserialize() {
    //     dialogueStatesDict = new Dictionary<string, DialogueState>();
    //     foreach(var i in _dialogueStates)
    //         dialogueStatesDict.Add(i.dialogueId, i);
    // }
}
}