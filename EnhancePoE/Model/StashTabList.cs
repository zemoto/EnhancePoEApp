using System;
using System.Collections.Generic;
using System.Windows;

namespace EnhancePoE.Model
{
   internal static class StashTabList
   {
      public static List<StashTab> StashTabs { get; set; } = new List<StashTab>();
      public static List<int> StashTabIndices { get; set; }
      public static void GetStashTabIndices()
      {
         if ( Properties.Settings.Default.StashTabIndices != "" )
         {
            var ret = new List<int>();
            string indices = Properties.Settings.Default.StashTabIndices;
            string[] sep = { "," };
            foreach ( string s in indices.Split( sep, StringSplitOptions.None ) )
            {
               if ( int.TryParse( s.Trim(), out int parsedIndex ) )
               {
                  if ( !ret.Contains( parsedIndex ) )
                  {
                     ret.Add( parsedIndex );
                  }
               }
               else
               {
                  _ = MessageBox.Show( "Stashtab Index has to be a number!", "Stashtab Error", MessageBoxButton.OK, MessageBoxImage.Error );
               }
            }
            if ( ret.Count == 0 ) { _ = MessageBox.Show( "Stashtab Indices empty!", "Stashtab Error", MessageBoxButton.OK, MessageBoxImage.Error ); }
            StashTabIndices = ret;
         }
         else
         {
            _ = MessageBox.Show( "Stashtab Indices empty!", "Stashtab Error", MessageBoxButton.OK, MessageBoxImage.Error );
         }
      }
   }
}
