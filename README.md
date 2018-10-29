# PS3-SPRX-Loader
Tool for a DEX Playstation 3 System. <br>
This tool allows you to load SPRX modules into game memory without having to modify the eboot to load it.<br>
ProDG and Target Manager are required to use the tool
<br><br>
This tool uses a games menu drawing function to load/unload the modules<br>
This is why when you load and unload your screen goes black/freezes for a second
<br><br>
You can look up error codes here: http://www.psdevwiki.com/ps3/Error_Codes
<br>
If you get 0x80010004 (out of memory) you can unload cellHttp_Library to allow space for your SPRX<br>
Just make sure if you are going to unload this library you are already signed in and online with the game loaded

# Features
- Load and Unload SPRX Modules from games
- View moduleId, entry address, module size for loaded modules

# Supported Games
- Modern Warfare 2 (1.14)
- Modern Warfare 3 (1.24)
- Black Ops 2 (1.19)

# Example
[![PS3 SPRX Loader Example](https://img.youtube.com/vi/9yjhm56ddSY/0.jpg)](https://www.youtube.com/watch?v=9yjhm56ddSY "PS3 SPRX Loader Example")

# Features being added
- All COD support
- Game function calls

# Credits
- Partially used PS3Lib (iMCSx)'s implementation of PS3TMAPI_NET.dll
