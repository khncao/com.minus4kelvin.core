# Core
Core gameplay systems and utilities that can be widely used across 3D projects.  Includes inventory system, characters, interaction, progression, UI, and utilities. Runtime building system, AI, serialization, damage system, and other systems are in standalone packages.

### Dependencies & Unity Version
- TextMeshPro
- Cinemachine
- Timeline
- Tested working on Unity 2020.3.6f1+

### Installation
- Add Git URL(https://github.com/khncao/com.minus4kelvin.core.git) through Unity Package Manager

### Running Goals
- Refine framework features and optimize performance
- Further isolation of gameplay elements with defines

### Inventory
Item scriptableobject class serves as basis for many item-like uses such as consumables, equipment, buildables, etc.  
Includes singleton manager class, UI display classes, rated item tier table, and simple crafting implementation.
### Characters
Includes various elements of a 3D mecanim character such as control, animation, IK, equipment, customization, etc. A manager class handles runtime character registration and management. 
### Interaction
Various forms of 3D interaction(hover, click, trigger, visibility, etc). Multiple interactable cycling, UI, manager, etc. 
### Progression
Dialogue, objectives, conditions, storyline, playables, states. States such as completion, dialogue, objectives, etc. are handled by Progression Manager.
### UI
Color picker, message queue, world to screen follow, UI themer, etc.
### Utility
Asset registry database, record keeping, object pooler, material swapper, scene scriptableobject, etc.