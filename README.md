# Shadster's Avatar Tools [Download](https://github.com/Shadsterwolf/ShadsterAvatarTools/releases)
Shadsterwolf's own avatar tools, mostly used for personal use and subject to change. <br />
<b>Intended for Unity Editor to support VRChat Avatar models</b>
- Please note this is still under-developed
- Backup at least a prefab of your avatar, use at your own risk!!!
- Theres missing feedback, but once the button is pressed the code executes. If the console shows no errors, it probably worked.

# Instructions
1. With a project open with VRCSDK (from VRChat Content Creator)
2. Import my provided released package
3. In Unity, on your toolbar, click "ShadsterWolf" > "Shadster Tools"

(Pro-tip, use Alt+S to bring up my tool menu, S again for Main tool window, G for Gogo Loco Setup, F for FX Setup)

# Features
- Quick Sceneview and Playmode speed up toggles
- Quick settings of mesh boundaries and anchor probes
- Quick setup for Gogo Loco (after import): https://franadavrc.gumroad.com/l/gogoloco
- Quick export preparation of Prefab backup and clearing Blueprint ID 
- Quick Physbone setup
- Quick Generation of Animation Renders and Shapekeys
- Quick Toggle and BlendTree setup
- Quick Gogo Loco Setup

# Functions

Gogo Loco:
- Setup Prefab - Copies "Beyond Prefab" to Avatar hierarchy and assigns constraint source to head
- Setup Layers - Updates Avatar Descriptor's Playable layers except FX and unchecks "Force Locomotion animations for 6 point tracking
- Setup Menu - Adds Gogo Loco menu to your main menu
- Setup Params - Adds Gogo's parameters to your VRC parameters (and deletes the old previous version ones too)
- Setup FX - Copies the two fly layers over to your current FX layer (adds the controller parameters too) 

# Credits
Made by ShadsterWolf

Some code inspired by the VRCSDK, PumkinsAvatarTools, and Av3Creator
