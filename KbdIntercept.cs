using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace lockit
{
    public class KbdIntercept
    {
        private static int HookId;
        private const int Wh_Keyboard_LL = 13;
        private const int VK_TAB = 0x09;
        private const int VK_ESCAPE = 0x1B;
        private const int VK_LMENU = 0xA4;
        private const int VK_RMENU = 0xA5;
        private const int VK_MENU = 0x12;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_F4 = 0x73;

        private static Pinvoke.HookProc HookDelegate;

        private static int KeyBoardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT KeyboardStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                if (Pinvoke.GetKeyState(VK_LMENU) != 0 || Pinvoke.GetKeyState(VK_RMENU) != 0 || Pinvoke.GetKeyState(VK_MENU) != 0)
                {
                    if (KeyboardStruct.vkCode == VK_F4) // alt + F4
                        return 1;
                    if (KeyboardStruct.vkCode == VK_TAB) // alt + tab
                        return 1;
                }
                if (KeyboardStruct.vkCode == VK_LWIN || KeyboardStruct.vkCode == VK_RWIN) // windows keys
                    return 1;
                if (KeyboardStruct.vkCode == VK_ESCAPE)
                    return 1;
            }
            return Pinvoke.CallNextHookEx(HookId, nCode, (IntPtr)wParam, lParam);
        }

        public static void InstallHook()
        {
            HookDelegate = new Pinvoke.HookProc(KeyBoardHookProc);
            HookId = (int)Pinvoke.SetWindowsHookEx(Wh_Keyboard_LL, HookDelegate, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
        }

        public static void RemoveHook()
        {
            if (HookId != 0)
                Pinvoke.UnhookWindowsHookEx(HookId);
        }

        private struct KBDLLHOOKSTRUCT
        {
#pragma warning disable 0649
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
#pragma warning restore 0649
        }
    }
}
