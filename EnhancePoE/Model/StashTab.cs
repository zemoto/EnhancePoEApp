using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EnhancePoE.Model
{
   internal sealed class StashTab
   {
      public Uri StashTabUri { get; set; }
      public List<Item> ItemsForChaosRecipe { get; } = new List<Item>();

      public ObservableCollection<Cell> OverlayCellsList { get; private set; }

      // used for registering clicks on tab headers
      public string TabName { get; set; }
      public int TabIndex { get; set; }
      public bool Quad { get; set; }

      public StashTab( string name, int index )
      {
         TabName = name;
         TabIndex = index;
      }

      public void InitializeCellList()
      {
         OverlayCellsList = new ObservableCollection<Cell>();

         int size = Quad ? 24 : 12;
         for ( int i = 0; i < size; i++ )
         {
            for ( int j = 0; j < size; j++ )
            {
               OverlayCellsList.Add( new Cell( j, i ) );
            }
         }
      }

      public void FilterItemsForChaosRecipe( List<Item> itemList )
      {
         ItemsForChaosRecipe.Clear();
         foreach( var item in itemList )
         {
            if ( ( item.identified && !Properties.Settings.Default.IncludeIdentified ) || item.frameType != 2 )
            {
               continue;
            }

            item.GetItemClass();
            if ( item.ItemType == null )
            {
               continue;
            }

            if ( item.ilvl >= 60 && item.ilvl <= 74 )
            {
               item.StashTabIndex = TabIndex;
               ItemsForChaosRecipe.Add( item );
            }
         }
      }

      public void DeactivateItemCells()
      {
         foreach ( var cell in OverlayCellsList )
         {
            cell.Deactivate();
         }
      }

      public void DeactivateItemCells( Item item )
      {
         var itemCoordinates = new List<List<int>>();

         for ( int i = 0; i < item.w; i++ )
         {
            for ( int j = 0; j < item.h; j++ )
            {
               itemCoordinates.Add( new List<int> { item.x + i, item.y + j } );
            }
         }

         foreach ( var cell in OverlayCellsList )
         {
            foreach ( var coordinate in itemCoordinates )
            {
               if ( coordinate[0] == cell.XIndex && coordinate[1] == cell.YIndex )
               {
                  cell.Deactivate();
               }
            }
         }
      }

      public void ActivateItemCells( Item item )
      {
         var AllCoordinates = new List<List<int>>();

         for ( int i = 0; i < item.w; i++ )
         {
            for ( int j = 0; j < item.h; j++ )
            {
               AllCoordinates.Add( new List<int> { item.x + i, item.y + j } );
            }
         }

         foreach ( var cell in OverlayCellsList )
         {
            foreach ( var coordinate in AllCoordinates )
            {
               if ( coordinate[0] == cell.XIndex && coordinate[1] == cell.YIndex )
               {
                  cell.Activate( ref item );
               }
            }
         }
      }
   }
}
