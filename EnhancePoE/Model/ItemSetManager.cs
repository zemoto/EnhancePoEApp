﻿using System.Collections.Generic;
using System.Linq;
using EnhancePoE.Model;
using ZemotoCommon.UI;

namespace EnhancePoE
{
   internal interface ISelectedStashTabHandler
   {
      public StashTab SelectedStashTab { get; set; }
   }

   internal sealed class ItemSetManager : ViewModelBase, ISelectedStashTabHandler
   {
      private readonly List<ItemSet> _itemSetList = new();

      private StashTab _selectedStashTab;
      public StashTab SelectedStashTab
      {
         get => _selectedStashTab;
         set
         {
            if ( SetProperty( ref _selectedStashTab, value ) && _selectedStashTab is not null )
            {
               Properties.Settings.Default.SelectedStashTabName = _selectedStashTab.TabName;
            }
         }
      }

      public string LastError { get; private set; }

      public ItemSetManager() => Properties.Settings.Default.PropertyChanged += OnSettingsChanged;

      private void OnSettingsChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
      {
         if ( e.PropertyName == nameof( Properties.Settings.Sets ) || e.PropertyName == nameof( Properties.Settings.ShowItemAmount ) )
         {
            _selectedStashTab.UpdateDisplay();
         }
      }

      public void UpdateData()
      {
         if ( ApiAdapter.FetchError )
         {
            LastError = "Fetching Error...";
            return;
         }

         if ( _selectedStashTab is null )
         {
            return;
         }

         CalculateItemAmounts();
         GenerateItemSets();
         ActivateAllCellsForNextSet();

         _selectedStashTab.FullSets = _itemSetList.Count( x => x.EmptyItemSlots.Count == 0 );
      }

      private void CalculateItemAmounts()
      {
         // 0: rings
         // 1: amulets
         // 2: belts
         // 3: chests
         // 4: weaponsSmall
         // 5: weaponsBig
         // 6: gloves
         // 7: helmets
         // 8: boots
         int[] amounts = new int[9];
         foreach ( var item in _selectedStashTab.ItemsForChaosRecipe )
         {
            if ( item.ItemType == "Rings" )
            {
               amounts[0]++;
            }
            else if ( item.ItemType == "Amulets" )
            {
               amounts[1]++;
            }
            else if ( item.ItemType == "Belts" )
            {
               amounts[2]++;
            }
            else if ( item.ItemType == "BodyArmours" )
            {
               amounts[3]++;
            }
            else if ( item.ItemType == "OneHandWeapons" )
            {
               amounts[4]++;
            }
            else if ( item.ItemType == "TwoHandWeapons" )
            {
               amounts[5]++;
            }
            else if ( item.ItemType == "Gloves" )
            {
               amounts[6]++;
            }
            else if ( item.ItemType == "Helmets" )
            {
               amounts[7]++;
            }
            else if ( item.ItemType == "Boots" )
            {
               amounts[8]++;
            }
         }

         _selectedStashTab.UpdateAmounts( amounts );
      }

      private void ActivateAllCellsForNextSet()
      {
         // Sets are filled from first index so if first set has missing items we have no full sets
         if ( _itemSetList.Count == 0 || _itemSetList[0].EmptyItemSlots.Count > 0 )
         {
            return;
         }

         foreach ( var i in _itemSetList[0].ItemList )
         {
            _selectedStashTab.ActivateItemCells( i );
         }
      }

      public void OnItemCellClicked( Cell cell )
      {
         // Sets are filled from first index so if first set has missing items we have no full sets
         if ( cell is null || _itemSetList[0].EmptyItemSlots.Count > 0 )
         {
            return;
         }

         _ = _itemSetList[0].ItemList.Remove( cell.Item );
         _selectedStashTab.DeactivateItemCells( cell.Item );

         if ( _itemSetList[0].ItemList.Count == 0 )
         {
            _itemSetList.RemoveAt( 0 );
            ActivateAllCellsForNextSet();
         }
      }

      private void GenerateItemSets()
      {
         _itemSetList.Clear();
         for ( int i = 0; i < Properties.Settings.Default.Sets; i++ )
         {
            var itemSet = new ItemSet();
            while ( true )
            {
               Item closestMissingItem = null;
               double minDistance = double.PositiveInfinity;

               foreach ( var item in _selectedStashTab.ItemsForChaosRecipe.Where( item => itemSet.NeedsItem( item ) && itemSet.GetItemDistance( item ) < minDistance ) )
               {
                  minDistance = itemSet.GetItemDistance( item );
                  closestMissingItem = item;
               }

               if ( closestMissingItem is not null )
               {
                  _ = itemSet.AddItem( closestMissingItem );
                  _ = _selectedStashTab.ItemsForChaosRecipe.Remove( closestMissingItem );
               }
               else
               {
                  break;
               }
            }

            _itemSetList.Add( itemSet );
         }
      }
   }
}
