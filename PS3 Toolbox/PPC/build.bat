@echo off
ppu-lv2-as.exe PPC\\\\assembly.s
ppu-lv2-objcopy.exe -O binary a.out