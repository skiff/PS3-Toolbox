using System;
using System.Net;
using System.Linq;
using System.Text;

namespace PS3_SPRX_Loader {
    public class TMAPI {
        public static int Target = 0;
        public bool IsConnected = false;

        public class Parameters {
            public static string Info, Usage, Status, ConsoleName;

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

        public bool AttachProcess() {
            PS3TMAPI.GetProcessList(Target, out Parameters.ProcessIDs);

            if (Parameters.ProcessIDs.Length > 0) {
                ulong uProcess = Parameters.ProcessIDs[0];
                Parameters.ProcessID = Convert.ToUInt32(uProcess);

                PS3TMAPI.GetModuleList(Target, Parameters.ProcessID, out Parameters.ModuleIDs);
                PS3TMAPI.ProcessAttach(Target, PS3TMAPI.UnitType.PPU, Parameters.ProcessID);
                PS3TMAPI.ProcessContinue(Target, Parameters.ProcessID);

                Parameters.Info = "The Process 0x" + Parameters.ProcessID.ToString("X8") + " Has Been Attached !";
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

