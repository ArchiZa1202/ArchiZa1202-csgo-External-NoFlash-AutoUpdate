using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;
using System.Text;

namespace MyPrroj
{
    public class Memory
    {
        public static int m_iNumberOfBytesRead = 0;
        public static int m_iNumberOfBytesWritten = 0;
        IntPtr processHandle = IntPtr.Zero;
        IntPtr handle;
        Process _proc;
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        
        #region OpenProcess
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        public Memory(string proc)
        {
            try
            {
                if (proc != null)
                {
                    _proc = Process.GetProcessesByName(proc)[0];
                    IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, _proc.Id);
                    handle = processHandle;
                }
            }
            catch
            {
                MessageBox.Show("Invalid name Process!");
            }
        }
        #endregion OpenProcess

        #region ReadProcessMemory
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        public IntPtr Read (IntPtr adresModul)
        {
            int bytesRead = Marshal.SizeOf(typeof(IntPtr));
            byte[] buffer = new byte[bytesRead];
            
                ReadProcessMemory(handle, adresModul, buffer, buffer.Length, ref m_iNumberOfBytesRead);
                return (IntPtr)BitConverter.ToInt32(buffer, 0);                  
        }
        #endregion ReadProcessMemory

        #region WriteProcessMemory
        [DllImport("kernel32.dll", SetLastError = true)]

        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);
        public void Write(IntPtr adresModul, float f) 
        {
            int bytesRead = Marshal.SizeOf(typeof(float));
            byte[] buffers = StructureToByteArray(bytesRead);
            WriteProcessMemory(handle, adresModul, buffers, buffers.Length, out m_iNumberOfBytesWritten);
        }

        public void Write(IntPtr Adress, char[] Value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(Value);

            WriteProcessMemory(handle, Adress, buffer, buffer.Length, out m_iNumberOfBytesWritten);
        }

        public void Write(IntPtr Adress, bool flag)
        {
            int bytesRead = Marshal.SizeOf(typeof(bool));
            byte[] buffer = new byte[bytesRead];

            WriteProcessMemory(handle, Adress, buffer, buffer.Length, out m_iNumberOfBytesWritten);
        }
        #endregion WriteProcessMemory


        private static byte[] StructureToByteArray(object obj)
        {
            int length = Marshal.SizeOf(obj);

            byte[] array = new byte[length];

            IntPtr pointer = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);

            return array;
        }
    }
}
