using System;
using System.Net;
using System.Linq;
using System.Text;

namespace PS3_SPRX_Loader {
    public class TMAPI {
        public enum GPRegisters {
            SNPS3_gpr_0 = 0x00,
            SNPS3_gpr_1 = 0x01,
            SNPS3_gpr_2 = 0x02,
            SNPS3_gpr_3 = 0x03,
            SNPS3_gpr_4 = 0x04,
            SNPS3_gpr_5 = 0x05,
            SNPS3_gpr_6 = 0x06,
            SNPS3_gpr_7 = 0x07,
            SNPS3_gpr_8 = 0x08,
            SNPS3_gpr_9 = 0x09,
            SNPS3_gpr_10 = 0x0a,
            SNPS3_gpr_11 = 0x0b,
            SNPS3_gpr_12 = 0x0c,
            SNPS3_gpr_13 = 0x0d,
            SNPS3_gpr_14 = 0x0e,
            SNPS3_gpr_15 = 0x0f,
            SNPS3_gpr_16 = 0x10,
            SNPS3_gpr_17 = 0x11,
            SNPS3_gpr_18 = 0x12,
            SNPS3_gpr_19 = 0x13,
            SNPS3_gpr_20 = 0x14,
            SNPS3_gpr_21 = 0x15,
            SNPS3_gpr_22 = 0x16,
            SNPS3_gpr_23 = 0x17,
            SNPS3_gpr_24 = 0x18,
            SNPS3_gpr_25 = 0x19,
            SNPS3_gpr_26 = 0x1a,
            SNPS3_gpr_27 = 0x1b,
            SNPS3_gpr_28 = 0x1c,
            SNPS3_gpr_29 = 0x1d,
            SNPS3_gpr_30 = 0x1e,
            SNPS3_gpr_31 = 0x1f
        }

        public enum FPRegisters {
            SNPS3_fpr_0 = 0x20,
            SNPS3_fpr_1 = 0x21,
            SNPS3_fpr_2 = 0x22,
            SNPS3_fpr_3 = 0x23,
            SNPS3_fpr_4 = 0x24,
            SNPS3_fpr_5 = 0x25,
            SNPS3_fpr_6 = 0x26,
            SNPS3_fpr_7 = 0x27,
            SNPS3_fpr_8 = 0x28,
            SNPS3_fpr_9 = 0x29,
            SNPS3_fpr_10 = 0x2A,
            SNPS3_fpr_11 = 0x2B,
            SNPS3_fpr_12 = 0x2C,
            SNPS3_fpr_13 = 0x2D,
            SNPS3_fpr_14 = 0x2E,
            SNPS3_fpr_15 = 0x2F,
            SNPS3_fpr_16 = 0x30,
            SNPS3_fpr_17 = 0x31,
            SNPS3_fpr_18 = 0x32,
            SNPS3_fpr_19 = 0x33,
            SNPS3_fpr_20 = 0x34,
            SNPS3_fpr_21 = 0x35,
            SNPS3_fpr_22 = 0x36,
            SNPS3_fpr_23 = 0x37,
            SNPS3_fpr_24 = 0x38,
            SNPS3_fpr_25 = 0x39,
            SNPS3_fpr_26 = 0x3A,
            SNPS3_fpr_27 = 0x3B,
            SNPS3_fpr_28 = 0x3C,
            SNPS3_fpr_29 = 0x3D,
            SNPS3_fpr_30 = 0x3E,
            SNPS3_fpr_31 = 0x3F,
        }

        public enum SPRegisters {
            SNPS3_pc = 0x40,
            SNPS3_cr = 0x41,
            SNPS3_lr = 0x42,
            SNPS3_ctr = 0x43,
            SNPS3_xer = 0x44,
            SNPS3_fpscr = 0x45,
            SNPS3_vscr = 0x46,
            SNPS3_vrsave = 0x47,
            SNPS3_msr = 0x48,
        }

        public static int Target = 0;
        public bool IsConnected = false;

        public class Parameters {
            public static string Info, Usage, Status, ConsoleName;

