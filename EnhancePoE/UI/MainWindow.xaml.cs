using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using EnhancePoE.Model;
using EnhancePoE.Utils;
using System.Linq;
using System.Threading.Tasks;
using ZemotoCommon;

namespace EnhancePoE.UI
{
   internal partial class MainWindow
   {
      private static MainWindow _instance;
      public static MainWindow Instance => _instance ??= new MainWindow();

      private bool _closingFromTrayIcon;

      private readonly MainViewModel _model = new();
      private readonly LeagueGetter _leagueGetter = new();
      private readonly System.Windows.Forms.NotifyIcon _trayIcon = new();
      private readonly RecipeStatusOverlay _recipeOverlay = new();

      public StashTab SelectedStashTab => _model.SelectedStashTab;

      public MainWindow()
      {
         DataContext = _model;

         InitializeComponent();

         InitializeTray();
         LoadLeagueList();
      }

      private async void OnWindowLoaded( object sender, RoutedEventArgs e )
      {
         if ( CheckAllSettings( showError: false ) )
         {
            await LoadStashTabsAsync();
         }
      }

      private void InitializeTray()
      {
         _trayIcon.Icon = new System.Drawing.Icon( @"Assets\coin.ico" );
         _trayIcon.Visible = true;
         _trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
         _ = _trayIcon.ContextMenuStrip.Items.Add( "Close", null, OnTrayItemMenuClicked );
         _trayIcon.DoubleClick += ( s, a ) =>
         {
            Show();
            _ = Activate();
            ShowInTaskbar = true;
            WindowState = WindowState.Normal;
         };
      }

      private void OnTrayItemMenuClicked( object Sender, EventArgs e )
      {
         _closingFromTrayIcon = true;
         Close();
      }

      protected override void OnClosing( CancelEventArgs e )
      {
         if ( Properties.Settings.Default.hideOnClose && !_closingFromTrayIcon )
         {
            e.Cancel = true;
            Hide();
            ShowInTaskbar = false;
            base.OnClosing( e );
         }
         else if ( !Properties.Settings.Default.hideOnClose || _closingFromTrayIcon )
         {
            _trayIcon.Visible = false;
            MouseHook.Stop();
            Properties.Settings.Default.Save();
            Application.Current.Shutdown();
         }
      }

      private void OnRunOverlayButtonClicked( object sender, RoutedEventArgs e )
      {
         if ( _recipeOverlay.IsOpen )
         {
            _recipeOverlay.Hide();
            RunOverlayButton.Content = "Run Overlay";
         }
         else
         {
            if ( CheckAllSettings( showError: true ) )
            {
               _recipeOverlay.Show();
               RunOverlayButton.Content = "Stop Overlay";
            }
         }
      }

      public static bool CheckAllSettings( bool showError )
      {
         var missingSettings = new List<string>();
         string errorMessage = "Please add: \n";

         if ( string.IsNullOrEmpty( Properties.Settings.Default.accName ) )
         {
            missingSettings.Add( "- Account Name \n" );
         }
         if ( string.IsNullOrEmpty( Properties.Settings.Default.SessionId ) )
         {
            missingSettings.Add( "- PoE Session ID \n" );
         }
         if ( string.IsNullOrEmpty( Properties.Settings.Default.League ) )
         {
            missingSettings.Add( "- League \n" );
         }

         if ( missingSettings.Count == 0 )
         {
            return true;
         }

         foreach ( string setting in missingSettings )
         {
            errorMessage += setting;
         }

         if ( showError )
         {
            _ = MessageBox.Show( errorMessage, "Missing Settings", MessageBoxButton.OK, MessageBoxImage.Error );
         }

         return false;
      }

      private async void LoadLeagueList()
      {
         var leagues = await _leagueGetter.GetLeaguesAsync();
         _model.UpdateLeagueList( leagues );
      }

      private async Task LoadStashTabsAsync()
      {
         _model.FetchingStashTabs = true;
         using var __ = new ScopeGuard( () => _model.FetchingStashTabs = false );

         _model.SelectedStashTab = null;
         var stashTabs = await ApiAdapter.FetchStashTabs();
         if ( stashTabs is null )
         {
            _ = MessageBox.Show( "Failed to fetch stash tabs", "Request Failed", MessageBoxButton.OK, MessageBoxImage.Error );
            return;
         }

         if ( stashTabs.Count == 0 )
         {
            return;
         }

         _model.StashTabList.Clear();
         foreach ( var tab in stashTabs )
         {
            _model.StashTabList.Add( tab );
         }

         var selectedStashTabName = Properties.Settings.Default.SelectedStashTabName;
         if ( !string.IsNullOrEmpty( selectedStashTabName ) )
         {
            var previouslySelectedStashTab = _model.StashTabList.FirstOrDefault( x => x.TabName == selectedStashTabName );
            if ( previouslySelectedStashTab is not null )
            {
               _model.SelectedStashTab = previouslySelectedStashTab;
            }
         }

         _model.SelectedStashTab ??= _model.StashTabList[0];
      }

      private void OnRefreshLeaguesButtonClicked( object sender, RoutedEventArgs e ) => LoadLeagueList();

      private async void OnFetchStashTabsButtonClicked( object sender, RoutedEventArgs e )
      {
         if ( CheckAllSettings( showError: true ) )
         {
            await LoadStashTabsAsync();
         }
      }

      private void OnSaveButtonClicked( object sender, RoutedEventArgs e ) => Properties.Settings.Default.Save();

      private void OnResetButtonClicked( object sender, RoutedEventArgs e )
      {
         switch ( MessageBox.Show( "This will reset all of your settings!", "Reset Settings", MessageBoxButton.YesNo ) )
         {
            case MessageBoxResult.Yes:
               Properties.Settings.Default.Reset();
               break;
            case MessageBoxResult.No:
               break;
         }
      }
   }
}
