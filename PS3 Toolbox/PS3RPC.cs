using System;
using System.Threading;

namespace PS3_SPRX_Loader {
    class PS3RPC {
        private static TMAPI PS3;

        private const uint RPC_BASE = 0x10040;
        private const uint INSTALL_ADDR = RPC_BASE + 0x1C;
        private const uint RPC_THREAD_NAME_ADDR = RPC_BASE + 0x8;
        private const uint RPC_STACK_ADDR_PTR = RPC_BASE + 0xC;
        private const string RPC_THREAD_NAME = "RPC";
        private static uint RPC_STACK_ADDR = 0x0;

        //System Calls RPC Offsets
        private const uint SYS_CALL_ID = 0x20;
        private const uint SYS_CALL_R3 = 0x28;
        private const uint SYS_CALL_R4 = 0x30;
        private const uint SYS_CALL_R5 = 0x38;
        private const uint SYS_CALL_R6 = 0x40;
        private const uint SYS_CALL_R7 = 0x48;
        private const uint SYS_CALL_R8 = 0x50;
        private const uint SYS_CALL_R9 = 0x58;
        private const uint SYS_CALL_R10 = 0x60;
        private const uint SYS_CALL_RET = 0x70;

        //Function Calls RPC Offsets
        private const uint FUNC_CALL_ADDR = 0x80;
        private const uint FUNC_CALL_RX = 0x88;
        private const uint FUNC_CALL_FX = 0xD0;
        private const uint FUNC_CALL_RX_RET = 0x118;
        private const uint FUNC_CALL_FX_RET = 0x120;
        private const uint FUNC_CALL_STR = 0x800;
        private const uint FUNC_CALL_FLOATS = 0xC00;
        private const uint FUNC_CALL_TOC_SAVE = 0x128;
        private const uint FUNC_CALL_TOC_NEW = 0x130;

        //SPRX Loading Offsets
        private const uint PRX_MODULE_PATH = 0x240;
        private const uint PRX_FLAGS = 0x200;

