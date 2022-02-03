﻿using System;
using System.Collections.Generic;

namespace EnhancePoE.Model
{
   public static class Coordinates
   {
      private static bool CheckForHit( System.Windows.Point pt, System.Windows.Controls.Button btn )
      {
         int clickX = MouseHook.ClickLocationX;
         int clickY = MouseHook.ClickLocationY;

         pt.X--;
         pt.Y--;

         // +1 border thickness
         int btnX = Convert.ToInt32( Math.Ceiling( pt.X + btn.ActualWidth + 1 ) );
         int btnY = Convert.ToInt32( Math.Ceiling( pt.Y + btn.ActualHeight + 1 ) );

         if ( clickX > pt.X
             && clickY > pt.Y
             && clickX < btnX
             && clickY < btnY )
         {
            return true;
         }
         return false;
      }

      private static System.Windows.Point GetCoordinates( System.Windows.Controls.Button item )
      {
         if ( item != null )
         {
            return item.PointToScreen( new System.Windows.Point( 0, 0 ) );
         }
         return new System.Windows.Point( 0, 0 );
      }

      private static bool CheckForHeaderHit( StashTab s )
      {
         int clickX = MouseHook.ClickLocationX;
         int clickY = MouseHook.ClickLocationY;

         var pt = GetTabHeaderCoordinates( s.TabHeader );

         // adjust btn x,y position a bit
         pt.X--;
         pt.Y--;

         int tabX = Convert.ToInt32( Math.Floor( pt.X + s.TabHeader.ActualWidth + 1 ) );
         int tabY = Convert.ToInt32( Math.Floor( pt.Y + s.TabHeader.ActualHeight + 1 ) );
         if ( clickX > pt.X
             && clickY > pt.Y
             && clickX < tabX
             && clickY < tabY )
         {
            return true;
         }
         return false;
      }

      private static bool CheckForEditButtonHit( System.Windows.Controls.Button btn )
      {
         int clickX = MouseHook.ClickLocationX;
         int clickY = MouseHook.ClickLocationY;

         var pt = GetEditButtonCoordinates( btn );

         // adjust btn x,y position a bit
         pt.X--;
         pt.Y--;

         int btnX = Convert.ToInt32( Math.Floor( pt.X + btn.ActualWidth + 1 ) );
         int btnY = Convert.ToInt32( Math.Floor( pt.Y + btn.ActualHeight + 1 ) );
         if ( clickX > pt.X
             && clickY > pt.Y
             && clickX < btnX
             && clickY < btnY )
         {
            return true;
         }
         return false;
      }

      private static System.Windows.Point GetTabHeaderCoordinates( System.Windows.Controls.TextBlock item )
      {
         if ( item != null )
         {
            return item.PointToScreen( new System.Windows.Point( 0, 0 ) );
         }
         return new System.Windows.Point( 0, 0 );
      }

      private static System.Windows.Point GetEditButtonCoordinates( System.Windows.Controls.Button button )
      {
         if ( button != null )
         {
            return button.PointToScreen( new System.Windows.Point( 0, 0 ) );
         }
         return new System.Windows.Point( 0, 0 );
      }

      private static List<Cell> GetAllActiveCells( int index )
      {
         var activeCells = new List<Cell>();
         foreach ( var cell in StashTabList.StashTabs[index].OverlayCellsList )
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
         if ( MainWindow.stashTabOverlay.IsOpen )
         {
            int selectedIndex = MainWindow.stashTabOverlay.StashTabOverlayTabControl.SelectedIndex;
            bool isHit = false;
            int hitIndex = -1;

            var activeCells = GetAllActiveCells( selectedIndex );

            var buttonList = new List<ButtonAndCell>();

            if ( CheckForEditButtonHit( MainWindow.stashTabOverlay.EditModeButton ) )
            {
               MainWindow.stashTabOverlay.HandleEditButton();
            }

            if ( StashTabList.StashTabs[selectedIndex].Quad )
            {
               var ctrl = MainWindow.stashTabOverlay.StashTabOverlayTabControl.SelectedContent as UserControls.DynamicGridControlQuad;
               foreach ( var cell in activeCells )
               {
                  buttonList.Add( new ButtonAndCell
                  {
                     Button = ctrl.GetButtonFromCell( cell ),
                     Cell = cell
                  } );
               }
               for ( int b = 0; b < buttonList.Count; b++ )
               {
                  if ( CheckForHit( GetCoordinates( buttonList[b].Button ), buttonList[b].Button ) )
                  {
                     isHit = true;
                     hitIndex = b;
                  }
               }

               if ( isHit )
               {
                  Data.ActivateNextCell( true, buttonList[hitIndex].Cell );
               }

               for ( int stash = 0; stash < StashTabList.StashTabs.Count; stash++ )
               {
                  if ( CheckForHeaderHit( StashTabList.StashTabs[stash] ) )
                  {
                     MainWindow.stashTabOverlay.StashTabOverlayTabControl.SelectedIndex = stash;
                  }
               }
            }
            else
            {
               var ctrl = MainWindow.stashTabOverlay.StashTabOverlayTabControl.SelectedContent as UserControls.DynamicGridControl;
               foreach ( var cell in activeCells )
               {
                  buttonList.Add( new ButtonAndCell
                  {
                     Button = ctrl.GetButtonFromCell( cell ),
                     Cell = cell
                  } );
               }
               for ( int b = 0; b < buttonList.Count; b++ )
               {
                  if ( CheckForHit( GetCoordinates( buttonList[b].Button ), buttonList[b].Button ) )
                  {
                     isHit = true;
                     hitIndex = b;
                  }
               }

               if ( isHit )
               {
                  Data.ActivateNextCell( true, buttonList[hitIndex].Cell );
               }
               for ( int stash = 0; stash < StashTabList.StashTabs.Count; stash++ )
               {
                  if ( CheckForHeaderHit( StashTabList.StashTabs[stash] ) )
                  {
                     MainWindow.stashTabOverlay.StashTabOverlayTabControl.SelectedIndex = stash;
                  }
               }
            }
         }
      }
   }

   internal class ButtonAndCell
   {
      public System.Windows.Controls.Button Button { get; set; }
      public Cell Cell { get; set; }
   }
}
