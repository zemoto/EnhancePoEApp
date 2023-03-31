using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using EnhancePoE.Model;
using EnhancePoE.Utils;

namespace EnhancePoE.UI
{
   internal partial class StashTabWindow : Window, INotifyPropertyChanged
   {
      public bool IsOpen { get; set; }
      public bool IsEditing { get; set; }

      private Thickness _tabMargin;
      public Thickness TabMargin
      {
         get => _tabMargin;
         set
         {
            if ( value != _tabMargin )
            {
               _tabMargin = value;
               OnPropertyChanged( nameof( TabMargin ) );
            }
         }
      }

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

      public StashTabWindow()
      {
         InitializeComponent();
         DataContext = this;
      }

      public new virtual void Hide()
      {
         Transparentize();
         EditModeButton.Content = "Edit";
         IsEditing = false;
         MouseHook.Stop();

         IsOpen = false;
         IsEditing = false;
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

         if ( tab.ItemList is null )
         {
            _ = MessageBox.Show( "No stash data! Fetch before opening stash tab overlay.", "Stash Tab Error", MessageBoxButton.OK, MessageBoxImage.Error );
            return;
         }

         IsOpen = true;

         var size = tab.Quad ? 24 : 12;
         var stashTabItem = new TabItem { Content = new DynamicGridControl( size ) { ItemsSource = tab.OverlayCellsList } };

         StashTabOverlayTabControl.ItemsSource = new List<TabItem>() { stashTabItem };
         StashTabOverlayTabControl.SelectedIndex = 0;

         Data.PrepareSelling();
         Data.ActivateNextCell( true, null );

         MainWindow.RecipeOverlay.OpenStashOverlayButtonContent = "Hide";

         MouseHook.Start();
         base.Show();
      }

      public void StartEditMode()
      {
         MouseHook.Stop();
         EditModeButton.Content = "Save";
         StashBorderVisibility = Visibility.Visible;
         Normalize();
         IsEditing = true;
      }

      public void StopEditMode()
      {
         Transparentize();
         EditModeButton.Content = "Edit";
         StashBorderVisibility = Visibility.Hidden;
         MouseHook.Start();
         IsEditing = false;
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

         // Get this window's handle
         var hwnd = new WindowInteropHelper( this ).Handle;

         Win32.MakeTransparent( hwnd );
      }

      public void Transparentize()
      {
         Trace.WriteLine( "make transparent" );
         var hwnd = new WindowInteropHelper( this ).Handle;

         Win32.MakeTransparent( hwnd );
      }

      public void Normalize()
      {
         Trace.WriteLine( "make normal" );
         var hwnd = new WindowInteropHelper( this ).Handle;

         Win32.MakeNormal( hwnd );
      }

      protected override void OnClosing( CancelEventArgs e )
      {
      }

      public void HandleEditButton()
      {
         if ( MainWindow.StashTabOverlay.IsEditing )
         {
            StopEditMode();
         }
         else
         {
            StartEditMode();
         }
      }

      private void EditModeButton_Click( object sender, RoutedEventArgs e )
      {
         HandleEditButton();
      }

      #region INotifyPropertyChanged implementation
      // Basically, the UI thread subscribes to this event and update the binding if the received Property Name correspond to the Binding Path element
      public event PropertyChangedEventHandler PropertyChanged;
      protected virtual void OnPropertyChanged( string propertyName )
      {
         var handler = PropertyChanged;
         if ( handler != null )
            handler( this, new PropertyChangedEventArgs( propertyName ) );
      }
      #endregion
   }
}