            public static PS3TMAPI.PPUThreadInfo threadInfo;
            public static uint ProcessID;
            public static uint[] ProcessIDs;
            public static uint[] ModuleIDs;

            public static PS3TMAPI.ConnectStatus connectStatus;
        }

        public Extension Ext {
            get { return new Extension(); }
        }

        public bool ConnectTarget() {
            bool result = false;
            result = PS3TMAPI.SUCCEEDED(PS3TMAPI.InitTargetComms());
            result = PS3TMAPI.SUCCEEDED(PS3TMAPI.Connect(Target, null));
            IsConnected = result;
            return result;
        }

        public void DisconnectTarget() {
            PS3TMAPI.Disconnect(Target);
            IsConnected = false;
        }

        public static void GetThreadInfo() {
            PS3TMAPI.ProcessInfo processInfo;
            PS3TMAPI.GetProcessInfo(Target, Parameters.ProcessID, out processInfo);

            if (processInfo.ThreadIDs.Length <= 0)
                return;

            for (int i = 0; i < processInfo.ThreadIDs.Length; i++) {
                PS3TMAPI.GetPPUThreadInfo(Target, Parameters.ProcessID, processInfo.ThreadIDs[i], out Parameters.threadInfo);

                if (Parameters.threadInfo.ThreadName == null)
                    continue;
                if (Parameters.threadInfo.ThreadName.Contains("EBOOT"))
                    break;
            }
        }

        public bool GetThreadByName(string name, ref PS3TMAPI.PPUThreadInfo LocalthreadInfo) {
            PS3TMAPI.ProcessInfo processInfo;
            PS3TMAPI.GetProcessInfo(Target, Parameters.ProcessID, out processInfo);

            if (processInfo.ThreadIDs.Length <= 0)
                return false;

            for (int i = 0; i < processInfo.ThreadIDs.Length; i++) {
                PS3TMAPI.GetPPUThreadInfo(Target, Parameters.ProcessID, processInfo.ThreadIDs[i], out LocalthreadInfo);

                if (LocalthreadInfo.ThreadName == null)
                    continue;
                if (LocalthreadInfo.ThreadName.Contains(name))
                    return true;
            }
            return false;
        }

        public bool AttachProcess() {
            PS3TMAPI.GetProcessList(Target, out Parameters.ProcessIDs);

            if (Parameters.ProcessIDs.Length > 0) {
                ulong uProcess = Parameters.ProcessIDs[0];
                Parameters.ProcessID = Convert.ToUInt32(uProcess);

                PS3TMAPI.GetModuleList(Target, Parameters.ProcessID, out Parameters.ModuleIDs);
                PS3TMAPI.ProcessAttach(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID);
                PS3TMAPI.ProcessContinue(Target, Parameters.ProcessID);

                GetThreadInfo();

                Parameters.Info = "The Process 0x" + Parameters.ProcessID.ToString("X8") + " Has Been Attached!";
                return true;
            }

            return false;
        }

        public void Shutdown(bool Force) {
            PS3TMAPI.PowerOff(Target, Force);
        }

        public void ResetToXMB() {
            PS3TMAPI.Reset(Target, PS3TMAPI.ResetParameter.Hard);
        }

        public void MainThreadStop() {
            PS3TMAPI.ThreadStop(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, Parameters.threadInfo.ThreadID);
        }

        public void MainThreadContinue() {
            PS3TMAPI.ThreadContinue(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, Parameters.threadInfo.ThreadID);
        }

        public void StopThreadyID(ulong ID) {
            PS3TMAPI.ThreadStop(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, ID);
        }

        public void ContinueThreadByID(ulong ID) {
            PS3TMAPI.ThreadContinue(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, ID);
        }

        public static ulong ULongReverse(ulong value) {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }

        public ulong GetSingleRegister(uint Register) {
            ulong[] Return = new ulong[2];
            uint[] Registers = new uint[1];

            Registers[0] = Register;
            PS3TMAPI.ThreadGetRegisters(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, Parameters.threadInfo.ThreadID, (uint[])Registers, out Return);

            return ULongReverse(Return[0]);
        }

