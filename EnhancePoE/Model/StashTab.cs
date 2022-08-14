using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnhancePoE.Model
{
   public class StashTab : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      protected void OnPropertyChanged( string name = null ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );

      public Uri StashTabUri { get; set; }
      public List<Item> ItemList { get; set; }
      public List<Item> ItemListChaos { get; } = new List<Item>();
      public List<Item> ItemListShaper { get; } = new List<Item>();
      public List<Item> ItemListElder { get; } = new List<Item>();
      public List<Item> ItemListWarlord { get; } = new List<Item>();
      public List<Item> ItemListCrusader { get; } = new List<Item>();
      public List<Item> ItemListHunter { get; } = new List<Item>();
      public List<Item> ItemListRedeemer { get; } = new List<Item>();

      public ObservableCollection<Cell> OverlayCellsList { get; set; } = new ObservableCollection<Cell>();

      // used for registering clicks on tab headers
      public TextBlock TabHeader { get; set; }
      public string TabName { get; set; }
      public bool Quad { get; set; }
      public int TabIndex { get; set; }

      private SolidColorBrush _tabHeaderColor = Brushes.Transparent;
      public SolidColorBrush TabHeaderColor
      {
         get => _tabHeaderColor;
         set
         {
            _tabHeaderColor = value;
            OnPropertyChanged( nameof( TabHeaderColor ) );
         }
      }

      private Thickness _tabHeaderWidth = new( Properties.Settings.Default.TabHeaderWidth, 2, Properties.Settings.Default.TabHeaderWidth, 2 );
      public Thickness TabHeaderWidth
      {
         get => _tabHeaderWidth;
         set
         {
            if ( value != _tabHeaderWidth )
            {
               _tabHeaderWidth = value;
               OnPropertyChanged( nameof( TabHeaderWidth ) );
            }
         }
      }

      public StashTab( string name, int index )
      {
         TabName = name;
         TabIndex = index;
      }

      private void Generate2dArr( int size )
      {
         for ( int i = 0; i < size; i++ )
         {
            for ( int j = 0; j < size; j++ )
            {
               OverlayCellsList.Add( new Cell
               {
                  Active = false,
                  XIndex = j,
                  YIndex = i
               } );
            }
         }
      }

      public void PrepareOverlayList()
      {
         int size = Quad ? 24 : 12;
         Generate2dArr( size );
      }

      public void CleanItemList()
      {
         ItemListChaos.Clear();
         ItemListShaper.Clear();
         ItemListElder.Clear();
         ItemListCrusader.Clear();
         ItemListWarlord.Clear();
         ItemListHunter.Clear();
         ItemListRedeemer.Clear();

         // for loop backwards for deleting from list 
         for ( int i = ItemList.Count - 1; i > -1; i-- )
         {
            if ( ItemList[i].identified && !Properties.Settings.Default.IncludeIdentified )
            {
               ItemList.RemoveAt( i );
               continue;
            }
            if ( ItemList[i].frameType != 2 )
            {
               ItemList.RemoveAt( i );
               continue;
            }

            ItemList[i].GetItemClass();
            if ( ItemList[i].ItemType == null )
            {
               ItemList.RemoveAt( i );
               continue;
            }

            ItemList[i].StashTabIndex = TabIndex;
            //exalted recipe every ilvl allowed, same bases, sort in itemlists
            if ( Properties.Settings.Default.ExaltedRecipe && ItemList[i].influences != null )
            {
               if ( ItemList[i].influences.shaper ) { ItemListShaper.Add( ItemList[i] ); }
               else if ( ItemList[i].influences.elder ) { ItemListElder.Add( ItemList[i] ); }
               else if ( ItemList[i].influences.warlord ) { ItemListWarlord.Add( ItemList[i] ); }
               else if ( ItemList[i].influences.crusader ) { ItemListCrusader.Add( ItemList[i] ); }
               else if ( ItemList[i].influences.hunter ) { ItemListHunter.Add( ItemList[i] ); }
               else if ( ItemList[i].influences.redeemer ) { ItemListRedeemer.Add( ItemList[i] ); }

               ItemList.RemoveAt( i );
               continue;
            }
            if ( !Properties.Settings.Default.ChaosRecipe && !Properties.Settings.Default.RegalRecipe )
            {
               ItemList.RemoveAt( i );
               continue;
            }
            if ( ItemList[i].ilvl < 60 )
            {
               ItemList.RemoveAt( i );
               continue;
            }
            if ( Properties.Settings.Default.RegalRecipe && ItemList[i].ilvl < 75 )
            {
               ItemList.RemoveAt( i );
               continue;
            }
            if ( ItemList[i].ilvl <= 74 )
            {
               ItemListChaos.Add( ItemList[i] );
               ItemList.RemoveAt( i );
            }
         }
      }

      public void DeactivateItemCells()
      {
         foreach ( var cell in OverlayCellsList )
         {
            cell.Active = false;
         }
      }

      public void DeactivateSingleItemCells( Item item )
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
                  cell.Active = false;
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
                  cell.Active = true;
                  cell.CellItem = item;
                  cell.TabIndex = TabIndex;
               }
            }
         }
      }

      public void MarkNextItem( Item item )
      {
         foreach ( var cell in OverlayCellsList )
         {
            if ( cell.CellItem == item )
            {
               cell.ButtonName = "X";
            }
         }
      }

      public void ShowNumbersOnActiveCells( int index )
      {
         index++;
         foreach ( var cell in OverlayCellsList )
         {
            if ( cell.Active )
            {
               cell.ButtonName = index.ToString();
            }
         }
      }
   }
}
