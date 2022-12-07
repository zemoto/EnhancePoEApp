using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using EnhancePoE.Model;
using EnhancePoE.Utils;

namespace EnhancePoE.View
{
   public partial class StashTabWindow : Window, INotifyPropertyChanged
   {
      public bool IsOpen { get; set; }
      public bool IsEditing { get; set; }

      private Thickness _tabHeaderGap;
      public Thickness TabHeaderGap
      {
         get => _tabHeaderGap;
         set
         {
            if ( value != _tabHeaderGap )
            {
               _tabHeaderGap = value;
               OnPropertyChanged( nameof( TabHeaderGap ) );
            }
         }
      }

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

      public static ObservableCollection<TabItem> OverlayStashTabList = new();
      public StashTabWindow()
      {
         InitializeComponent();
         DataContext = this;
         StashTabOverlayTabControl.ItemsSource = OverlayStashTabList;
      }

      public new virtual void Hide()
      {
         Transparentize();
         EditModeButton.Content = "Edit";
         IsEditing = false;
         MouseHook.Stop();

         MainWindow.Instance.SelectedStashTab.TabHeader = null;

         IsOpen = false;
         IsEditing = false;
         MainWindow.Overlay.OpenStashOverlayButtonContent = "Stash";

         base.Hide();
      }

      public new virtual void Show()
      {
         var tab = MainWindow.Instance.SelectedStashTab;
         if ( tab is not null )
         {
            IsOpen = true;
            OverlayStashTabList.Clear();
            _tabHeaderGap.Right = Properties.Settings.Default.TabHeaderGap;
            _tabHeaderGap.Left = Properties.Settings.Default.TabHeaderGap;
            TabMargin = new Thickness( Properties.Settings.Default.TabMargin, 0, 0, 0 );

            TabItem newStashTabItem;
            var tbk = new TextBlock() { Text = tab.TabName };

            tbk.DataContext = tab;
            _ = tbk.SetBinding( TextBlock.BackgroundProperty, new System.Windows.Data.Binding( "TabHeaderColor" ) );
            _ = tbk.SetBinding( TextBlock.PaddingProperty, new System.Windows.Data.Binding( "TabHeaderWidth" ) );
            tbk.FontSize = 16;
            tab.TabHeader = tbk;

            if ( tab.Quad )
            {
               newStashTabItem = new TabItem
               {
                  Header = tbk,
                  Content = new UserControls.DynamicGridControlQuad
                  {
                     ItemsSource = tab.OverlayCellsList,
                  }
               };
            }
            else
            {
               newStashTabItem = new TabItem
               {
                  Header = tbk,
                  Content = new UserControls.DynamicGridControl
                  {
                     ItemsSource = tab.OverlayCellsList
                  }
               };
            }

            OverlayStashTabList.Add( newStashTabItem );

            StashTabOverlayTabControl.SelectedIndex = 0;

            Data.PrepareSelling();
            MainWindow.Instance.SelectedStashTab.InitializeCellList();
            Data.ActivateNextCell( true, null );
            if ( Properties.Settings.Default.HighlightMode == 2 )
            {
               foreach ( var set in Data.ItemSetListHighlight )
               {
                  foreach ( var item in set.ItemList )
                  {
                     tab.ActivateItemCells( item );
                  }
               }
            }

            MainWindow.Overlay.OpenStashOverlayButtonContent = "Hide";

            MouseHook.Start();
            base.Show();
         }
         else
         {
            _ = MessageBox.Show( "No StashTabs Available! Fetch before opening Overlay.", "Stashtab Error", MessageBoxButton.OK, MessageBoxImage.Error );
         }
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
