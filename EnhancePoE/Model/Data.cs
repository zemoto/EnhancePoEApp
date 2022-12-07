using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using EnhancePoE.Model;

namespace EnhancePoE
{
   public static class Data
   {
      public static ActiveItemTypes ActiveItems { get; set; } = new ActiveItemTypes();
      public static ActiveItemTypes PreviousActiveItems { get; set; }

      public static int SetAmount { get; set; }
      public static int SetTargetAmount { get; set; }

      public static List<ItemSet> ItemSetList { get; set; }
      public static List<ItemSet> ItemSetListHighlight { get; set; } = new List<ItemSet>();

      public static ItemSet ItemSetShaper { get; set; }
      public static ItemSet ItemSetElder { get; set; }
      public static ItemSet ItemSetWarlord { get; set; }
      public static ItemSet ItemSetCrusader { get; set; }
      public static ItemSet ItemSetRedeemer { get; set; }
      public static ItemSet ItemSetHunter { get; set; }

      public static CancellationTokenSource cs { get; set; } = new CancellationTokenSource();
      public static CancellationToken ct { get; set; } = cs.Token;

      public static void GetSetTargetAmount( StashTab stash )
      {
         if ( Properties.Settings.Default.Sets > 0 )
         {
            SetTargetAmount = Properties.Settings.Default.Sets;
         }
         else
         {
            if ( stash.Quad )
            {
               SetTargetAmount += 16;
            }
            else
            {
               SetTargetAmount += 4;
            }
         }
      }

      private static void GenerateInfluencedItemSets()
      {
         ItemSetShaper = new ItemSet { InfluenceType = "shaper" };
         ItemSetElder = new ItemSet { InfluenceType = "elder" };
         ItemSetWarlord = new ItemSet { InfluenceType = "warlord" };
         ItemSetHunter = new ItemSet { InfluenceType = "hunter" };
         ItemSetCrusader = new ItemSet { InfluenceType = "crusader" };
         ItemSetRedeemer = new ItemSet { InfluenceType = "redeemer" };
      }

      private static void GenerateItemSetList()
      {
         var ret = new List<ItemSet>();
         for ( int i = 0; i < SetTargetAmount; i++ )
         {
            ret.Add( new ItemSet() );
         }
         ItemSetList = ret;
         Trace.WriteLine( ItemSetList.Count, "item set list count" );
         if ( Properties.Settings.Default.ExaltedRecipe )
         {
            GenerateInfluencedItemSets();
         }
      }

      // tries to add item, if item added returns
      private static bool AddItemToItemSet( ItemSet set, bool chaosItems = false, bool honorOrder = true )
      {
         string listName;
         switch ( chaosItems )
         {
            case false:
               listName = "ItemList";
               break;
            case true:
               listName = "ItemListChaos";
               break;
            default:
               throw new Exception( "How did you manage to provide a value neither true or false for a boolean?" );
         }

         Item minItem = null;
         double minDistance = double.PositiveInfinity;

         // TODO: crashes here after some time
         var stashTab = MainWindow.Instance.SelectedStashTab;
         foreach ( var i in (List<Item>)Utility.GetPropertyValue( stashTab, listName ) )
         {
            if ( set.GetNextItemClass() == i.ItemType || ( !honorOrder && set.IsValidItem( i ) ) )
            {
               if ( set.GetItemDistance( i ) < minDistance )
               {
                  minDistance = set.GetItemDistance( i );
                  minItem = i;
               }
            }
         }

         if ( minItem != null )
         {
            _ = set.AddItem( minItem );
            _ = ( (List<Item>)Utility.GetPropertyValue( stashTab, listName ) ).Remove( minItem );
            return true;
         }
         else
         {
            // Looks ugly but in case we allow TwoHandWeapons we need to consider that adding a 1H fails but we might have a 2H (this only applies if we honor the order)
            if ( honorOrder )
            {
               string nextItemType = set.GetNextItemClass();
               if ( nextItemType == "TwoHandWeapons" )
               {
                  nextItemType = "OneHandWeapons";
                  foreach ( var i in (List<Item>)Utility.GetPropertyValue( stashTab, listName ) )
                  {
                     if ( nextItemType == i.ItemType && set.GetItemDistance( i ) < minDistance )
                     {
                        minDistance = set.GetItemDistance( i );
                        minItem = i;
                     }
                  }
                  if ( minItem != null )
                  {
                     _ = set.AddItem( minItem );
                     _ = ( (List<Item>)Utility.GetPropertyValue( stashTab, listName ) ).Remove( minItem );
                     return true;
                  }
               }
            }
         }
         return false;
      }