        private static byte[] RPC_INSTRUCTIONS = new byte[] {
            //RPC Setup Instructions
            0x3C, 0x60, 0x00, 0x01, //lis %r3, 0x1
            0x60, 0x63, 0x00, 0xC8, //ori %r3, %r3, 0xC8                    //RPC Thread Entry Point (0x100C8)
            0x3C, 0x80, 0x00, 0x01, //lis %r4, 0x1
            0x60, 0x84, 0x00, 0x50, //ori %r4, %r4, 0x50                    //RPC Thread OPD
            0x90, 0x64, 0x00, 0x00, //stw %r3, 0x0(%r4)                     //Add entry point to OPD
            0x90, 0x44, 0x00, 0x04, //stw %r2, 0x4(%r4)                     //Add TOC to OPD
            0x90, 0x84, 0x00, 0x08, //stw %r4, 0x8(%r4)                     //Write OPD pointer (0x10058)
            0x3C, 0x60, 0x00, 0x01, //lis %r3, 0x1
            0x60, 0x63, 0x00, 0x40, //ori %r3, %r3, 0x40
            0x3C, 0x80, 0x00, 0x01, //lis %r4, 0x1
            0x60, 0x84, 0x00, 0x58, //ori %r4, %r4, 0x58
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0x38, 0xC0, 0x00, 0x00, //li %r6, 0
            0x38, 0xE0, 0x03, 0x00, //li %r7, 0x300
            0x39, 0x00, 0x40, 0x00, //li %r8, 0x4000
            0x39, 0x20, 0x00, 0x00, //li %r9, 0
            0x3D, 0x40, 0x00, 0x01, //lis %r10, 0x1
            0x61, 0x4A, 0x00, 0x48, //ori %r10, %r10, 0x48
            0x39, 0x60, 0x00, 0x34, //li %r11, 0x34
            0x44, 0x00, 0x00, 0x02, //;sc
            0x3C, 0x60, 0x00, 0x01, //lis %r3, 0x1
            0xE8, 0x63, 0x00, 0x40, //ld %r3, 0x40(%r3)
            0x39, 0x60, 0x00, 0x35, //li %r11, 0x35
            0x44, 0x00, 0x00, 0x02, //;sc
            0x60, 0x00, 0x00, 0x00, //nop
            0x60, 0x00, 0x00, 0x00, //nop
            0x4B, 0xFF, 0xFF, 0xF8, //b -0x8

            //RPC Thread Start
            0x3C, 0x60, 0x00, 0x01, //lis %r3, 0x1
            0x7C, 0x24, 0x0B, 0x78, //mr %r4, %r1
            0x38, 0x84, 0xF0, 0x00, //addi %r4, %r4, -0x1000
            0x90, 0x83, 0x00, 0x4C, //stw %r4, 0x4C(%r3)
            0xF8, 0x21, 0xF0, 0x01, //stdu %r1, -0x1000(%r1)

            //System Calls RPC Instructions
            0x80, 0x61, 0x00, 0x20, //lwz %r3, 0x20(%r1)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x82, 0x00, 0x38, //beq 0x38
            0x7C, 0x6B, 0x1B, 0x78, //mr %r11, %r3
            0x38, 0x60, 0x00, 0x00, //li %r3, 0
            0x90, 0x61, 0x00, 0x20, //stw %r3, 0x20(%r1)
            0xE8, 0x61, 0x00, 0x28, //ld %r3, 0x28(%r1)
            0xE8, 0x81, 0x00, 0x30, //ld %r4, 0x30(%r1)
            0xE8, 0xA1, 0x00, 0x38, //ld %r5, 0x38(%r1)
            0xE8, 0xC1, 0x00, 0x40, //ld %r6, 0x40(%r1)
            0xE8, 0xE1, 0x00, 0x48, //ld %r7, 0x48(%r1)
            0xE9, 0x01, 0x00, 0x50, //ld %r8, 0x50(%r1)
            0xE9, 0x21, 0x00, 0x58, //ld %r9, 0x58(%r1)
            0xE9, 0x41, 0x00, 0x60, //ld %r10, 0x60(%r1)
            0x44, 0x00, 0x00, 0x02, //;sc
            0xF8, 0x61, 0x00, 0x70, //std %r3, 0x70(%r1)

            //Function Calls RPC Instructions
            0x81, 0x81, 0x00, 0x80, //lwz %r12, 0x80(%r1)
            0x2C, 0x0C, 0x00, 0x00, //cmpwi %r12, 0
            0x41, 0x82, 0x00, 0x80, //beq 0x80
            0xE8, 0x61, 0x00, 0x88, //ld %r3, 0x88(%r1)
            0xE8, 0x81, 0x00, 0x90, //ld %r4, 0x90(%r1)
            0xE8, 0xA1, 0x00, 0x98, //ld %r5, 0x98(%r1)
            0xE8, 0xC1, 0x00, 0xA0, //ld %r6, 0xA0(%r1)
            0xE8, 0xE1, 0x00, 0xA8, //ld %r7, 0xA8(%r1)
            0xE9, 0x01, 0x00, 0xB0, //ld %r8, 0xB0(%r1)
            0xE9, 0x21, 0x00, 0xB8, //ld %r9, 0xB8(%r1)
            0xE9, 0x41, 0x00, 0xC0, //ld %r10, 0xC0(%r1)
            0xE9, 0x61, 0x00, 0xC8, //ld %r11, 0xC8(%r1)
            0xC0, 0x21, 0x00, 0xD0, //lfs %f1, 0xD0(%r1)
            0xC0, 0x41, 0x00, 0xD8, //lfs %f2, 0xD8(%r1)
            0xC0, 0x61, 0x00, 0xE0, //lfs %f3, 0xE0(%r1)
            0xC0, 0x81, 0x00, 0xE8, //lfs %f4, 0xE8(%r1)
            0xC0, 0xA1, 0x00, 0xF0, //lfs %f5, 0xF0(%r1)
            0xC0, 0xC1, 0x00, 0xF8, //lfs %f6, 0xF8(%r1)
            0xC0, 0xE1, 0x01, 0x00, //lfs %f7, 0x100(%r1)
            0xC1, 0x01, 0x01, 0x08, //lfs %f8, 0x108(%r1)
            0xC1, 0x21, 0x01, 0x10, //lfs %f9, 0x110(%r1)
            0x7D, 0x89, 0x03, 0xA6, //mtctr %r12
            0xF8, 0x41, 0x01, 0x28, //std %r2, 0x128(%r1)
            0xE9, 0x81, 0x01, 0x30, //ld %r12, 0x130(%r1)
            0x2C, 0x0C, 0x00, 0x00, //cmpwi %r12, 0
            0x41, 0x82, 0x00, 0x08, //beq 0x8
            0x7D, 0x82, 0x63, 0x78, //mr %r2, %r12
            0x4E, 0x80, 0x04, 0x21, //bctrl
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0xE8, 0x41, 0x01, 0x28, //ld %r2, 0x128(%r1)
            0xF8, 0xA1, 0x01, 0x30, //std %r5, 0x130(%r1)
            0x90, 0xA1, 0x00, 0x80, //stw %r5, 0x80(%r1)
            0xF8, 0x61, 0x01, 0x18, //std %r3, 0x118(%r1)
            0xD0, 0x21, 0x01, 0x20, //stfs %f1, 0x120(%r1)
            0x38, 0x21, 0x10, 0x00, //addi %r1, %r1, 0x1000
            0x38, 0x60, 0x00, 0x64, //li %r3, 0x64
            0x39, 0x60, 0x00, 0x8D, //li %r11, 0x08D
            0x44, 0x00, 0x00, 0x02, //;sc
            0x4B, 0xFF, 0xFF, 0x24  //b -0xDC
        };

