﻿using System;
using System.Runtime.InteropServices;

namespace EnhancePoE.Model
{
   internal static class Win32
   {
      //Rest of this code in located in the class itself
      public const int WS_EX_TRANSPARENT = 0x00000020;
      public const int GWL_EXSTYLE = -20;

      [DllImport( "user32.dll" )]
      public static extern int GetWindowLong( IntPtr hwnd,
      int index );

      [DllImport( "user32.dll" )]
      public static extern int SetWindowLong( IntPtr hwnd,
      int index, int newStyle );

      public static void makeTransparent( IntPtr hwnd )
      {
         // Change the extended window style to include WS_EX_TRANSPARENT
         int extendedStyle = GetWindowLong( hwnd, GWL_EXSTYLE );
         _ = SetWindowLong( hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT );
      }

      public static void makeNormal( IntPtr hwnd )
      {
         int extendedStyle = GetWindowLong( hwnd, GWL_EXSTYLE );
         _ = SetWindowLong( hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT );
      }
   }
}
