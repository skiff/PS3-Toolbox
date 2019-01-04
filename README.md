# PS3-Toolbox
Tool for a DEX Playstation 3 System. <br>
ProDG and Target Manager are required to use the tool<br>
<br>
You can look up error codes here: http://www.psdevwiki.com/ps3/Error_Codes

# Features
- Load and unload SPRX modules from games
- View module id, entry address, module size for loaded modules
- Auto detect current game
- Custom system calls
- Custom game function calls
[Downloads](https://github.com/skiffaw/PS3-Toolbox/releases)

# Explanation
This tool works for all games on the PS3 system. Every game has the header of the loaded elf at 0x10000 with 0x200 bytes before the first
segment of the executable begins. This is where we place our RPC. The RPC is just a thread testing values at an address in memory that is
also empty memory on any game. After installing our RPC, we use TMAPI to pause the main thread, change the PC to our RPC and let it execute.
This main RPC will spawn a thread and start it on the second half of our RPC. We then restore the PC and registers of the main thread so the game
can continue as wanted. We now have a custom thread in the game which we can use to load sprx modules, use system calls and call functions within
the game.

# Example
![Modules Page](https://i.imgur.com/zGpyuUG.png)
![Functions Page](https://i.imgur.com/mKwmmUS.png)

# Credits
[Matrix](https://github.com/skiffaw)<br>
[Sabotage](https://github.com/egatobaS)
