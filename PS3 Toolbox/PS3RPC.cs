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
            0x3C, 0x60, 0x00, 0x01, //lis %r3, 0x1
            0x60, 0x63, 0x00, 0x68, //ori %r3, %r3, 0x68
            0x3C, 0x80, 0x10, 0x05, //lis %r4, 0x1005
            0x60, 0x84, 0x00, 0x08, //ori %r4, %r4, 0x8
            0x90, 0x64, 0x00, 0x00, //stw %r3, 0x0(%r4)
            0x90, 0x44, 0x00, 0x04, //stw %r2, 0x4(%r4)
            0x90, 0x84, 0x00, 0x08, //stw %r4, 0x8(%r4)
            0x3C, 0x60, 0x10, 0x05, //lis %r3, 0x1005
            0x3C, 0x80, 0x10, 0x05, //lis %r4, 0x1005
            0x60, 0x84, 0x00, 0x10, //ori %r4, %r4, 0x10
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0x38, 0xC0, 0x00, 0x00, //li %r6, 0
            0x38, 0xE0, 0x03, 0x00, //li %r7, 0x300
            0x39, 0x00, 0x30, 0x00, //li %r8, 0x3000
            0x39, 0x20, 0x00, 0x00, //li %r9, 0
            0x3D, 0x40, 0x10, 0x05, //lis %r10, 0x1005
            0x61, 0x4A, 0x00, 0x50, //ori %r10, %r10, 0x50
            0x39, 0x60, 0x00, 0x34, //li %r11, 0x34
            0x44, 0x00, 0x00, 0x02, //;sc
            0x3C, 0x60, 0x10, 0x05, //lis %r3, 0x1005
            0xE8, 0x63, 0x00, 0x00, //ld %r3, 0(%r3)
            0x39, 0x60, 0x00, 0x35, //li %r11, 0x35
            0x44, 0x00, 0x00, 0x02, //;sc
            0x60, 0x00, 0x00, 0x00, //nop
            0x60, 0x00, 0x00, 0x00, //nop 
            0x4B, 0xFF, 0xFF, 0xF8, //b -0x8
            0x3D, 0x80, 0x10, 0x05, //lis %r12, 0x1005
            0x61, 0x8C, 0x01, 0x00, //ori %r12, %r12, 0x100
            0x80, 0x6C, 0x00, 0x00, //lwz %r3, 0(%r12)
            0x2C, 0x03, 0x00, 0x00, //cmpwi %r3, 0
            0x41, 0x82, 0x00, 0x40, //beq 0x40
            0x7C, 0x6B, 0x1B, 0x78, //mr %r11, %r3
            0x38, 0x60, 0x00, 0x00, //li %r3, 0
            0x90, 0x6C, 0x00, 0x00, //stw %r3, 0(%r12)
            0xE8, 0x6C, 0x00, 0x08, //ld %r3, 0x8(%r12)
            0xE8, 0x8C, 0x00, 0x10, //ld %r4, 0x10(%r12)
            0xE8, 0xAC, 0x00, 0x18, //ld %r5, 0x18(%r12)
            0xE8, 0xCC, 0x00, 0x20, //ld %r6, 0x20(%r12)
            0xE8, 0xEC, 0x00, 0x28, //ld %r7, 0x28(%r12)
            0xE9, 0x0C, 0x00, 0x30, //ld %r8, 0x30(%r12)
            0xE9, 0x2C, 0x00, 0x38, //ld %r9, 0x38(%r12)
            0xE9, 0x4C, 0x00, 0x40, //ld %r10, 0x40(%r12)
            0x44, 0x00, 0x00, 0x02, //;sc
            0x3D, 0x80, 0x10, 0x05, //lis %r12, 0x1005
            0x61, 0x8C, 0x01, 0x00, //ori %r12, %r12, 0x100
            0xF8, 0x6C, 0x00, 0x48, //std %r3, 0x48(%r12)
            0x3C, 0x60, 0x10, 0x06, //lis %r3, 0x1006
            0x81, 0x83, 0x00, 0x90, //lwz %r12, 0x90(%r3)
            0x2C, 0x0C, 0x00, 0x00, //cmpwi %r12, 0
            0x41, 0x82, 0x00, 0x88, //beq 0x88
            0xE8, 0x83, 0x00, 0x08, //ld %r4, 0x08(%r3)
            0xE8, 0xA3, 0x00, 0x10, //ld %r5, 0x10(%r3)
            0xE8, 0xC3, 0x00, 0x18, //ld %r6, 0x18(%r3)
            0xE8, 0xE3, 0x00, 0x20, //ld %r7, 0x20(%r3)
            0xE9, 0x03, 0x00, 0x28, //ld %r8, 0x28(%r3)
            0xE9, 0x23, 0x00, 0x30, //ld %r9, 0x30(%r3)
            0xE9, 0x43, 0x00, 0x38, //ld %r10, 0x38(%r3)
            0xE9, 0x63, 0x00, 0x40, //ld %r11, 0x40(%r3)
            0xC0, 0x23, 0x00, 0x48, //lfs %f1, 0x48(%r3)
            0xC0, 0x43, 0x00, 0x50, //lfs %f2, 0x50(%r3)
            0xC0, 0x63, 0x00, 0x58, //lfs %f3, 0x58(%r3)
            0xC0, 0x83, 0x00, 0x60, //lfs %f4, 0x60(%r3)
            0xC0, 0xA3, 0x00, 0x68, //lfs %f5, 0x68(%r3)
            0xC0, 0xC3, 0x00, 0x70, //lfs %f6, 0x70(%r3)
            0xC0, 0xE3, 0x00, 0x78, //lfs %f7, 0x78(%r3)
            0xC1, 0x03, 0x00, 0x80, //lfs %f8, 0x80(%r3)
            0xC1, 0x23, 0x00, 0x88, //lfs %f9, 0x88(%r3)
            0xE8, 0x63, 0x00, 0x00, //ld %r3, 0x0(%r3)
            0x7D, 0x89, 0x03, 0xA6, //mtctr %r12
            0x3D, 0x80, 0x10, 0x06, //lis %r12, 0x1006
            0xF8, 0x4C, 0x00, 0xA8, //std %r2, 0xA8(%r12)
            0xE9, 0x8C, 0x00, 0xB0, //ld %r12, 0xB0(%r12)
            0x2C, 0x0C, 0x00, 0x00, //cmpwi %r12, 0
            0x41, 0x82, 0x00, 0x08, //beq 0x8
            0x7D, 0x82, 0x63, 0x78, //mr %r2, %r12
            0x4E, 0x80, 0x04, 0x21, //bctrl
            0x3C, 0x80, 0x10, 0x06, //lis %r4, 0x1006
            0x38, 0xA0, 0x00, 0x00, //li %r5, 0
            0xE8, 0x44, 0x00, 0xA8, //ld %r2, 0xA8(%r4)
            0xF8, 0xA4, 0x00, 0xB0, //std %r5, 0xB0(%r4)
            0x90, 0xA4, 0x00, 0x90, //stw %r5, 0x90(%r4)
            0xF8, 0x64, 0x00, 0x98, //std %r3, 0x98(%r4)
            0xD0, 0x24, 0x00, 0xA0, //stfs %f1, 0xA0(%r4)
            0x38, 0x60, 0x00, 0x64, //li %r3, 0x64
            0x39, 0x60, 0x00, 0x8D, //li %r11, 0x08D
            0x44, 0x00, 0x00, 0x02, //;sc
            0x4B, 0xFF, 0xFF, 0x10  //b -0xF0
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

        public static T CallTOC<T>(uint address, uint toc, params object[] parameters) {
            if (!PS3RPC.Install())
                return (T)Convert.ChangeType(0, typeof(int));

            PS3.Ext.WriteUInt64(FUNC_CALL_RX_RET, 0xDEADC0DE);
            PS3.Ext.WriteUInt64(FUNC_CALL_FX_RET, 0xDEADC0DE);

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

            Type type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if(type == typeof(float)) {
                float ret = 0.0f;
                while ((ret = PS3.Ext.ReadFloat(FUNC_CALL_FX_RET)) == 0xDEADC0DE)
                    Thread.Sleep(10);

                return (T)Convert.ChangeType(ret, typeof(T));
            }
            else if(type == typeof(string)) {
                ulong ret = 0;
                while ((ret = PS3.Ext.ReadUInt64(FUNC_CALL_RX_RET)) == 0xDEADC0DE)
                    Thread.Sleep(10);
                string value = PS3.Ext.ReadString((uint)ret);

                return (T)Convert.ChangeType(value, typeof(T));
            }
            else {
                ulong ret = 0;
                while ((ret = PS3.Ext.ReadUInt64(FUNC_CALL_RX_RET)) == 0xDEADC0DE)
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
            CallTOC<int>(address, toc);

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
            CallTOC<int>(address, toc);

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
