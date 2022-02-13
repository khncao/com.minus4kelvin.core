# Core
Still prone to breaking changes. Core gameplay systems and utilities that can be widely used across 3D projects. Includes inventory system, characters, interaction, progression, UI, and utilities. Runtime building system, AI, serialization, damage system, and other systems are in standalone packages.

### Dependencies & Unity Version
- TextMeshPro
- Cinemachine
- Timeline
- (Required tentative)(SerializeReferenceExtensions)[https://github.com/mackysoft/Unity-SerializeReferenceExtensions] for automatic SerializableReference editability. May phase out if implement own custom editor for conditions.
- Tested working on Unity 2021.2+

### Installation
- Add Git URL package through Package Manager
- or, clone and add as local package

### Todo
- Character system improvements
  - traits; integration with dialogue and AI
  - improved customization and random generation
  - character stat system
- Further isolation of key functionality to distinct groups
- More inventory types: weight, grid

### Inventory
Item scriptableobject class serves as basis for many item-like uses such as consumables, equipment, buildables, etc.  
Includes singleton manager class, UI display classes, rated item tier table, and simple crafting implementation.
### Characters
Includes various elements of a character such as control, 3D IK, loadout, customization, etc. A manager class handles runtime character registration and management. 
### Interaction
Various forms of 3D interaction(hover, click, trigger, visibility, etc). Multiple interactable cycling, UI, manager, etc. 
### Progression
Todo: xNode optional integration, character features(affinity, storyline), better import/export text-scriptableobject
Dialogue, objectives, conditions, playables, key states, etc. Aims for ease of writing extensive narratives with some branching capability. WIP
### UI
Color picker, message queue, world to screen follow, UI themer, etc.
### Utility
Asset registry database, record keeping, object pooler, material swapper, scene utilities, etc.