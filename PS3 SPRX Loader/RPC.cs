using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PS3_SPRX_Loader {
    public class RPC {
        private static TMAPI PS3;

        private const uint ENABLE_ADDR =        0x10060000;
        private const uint MODULE_IDS_ADDR =    0x10060004;
        private const uint MODULE_ERROR_ADDR =  0x10060008;
        private const uint PLUGIN_PATH_ADDR =   0x10060010;
        private const uint PLUGIN_TABLE =       0x10060040;

        public static bool RPC_ENABLED = false;
        private static uint RPC_INSTALL_ADDR = 0x0;
        private static byte[] RESTORE_BYTES = new byte[0x13C];

        private static byte[] RPC_BYTES = new byte[] {
            0xF8, 0x21, 0xFF, 0x91, //stdu r1, -0x70(r1)
            0x7C, 0x08, 0x02, 0xA6, //mflr r0
            0xF8, 0x01, 0x00, 0x80, //std r0, 0x80(r1)
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x80, 0x64, 0x00, 0x00, //lwz %r3, 0(%r4)

            //LOAD MODULE CHECK
            0x2C, 0x03, 0x00, 0x01, //cmpwi %r3, 1
            0x40, 0x82, 0x00, 0x78, //bne 0x78

            //SYS_PRX_LOAD_MODULE
            0x38, 0x60, 0x00, 0x00, //li %r3, 0x0
            0x90, 0x64, 0x00, 0x00, //stw %r3, 0(%r4)
            0x3C, 0x60, 0x10, 0x06, //lis %r3, 0x1006
            0x60, 0x63, 0x00, 0x10, //ori %r3, %r3, 0x10
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0x39, 0x60, 0x01, 0xE0, //li %r11, 0x1E0
            0x44, 0x00, 0x00, 0x02, //sc                        #SYS_PRX_LOAD_MODULE
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x00, 0xE4, //blt 0xE4

            //SYS_PRX_START_MODULE
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x90, 0x64, 0x00, 0x04, //stw %r3, 0x4(%r4)
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0x3C, 0xC0, 0x10, 0x05, //lis %r6, 0x1005
            0x60, 0xC6, 0xFF, 0xF0, //ori %r6, %r6, 0xFFF0
            0x38, 0xE0, 0x00, 0x00, //li %r7, 0
            0x39, 0x00, 0x00, 0x00, //li %r8, 0
            0x3D, 0x20, 0x02, 0x4C, //lis %r9, 0x24C
            0x61, 0x29, 0x08, 0xB8, //ori %r9, %r9, 0x8B8
            0xF8, 0x41, 0x00, 0x28, //std %r2, 0x28(%r1)
            0x81, 0x49, 0x00, 0x00, //lwz %r10, 0(%r9)
            0x80, 0x49, 0x00, 0x04, //lwz %r2, 4(%r9
            0x7D, 0x49, 0x03, 0xA6, //mtctr %r10
            0x4E, 0x80, 0x04, 0x21, //bctrl                     #SYS_PRX_START_MODULE
            0xE8, 0x41, 0x00, 0x28, //ld %r2, 0x28(%r1)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x00, 0x9C, //blt 0x9C
            0x48, 0x00, 0x00, 0x94, //b 0x94

            //UNLOAD MODULE CHECK
            0x2C, 0x03, 0x00, 0x02, //cmpwi %r3, 2
            0x40, 0x82, 0x00, 0x98, //bne 0x98

            //SYS_PRX_STOP_MODULE
            0x38, 0x60, 0x00, 0x00, //li %r3, 0x0
            0x90, 0x64, 0x00, 0x00, //stw %r3, 0(%r4)
            0x3C, 0x60, 0x10, 0x06, //lis %r3, 0x1006
            0x80, 0x63, 0x00, 0x04, //lwz %r3, 0x4(%r3)
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0x3C, 0xC0, 0x10, 0x05, //lis %r6, 0x1005
            0x60, 0xC6, 0xFF, 0xF0, //ori %r6, %r6, 0xFFF0
            0x38, 0xE0, 0x00, 0x00, //li %r7, 0
            0x39, 0x00, 0x00, 0x00, //li %r8, 0
            0x3D, 0x20, 0x02, 0x4C, //lis %r9, 0x24C
            0x61, 0x29, 0x08, 0x50, //ori %r9, %r9, 0x850
            0xF8, 0x41, 0x00, 0x28, //std %r2, 0x28(%r1)
            0x81, 0x49, 0x00, 0x00, //lwz %r10, 0(%r9)
            0x80, 0x49, 0x00, 0x04, //lwz %r2, 4(%r9)
            0x7D, 0x49, 0x03, 0xA6, //mtctr %r10
            0x4E, 0x80, 0x04, 0x21, //bctrl                     #SYS_PRX_STOP_MODULE
            0xE8, 0x41, 0x00, 0x28, //ld %r2, 0x28(%r1)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x00, 0x40, //blt 0x40

            //SYS_PRX_UNLOAD_MODULE
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x80, 0x64, 0x00, 0x04, //lwz %r3, 0x4(%r4)
            0x38, 0x80, 0x00, 0x00, //li %r4, 0
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0x3D, 0x20, 0x02, 0x4C, //lis %r9, 0x24C
            0x61, 0x29, 0x08, 0xB0, //ori %r9, %r9, 0x8B0
            0xF8, 0x41, 0x00, 0x28, //std %r2, 0x28(%r1)
            0x81, 0x49, 0x00, 0x00, //lwz %r10, 0(%r9)
            0x80, 0x49, 0x00, 0x04, //lwz %r2, 4(%r9)
            0x7D, 0x49, 0x03, 0xA6, //mtctr %r10
            0x4E, 0x80, 0x04, 0x21, //bctrl                     #SYS_PRX_UNLOAD_MODULE
            0xE8, 0x41, 0x00, 0x28, //ld %r2, 0x28(%r1)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x80, 0x00, 0x08, //blt 0x8

            //ERROR REPORTING
            0x38, 0x60, 0x00, 0x00, //li %r3, 0x0
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x90, 0x64, 0x00, 0x08, //stw %r3, 0x8(%r4)

            //FIX STACK AND RETURN
            0xE8, 0x01, 0x00, 0x80, //ld %r0, 0x80(%r1)
            0x7C, 0x08, 0x03, 0xA6, //mtlr %r0
            0x38, 0x21, 0x00, 0x70, //addi %r1, %r1, 0x70
            0x4E, 0x80, 0x00, 0x20 //blr
        };

        public RPC(TMAPI PS3) {
            RPC.PS3 = PS3;
        }

        private static void UpdateRPC(uint address) {
            if(address == RPC_ADDRESS.MW2) {
                RPC_BYTES[0x66] = 0x02;
                RPC_BYTES[0x67] = 0x4C;

                RPC_BYTES[0xC2] = 0x02;
                RPC_BYTES[0xC3] = 0x4C;

                RPC_BYTES[0xFA] = 0x02;
                RPC_BYTES[0xFB] = 0x4C;
            }

            else if (address == RPC_ADDRESS.MW3) {
                RPC_BYTES[0x66] = 0x02;
                RPC_BYTES[0x67] = 0x2C;

                RPC_BYTES[0xC2] = 0x02;
                RPC_BYTES[0xC3] = 0x2C;

                RPC_BYTES[0xFA] = 0x02;
                RPC_BYTES[0xFB] = 0x2C;
            }

            else if(address == RPC_ADDRESS.BO2) {
                RPC_BYTES[0x66] = 0x02;
                RPC_BYTES[0x67] = 0xFD;

                RPC_BYTES[0xC2] = 0x02;
                RPC_BYTES[0xC3] = 0xFD;

                RPC_BYTES[0xFA] = 0x02;
                RPC_BYTES[0xFB] = 0xFD;
            }
        }

        public static bool Enable(string game) {
            if (!RPC_ENABLED) {
                RPC_INSTALL_ADDR = RPC_ADDRESS.GetAddress(game);
                if (RPC_INSTALL_ADDR != 0x0) {
                    UpdateRPC(RPC_INSTALL_ADDR);
               
                    PS3.GetMemory(RPC_INSTALL_ADDR, RESTORE_BYTES);

                    PS3.Ext.WriteUInt32(ENABLE_ADDR, 0x0);
                    PS3.SetMemory(RPC_INSTALL_ADDR, RPC_BYTES);

                    RPC_ENABLED = true;
                    return true;
                }
                return false;
            }
            return true;
        }

        public static void Disable() {
            if(RPC_ENABLED) {
                PS3.Ext.WriteUInt32(ENABLE_ADDR, 0x0);
                PS3.SetMemory(RPC_INSTALL_ADDR, RESTORE_BYTES);

                RPC_ENABLED = false;
            }
        }

        public static uint LoadModule(string path) {
            if(RPC_ENABLED) {
                PS3.Ext.WriteString(PLUGIN_PATH_ADDR, path);
                PS3.Ext.WriteUInt32(ENABLE_ADDR, 0x1);

                Thread.Sleep(100);

                return PS3.Ext.ReadUInt32(MODULE_ERROR_ADDR);
            }

            return 0x0;
        }

        public static uint UnloadModule(uint prxId) {
            if (RPC_ENABLED) {
                PS3.Ext.WriteUInt32(MODULE_IDS_ADDR, prxId);
                PS3.Ext.WriteUInt32(ENABLE_ADDR, 0x2);

                Thread.Sleep(100);

                return PS3.Ext.ReadUInt32(MODULE_ERROR_ADDR);
            }
            return 0x0;
        }

        public static uint[] GetModules() {
            return PS3.ModuleIds();
        }
    }

    public static class RPC_ADDRESS {
        public const uint COD4 = 0x0;
        public const uint WAW = 0x0;
        public const uint MW2 = 0x38EDE8;
        public const uint BO1 = 0x0;
        public const uint MW3 = 0x3BC990;
        public const uint BO2 = 0x7AA050;
        public const uint GHOST = 0x0;
        public const uint AW = 0x0;

        public static uint GetAddress(string game) {
            if (game.Equals("MW2"))
                return MW2;

            if (game.Equals("MW3"))
                return MW3;

            if (game.Equals("BO2"))
                return BO2;

            return 0x0;
        }
    }
}
