using System;
using System.Collections.Generic;
using System.Linq;

namespace EnhancePoE.Model
{
   public class ItemSet
   {
      public List<Item> ItemList { get; set; } = new List<Item>();
      // We'll use the list to check which types we still needs to add to the set. We'll need some logic to extract that from the item icon path but that should be possible
      public List<string> EmptyItemSlots { get; set; } = new List<string>() { "BodyArmours", "TwoHandWeapons", "OneHandWeapons", "OneHandWeapons", "Helmets", "Gloves", "Boots", "Belts", "Rings", "Rings", "Amulets" };
      public bool HasChaos { get; set; }
      public List<int> CurrentPosition { get; set; } = new List<int> { 0, 0, 0 };
      public string InfluenceType { get; set; }
      public bool AddItem( Item item )
      {
         if ( EmptyItemSlots.Contains( item.ItemType ) )
         {
            if ( item.ItemType == "OneHandWeapons" )
            {
               _ = EmptyItemSlots.Remove( "TwoHandWeapons" );
            }
            else if ( item.ItemType == "TwoHandWeapons" )
            {
               _ = EmptyItemSlots.Remove( "OneHandWeapons" );
               _ = EmptyItemSlots.Remove( "OneHandWeapons" );
            }
            if ( item.ilvl <= 74 )
            {
               HasChaos = true;
            }
            _ = EmptyItemSlots.Remove( item.ItemType );
            ItemList.Add( item );
            CurrentPosition[0] = item.x;
            CurrentPosition[1] = item.y;
            CurrentPosition[2] = item.StashTabIndex;
            return true;
         }
         return false;
      }
      public void OrderItems()
      {
         var orderedClasses = new List<string>() { "BodyArmours", "TwoHandWeapons", "OneHandWeapons", "OneHandWeapons", "Helmets", "Gloves", "Boots", "Belts", "Rings", "Rings", "Amulets" };
         ItemList = ItemList.OrderBy( d => orderedClasses.IndexOf( d.ItemType ) ).ToList();
      }

      public double GetItemDistance( Item item )
      {
         return item.StashTabIndex != CurrentPosition[2]
             ? 40
             : Math.Sqrt( Math.Pow( item.x - CurrentPosition[0], 2 ) + Math.Pow( item.y - CurrentPosition[1], 2 ) );
      }

      public bool IsValidItem( Item item ) => EmptyItemSlots.Contains( item.ItemType );

      public string GetNextItemClass() => EmptyItemSlots[0];
   }
}
