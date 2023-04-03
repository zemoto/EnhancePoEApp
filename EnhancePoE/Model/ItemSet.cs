﻿using EnhancePoE.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnhancePoE.Model;

internal sealed class ItemSet
{
   public List<Item> ItemList { get; }
   public List<string> EmptyItemSlots { get; }

   public ItemSet()
   {
      ItemList = new();
      EmptyItemSlots = new List<string>() { "BodyArmours", "TwoHandWeapons", "OneHandWeapons", "OneHandWeapons", "Helmets", "Gloves", "Boots", "Belts", "Rings", "Rings", "Amulets" };
   }

   public ItemSet( ItemSet other )
   {
      ItemList = other.ItemList;
      EmptyItemSlots = other.EmptyItemSlots;
   }

   public bool AddItem( Item item )
   {
      if ( !EmptyItemSlots.Contains( item.ItemType ) )
      {
         return false;
      }

      if ( item.ItemType == "OneHandWeapons" )
      {
         _ = EmptyItemSlots.Remove( "TwoHandWeapons" );
      }
      else if ( item.ItemType == "TwoHandWeapons" )
      {
         _ = EmptyItemSlots.Remove( "OneHandWeapons" );
         _ = EmptyItemSlots.Remove( "OneHandWeapons" );
      }

      _ = EmptyItemSlots.Remove( item.ItemType );

      ItemList.Add( item );

      return true;
   }

   public double GetItemDistance( Item item )
   {
      var lastItemAdded = ItemList.LastOrDefault();
      return lastItemAdded is null ? 0 : Math.Sqrt( Math.Pow( item.x - lastItemAdded.x, 2 ) + Math.Pow( item.y - lastItemAdded.y, 2 ) );
   }

   public bool NeedsItem( Item item ) => EmptyItemSlots.Contains( item.ItemType );
}