        public void SetSingleRegister(uint Register, ulong Value) {
            ulong[] Return = new ulong[1];
            uint[] Registers = new uint[1];

            Registers[0] = Register;
            Return[0] = ULongReverse(Value);

            PS3TMAPI.SNRESULT a = PS3TMAPI.ThreadSetRegisters(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, Parameters.threadInfo.ThreadID, (uint[])Registers, Return);
        }

        public ulong GetSingleRegisterByThreadID(ulong ID, uint Register) {
            ulong[] Return = new ulong[2];
            uint[] Registers = new uint[1];

            Registers[0] = Register;

            PS3TMAPI.ThreadGetRegisters(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, ID, (uint[])Registers, out Return);
            return ULongReverse(Return[0]);
        }

        public void SetSingleRegisterByThreadID(ulong ID, uint Register, ulong Value) {
            ulong[] Return = new ulong[1];
            uint[] Registers = new uint[1];

            Registers[0] = Register;
            Return[0] = ULongReverse(Value);

            PS3TMAPI.SNRESULT a = PS3TMAPI.ThreadSetRegisters(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, ID, (uint[])Registers, Return);
        }

        public void SetMemory(uint Address, byte[] Bytes) {
            PS3TMAPI.ProcessSetMemory(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, 0, Address, Bytes);
        }

        public void GetMemory(uint Address, byte[] Bytes) {
            PS3TMAPI.ProcessGetMemory(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID, 0, Address, ref Bytes);
        }

        public string GetTargetName() {
            if (Parameters.ConsoleName == null || Parameters.ConsoleName == String.Empty) {
                PS3TMAPI.InitTargetComms();
                PS3TMAPI.TargetInfo TargetInfo = new PS3TMAPI.TargetInfo();
                TargetInfo.Flags = PS3TMAPI.TargetInfoFlag.TargetID;
                TargetInfo.Target = TMAPI.Target;
                PS3TMAPI.GetTargetInfo(ref TargetInfo);
                Parameters.ConsoleName = TargetInfo.Name;
            }
            return Parameters.ConsoleName;
        }

        public string GetStatus() {
            Parameters.connectStatus = new PS3TMAPI.ConnectStatus();
            PS3TMAPI.GetConnectStatus(Target, out Parameters.connectStatus, out Parameters.Usage);
            Parameters.Status = Parameters.connectStatus.ToString();
            return Parameters.Status;
        }

        public uint ProcessID() {
            PS3TMAPI.GetProcessList(Target, out Parameters.ProcessIDs);
            Parameters.ProcessID = Parameters.ProcessIDs[0];
            return Parameters.ProcessID;
        }

        public uint[] ProcessIDs() {
            PS3TMAPI.GetProcessList(Target, out Parameters.ProcessIDs);
            return Parameters.ProcessIDs;
        }

        public string GetProcessName(uint processId) {
            if (processId != 0) {
                PS3TMAPI.ProcessInfo processInfo;
                PS3TMAPI.GetProcessInfo(Target, processId, out processInfo);
                return processInfo.Hdr.ToString();
            }
            return "";
        }

        public uint GetProcessParent(uint processId) {
            if (processId != 0) {
                PS3TMAPI.ProcessInfo processInfo;
                PS3TMAPI.GetProcessInfo(Target, processId, out processInfo);
                return processInfo.Hdr.ParentProcessID;
            }
            return 0x0;
        }

        public ulong GetProcessSize(uint processId) {
            if (processId != 0) {
                PS3TMAPI.ProcessInfo processInfo;
                PS3TMAPI.GetProcessInfo(Target, processId, out processInfo);
                return processInfo.Hdr.MaxMemorySize;
            }
            return 0x0;
        }

        public uint[] ModuleIds() {
            PS3TMAPI.GetModuleList(Target, Parameters.ProcessID, out Parameters.ModuleIDs);

            return Parameters.ModuleIDs;
        }

        public string GetModuleName(uint moduleId) {
            if (Parameters.ProcessIDs.Length > 0) {
                PS3TMAPI.ModuleInfo moduleInfo;
                PS3TMAPI.GetModuleInfo(Target, Parameters.ProcessID, moduleId, out moduleInfo);
                return moduleInfo.Hdr.Name;
            }
            return "";
        }

