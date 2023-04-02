using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using EnhancePoE.Utils;

namespace EnhancePoE.UI
{
   internal partial class StashTabWindow : Window
   {
      public bool IsOpen { get; set; }

      private readonly ItemSetManager _itemSetManager;
      private readonly StashTabWindowViewModel _model = new();

      public StashTabWindow( ItemSetManager itemSetManager )
      {
         DataContext = _model;
         _itemSetManager = itemSetManager;

         InitializeComponent();

         MouseHook.MouseAction += OnMouseHookClick;
      }

      public new virtual void Hide()
      {
         if ( !IsOpen )
         {
            return;
         }

         MakeWindowTransparent();
         EditModeButton.Content = "Edit";
         _model.IsEditing = false;
         MouseHook.Stop();

         IsOpen = false;
         base.Hide();
      }

      public new virtual void Show()
      {
         var tab = MainWindow.Instance.SelectedStashTab;
         if ( tab is null )
         {
            _ = MessageBox.Show( "No stash tabs available! Fetch before opening overlay.", "Stash Tab Error", MessageBoxButton.OK, MessageBoxImage.Error );
            return;
         }

         if ( tab.OverlayCellsList is null )
         {
            _ = MessageBox.Show( "No stash data! Fetch before opening stash tab overlay.", "Stash Tab Error", MessageBoxButton.OK, MessageBoxImage.Error );
            return;
         }

         IsOpen = true;

         var size = tab.Quad ? 24 : 12;
         var stashTabItem = new TabItem { Content = new DynamicGridControl( size ) { ItemsSource = tab.OverlayCellsList } };

         StashTabOverlayTabControl.ItemsSource = new List<TabItem>() { stashTabItem };
         StashTabOverlayTabControl.SelectedIndex = 0;

         _itemSetManager.ActivateAllCellsForNextSet();

         MouseHook.Start();
         base.Show();
      }

      private void OnMouseHookClick( object sender, MouseHookEventArgs e )
      {
         if ( !IsOpen || MainWindow.Instance.SelectedStashTab is null )
         {
            return;
         }

         if ( UtilityMethods.HitTest( EditModeButton, e.ClickLocation ) )
         {
            HandleEditButton();
         }
         else
         {
            var ctrl = (ItemsControl)StashTabOverlayTabControl.SelectedContent;
            foreach ( var cell in MainWindow.Instance.SelectedStashTab.OverlayCellsList.Where( cell => cell.Active ) )
            {
               if ( UtilityMethods.HitTest( UtilityMethods.GetContainerForDataObject<Button>( ctrl, cell ), e.ClickLocation ) )
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
         MakeWindowTransparent();
      }

      private void MakeWindowTransparent() => Win32.MakeTransparent( new WindowInteropHelper( this ).Handle );

      private void MakeWindowNormal() => Win32.MakeNormal( new WindowInteropHelper( this ).Handle );

      private void HandleEditButton()
      {
         if ( _model.IsEditing )
         {
            MakeWindowTransparent();
            MouseHook.Start();
            _model.IsEditing = false;
         }
         else
         {
            MouseHook.Stop();
            MakeWindowNormal();
            _model.IsEditing = true;
         }
      }

      private void OnEditModeButtonClick( object sender, RoutedEventArgs e ) => HandleEditButton();

      private void OnMouseLeftButtonDown( object sender, MouseButtonEventArgs e ) => DragMove();
   }
}