        public PS3RPC(TMAPI PS3) {
            PS3RPC.PS3 = PS3;
        }

        public static uint[] GetModules() {
            return PS3.ModuleIds();
        }

        public static bool Install() {
            try {
                if (PS3.Ext.ReadUInt64(INSTALL_ADDR) == 0x3C600001606300C8)
                {
                    RPC_STACK_ADDR = PS3.Ext.ReadUInt32 (RPC_STACK_ADDR_PTR);
                    return true;
                }

                PS3.SetMemory(RPC_BASE, new byte[RPC_INSTRUCTIONS.Length]);

                ulong PC = 0;
                ulong[] Registers = new ulong[0x49];

                PS3.Ext.WriteString(RPC_THREAD_NAME_ADDR, RPC_THREAD_NAME);
                PS3.SetMemory(INSTALL_ADDR, RPC_INSTRUCTIONS);

                PS3.MainThreadStop();
                
                for (uint i = 0; i < 0x49; i++) {
                    Registers[i] = PS3.GetSingleRegister(i);
                }
                
                PC = PS3.GetSingleRegister((uint)TMAPI.SPRegisters.SNPS3_pc);
                PS3.SetSingleRegister((uint)TMAPI.SPRegisters.SNPS3_pc, INSTALL_ADDR);
                
                PS3.MainThreadContinue();
                
                while (PS3.Ext.ReadUInt64(INSTALL_ADDR) == 0)
                    Thread.Sleep(1);
                
                PS3.MainThreadStop();
                
                for (uint i = 0; i < 0x49; i++) {
                    PS3.SetSingleRegister(i, Registers[i]);
                }
                
                PS3.SetSingleRegister((uint)TMAPI.SPRegisters.SNPS3_pc, PC);
                PS3.MainThreadContinue();
                
                PS3TMAPI.PPUThreadInfo ThreadInfo = new PS3TMAPI.PPUThreadInfo();
                
                if (PS3.GetThreadByName("RPC", ref ThreadInfo)) {
                    PS3.StopThreadyID(ThreadInfo.ThreadID);
                    PS3.SetSingleRegisterByThreadID(ThreadInfo.ThreadID, (uint)TMAPI.GPRegisters.SNPS3_gpr_13, Registers[13]);
                    PS3.ContinueThreadByID(ThreadInfo.ThreadID);
                }
                
                while (PS3.Ext.ReadUInt32(RPC_STACK_ADDR_PTR) == 0)
                    Thread.Sleep(1);
                
                RPC_STACK_ADDR = PS3.Ext.ReadUInt32(RPC_STACK_ADDR_PTR);

                return true;
            }
            catch {
                return false;
            }
        }