        public uint GetModuleStartAddress(uint moduleId) {
            if (Parameters.ProcessIDs.Length > 0) {
                PS3TMAPI.ModuleInfo moduleInfo;
                PS3TMAPI.GetModuleInfo(Target, Parameters.ProcessID, moduleId, out moduleInfo);
                return moduleInfo.Hdr.StartEntry;
            }
            return 0x0;
        }

        public ulong GetModuleSize(uint moduleId) {
            if (Parameters.ProcessIDs.Length > 0) {
                PS3TMAPI.ModuleInfo moduleInfo;
                PS3TMAPI.GetModuleInfo(Target, Parameters.ProcessID, moduleId, out moduleInfo);
                ulong memSize = 0x0;
                for (int i = 0; i < moduleInfo.Segments.Length; i++)
                    memSize += moduleInfo.Segments[i].MemSize;
                return memSize;
            }
            return 0x0;
        }

        public string GetCurrentGame() {
            PS3TMAPI.ProcessInfo processInfo = new PS3TMAPI.ProcessInfo();
            PS3TMAPI.GetProcessInfo(0, ProcessID(), out processInfo);
            string GameCode = processInfo.Hdr.ELFPath.Split('/')[3];

            try {
                WebClient client = new System.Net.WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                string content = client.DownloadString(String.Format("https://a0.ww.np.dl.playstation.net/tpl/np/{0}/{1}-ver.xml", GameCode, GameCode)).Replace("<TITLE>", ";");
                return content.Split(';')[1].Replace("</TITLE>", ";").Split(';')[0].Replace("Â", "");
            }
            catch {
                return "Unknown Game";
            }
        }

        public class Extension {
            private TMAPI Target;

            public Extension() {
                Target = new TMAPI();
            }
            private byte[] GetBytes(uint offset, int length) {
                byte[] buffer = new byte[length];
                Target.GetMemory(offset, buffer);
                return buffer;
            }

            private void SetBytes(uint offset, byte[] bytes) {
                Target.SetMemory(offset, bytes);
            }

            public bool ReadBool(uint offset) {
                byte[] buffer = GetBytes(offset, 1);
                return buffer[0] != 0;
            }

            public byte ReadByte(uint offset) {
                byte[] buffer = GetBytes(offset, 1);
                return buffer[0];
            }

            public short ReadInt16(uint offset) {
                byte[] buffer = GetBytes(offset, 2);
                Array.Reverse(buffer, 0, 2);
                return BitConverter.ToInt16(buffer, 0);
            }

            public int ReadInt32(uint offset) {
                byte[] buffer = GetBytes(offset, 4);
                Array.Reverse(buffer, 0, 4);
                return BitConverter.ToInt32(buffer, 0);
            }

            public long ReadInt64(uint offset) {
                byte[] buffer = GetBytes(offset, 8);
                Array.Reverse(buffer, 0, 8);
                return BitConverter.ToInt64(buffer, 0);
            }

            public byte[] ReadBytes(uint offset, int length) {
                byte[] buffer = GetBytes(offset, length);
                return buffer;
            }

            public ushort ReadUInt16(uint offset) {
                byte[] buffer = GetBytes(offset, 2);
                Array.Reverse(buffer, 0, 2);
                return BitConverter.ToUInt16(buffer, 0);
            }

            public uint ReadUInt32(uint offset) {
                byte[] buffer = GetBytes(offset, 4);
                Array.Reverse(buffer, 0, 4);
                return BitConverter.ToUInt32(buffer, 0);
            }

            public ulong ReadUInt64(uint offset) {
                byte[] buffer = GetBytes(offset, 8);
                Array.Reverse(buffer, 0, 8);
                return BitConverter.ToUInt64(buffer, 0);
            }

            public float ReadFloat(uint offset) {
                byte[] buffer = GetBytes(offset, 4);
                Array.Reverse(buffer, 0, 4);
                return BitConverter.ToSingle(buffer, 0);
            }