      private static void FillItemSets()
      {
         foreach ( var i in ItemSetList )
         {
            // Try to fill the set in order until one chaos item is present, lastEmptySlots counter prevents infinite loops
            int lastEmptySlots = 0;
            while ( i.EmptyItemSlots.Count > 0 && lastEmptySlots != i.EmptyItemSlots.Count )
            {
               lastEmptySlots = i.EmptyItemSlots.Count;
               if ( !i.HasChaos && !Properties.Settings.Default.RegalRecipe && AddItemToItemSet( i, true ) )
               {
                  continue;
               }
               if ( !AddItemToItemSet( i ) && Properties.Settings.Default.FillWithChaos && !Properties.Settings.Default.RegalRecipe )
               {
                  _ = AddItemToItemSet( i, true );
               }
            }

            /* At this point in time the following conditions may be met, exclusively
             * 1.) We obtained a full set and it contains one chaos item
             * 1.1) We obtained a full set and it contains multiple chaos items (only if filling with chaos items is allowed)
             * 2.) We obtained a full set without a chaos item -> We aren't lacking a regal item in this set but we don't have enough chaos items. 
             * 3.) We couldn't obtain a full set. That means that at least one item slot is missing. We need to check which of the remaining slots we can still fill. We could still be missing a chaos item.
             */
            if ( i.EmptyItemSlots.Count == 0 && ( i.HasChaos || Properties.Settings.Default.RegalRecipe ) )
            {
               // Set full, continue
            }
            else if ( i.EmptyItemSlots.Count > 0 )
            {
               lastEmptySlots = 0;
               while ( i.EmptyItemSlots.Count > 0 && i.EmptyItemSlots.Count != lastEmptySlots )
               {
                  lastEmptySlots = i.EmptyItemSlots.Count;
                  if ( !i.HasChaos && !Properties.Settings.Default.RegalRecipe && AddItemToItemSet( i, true, false ) )
                  {
                     continue;
                  }
                  if ( !AddItemToItemSet( i, false, false ) )
                  {
                     // couldn't add a regal item. Try chaos item if filling with chaos is allowed
                     if ( Properties.Settings.Default.FillWithChaos && !Properties.Settings.Default.RegalRecipe )
                     {
                        _ = AddItemToItemSet( i, true, false );
                     }
                  }
               }
               // At this point the set will contain a chaos item as long as we had at least one left. If not we didn't have any chaos items left.
               // If the set is not full at this time we're missing at least one regal item. If it has not chaos item we're also missing chaos items. 
               // Technically it could be only the chaos item that's missing but that can be neglected since when mixing you'll always be short on chaos items. 
               // If not in "endgame" mode (always show chaos) have the loot filter apply to chaos and regal items the same way.
            }
         }
         if ( Properties.Settings.Default.ExaltedRecipe )
         {
            FillItemSetsInfluenced();
         }
      }

      private static void FillItemSetsInfluenced()
      {
         var tab = MainWindow.Instance.SelectedStashTab;
         foreach ( var i in tab.ItemListShaper )
         {
            if ( ItemSetShaper.EmptyItemSlots.Count == 0 )
            {
               break;
            }
            _ = ItemSetShaper.AddItem( i );
         }
         foreach ( var i in tab.ItemListElder )
         {
            if ( ItemSetElder.EmptyItemSlots.Count == 0 )
            {
               break;
            }
            _ = ItemSetElder.AddItem( i );
         }
         foreach ( var i in tab.ItemListCrusader )
         {
            if ( ItemSetCrusader.EmptyItemSlots.Count == 0 )
            {
               break;
            }
            _ = ItemSetCrusader.AddItem( i );
         }
         foreach ( var i in tab.ItemListWarlord )
         {
            if ( ItemSetWarlord.EmptyItemSlots.Count == 0 )
            {
               break;
            }
            _ = ItemSetWarlord.AddItem( i );
         }
         foreach ( var i in tab.ItemListRedeemer )
         {
            if ( ItemSetRedeemer.EmptyItemSlots.Count == 0 )
            {
               break;
            }
            _ = ItemSetRedeemer.AddItem( i );
         }
         foreach ( var i in tab.ItemListHunter )
         {
            if ( ItemSetHunter.EmptyItemSlots.Count == 0 )
            {
               break;
            }
            _ = ItemSetHunter.AddItem( i );
         }
      }

