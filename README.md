# PS3-Toolbox
Tool for a DEX Playstation 3 System. <br>
This tool allows you to load SPRX modules into game memory without having to modify the eboot to load it.<br>
ProDG and Target Manager are required to use the tool
<br><br>
You can look up error codes here: http://www.psdevwiki.com/ps3/Error_Codes

# Features
- Load and Unload SPRX Modules from games
- View module id, entry address, module size for loaded modules
- Auto detect current game
- Custom system calls
- Custom game function calls

# Explanation
This tool works for all games on the PS3 system. Every game has the header of the loaded elf at 0x10000 with 0x200 bytes before the first<br>
segment of the executable begins. This is where we place our RPC. The RPC is just a thread testing values at an address in memory that is<br>
also empty memory on any game. After installing our RPC, we use TMAPI to pause the main thread, change the PC to our RPC and let it execute.<br>
This main RPC will spawn a thread and start it on the second half of our RPC. We then restore the PC and registers of the main thread so the game<br>
can continue as wanted. We now have a custom thread in the game which we can use to load sprx modules, use system calls and call functions within<br>
the game.

# Example
![Modules Page](https://i.imgur.com/zGpyuUG.png)
![Functions Page](https://i.imgur.com/mKwmmUS.png)

# Credits
- Matrix
- Sabotage
- Sony
