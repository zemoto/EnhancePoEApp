using System;
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

         MakeWindowClickThrough( true );
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

         _model.Tab = tab;
         IsOpen = true;

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
            foreach ( var cell in _model.Tab.OverlayCellsList.Where( cell => cell.Active ) )
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
         var handle = new WindowInteropHelper( this ).Handle;
         if ( clickThrough )
         {
            Win32.MakeTransparent( handle );
         }
         else
         {
            Win32.MakeNormal( handle );
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
}