        public static ulong SystemCall(uint ID, ulong r3 = 0, ulong r4 = 0, ulong r5 = 0, ulong r6 = 0, ulong r7 = 0, ulong r8 = 0, ulong r9 = 0, ulong r10 = 0)
        {
            if (!PS3RPC.Install())
                return 0;

            ulong ret = 0;

            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_RET, 0xDEADC0DE);

            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_R3, r3);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_R4, r4);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_R5, r5);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_R6, r6);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_R7, r7);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_R8, r8);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_R9, r9);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + SYS_CALL_R10, r10);

            PS3.Ext.WriteUInt32(RPC_STACK_ADDR + SYS_CALL_ID, ID);

            while ((ret = PS3.Ext.ReadUInt64(RPC_STACK_ADDR + SYS_CALL_RET)) == 0xDEADC0DE)
                Thread.Sleep(1);

            return ret;
        }

        public static T CallTOC<T>(uint address, uint toc, params object[] parameters) {
            if (!PS3RPC.Install())
                return (T)Convert.ChangeType(0, typeof(int));

            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + FUNC_CALL_RX_RET, 0xDEADC0DE);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + FUNC_CALL_FX_RET, 0xDEADC0DE);

            int index = 0;
            uint intCount = 0;
            uint floatCount = 0;
            uint stringOffset = 0;
            uint floatArrayCount = 0;

            while (index < parameters.Length) {
                if (parameters[index] is int || parameters[index] is uint || parameters[index] is long || parameters[index] is ulong)
                {
                    PS3.Ext.WriteUInt64(RPC_STACK_ADDR + FUNC_CALL_RX + (intCount++ * 0x8), Convert.ToUInt64(parameters[index]));
                }
                else if (parameters[index] is float)
                {
                    PS3.Ext.WriteFloat(RPC_STACK_ADDR + FUNC_CALL_FX + (floatCount++ * 0x8), (float)parameters[index]);
                }
                else if (parameters[index] is double)
                {
                    PS3.Ext.WriteDouble(RPC_STACK_ADDR + FUNC_CALL_FX + (floatCount++ * 0x8), (double)parameters[index]);
                }
                else if (parameters[index] is float[])
                {
                    PS3.Ext.WriteUInt64(RPC_STACK_ADDR + FUNC_CALL_RX + (intCount++ * 0x8), RPC_STACK_ADDR + FUNC_CALL_FLOATS + (floatArrayCount * 0x4));
                    PS3.Ext.WriteFloats(RPC_STACK_ADDR + FUNC_CALL_FLOATS + (floatArrayCount * 0x4), (float[])parameters[index]);
                    floatArrayCount += (uint)((float[])parameters[index]).Length + 0x4;
                }
                else if (parameters[index] is string)
                {
                    string value = (string)parameters[index];
                    PS3.Ext.WriteString(RPC_STACK_ADDR + FUNC_CALL_STR + stringOffset, value);
                    PS3.Ext.WriteUInt64(RPC_STACK_ADDR + FUNC_CALL_RX + (intCount++ * 0x8), RPC_STACK_ADDR + FUNC_CALL_STR + stringOffset);
                    stringOffset += (uint)value.Length + 0x4;
                }
                index++;
            }

            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + FUNC_CALL_TOC_NEW, toc);
            PS3.Ext.WriteUInt32(RPC_STACK_ADDR + FUNC_CALL_ADDR, address);

            Type type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if(type == typeof(float)) {
                float ret = 0.0f;
                while ((ret = PS3.Ext.ReadFloat(RPC_STACK_ADDR + FUNC_CALL_FX_RET)) == 0xDEADC0DE)
                    Thread.Sleep(10);

                return (T)Convert.ChangeType(ret, typeof(T));
            }
            else if(type == typeof(string)) {
                ulong ret = 0;
                while ((ret = PS3.Ext.ReadUInt64(RPC_STACK_ADDR + FUNC_CALL_RX_RET)) == 0xDEADC0DE)
                    Thread.Sleep(10);
                string value = PS3.Ext.ReadString((uint)ret);

                return (T)Convert.ChangeType(value, typeof(T));
            }
            else {
                ulong ret = 0;
                while ((ret = PS3.Ext.ReadUInt64(RPC_STACK_ADDR + FUNC_CALL_RX_RET)) == 0xDEADC0DE)
                    Thread.Sleep(10);

                return (T)Convert.ChangeType(ret, typeof(T));
            }
        }

        public static T Call<T>(uint address, params object[] parameters) {
            return CallTOC<T>(address, 0x0, parameters);
        }

        public static ulong LoadModule(string path)
        {
            if (!PS3RPC.Install())
                return 0;

            ulong ret = 0;
            PS3.Ext.WriteString(RPC_STACK_ADDR + PRX_MODULE_PATH, path);

            ulong prxId = SystemCall(0x1E0, RPC_STACK_ADDR + PRX_MODULE_PATH, 0, 0);
            if ((long)prxId < 0)
                return prxId;

            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS, 0x28);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x8, 0x1);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x10, 0x0);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x18, 0x0);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x20, UInt64.MaxValue);
            ret = SystemCall(0x1E1, prxId, 0x0, RPC_STACK_ADDR + PRX_FLAGS);
            if ((long)ret < 0)
                return ret;

            ulong OPD = PS3.Ext.ReadUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x10);
            uint address = PS3.Ext.ReadUInt32((uint)OPD);
            uint toc = PS3.Ext.ReadUInt32((uint)OPD + 0x4);
            CallTOC<int>(address, toc);

            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS, 0x28);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x8, 0x2);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x10, 0x0);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x18, 0x0);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x20, UInt64.MaxValue);
            ret = SystemCall(0x1E1, prxId, 0x0, RPC_STACK_ADDR + PRX_FLAGS);
            if ((long)ret < 0)
                return ret;

            return 0x0;
        }

        public static ulong UnloadModule(uint prxId)
        {
            if (!PS3RPC.Install())
                return 0;

            ulong ret = 0;

            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS, 0x28);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x8, 0x1);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x10, 0x0);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x18, 0x0);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x20, UInt64.MaxValue);
            ret = SystemCall(0x1E2, prxId, 0x0, RPC_STACK_ADDR + PRX_FLAGS);

            if ((long)ret < 0)
                return ret;

            ulong OPD = PS3.Ext.ReadUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x10);
            uint address = PS3.Ext.ReadUInt32((uint)OPD);
            uint toc = PS3.Ext.ReadUInt32((uint)OPD + 0x4);
            CallTOC<int>(address, toc);

            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS, 0x28);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x8, 0x2);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x10, 0x0);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x18, 0x0);
            PS3.Ext.WriteUInt64(RPC_STACK_ADDR + PRX_FLAGS + 0x20, UInt64.MaxValue);
            ret = SystemCall(0x1E2, prxId, 0x0, RPC_STACK_ADDR + PRX_FLAGS);
            if ((long)ret < 0)
                return ret;

            ret = SystemCall(0x1E3, prxId, 0, 0, 0);
            if ((long)ret < 0)
                return ret;

            return 0x0;
        }
    }
}
