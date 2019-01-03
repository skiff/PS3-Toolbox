using System;
using System.Threading;

namespace PS3_SPRX_Loader {
    class PS3RPC {
        private static TMAPI PS3;

        private const uint INSTALL_ADDR = 0x10000;
        private const uint RPC_CREATION = 0x10050000;
        private const uint RPC_THREAD_NAME_ADDR = 0x10050050;
        private const string RPC_THREAD_NAME = "RPC Thread";

        //System Calls RPC
        private const uint SYS_CALL_ID = 0x10050100;
        private const uint SYS_CALL_R3 = 0x10050108;
        private const uint SYS_CALL_R4 = 0x10050110;
        private const uint SYS_CALL_R5 = 0x10050118;
        private const uint SYS_CALL_R6 = 0x10050120;
        private const uint SYS_CALL_R7 = 0x10050128;
        private const uint SYS_CALL_R8 = 0x10050130;
        private const uint SYS_CALL_R9 = 0x10050138;
        private const uint SYS_CALL_R10 = 0x10050140;
        private const uint SYS_CALL_RET = 0x10050148;

        //Function Calls RPC
        private const uint FUNC_CALL_ADDR = 0x10060090;
        private const uint FUNC_CALL_RX = 0x10060000;
        private const uint FUNC_CALL_FX = 0x10060048;
        private const uint FUNC_CALL_RX_RET = 0x10060098;
        private const uint FUNC_CALL_FX_RET = 0x100600A0;
        private const uint FUNC_CALL_STR = 0x10060200;
        private const uint FUNC_CALL_TOC_SAVE = 0x100600A8;
        private const uint FUNC_CALL_TOC_NEW = 0x100600B0;

        //SPRX Loading 
        private const uint PRX_MODULE_PATH = 0x10051000;
        private const uint PRX_FLAGS = 0x100510A0;

        private static byte[] RPC_INSTRUCTIONS = new byte[] {
            0x3C, 0x60, 0x00, 0x01, 0x60, 0x63, 0x00, 0x68, 0x3C, 0x80, 0x10, 0x05,
    0x60, 0x84, 0x00, 0x08, 0x90, 0x64, 0x00, 0x00, 0x90, 0x44, 0x00, 0x04,
    0x90, 0x84, 0x00, 0x08, 0x3C, 0x60, 0x10, 0x05, 0x3C, 0x80, 0x10, 0x05,
    0x60, 0x84, 0x00, 0x10, 0x38, 0xA0, 0x00, 0x00, 0x38, 0xC0, 0x00, 0x00,
    0x38, 0xE0, 0x03, 0x00, 0x39, 0x00, 0x30, 0x00, 0x39, 0x20, 0x00, 0x00,
    0x3D, 0x40, 0x10, 0x05, 0x61, 0x4A, 0x00, 0x50, 0x39, 0x60, 0x00, 0x34,
    0x44, 0x00, 0x00, 0x02, 0x3C, 0x60, 0x10, 0x05, 0xE8, 0x63, 0x00, 0x00,
    0x39, 0x60, 0x00, 0x35, 0x44, 0x00, 0x00, 0x02, 0x60, 0x00, 0x00, 0x00,
    0x60, 0x00, 0x00, 0x00, 0x4B, 0xFF, 0xFF, 0xF8, 0x3D, 0x80, 0x10, 0x05,
    0x61, 0x8C, 0x01, 0x00, 0x80, 0x6C, 0x00, 0x00, 0x2C, 0x03, 0x00, 0x00,
    0x41, 0x82, 0x00, 0x40, 0x7C, 0x6B, 0x1B, 0x78, 0x38, 0x60, 0x00, 0x00,
    0x90, 0x6C, 0x00, 0x00, 0xE8, 0x6C, 0x00, 0x08, 0xE8, 0x8C, 0x00, 0x10,
    0xE8, 0xAC, 0x00, 0x18, 0xE8, 0xCC, 0x00, 0x20, 0xE8, 0xEC, 0x00, 0x28,
    0xE9, 0x0C, 0x00, 0x30, 0xE9, 0x2C, 0x00, 0x38, 0xE9, 0x4C, 0x00, 0x40,
    0x44, 0x00, 0x00, 0x02, 0x3D, 0x80, 0x10, 0x05, 0x61, 0x8C, 0x01, 0x00,
    0xF8, 0x6C, 0x00, 0x48, 0x3C, 0x60, 0x10, 0x06, 0x81, 0x83, 0x00, 0x90,
    0x2C, 0x0C, 0x00, 0x00, 0x41, 0x82, 0x00, 0x88, 0xE8, 0x83, 0x00, 0x08,
    0xE8, 0xA3, 0x00, 0x10, 0xE8, 0xC3, 0x00, 0x18, 0xE8, 0xE3, 0x00, 0x20,
    0xE9, 0x03, 0x00, 0x28, 0xE9, 0x23, 0x00, 0x30, 0xE9, 0x43, 0x00, 0x38,
    0xE9, 0x63, 0x00, 0x40, 0xC0, 0x23, 0x00, 0x48, 0xC0, 0x43, 0x00, 0x50,
    0xC0, 0x63, 0x00, 0x58, 0xC0, 0x83, 0x00, 0x60, 0xC0, 0xA3, 0x00, 0x68,
    0xC0, 0xC3, 0x00, 0x70, 0xC0, 0xE3, 0x00, 0x78, 0xC1, 0x03, 0x00, 0x80,
    0xC1, 0x23, 0x00, 0x88, 0xE8, 0x63, 0x00, 0x00, 0x7D, 0x89, 0x03, 0xA6,
    0x3D, 0x80, 0x10, 0x06, 0xF8, 0x4C, 0x00, 0xA8, 0xE9, 0x8C, 0x00, 0xB0,
    0x2C, 0x0C, 0x00, 0x00, 0x41, 0x82, 0x00, 0x08, 0x7D, 0x82, 0x63, 0x78,
    0x4E, 0x80, 0x04, 0x21, 0x3C, 0x80, 0x10, 0x06, 0x38, 0xA0, 0x00, 0x00,
    0xE8, 0x44, 0x00, 0xA8, 0xF8, 0xA4, 0x00, 0xB0, 0x90, 0xA4, 0x00, 0x90,
    0xF8, 0x64, 0x00, 0x98, 0xD0, 0x24, 0x00, 0xA0, 0x38, 0x60, 0x00, 0x64,
    0x39, 0x60, 0x00, 0x8D, 0x44, 0x00, 0x00, 0x02, 0x4B, 0xFF, 0xFF, 0x10
        };

