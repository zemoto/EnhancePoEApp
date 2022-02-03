﻿using System.Windows;

namespace EnhancePoE.Model.Utils
{
   public static class UserWarning
   {
      public static void WarnUser( string content, string title )
      {
         _ = MessageBox.Show( content, title, MessageBoxButton.OK, MessageBoxImage.Error );
      }
   }
}
