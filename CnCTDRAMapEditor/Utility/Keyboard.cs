//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// List of scan codes for standard 104-key keyboard US English keyboard,
    /// from https://stackoverflow.com/questions/2569268/net-difference-between-right-shift-and-left-shift-keys
    /// and https://handmade.network/forums/articles/t/2823-keyboard_inputs_-_scancodes%252C_raw_input%252C_text_input%252C_key_names
    /// </summary>
    public enum OemScanCode
    {
        None           /**/ = 0x00,
        Escape         /**/ = 0x01,
        /// <summary>1 !</summary>
        Number1        /**/ = 0x02,
        /// <summary>2 @</summary>
        Number2        /**/ = 0x03,
        /// <summary>3 #</summary>
        Number3        /**/ = 0x04,
        /// <summary>4 $</summary>
        Number4        /**/ = 0x05,
        /// <summary>5 %</summary>
        Number5        /**/ = 0x06,
        /// <summary>6 ^</summary>
        Number6        /**/ = 0x07,
        /// <summary>7 &amp;</summary>
        Number7        /**/ = 0x08,
        /// <summary>8 *</summary>
        Number8        /**/ = 0x09,
        /// <summary>9 (</summary>
        Number9        /**/ = 0x0A,
        /// <summary>0 )</summary>
        Number0        /**/ = 0x0B,
        /// <summary>- _</summary>
        MinusDash      /**/ = 0x0C,
        /// <summary>= +</summary>
        Equals         /**/ = 0x0D,
        Backspace      /**/ = 0x0E,
        Tab            /**/ = 0x0F,
        Q              /**/ = 0x10,
        W              /**/ = 0x11,
        E              /**/ = 0x12,
        R              /**/ = 0x13,
        T              /**/ = 0x14,
        Y              /**/ = 0x15,
        U              /**/ = 0x16,
        I              /**/ = 0x17,
        O              /**/ = 0x18,
        P              /**/ = 0x19,
        /// <summary>[ {</summary>
        LeftBracket    /**/ = 0x1A,
        /// <summary>] }</summary>
        RightBracket   /**/ = 0x1B,
        Enter          /**/ = 0x1C,
        LeftControl    /**/ = 0x1D,
        A              /**/ = 0x1E,
        S              /**/ = 0x1F,
        D              /**/ = 0x20,
        F              /**/ = 0x21,
        G              /**/ = 0x22,
        H              /**/ = 0x23,
        J              /**/ = 0x24,
        K              /**/ = 0x25,
        L              /**/ = 0x26,
        /// <summary>; :</summary>
        SemiColon      /**/ = 0x27,
        /// <summary>' "</summary>
        Quotes         /**/ = 0x28,
        /// <summary>` ~</summary>
        BacktickTilde  /**/ = 0x29,
        LeftShift      /**/ = 0x2A,
        /// <summary>| \</summary>
        Pipe           /**/ = 0x2B,
        Z              /**/ = 0x2C,
        X              /**/ = 0x2D,
        C              /**/ = 0x2E,
        V              /**/ = 0x2F,
        B              /**/ = 0x30,
        N              /**/ = 0x31,
        M              /**/ = 0x32,
        /// <summary>, &lt;</summary>
        Comma          /**/ = 0x33,
        /// <summary>. &gt;</summary>
        Period         /**/ = 0x34,
        /// <summary>/ ?</summary>
        Slash          /**/ = 0x35,
        RightShift     /**/ = 0x36,
        NumPadAsterisk /**/ = 0x37,
        LeftAlt        /**/ = 0x38,
        SpaceBar       /**/ = 0x39,
        CapsLock       /**/ = 0x3A,
        F1             /**/ = 0x3B,
        F2             /**/ = 0x3C,
        F3             /**/ = 0x3D,
        F4             /**/ = 0x3E,
        F5             /**/ = 0x3F,
        F6             /**/ = 0x40,
        F7             /**/ = 0x41,
        F8             /**/ = 0x42,
        F9             /**/ = 0x43,
        F10            /**/ = 0x44,
        PauseBreak     /**/ = 0x45,
        ScrollLock     /**/ = 0x46,
        NumPad7        /**/ = 0x47,
        NumPad8        /**/ = 0x48,
        NumPad9        /**/ = 0x49,
        NumPadMinus    /**/ = 0x4A,
        NumPad4        /**/ = 0x4B,
        NumPad5        /**/ = 0x4C,
        NumPad6        /**/ = 0x4D,
        NumPadPlus     /**/ = 0x4E,
        NumPad1        /**/ = 0x4F,
        NumPad2        /**/ = 0x50,
        NumPad3        /**/ = 0x51,
        NumPad0        /**/ = 0x52,
        NumPadDecimal  /**/ = 0x53,
        /// <summary> Alt + print screen. MapVirtualKeyEx( VK_SNAPSHOT, MAPVK_VK_TO_VSC_EX, 0 ) returns scancode 0x54.</summary>
        AltPrintScreen /**/ = 0x54,
        //---
        /// <summary>Key between the left shift and Z.</summary>
        BracketAngle   /**/ = 0x56,
        F11            /**/ = 0x57,
        F12            /**/ = 0x58,
        //---
        OEM1           /**/ = 0x5A, /* VK_OEM_WSCTRL */
        OEM2           /**/ = 0x5B, /* VK_OEM_FINISH */
        OEM3           /**/ = 0x5C, /* VK_OEM_JUMP */
        EraseEOF       /**/ = 0x5D,
        OEM4           /**/ = 0x5E, /* VK_OEM_BACKTAB */
        OEM5           /**/ = 0x5F, /* VK_OEM_AUTO */
        Zoom           /**/ = 0x62,
        Help           /**/ = 0x63,
        F13            /**/ = 0x64,
        F14            /**/ = 0x65,
        F15            /**/ = 0x66,
        F16            /**/ = 0x67,
        F17            /**/ = 0x68,
        F18            /**/ = 0x69,
        F19            /**/ = 0x6A,
        F20            /**/ = 0x6B,
        F21            /**/ = 0x6C,
        F22            /**/ = 0x6D,
        F23            /**/ = 0x6E,
        OEM6           /**/ = 0x6F, /* VK_OEM_PA3 */
        Katakana       /**/ = 0x70,
        OEM7           /**/ = 0x71, /* VK_OEM_RESET */
        //---
        F24            /**/ = 0x76,
        SBCSChar       /**/ = 0x77,
        //---
        Convert        /**/ = 0x79,
        //---
        Nonconvert     /**/ = 0x7B, /* VK_OEM_PA1 */
        //---
        RControl       /**/ = 0x11D,
        NumPadEnter    /**/ = 0x11C,
        //---
        NumPadSlash    /**/ = 0x135,
        //---
        PrintScreen    /**/ = 0x137,
        RightAlt       /**/ = 0x138,
        //---
        NumLock        /**/ = 0x145,
        //---
        Home           /**/ = 0x147,
        UpArrow        /**/ = 0x148,
        PageUp         /**/ = 0x149,
        //---
        LeftArrow      /**/ = 0x14B,
        //---
        RightArrow     /**/ = 0x14D,
        //---
        End            /**/ = 0x14F,
        DownArrow      /**/ = 0x150,
        PageDown       /**/ = 0x151,
        Insert         /**/ = 0x152,
        Delete         /**/ = 0x153,
        //---
        LeftWindows    /**/ = 0x15B,
        RightWindows   /**/ = 0x15C,
        /// <summary>The menu key thingy</summary>
        Application    /**/ = 0x15D,
    }

    public class Keyboard
    {
        /// <summary>Shift key</summary>
        const int VK_SHIFT   /**/ = 0x10;
        /// <summary>Control key</summary>
        const int VK_CONTROL /**/ = 0x11;
        /// <summary>Alt key</summary>
        const int VK_MENU    /**/ = 0x12;


        public static OemScanCode GetScanCodeFromLParam(int lParam)
        {
            return (OemScanCode)((lParam >> 16) & 0x1FF);
        }

        public static OemScanCode GetScanCode(Message msg)
        {
            return GetScanCodeFromLParam((int)msg.LParam);
        }

        public static bool HasShift(Message msg)
        {
            return (int)msg.WParam == VK_SHIFT;
        }

        public static bool HasControl(Message msg)
        {
            return (int)msg.WParam == VK_CONTROL;
        }

        public static bool HasAlt(Message msg)
        {
            return (int)msg.WParam == VK_MENU;
        }
    }

}