        public PS3RPC(TMAPI PS3) {
            PS3RPC.PS3 = PS3;
        }

        public static uint[] GetModules() {
            return PS3.ModuleIds();
        }

        public static bool Install() {
            try {
                if (PS3.Ext.ReadUInt64(INSTALL_ADDR) == 0x3C60000160630068)
                    return true;

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

                while (PS3.Ext.ReadUInt64(RPC_CREATION) == 0)
                    Thread.Sleep(1);

                PS3.MainThreadStop();

                for (uint i = 0; i < 0x49; i++) {
                    PS3.SetSingleRegister(i, Registers[i]);
                }

                PS3.SetSingleRegister((uint)TMAPI.SPRegisters.SNPS3_pc, PC);
                PS3.MainThreadContinue();

                PS3TMAPI.PPUThreadInfo ThreadInfo = new PS3TMAPI.PPUThreadInfo();

                if (PS3.GetThreadByName(RPC_THREAD_NAME, ref ThreadInfo)) {
                    PS3.StopThreadyID(ThreadInfo.ThreadID);
                    PS3.SetSingleRegisterByThreadID(ThreadInfo.ThreadID, (uint)TMAPI.GPRegisters.SNPS3_gpr_13, Registers[13]);
                    PS3.ContinueThreadByID(ThreadInfo.ThreadID);
                }

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

            PS3.Ext.WriteUInt64(SYS_CALL_RET, 0xDEADC0DE);

            PS3.Ext.WriteUInt64(SYS_CALL_R3, r3);
            PS3.Ext.WriteUInt64(SYS_CALL_R4, r4);
            PS3.Ext.WriteUInt64(SYS_CALL_R5, r5);
            PS3.Ext.WriteUInt64(SYS_CALL_R6, r6);
            PS3.Ext.WriteUInt64(SYS_CALL_R7, r7);
            PS3.Ext.WriteUInt64(SYS_CALL_R8, r8);
            PS3.Ext.WriteUInt64(SYS_CALL_R9, r9);
            PS3.Ext.WriteUInt64(SYS_CALL_R10, r10);

            PS3.Ext.WriteUInt32(SYS_CALL_ID, ID);

            while ((ret = PS3.Ext.ReadUInt64(SYS_CALL_RET)) == 0xDEADC0DE)
                Thread.Sleep(1);

            return ret;
        }

        public static object FunctionCallTOC(uint address, uint toc, params object[] parameters)
        {
            if (!PS3RPC.Install())
                return 0;

            PS3.Ext.WriteUInt64(FUNC_CALL_RX_RET, 0xDEADC0DE);

            int index = 0;
            uint intCount = 0;
            uint floatCount = 0;
            uint stringOffset = 0;

            while (index < parameters.Length) {
                if (parameters[index] is int || parameters[index] is uint || parameters[index] is long || parameters[index] is ulong)
                {
                    PS3.Ext.WriteUInt64(FUNC_CALL_RX + (intCount++ * 0x8), Convert.ToUInt64(parameters[index]));
                }
                else if (parameters[index] is float)
                {
                    PS3.Ext.WriteFloat(FUNC_CALL_FX + (floatCount++ * 0x8), (float)parameters[index]);
                }
                else if (parameters[index] is double)
                {
                    PS3.Ext.WriteDouble(FUNC_CALL_FX + (floatCount++ * 0x8), (double)parameters[index]);
                }
                else if (parameters[index] is float[])
                {
                    PS3.Ext.WriteUInt64(FUNC_CALL_RX + (intCount++ * 0x8), FUNC_CALL_FX + (floatCount * 0x8));
                    PS3.Ext.WriteFloats(FUNC_CALL_FX + (floatCount * 0x8), (float[])parameters[index]);
                    floatCount += (uint)(((float[])parameters[index]).Length * 0x8);
                }
                else if (parameters[index] is string)
                {
                    string value = (string)parameters[index];
                    PS3.Ext.WriteString(FUNC_CALL_STR + stringOffset, value);
                    PS3.Ext.WriteUInt64(FUNC_CALL_RX + (intCount++ * 0x8), FUNC_CALL_STR + stringOffset);
                    stringOffset += (uint)value.Length + 0x4;
                }
                index++;
            }

            PS3.Ext.WriteUInt64(FUNC_CALL_TOC_NEW, toc);
            PS3.Ext.WriteUInt32(FUNC_CALL_ADDR, address);

            ulong uReturn = 0;

            while ((uReturn = PS3.Ext.ReadUInt64(FUNC_CALL_RX_RET)) == 0xDEADC0DE)
                Thread.Sleep(10);

            return uReturn;
        }

        public static object FunctionCall(uint address, params object[] parameters)
        {
            return FunctionCallTOC(address, 0x0, parameters);
        }

        public static ulong LoadModule(string path)
        {
            if (!PS3RPC.Install())
                return 0;

            ulong ret = 0;
            PS3.Ext.WriteString(PRX_MODULE_PATH, path);

            ulong prxId = SystemCall(0x1E0, PRX_MODULE_PATH, 0, 0);
            if ((long)prxId < 0)
                return prxId;

            PS3.Ext.WriteUInt64(PRX_FLAGS, 0x28);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x8, 0x1);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x10, 0x0);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x18, 0x0);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x20, UInt64.MaxValue);
            ret = SystemCall(0x1E1, prxId, 0x0, PRX_FLAGS);
            if ((long)ret < 0)
                return ret;

            ulong OPD = PS3.Ext.ReadUInt64(PRX_FLAGS + 0x10);
            uint address = PS3.Ext.ReadUInt32((uint)OPD);
            uint toc = PS3.Ext.ReadUInt32((uint)OPD + 0x4);
            FunctionCallTOC(address, toc);

            PS3.Ext.WriteUInt64(PRX_FLAGS, 0x28);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x8, 0x2);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x10, 0x0);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x18, 0x0);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x20, UInt64.MaxValue);
            ret = SystemCall(0x1E1, prxId, 0x0, PRX_FLAGS);
            if ((long)ret < 0)
                return ret;

            return 0x0;
        }

        public static ulong UnloadModule(uint prxId)
        {
            if (!PS3RPC.Install())
                return 0;

            ulong ret = 0;

            PS3.Ext.WriteUInt64(PRX_FLAGS, 0x28);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x8, 0x1);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x10, 0x0);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x18, 0x0);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x20, UInt64.MaxValue);
            ret = SystemCall(0x1E2, prxId, 0x0, PRX_FLAGS);

            if ((long)ret < 0)
                return ret;

            ulong OPD = PS3.Ext.ReadUInt64(PRX_FLAGS + 0x10);
            uint address = PS3.Ext.ReadUInt32((uint)OPD);
            uint toc = PS3.Ext.ReadUInt32((uint)OPD + 0x4);
            object x = FunctionCallTOC(address, toc);

            PS3.Ext.WriteUInt64(PRX_FLAGS, 0x28);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x8, 0x2);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x10, 0x0);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x18, 0x0);
            PS3.Ext.WriteUInt64(PRX_FLAGS + 0x20, UInt64.MaxValue);
            ret = SystemCall(0x1E2, prxId, 0x0, PRX_FLAGS);
            if ((long)ret < 0)
                return ret;

            ret = SystemCall(0x1E3, prxId, 0, 0, 0);
            if ((long)ret < 0)
                return ret;

            return 0x0;
        }
    }
}
