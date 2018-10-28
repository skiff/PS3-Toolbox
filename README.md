# PS3-SPRX-Loader
Tool for a DEX Playstation 3 System. <br>
This tool allows you to load SPRX modules into game memory without having to modify the eboot to load it.<br>
ProDG and Target Manager are required to use the tool<br>
<br>
This tool uses the games' R_SetFrameFog function on cod games to load the modules<br>
If your plugin uses this address, you may experience issues after loading

# Features
- Load and Unload SPRX Modules from games
- View moduleId, entry address, module size for loaded modules

# Supported Games
- Modern Warfare 2 (1.14)
- Black Ops 2 (1.19)

# Features being added
- All COD support
- Game function calls

# Credits
- Partially used PS3Lib (iMCSx)'s implementation of PS3TMAPI_NET.dll
