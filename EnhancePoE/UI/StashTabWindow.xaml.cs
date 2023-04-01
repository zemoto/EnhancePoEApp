using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using EnhancePoE.Utils;

namespace EnhancePoE.UI
{
   internal partial class StashTabWindow : Window, INotifyPropertyChanged
   {
      public bool IsOpen { get; set; }

      private Visibility _stashBorderVisibility = Visibility.Hidden;
      public Visibility StashBorderVisibility
      {
         get => _stashBorderVisibility;
         set
         {
            _stashBorderVisibility = value;
            OnPropertyChanged( nameof( StashBorderVisibility ) );
         }
      }

      private bool _isEditing;

      public StashTabWindow()
      {
         InitializeComponent();
         DataContext = this;

         MouseHook.MouseAction += OnMouseHookClick;
      }

      public new virtual void Hide()
      {
         MakeWindowTransparent();
         EditModeButton.Content = "Edit";
         _isEditing = false;
         MouseHook.Stop();

         IsOpen = false;
         MainWindow.RecipeOverlay.OpenStashOverlayButtonContent = "Stash";

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

         Data.ActivateAllCellsForNextSet();

         MainWindow.RecipeOverlay.OpenStashOverlayButtonContent = "Hide";

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
                  Data.OnItemCellClicked( cell );
                  return;
               }
            }
         }
      }

      private void Window_MouseDown( object sender, MouseButtonEventArgs e )
      {
         if ( e.ChangedButton == MouseButton.Left )
         {
            DragMove();
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
         if ( _isEditing )
         {
            MakeWindowTransparent();
            EditModeButton.Content = "Edit";
            StashBorderVisibility = Visibility.Hidden;
            MouseHook.Start();
            _isEditing = false;
         }
         else
         {
            MouseHook.Stop();
            EditModeButton.Content = "Save";
            StashBorderVisibility = Visibility.Visible;
            MakeWindowNormal();
            _isEditing = true;
         }
      }

      private void OnEditModeButtonClick( object sender, RoutedEventArgs e ) => HandleEditButton();

      public event PropertyChangedEventHandler PropertyChanged;
      protected virtual void OnPropertyChanged( string propertyName ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
   }
}
