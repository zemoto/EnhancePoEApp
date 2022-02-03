using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using EnhancePoE.Model;

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

      //public double Gap { get; set; } = 0;

      public static ObservableCollection<TabItem> OverlayStashTabList = new ObservableCollection<TabItem>();
      public StashTabWindow()
      {
         InitializeComponent();
         DataContext = this;
         StashTabOverlayTabControl.ItemsSource = OverlayStashTabList;
      }

      public new virtual void Hide()
      {
         Transparentize();
         //MainWindow.overlay.EditStashTabOverlay.Content = "Edit";
         EditModeButton.Content = "Edit";
         IsEditing = false;
         MouseHook.Stop();

         foreach ( var i in StashTabList.StashTabs )
         {
            i.OverlayCellsList.Clear();
            i.TabHeader = null;
         }

         IsOpen = false;
         IsEditing = false;
         MainWindow.overlay.OpenStashOverlayButtonContent = "Stash";

         base.Hide();
      }

      public new virtual void Show()
      {
         if ( StashTabList.StashTabs.Count != 0 )
         {
            IsOpen = true;
            OverlayStashTabList.Clear();
            _tabHeaderGap.Right = Properties.Settings.Default.TabHeaderGap;
            _tabHeaderGap.Left = Properties.Settings.Default.TabHeaderGap;
            TabMargin = new Thickness( Properties.Settings.Default.TabMargin, 0, 0, 0 );

            foreach ( var i in StashTabList.StashTabs )
            {
               TabItem newStashTabItem;
               var tbk = new TextBlock() { Text = i.TabName };

               tbk.DataContext = i;
               _ = tbk.SetBinding( TextBlock.BackgroundProperty, new System.Windows.Data.Binding( "TabHeaderColor" ) );
               _ = tbk.SetBinding( TextBlock.PaddingProperty, new System.Windows.Data.Binding( "TabHeaderWidth" ) );
               tbk.FontSize = 16;
               i.TabHeader = tbk;

               if ( i.Quad )
               {
                  newStashTabItem = new TabItem
                  {
                     Header = tbk,
                     Content = new UserControls.DynamicGridControlQuad
                     {
                        ItemsSource = i.OverlayCellsList,
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
                        ItemsSource = i.OverlayCellsList
                     }
                  };
               }

               OverlayStashTabList.Add( newStashTabItem );
            }

            StashTabOverlayTabControl.SelectedIndex = 0;

            Data.PrepareSelling();
            Data.ActivateNextCell( true, null );
            if ( Properties.Settings.Default.HighlightMode == 2 )
            {
               foreach ( var set in Data.ItemSetListHighlight )
               {
                  foreach ( var i in set.ItemList )
                  {
                     var currTab = Data.GetStashTabFromItem( i );
                     currTab.ActivateItemCells( i );
                  }
               }
            }

            MainWindow.overlay.OpenStashOverlayButtonContent = "Hide";

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
         if ( MainWindow.stashTabOverlay.IsEditing )
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