      public static void CheckActives()
      {
         try
         {
            if ( ApiAdapter.FetchError )
            {
               MainWindow.Overlay.WarningMessage = "Fetching Error...";
               MainWindow.Overlay.ShadowOpacity = 1;
               MainWindow.Overlay.WarningMessageVisibility = System.Windows.Visibility.Visible;
               return;
            }

            bool exaltedActive = Properties.Settings.Default.ExaltedRecipe;

            SetTargetAmount = 0;
            GetSetTargetAmount( MainWindow.Instance.SelectedStashTab );

            if ( Properties.Settings.Default.ShowItemAmount != 0 )
            {
               Trace.WriteLine( "Calculating Items" );
               CalculateItemAmounts();
            }

            GenerateItemSetList();
            FillItemSets();

            // check for full sets/ missing items
            bool missingChaos = false;
            int fullSets = 0;
            // unique missing item classes
            var missingItemClasses = new HashSet<string>();
            var deactivatedItemClasses = new List<string> { "Helmets", "BodyArmours", "Gloves", "Boots", "Rings", "Amulets", "Belts", "OneHandWeapons", "TwoHandWeapons" };

            foreach ( var set in ItemSetList )
            {
               if ( set.EmptyItemSlots.Count == 0 )
               {
                  // fix for: condition (fullSets == SetTargetAmount && missingChaos) never true cause fullsets < settargetamount when missingChaos @ikogan
                  fullSets++;

                  if ( !set.HasChaos && !Properties.Settings.Default.RegalRecipe )
                  {
                     missingChaos = true;
                  }
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


            if ( fullSets == SetTargetAmount && missingChaos )
            {
               Trace.WriteLine( "filter here 1" );
               // activate only chaos items
               ActiveItems.GlovesActive = false;
               ActiveItems.HelmetActive = false;
               ActiveItems.ChestActive = false;
               ActiveItems.BootsActive = false;
               ActiveItems.WeaponActive = false;
               ActiveItems.RingActive = false;
               ActiveItems.AmuletActive = false;
               ActiveItems.BeltActive = false;

               ActiveItems.ChaosMissing = true;
            }
            else if ( fullSets == SetTargetAmount && !missingChaos )
            {
               Trace.WriteLine( "filter here 2" );
               //deactivate all
               ActiveItems.GlovesActive = false;
               ActiveItems.HelmetActive = false;
               ActiveItems.ChestActive = false;
               ActiveItems.BootsActive = false;
               ActiveItems.WeaponActive = false;
               ActiveItems.RingActive = false;
               ActiveItems.AmuletActive = false;
               ActiveItems.BeltActive = false;

               ActiveItems.ChaosMissing = false;
            }
            else
            {
               Trace.WriteLine( "filter here 3" );
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
                  //ActiveItems.ChaosMissing = true;
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

            if ( !Properties.Settings.Default.ChaosRecipe && !Properties.Settings.Default.RegalRecipe )
            {
               sectionList.Clear();
               Trace.WriteLine( "section list cleared" );
            }

            _ = MainWindow.Overlay.Dispatcher.Invoke( () => MainWindow.Overlay.FullSetsText = fullSets.ToString() );

            // invoke chaos missing
            if ( missingChaos && !Properties.Settings.Default.RegalRecipe )
            {
               MainWindow.Overlay.WarningMessage = "Need lower level items!";
               MainWindow.Overlay.ShadowOpacity = 1;
               MainWindow.Overlay.WarningMessageVisibility = System.Windows.Visibility.Visible;
            }

            // invoke exalted recipe ready
            if ( Properties.Settings.Default.ExaltedRecipe )
            {
               if ( ItemSetShaper.EmptyItemSlots.Count == 0
                   || ItemSetElder.EmptyItemSlots.Count == 0
                   || ItemSetCrusader.EmptyItemSlots.Count == 0
                   || ItemSetWarlord.EmptyItemSlots.Count == 0
                   || ItemSetHunter.EmptyItemSlots.Count == 0
                   || ItemSetRedeemer.EmptyItemSlots.Count == 0 )
               {
                  MainWindow.Overlay.WarningMessage = "Exalted Recipe ready!";
                  MainWindow.Overlay.ShadowOpacity = 1;
                  MainWindow.Overlay.WarningMessageVisibility = System.Windows.Visibility.Visible;
               }
            }

            // invoke set full
            if ( fullSets == SetTargetAmount && !missingChaos )
            {
               MainWindow.Overlay.WarningMessage = "Sets full!";
               MainWindow.Overlay.ShadowOpacity = 1;
               MainWindow.Overlay.WarningMessageVisibility = System.Windows.Visibility.Visible;
            }

            Trace.WriteLine( fullSets, "full sets" );
         }
         catch ( OperationCanceledException ex ) when ( ex.CancellationToken == ct )
         {
            Trace.WriteLine( "abort" );
         }
      }

      public static void CalculateItemAmounts()
      {
         var tab = MainWindow.Instance.SelectedStashTab;
         if ( tab is null )
         {
            return;
         }

         Trace.WriteLine( "calculating items amount" );
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
         Trace.WriteLine( "tab amount " + tab.ItemList.Count );
         Trace.WriteLine( "tab amount " + tab.ItemListChaos.Count );
         if ( tab.ItemList.Count > 0 )
         {
            foreach ( var i in tab.ItemList )
            {
               Trace.WriteLine( i.ItemType );
               if ( i.ItemType == "Rings" )
               {
                  amounts[0]++;
               }
               else if ( i.ItemType == "Amulets" )
               {
                  amounts[1]++;
               }
               else if ( i.ItemType == "Belts" )
               {
                  amounts[2]++;
               }
               else if ( i.ItemType == "BodyArmours" )
               {
                  amounts[3]++;
               }
               else if ( i.ItemType == "TwoHandWeapons" )
               {
                  weaponBig++;
               }
               else if ( i.ItemType == "OneHandWeapons" )
               {
                  weaponsSmall++;
               }
               else if ( i.ItemType == "Gloves" )
               {
                  amounts[5]++;
               }
               else if ( i.ItemType == "Helmets" )
               {
                  amounts[6]++;
               }
               else if ( i.ItemType == "Boots" )
               {
                  amounts[7]++;
               }
            }
         }
         if ( tab.ItemListChaos.Count > 0 )
         {
            foreach ( var i in tab.ItemListChaos )
            {
               Trace.WriteLine( i.ItemType );
               if ( i.ItemType == "Rings" )
               {
                  amounts[0]++;
               }
               else if ( i.ItemType == "Amulets" )
               {
                  amounts[1]++;
               }
               else if ( i.ItemType == "Belts" )
               {
                  amounts[2]++;
               }
               else if ( i.ItemType == "BodyArmours" )
               {
                  amounts[3]++;
               }
               else if ( i.ItemType == "TwoHandWeapons" )
               {
                  weaponBig++;
               }
               else if ( i.ItemType == "OneHandWeapons" )
               {
                  weaponsSmall++;
               }
               else if ( i.ItemType == "Gloves" )
               {
                  amounts[5]++;
               }
               else if ( i.ItemType == "Helmets" )
               {
                  amounts[6]++;
               }
               else if ( i.ItemType == "Boots" )
               {
                  amounts[7]++;
               }
            }
         }

         if ( Properties.Settings.Default.ShowItemAmount == 1 )
         {
            Trace.WriteLine( "we are here" );

            // calculate amounts needed for full sets
            foreach ( int a in amounts )
            {
               Trace.WriteLine( a );
            }
            amounts[4] = weaponsSmall + weaponBig;
            MainWindow.Overlay.RingsAmount = amounts[0];
            MainWindow.Overlay.AmuletsAmount = amounts[1];
            MainWindow.Overlay.BeltsAmount = amounts[2];
            MainWindow.Overlay.ChestsAmount = amounts[3];
            MainWindow.Overlay.WeaponsAmount = amounts[4];
            MainWindow.Overlay.GlovesAmount = amounts[5];
            MainWindow.Overlay.HelmetsAmount = amounts[6];
            MainWindow.Overlay.BootsAmount = amounts[7];
         }
         else if ( Properties.Settings.Default.ShowItemAmount == 2 )
         {
            amounts[4] = weaponsSmall + weaponBig;
            MainWindow.Overlay.RingsAmount = Math.Max( ( SetTargetAmount * 2 ) - amounts[0], 0 );
            MainWindow.Overlay.AmuletsAmount = Math.Max( SetTargetAmount - amounts[1], 0 );
            MainWindow.Overlay.BeltsAmount = Math.Max( SetTargetAmount - amounts[2], 0 );
            MainWindow.Overlay.ChestsAmount = Math.Max( SetTargetAmount - amounts[3], 0 );
            MainWindow.Overlay.WeaponsAmount = Math.Max( ( SetTargetAmount * 2 ) - ( weaponsSmall + ( weaponBig * 2 ) ), 0 );
            MainWindow.Overlay.GlovesAmount = Math.Max( SetTargetAmount - amounts[5], 0 );
            MainWindow.Overlay.HelmetsAmount = Math.Max( SetTargetAmount - amounts[6], 0 );
            MainWindow.Overlay.BootsAmount = Math.Max( SetTargetAmount - amounts[7], 0 );
         }
      }

      public static void ActivateNextCell( bool active, Cell cell )
      {
         if ( !active )
         {
            return;
         }

         var stashTab = MainWindow.Instance.SelectedStashTab;
         if ( Properties.Settings.Default.HighlightMode == 0 )
         {
            //activate cell by cell
            stashTab.DeactivateItemCells();

            // remove if itemlist empty
            if ( ItemSetListHighlight.Count > 0 && ItemSetListHighlight[0].ItemList.Count == 0 )
            {
               ItemSetListHighlight.RemoveAt( 0 );
            }

            // next item if itemlist not empty
            if ( ItemSetListHighlight.Count > 0 && ItemSetListHighlight[0].ItemList.Count > 0 && ItemSetListHighlight[0].EmptyItemSlots.Count == 0 )
            {
               var highlightItem = ItemSetListHighlight[0].ItemList[0];
               stashTab.ActivateItemCells( highlightItem );
               ItemSetListHighlight[0].ItemList.RemoveAt( 0 );
            }
         }
         else if ( Properties.Settings.Default.HighlightMode == 1 )
         {
            // activate whole set 
            if ( ItemSetListHighlight.Count > 0 )
            {
               Trace.WriteLine( ItemSetListHighlight[0].ItemList.Count, "item list count" );
               Trace.WriteLine( ItemSetListHighlight.Count, "itemset list ocunt" );
               // check for full sets

               if ( ItemSetListHighlight[0].EmptyItemSlots.Count == 0 )
               {
                  if ( cell != null )
                  {
                     var highlightItem = cell.CellItem;
                     stashTab.DeactivateSingleItemCells( cell.CellItem );
                     _ = ItemSetListHighlight[0].ItemList.Remove( highlightItem );
                  }

                  foreach ( var i in ItemSetListHighlight[0].ItemList )
                  {
                     stashTab.ActivateItemCells( i );
                  }

                  // mark item order
                  if ( ItemSetListHighlight[0] != null && ItemSetListHighlight[0].ItemList.Count > 0 )
                  {
                     stashTab.MarkNextItem( ItemSetListHighlight[0].ItemList[0] );
                  }
                  if ( ItemSetListHighlight[0].ItemList.Count == 0 )
                  {
                     ItemSetListHighlight.RemoveAt( 0 );

                     // activate next set
                     ActivateNextCell( true, null );
                  }
               }
            }
         }
         else if ( Properties.Settings.Default.HighlightMode == 2 )
         {
            //activate all cells at once
            if ( ItemSetListHighlight.Count > 0 )
            {
               foreach ( var set in ItemSetListHighlight )
               {
                  if ( set.EmptyItemSlots.Count == 0 && cell != null )
                  {
                     var highlightItem = cell.CellItem;
                     stashTab.DeactivateSingleItemCells( cell.CellItem );
                     _ = ItemSetListHighlight[0].ItemList.Remove( highlightItem );
                  }
               }
            }
         }
      }

      public static void PrepareSelling()
      {
         ItemSetListHighlight.Clear();
         if ( ApiAdapter.IsFetching )
         {
            return;
         }
         if ( ItemSetList == null )
         {
            return;
         }

         foreach ( var itemSet in ItemSetList )
         {
            itemSet.OrderItems();
         }
         if ( Properties.Settings.Default.ExaltedRecipe )
         {
            ItemSetShaper.OrderItems();
            ItemSetElder.OrderItems();
            ItemSetWarlord.OrderItems();
            ItemSetCrusader.OrderItems();
            ItemSetHunter.OrderItems();
            ItemSetRedeemer.OrderItems();
            if ( ItemSetShaper.EmptyItemSlots.Count == 0 )
            {
               ItemSetListHighlight.Add( new ItemSet
               {
                  ItemList = new List<Item>( ItemSetShaper.ItemList ),
                  EmptyItemSlots = new List<string>( ItemSetShaper.EmptyItemSlots )
               } );
            }
            if ( ItemSetElder.EmptyItemSlots.Count == 0 )
            {
               ItemSetListHighlight.Add( new ItemSet
               {
                  ItemList = new List<Item>( ItemSetElder.ItemList ),
                  EmptyItemSlots = new List<string>( ItemSetElder.EmptyItemSlots )
               } );
            }
            if ( ItemSetCrusader.EmptyItemSlots.Count == 0 )
            {
               ItemSetListHighlight.Add( new ItemSet
               {
                  ItemList = new List<Item>( ItemSetCrusader.ItemList ),
                  EmptyItemSlots = new List<string>( ItemSetCrusader.EmptyItemSlots )
               } );
            }
            if ( ItemSetHunter.EmptyItemSlots.Count == 0 )
            {
               ItemSetListHighlight.Add( new ItemSet
               {
                  ItemList = new List<Item>( ItemSetHunter.ItemList ),
                  EmptyItemSlots = new List<string>( ItemSetHunter.EmptyItemSlots )
               } );
            }
            if ( ItemSetWarlord.EmptyItemSlots.Count == 0 )
            {
               ItemSetListHighlight.Add( new ItemSet
               {
                  ItemList = new List<Item>( ItemSetWarlord.ItemList ),
                  EmptyItemSlots = new List<string>( ItemSetWarlord.EmptyItemSlots )
               } );
            }
            if ( ItemSetRedeemer.EmptyItemSlots.Count == 0 )
            {
               ItemSetListHighlight.Add( new ItemSet
               {
                  ItemList = new List<Item>( ItemSetRedeemer.ItemList ),
                  EmptyItemSlots = new List<string>( ItemSetRedeemer.EmptyItemSlots )
               } );
            }
         }
         foreach ( var set in ItemSetList )
         {
            if ( set.HasChaos || Properties.Settings.Default.RegalRecipe )
            {
               ItemSetListHighlight.Add( new ItemSet
               {
                  ItemList = new List<Item>( set.ItemList ),
                  EmptyItemSlots = new List<string>( set.EmptyItemSlots )
               } );
            }
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
      public bool ChaosMissing { get; set; } = true;
   }

   public class ItemTypeAmounts
   {
      public int Gloves { get; set; }
      public int Helmets { get; set; }
      public int Boots { get; set; }
      public int Chests { get; set; }
      public int Weapons { get; set; }
      public int Amulets { get; set; }
      public int Rings { get; set; }
      public int Belts { get; set; }
   }
}
