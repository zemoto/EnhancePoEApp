using EnhancePoE.UI;
using EnhancePoE.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnhancePoE.Model
{
   internal static class Coordinates
   {
      private static bool CheckForHit( FrameworkElement frameworkElement )
      {
         var pt = GetLocationOfVisual( frameworkElement );
         int clickX = MouseHook.ClickLocationX;
         int clickY = MouseHook.ClickLocationY;

         pt.X--;
         pt.Y--;

         // +1 border thickness
         int btnX = Convert.ToInt32( Math.Ceiling( pt.X + frameworkElement.ActualWidth + 1 ) );
         int btnY = Convert.ToInt32( Math.Ceiling( pt.Y + frameworkElement.ActualHeight + 1 ) );

         return clickX > pt.X
             && clickY > pt.Y
             && clickX < btnX
             && clickY < btnY;
      }

      private static List<Cell> GetAllActiveCells()
      {
         var activeCells = new List<Cell>();
         foreach ( var cell in MainWindow.Instance.SelectedStashTab.OverlayCellsList )
         {
            if ( cell.Active )
            {
               activeCells.Add( cell );
            }
         }
         return activeCells;
      }

      // mouse hook action
      public static void Event( object sender, EventArgs e ) => OverlayClickEvent();

      private static void OverlayClickEvent()
      {
         if ( !MainWindow.StashTabOverlay.IsOpen )
         {
            return;
         }

         var activeCells = GetAllActiveCells();

         if ( CheckForHit( MainWindow.StashTabOverlay.EditModeButton ) )
         {
            MainWindow.StashTabOverlay.HandleEditButton();
         }
         else
         {
            var ctrl = (ItemsControl)MainWindow.StashTabOverlay.StashTabOverlayTabControl.SelectedContent;
            for ( int i = 0; i < activeCells.Count; i++ )
            {
               if ( CheckForHit( GetButtonFromCell( ctrl, activeCells[i] ) ) )
               {
                  Data.OnItemCellClicked( activeCells[i] );
                  return;
               }
            }
         }
      }

      public static Button GetButtonFromCell( ItemsControl itemsControl, object cell )
      {
         for ( int i = 0; i < itemsControl.Items.Count; i++ )
         {
            if ( itemsControl.Items[i] == cell )
            {
               var container = itemsControl.ItemContainerGenerator.ContainerFromIndex( i );
               return Utility.GetChild<Button>( container );
            }
         }

         return null;
      }

      private static Point GetLocationOfVisual( Visual visual ) => visual is not null ? visual.PointToScreen( new Point( 0, 0 ) ) : new Point( 0, 0 );
   }
}
