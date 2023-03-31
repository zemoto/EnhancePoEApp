using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnhancePoE.Model;
using EnhancePoE.UI;

namespace EnhancePoE
{
   internal static class Data
   {
      public static ActiveItemTypes ActiveItems { get; set; } = new ActiveItemTypes();

      private static readonly List<ItemSet> _itemSetList = new();

      public static CancellationTokenSource cs { get; set; } = new CancellationTokenSource();
      public static CancellationToken ct { get; set; } = cs.Token;

      private static void GenerateItemSets()
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

      public static void CheckActives()
      {
         try
         {
            if ( ApiAdapter.FetchError )
            {
               MainWindow.RecipeOverlay.WarningMessage = "Fetching Error...";
               MainWindow.RecipeOverlay.ShadowOpacity = 1;
               MainWindow.RecipeOverlay.WarningMessageVisibility = System.Windows.Visibility.Visible;
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

            var sectionList = new HashSet<string>();


            if ( fullSets == Properties.Settings.Default.Sets )
            {
               //deactivate all
               ActiveItems.GlovesActive = false;
               ActiveItems.HelmetActive = false;
               ActiveItems.ChestActive = false;
               ActiveItems.BootsActive = false;
               ActiveItems.WeaponActive = false;
               ActiveItems.RingActive = false;
               ActiveItems.AmuletActive = false;
               ActiveItems.BeltActive = false;
            }
            else
            {
               // activate missing classes
               foreach ( string itemClass in missingItemClasses )
               {
                  switch ( itemClass )
                  {
                     case "BodyArmours":
                        ActiveItems.ChestActive = true;
                        break;
                     case "Helmets":
                        ActiveItems.HelmetActive = true;
                        break;
                     case "Gloves":
                        ActiveItems.GlovesActive = true;
                        break;
                     case "Boots":
                        ActiveItems.BootsActive = true;
                        break;
                     case "Rings":
                        ActiveItems.RingActive = true;
                        break;
                     case "Amulets":
                        ActiveItems.AmuletActive = true;
                        break;
                     case "Belts":
                        ActiveItems.BeltActive = true;
                        break;
                     case "OneHandWeapons":
                     case "TwoHandWeapons":
                        ActiveItems.WeaponActive = true;
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
                        ActiveItems.ChestActive = false;
                        break;
                     case "Helmets":
                        ActiveItems.HelmetActive = false;
                        break;
                     case "Gloves":
                        ActiveItems.GlovesActive = false;
                        break;
                     case "Boots":
                        ActiveItems.BootsActive = false;
                        break;
                     case "OneHandWeapons":
                     case "TwoHandWeapons":
                        ActiveItems.WeaponActive = false;
                        break;
                     case "Rings":
                        ActiveItems.RingActive = false;
                        break;
                     case "Amulets":
                        ActiveItems.AmuletActive = false;
                        break;
                     case "Belts":
                        ActiveItems.BeltActive = false;
                        break;
                  }
               }
            }

            _ = MainWindow.RecipeOverlay.Dispatcher.Invoke( () => MainWindow.RecipeOverlay.FullSetsText = fullSets.ToString() );

            // invoke set full
            if ( fullSets == Properties.Settings.Default.Sets )
            {
               MainWindow.RecipeOverlay.WarningMessage = "Sets full!";
               MainWindow.RecipeOverlay.ShadowOpacity = 1;
               MainWindow.RecipeOverlay.WarningMessageVisibility = System.Windows.Visibility.Visible;
            }
         }
         catch ( OperationCanceledException ex ) when ( ex.CancellationToken == ct )
         {
            // cancelled
         }
      }

      public static void CalculateItemAmounts()
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
         // 4: weapons
         // 5: gloves
         // 6: helmets
         // 7: boots
         int[] amounts = new int[8];
         int weaponsSmall = 0;
         int weaponBig = 0;
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
            else if ( item.ItemType == "TwoHandWeapons" )
            {
               weaponBig++;
            }
            else if ( item.ItemType == "OneHandWeapons" )
            {
               weaponsSmall++;
            }
            else if ( item.ItemType == "Gloves" )
            {
               amounts[5]++;
            }
            else if ( item.ItemType == "Helmets" )
            {
               amounts[6]++;
            }
            else if ( item.ItemType == "Boots" )
            {
               amounts[7]++;
            }
         }
         amounts[4] = weaponsSmall + weaponBig;

         if ( Properties.Settings.Default.ShowItemAmount == 1 )
         {
            MainWindow.RecipeOverlay.RingsAmount = amounts[0];
            MainWindow.RecipeOverlay.AmuletsAmount = amounts[1];
            MainWindow.RecipeOverlay.BeltsAmount = amounts[2];
            MainWindow.RecipeOverlay.ChestsAmount = amounts[3];
            MainWindow.RecipeOverlay.WeaponsAmount = amounts[4];
            MainWindow.RecipeOverlay.GlovesAmount = amounts[5];
            MainWindow.RecipeOverlay.HelmetsAmount = amounts[6];
            MainWindow.RecipeOverlay.BootsAmount = amounts[7];
         }
         else if ( Properties.Settings.Default.ShowItemAmount == 2 )
         {
            var setTargetAmount = Properties.Settings.Default.Sets;
            MainWindow.RecipeOverlay.RingsAmount = Math.Max( ( setTargetAmount * 2 ) - amounts[0], 0 );
            MainWindow.RecipeOverlay.AmuletsAmount = Math.Max( setTargetAmount - amounts[1], 0 );
            MainWindow.RecipeOverlay.BeltsAmount = Math.Max( setTargetAmount - amounts[2], 0 );
            MainWindow.RecipeOverlay.ChestsAmount = Math.Max( setTargetAmount - amounts[3], 0 );
            MainWindow.RecipeOverlay.WeaponsAmount = Math.Max( ( setTargetAmount * 2 ) - ( weaponsSmall + ( weaponBig * 2 ) ), 0 );
            MainWindow.RecipeOverlay.GlovesAmount = Math.Max( setTargetAmount - amounts[5], 0 );
            MainWindow.RecipeOverlay.HelmetsAmount = Math.Max( setTargetAmount - amounts[6], 0 );
            MainWindow.RecipeOverlay.BootsAmount = Math.Max( setTargetAmount - amounts[7], 0 );
         }
      }

      public static void ActivateAllCellsForNextSet()
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

      public static void OnItemCellClicked( Cell cell )
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
   }

   public class ActiveItemTypes
   {
      public bool GlovesActive { get; set; } = true;
      public bool HelmetActive { get; set; } = true;
      public bool BootsActive { get; set; } = true;
      public bool ChestActive { get; set; } = true;
      public bool WeaponActive { get; set; } = true;
      public bool RingActive { get; set; } = true;
      public bool AmuletActive { get; set; } = true;
      public bool BeltActive { get; set; } = true;
   }
}
