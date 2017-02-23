using System;

namespace HotKeyServices
{
    public class HotkeyServices
    {
        public static void Main()
        {
            var hotkey = new Hotkey(IntPtr.Zero);

            hotkey.Add(new Hotkey.NativeKey(Hotkey.ModifierKeys.Alt, Hotkey.WindowsKeys.Left), () => Hotkey.KeyPress(Hotkey.WindowsKeys.PreviousTrack));
            hotkey.Add(new Hotkey.NativeKey(Hotkey.ModifierKeys.Alt, Hotkey.WindowsKeys.Right), () => Hotkey.KeyPress(Hotkey.WindowsKeys.NextTrack));
            hotkey.Add(new Hotkey.NativeKey(Hotkey.ModifierKeys.Alt, Hotkey.WindowsKeys.Up), () => Hotkey.KeyPress(Hotkey.WindowsKeys.PlayPause));
            hotkey.Add(new Hotkey.NativeKey(Hotkey.ModifierKeys.Alt, Hotkey.WindowsKeys.Down), () => Hotkey.KeyPress(Hotkey.WindowsKeys.StopMedia));

            hotkey.Start();

            var hotkey2 = new Hotkey(IntPtr.Zero);

            hotkey2.Add(new Hotkey.NativeKey(Hotkey.ModifierKeys.Control, Hotkey.WindowsKeys.Left), () => Hotkey.KeyPress(Hotkey.WindowsKeys.PreviousTrack));
            hotkey2.Add(new Hotkey.NativeKey(Hotkey.ModifierKeys.Control, Hotkey.WindowsKeys.Right), () => Hotkey.KeyUp(Hotkey.WindowsKeys.NextTrack));
            hotkey2.Add(new Hotkey.NativeKey(Hotkey.ModifierKeys.Control, Hotkey.WindowsKeys.Up), () => Hotkey.KeyPress(Hotkey.WindowsKeys.PlayPause));
            hotkey2.Add(new Hotkey.NativeKey(Hotkey.ModifierKeys.Control, Hotkey.WindowsKeys.Down), () => Hotkey.KeyPress(Hotkey.WindowsKeys.StopMedia));

            hotkey2.Start();

#if DEBUG
            string cmd;
            do
            {
                cmd = Console.ReadLine()?.ToLowerInvariant();
                switch (cmd)
                {
                    case "stop": _hotkey.Stop(); break;
                    case "start": _hotkey.Start(); break;
                    case "clear": _hotkey.Clear(); break;

                }
            } while (!string.IsNullOrEmpty(cmd));
#endif
        }
    }
}
