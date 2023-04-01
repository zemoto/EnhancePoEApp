using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnhancePoE.Model;
using EnhancePoE.UI;

namespace EnhancePoE
{
   internal sealed class ItemSetManager
   {
      private readonly List<ItemSet> _itemSetList = new();

      public CancellationTokenSource CancelTokenSource { get; private set; }

      public ItemSetData Data { get; } = new();

      public void ResetCancelToken()
      {
         CancelTokenSource?.Dispose();
         CancelTokenSource = new CancellationTokenSource();
      }

      public void CheckActives()
      {
         try
         {
            if ( ApiAdapter.FetchError )
            {
               Data.WarningMessage = "Fetching Error...";
               return;
            }

            if ( Properties.Settings.Default.ShowItemAmount != 0 )
            {
               CalculateItemAmounts();
            }

            GenerateItemSets();

            // check for full sets
            int fullSets = 0;
            // unique missing item classes
            var missingItemClasses = new HashSet<string>();
            var deactivatedItemClasses = new List<string> { "Helmets", "BodyArmours", "Gloves", "Boots", "Rings", "Amulets", "Belts", "OneHandWeapons", "TwoHandWeapons" };

            foreach ( var set in _itemSetList )
            {
               if ( set.EmptyItemSlots.Count == 0 )
               {
                  fullSets++;
               }
               else
               {
                  // all classes which are active over all ilvls
                  foreach ( string itemClass in set.EmptyItemSlots )
                  {
                     _ = missingItemClasses.Add( itemClass );
                  }
               }
            }

            if ( fullSets == Properties.Settings.Default.Sets )
            {
               Data.GlovesActive = false;
               Data.HelmetActive = false;
               Data.ChestActive = false;
               Data.BootsActive = false;
               Data.WeaponActive = false;
               Data.RingActive = false;
               Data.AmuletActive = false;
               Data.BeltActive = false;
            }
            else
            {
               // activate missing classes
               foreach ( string itemClass in missingItemClasses )
               {
                  switch ( itemClass )
                  {
                     case "BodyArmours":
                        Data.ChestActive = true;
                        break;
                     case "Helmets":
                        Data.HelmetActive = true;
                        break;
                     case "Gloves":
                        Data.GlovesActive = true;
                        break;
                     case "Boots":
                        Data.BootsActive = true;
                        break;
                     case "Rings":
                        Data.RingActive = true;
                        break;
                     case "Amulets":
                        Data.AmuletActive = true;
                        break;
                     case "Belts":
                        Data.BeltActive = true;
                        break;
                     case "OneHandWeapons":
                     case "TwoHandWeapons":
                        Data.WeaponActive = true;
                        _ = deactivatedItemClasses.Remove( "OneHandWeapons" );
                        _ = deactivatedItemClasses.Remove( "TwoHandWeapons" );
                        break;
                  }
                  _ = deactivatedItemClasses.Remove( itemClass );
               }

               //deactivate rest
               foreach ( string itemClass in deactivatedItemClasses )
               {
                  switch ( itemClass )
                  {
                     case "BodyArmours":
                        Data.ChestActive = false;
                        break;
                     case "Helmets":
                        Data.HelmetActive = false;
                        break;
                     case "Gloves":
                        Data.GlovesActive = false;
                        break;
                     case "Boots":
                        Data.BootsActive = false;
                        break;
                     case "OneHandWeapons":
                     case "TwoHandWeapons":
                        Data.WeaponActive = false;
                        break;
                     case "Rings":
                        Data.RingActive = false;
                        break;
                     case "Amulets":
                        Data.AmuletActive = false;
                        break;
                     case "Belts":
                        Data.BeltActive = false;
                        break;
                  }
               }
            }

            Data.FullSets = fullSets;
         }
         catch ( OperationCanceledException ex ) when ( ex.CancellationToken == CancelTokenSource.Token )
         {
            // cancelled
         }
      }

      public void CalculateItemAmounts()
      {
         var tab = MainWindow.Instance.SelectedStashTab;
         if ( tab is null )
         {
            return;
         }

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
         foreach ( var item in tab.ItemsForChaosRecipe )
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

         Data.UpdateAmounts( amounts );
      }

      public void ActivateAllCellsForNextSet()
      {
         // Sets are filled from first index so if first set has missing items we have no full sets
         if ( _itemSetList.Count == 0 || _itemSetList[0].EmptyItemSlots.Count > 0 )
         {
            return;
         }

         foreach ( var i in _itemSetList[0].ItemList )
         {
            MainWindow.Instance.SelectedStashTab.ActivateItemCells( i );
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
         MainWindow.Instance.SelectedStashTab.DeactivateItemCells( cell.Item );

         if ( _itemSetList[0].ItemList.Count == 0 )
         {
            _itemSetList.RemoveAt( 0 );
            ActivateAllCellsForNextSet();
         }
      }

      private void GenerateItemSets()
      {
         _itemSetList.Clear();
         var stashTab = MainWindow.Instance.SelectedStashTab;

         for ( int i = 0; i < Properties.Settings.Default.Sets; i++ )
         {
            var itemSet = new ItemSet();
            while ( true )
            {
               Item closestMissingItem = null;
               double minDistance = double.PositiveInfinity;

               foreach ( var item in stashTab.ItemsForChaosRecipe.Where( item => itemSet.NeedsItem( item ) && itemSet.GetItemDistance( item ) < minDistance ) )
               {
                  minDistance = itemSet.GetItemDistance( item );
                  closestMissingItem = item;
               }

               if ( closestMissingItem is not null )
               {
                  _ = itemSet.AddItem( closestMissingItem );
                  _ = stashTab.ItemsForChaosRecipe.Remove( closestMissingItem );
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
