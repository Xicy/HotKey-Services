using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;


namespace HotKeyServices
{
    public class Hotkey : IEnumerable<KeyValuePair<Hotkey.NativeKey, Action>>, IDisposable
    {
        private readonly IntPtr _hwnd;
        private bool _isRunning;
        private readonly Thread _win32Thread;
        private readonly IDictionary<NativeKey, Action> _hotkeys;

        public Hotkey(IntPtr hwnd)
        {
            _hwnd = hwnd;
            _hotkeys = new Dictionary<NativeKey, Action>();
            _win32Thread = new Thread(GetMessage)
            {
                Name = "Win32 Thread",
                Priority = ThreadPriority.Highest
            };
        }

        /* TODO: Add Error Codes
         * https://msdn.microsoft.com/en-us/library/windows/desktop/ms681381(v=vs.85).aspx
        */

        #region Dll Imports
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterHotKey"), SuppressUnmanagedCodeSecurity] private static extern int RegisterHotKey(IntPtr hwnd, NativeKey id, ModifierKeys fsModifiers, WindowsKeys vk);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "UnregisterHotKey"), SuppressUnmanagedCodeSecurity] private static extern int UnregisterHotKey(IntPtr hwnd, NativeKey id);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "PeekMessage"), SuppressUnmanagedCodeSecurity] private static extern int PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetMessage"), SuppressUnmanagedCodeSecurity] private static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "TranslateMessage"), SuppressUnmanagedCodeSecurity] private static extern int TranslateMessage(ref NativeMessage lpMsg);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "DispatchMessage"), SuppressUnmanagedCodeSecurity] private static extern int DispatchMessage(ref NativeMessage lpMsg);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "keybd_event"), SuppressUnmanagedCodeSecurity] private static extern void KeyboardEvent(WindowsKeys key, byte scan, int flags, int extraInfo);
        #endregion

        #region Structures
        [Flags]
        public enum ModifierKeys : short
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        public enum WindowsKeys : short
        {
            Backspace = 0x08,
            Tab = 0x09,
            Clear = 0x0C,
            Enter = 0x0D,
            Shift = 0x10,
            Control = 0x11,
            Alt = 0x12,
            Pause = 0x13,
            CapsLock = 0x14,
            Escape = 0x1B,
            Space = 0x20,
            PageUp = 0x21,
            PageDown = 0x22,
            End = 0x23,
            Home = 0x24,
            Left = 0x25,
            Up = 0x26,
            Right = 0x27,
            Down = 0x28,
            Select = 0x29,
            Print = 0x2A,
            Execute = 0x2B,
            PrintScreen = 0x2C,
            Insert = 0x2D,
            Delete = 0x2E,
            Help = 0x2F,
            Zero = 0x30,
            One = 0x31,
            Two = 0x32,
            Three = 0x33,
            Four = 0x34,
            Five = 0x35,
            Six = 0x36,
            Seven = 0x37,
            Eight = 0x38,
            Nine = 0x39,
            A = 0x41,
            B = 0x42,
            C = 0x43,
            D = 0x44,
            E = 0x45,
            F = 0x46,
            G = 0x47,
            H = 0x48,
            I = 0x49,
            J = 0x4A,
            K = 0x4B,
            L = 0x4C,
            M = 0x4D,
            N = 0x4E,
            O = 0x4F,
            P = 0x50,
            Q = 0x51,
            R = 0x52,
            S = 0x53,
            T = 0x54,
            U = 0x55,
            V = 0x56,
            W = 0x57,
            X = 0x58,
            Y = 0x59,
            Z = 0x5A,
            LeftWindowsKey = 0x5B,
            RightWindowsKey = 0x5C,
            ApplicationsKey = 0x5D,
            Sleep = 0x5F,
            NumPad0 = 0x60,
            NumPad1 = 0x61,
            NumPad2 = 0x62,
            NumPad3 = 0x63,
            NumPad4 = 0x64,
            NumPad5 = 0x65,
            NumPad6 = 0x66,
            NumPad7 = 0x67,
            NumPad8 = 0x68,
            NumPad9 = 0x69,
            Multiply = 0x6A,
            Add = 0x6B,
            Seperator = 0x6C,
            Subtract = 0x6D,
            Decimal = 0x6E,
            Divide = 0x6F,
            F1 = 0x70,
            F2 = 0x71,
            F3 = 0x72,
            F4 = 0x73,
            F5 = 0x74,
            F6 = 0x75,
            F7 = 0x76,
            F8 = 0x77,
            F9 = 0x78,
            F10 = 0x79,
            F11 = 0x7A,
            F12 = 0x7B,
            F13 = 0x7C,
            F14 = 0x7D,
            F15 = 0x7E,
            F16 = 0x7F,
            F17 = 0x80,
            F18 = 0x81,
            F19 = 0x82,
            F20 = 0x83,
            F21 = 0x84,
            F22 = 0x85,
            F23 = 0x86,
            F24 = 0x87,
            Numlock = 0x90,
            ScrollLock = 0x91,
            LeftShift = 0xA0,
            RightShift = 0xA1,
            LeftControl = 0xA2,
            RightContol = 0xA3,
            LeftMenu = 0xA4,
            RightMenu = 0xA5,
            BrowserBack = 0xA6,
            BrowserForward = 0xA7,
            BrowserRefresh = 0xA8,
            BrowserStop = 0xA9,
            BrowserSearch = 0xAA,
            BrowserFavorites = 0xAB,
            BrowserHome = 0xAC,
            VolumeMute = 0xAD,
            VolumeDown = 0xAE,
            VolumeUp = 0xAF,
            NextTrack = 0xB0,
            PreviousTrack = 0xB1,
            StopMedia = 0xB2,
            PlayPause = 0xB3,
            LaunchMail = 0xB4,
            SelectMedia = 0xB5,
            LaunchApp1 = 0xB6,
            LaunchApp2 = 0xB7,
            Oem1 = 0xBA,
            OemPlus = 0xB8,
            OemComma = 0xBC,
            OemMinus = 0xBD,
            OemPeriod = 0xBE,
            Oem2 = 0xBF,
            Oem3 = 0xC0,
            Oem4 = 0xDB,
            Oem5 = 0xDC,
            Oem6 = 0xDD,
            Oem7 = 0xDE,
            Oem8 = 0xDF,
            Oem102 = 0xE2,
            Process = 0xE5,
            Packet = 0xE7,
            Attn = 0xF6,
            CrSel = 0xF7,
            ExSel = 0xF8,
            EraseEof = 0xF9,
            Play = 0xFA,
            Zoom = 0xFB,
            Pa1 = 0xFD,
            OemClear = 0xFE
        };

        [StructLayout(LayoutKind.Sequential)]
        [DebuggerDisplay("{Modifier,nq} {WindowsKey,nq}")]
        public struct NativeKey
        {
            public ModifierKeys Modifier;
            public WindowsKeys WindowsKey;

            public NativeKey(ModifierKeys mKeys, WindowsKeys key)
            {
                Modifier = mKeys;
                WindowsKey = key;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr Handle;
            public int Message;
            public IntPtr wParam;
            public NativeKey lParam;
            public int Time;
            public int X;
            public int Y;
        }
        #endregion

        public static void KeyDown(WindowsKeys key)
        {
            KeyboardEvent(key, 0, 0, 0);
        }

        public static void KeyUp(WindowsKeys key)
        {
            KeyboardEvent(key, 0, 0x2, 0);
        }

        public static void KeyPress(WindowsKeys key)
        {
            KeyDown(key);
            KeyUp(key);
        }

        public void Start()
        {
            if (_isRunning) return;
            while (_win32Thread.ThreadState == System.Threading.ThreadState.Running)
                Thread.Sleep(1);

            _isRunning = true;
            
            _win32Thread.Start();
        }

        private void GetMessage()
        {
            var hks = _hotkeys.Keys.ToArray();

            if (hks.Any(key => RegisterHotKey(_hwnd, key, key.Modifier, key.WindowsKey) == 0))
                throw new InvalidOperationException($"An error happened in loop while processing windows messages. Error: {Marshal.GetLastWin32Error()}");

            while (_isRunning)
            {
                NativeMessage nativeMessage;
                if (PeekMessage(out nativeMessage, _hwnd, 0, 0, 0) == 0 && nativeMessage.Message != 0x312 && !Contains(ref nativeMessage.lParam))
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (GetMessage(out nativeMessage, _hwnd, 0, 0) == -1)
                    throw new InvalidOperationException($"An error happened in loop while processing windows messages. Error: {Marshal.GetLastWin32Error()}");

                this[nativeMessage.lParam]();

                TranslateMessage(ref nativeMessage);
                DispatchMessage(ref nativeMessage);
            }

            if (hks.Any(key => UnregisterHotKey(_hwnd, key) == 0))
                throw new InvalidOperationException($"An error happened in loop while processing windows messages. Error: {Marshal.GetLastWin32Error()}");
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public bool Contains(ref NativeKey key)
        {
            return _hotkeys.ContainsKey(key);
        }

        public bool Contains(NativeKey key)
        {
            return _hotkeys.ContainsKey(key);
        }

        public void Add(NativeKey key, Action value)
        {
            if (_isRunning) throw new InvalidOperationException($"Hotkey running now. Please Stop it");
            _hotkeys.Add(key, value);
        }

        public void Remove(NativeKey key)
        {
            if (_isRunning) throw new InvalidOperationException($"Hotkey running now. Please Stop it");
            _hotkeys.Remove(key);
        }

        public Action this[NativeKey key]
        {
            get { return _hotkeys[key]; }
            set
            {
                if (_isRunning) throw new InvalidOperationException($"Hotkey running now. Please Stop it");
                _hotkeys[key] = value;
            }
        }

        public void Clear()
        {
            if (_isRunning) throw new InvalidOperationException($"Hotkey running now. Please Stop it");
            _hotkeys.Clear();
        }

        public IEnumerator<KeyValuePair<NativeKey, Action>> GetEnumerator()
        {
            return _hotkeys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _hotkeys.GetEnumerator();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~Hotkey()
        {
            Dispose(true);
        }
    }
}