            public float[] ReadFloats(uint offset, int arrayLength = 3) {
                float[] vec = new float[arrayLength];
                for (int i = 0; i < arrayLength; i++) {
                    byte[] buffer = GetBytes(offset + ((uint)i * 4), 4);
                    Array.Reverse(buffer, 0, 4);
                    vec[i] = BitConverter.ToSingle(buffer, 0);
                }
                return vec;
            }

            public double ReadDouble(uint offset) {
                byte[] buffer = GetBytes(offset, 8);
                Array.Reverse(buffer, 0, 8);
                return BitConverter.ToDouble(buffer, 0);
            }

            public string ReadString(uint offset) {
                int blocksize = 40;
                int scalesize = 0;
                string str = string.Empty;

                while (!str.Contains('\0')) {
                    byte[] buffer = ReadBytes(offset + (uint)scalesize, blocksize);
                    str += Encoding.UTF8.GetString(buffer);
                    scalesize += blocksize;
                }

                return str.Substring(0, str.IndexOf('\0'));
            }

            public void WriteBool(uint offset, bool input) {
                byte[] buff = new byte[1];
                buff[0] = input ? (byte)1 : (byte)0;
                SetBytes(offset, buff);
            }

            public void WriteByte(uint offset, byte input) {
                byte[] buff = new byte[1];
                buff[0] = input;
                SetBytes(offset, buff);
            }

            public void WriteInt16(uint offset, short input) {
                byte[] buff = new byte[2];
                BitConverter.GetBytes(input).CopyTo(buff, 0);
                Array.Reverse(buff, 0, 2);
                SetBytes(offset, buff);
            }

            public void WriteInt32(uint offset, int input) {
                byte[] buff = new byte[4];
                BitConverter.GetBytes(input).CopyTo(buff, 0);
                Array.Reverse(buff, 0, 4);
                SetBytes(offset, buff);
            }

            public void WriteInt64(uint offset, long input) {
                byte[] buff = new byte[8];
                BitConverter.GetBytes(input).CopyTo(buff, 0);
                Array.Reverse(buff, 0, 8);
                SetBytes(offset, buff);
            }

            public void WriteBytes(uint offset, byte[] input) {
                byte[] buff = input;
                SetBytes(offset, buff);
            }

            public void WriteString(uint offset, string input) {
                byte[] buff = Encoding.UTF8.GetBytes(input);
                Array.Resize(ref buff, buff.Length + 1);
                SetBytes(offset, buff);
            }

            public void WriteUInt16(uint offset, ushort input) {
                byte[] buff = new byte[2];
                BitConverter.GetBytes(input).CopyTo(buff, 0);
                Array.Reverse(buff, 0, 2);
                SetBytes(offset, buff);
            }

            public void WriteUInt32(uint offset, uint input) {
                byte[] buff = new byte[4];
                BitConverter.GetBytes(input).CopyTo(buff, 0);
                Array.Reverse(buff, 0, 4);
                SetBytes(offset, buff);
            }

            public void WriteUInt64(uint offset, ulong input) {
                byte[] buff = new byte[8];
                BitConverter.GetBytes(input).CopyTo(buff, 0);
                Array.Reverse(buff, 0, 8);
                SetBytes(offset, buff);
            }

            public void WriteFloat(uint offset, float input) {
                byte[] buff = new byte[4];
                BitConverter.GetBytes(input).CopyTo(buff, 0);
                Array.Reverse(buff, 0, 4);
                SetBytes(offset, buff);
            }

            public void WriteFloats(uint offset, float[] input) {
                byte[] buff = new byte[4];
                for (int i = 0; i < input.Length; i++) {
                    BitConverter.GetBytes(input[i]).CopyTo(buff, 0);
                    Array.Reverse(buff, 0, 4);
                    SetBytes(offset + ((uint)i * 4), buff);
                }
            }

            public void WriteDouble(uint offset, double input) {
                byte[] buff = new byte[8];
                BitConverter.GetBytes(input).CopyTo(buff, 0);
                Array.Reverse(buff, 0, 8);
                SetBytes(offset, buff);
            }
        }
    }
}

