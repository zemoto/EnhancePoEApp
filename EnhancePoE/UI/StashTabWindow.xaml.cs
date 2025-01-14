﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnhancePoE.Utils;

namespace EnhancePoE.UI;

internal partial class StashTabWindow : Window
{
   public bool IsOpen { get; set; }

   private readonly ItemSetManager _itemSetManager;
   private readonly StashTabWindowViewModel _model;

   public StashTabWindow( ItemSetManager itemSetManager )
   {
      _itemSetManager = itemSetManager;
      DataContext = _model = new StashTabWindowViewModel( _itemSetManager );

      InitializeComponent();

      MouseHook.MouseAction += OnMouseHookClick;
   }

   private void OnLoaded( object sender, RoutedEventArgs e ) => Win32.MakeToolWindow( this );

   public new virtual void Hide()
   {
      if ( !IsOpen )
      {
         return;
      }

      MakeWindowClickThrough( true );
      _model.IsEditing = false;
      MouseHook.Stop();

      IsOpen = false;
      base.Hide();
   }

   public new virtual void Show()
   {
      IsOpen = true;

      MouseHook.Start();
      base.Show();
   }

   private void OnMouseHookClick( object sender, MouseHookEventArgs e )
   {
      if ( !IsOpen || _model.SelectedStashTabHandler.SelectedStashTab is null )
      {
         return;
      }

      if ( UtilityMethods.HitTest( EditModeButton, e.ClickLocation ) )
      {
         HandleEditButton();
      }
      else
      {
         foreach ( var cell in _model.SelectedStashTabHandler.SelectedStashTab.OverlayCellsList.Where( cell => cell.Active ) )
         {
            if ( UtilityMethods.HitTest( UtilityMethods.GetContainerForDataObject<Button>( StashTabControl, cell ), e.ClickLocation ) )
            {
               _itemSetManager.OnItemCellClicked( cell );
               return;
            }
         }
      }
   }

   protected override void OnSourceInitialized( EventArgs e )
   {
      base.OnSourceInitialized( e );
      MakeWindowClickThrough( true );
   }

   private void MakeWindowClickThrough( bool clickThrough )
   {
      if ( clickThrough )
      {
         Win32.MakeTransparent( this );
      }
      else
      {
         Win32.MakeNormal( this );
      }
   }

   private void HandleEditButton()
   {
      if ( _model.IsEditing )
      {
         MakeWindowClickThrough( true );
         MouseHook.Start();
         _model.IsEditing = false;
      }
      else
      {
         MouseHook.Stop();
         MakeWindowClickThrough( false );
         _model.IsEditing = true;
      }
   }

   private void OnEditModeButtonClick( object sender, RoutedEventArgs e ) => HandleEditButton();

   private void OnMouseLeftButtonDown( object sender, MouseButtonEventArgs e ) => DragMove();
}
