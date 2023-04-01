using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using EnhancePoE.Model;
using EnhancePoE.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EnhancePoE.UI
{
   internal partial class MainWindow : Window, INotifyPropertyChanged
   {
      private static MainWindow _instance;
      public static MainWindow Instance => _instance ??= new MainWindow();

      public static RecipeStatusOverlay RecipeOverlay { get; } = new RecipeStatusOverlay();

      public string AppVersionText { get; } = "v.1.5.1-zemoto";

      private StashTab _selectedStashTab;
      public StashTab SelectedStashTab
      {
         get => _selectedStashTab;
         set
         {
            if ( _selectedStashTab != value )
            {
               _selectedStashTab = value;
               if ( _selectedStashTab is not null )
               {
                  Properties.Settings.Default.SelectedStashTabName = _selectedStashTab.TabName;
               }

               OnPropertyChanged( nameof( SelectedStashTab ) );
            }
         }
      }

      public ObservableCollection<string> LeagueList { get; } = new ObservableCollection<string>();
      public ObservableCollection<StashTab> StashTabList { get; } = new ObservableCollection<StashTab>();

      private bool _closingFromTrayIcon;

      private readonly LeagueGetter _leagueGetter = new();
      private readonly System.Windows.Forms.NotifyIcon _trayIcon = new();

      public MainWindow()
      {
         InitializeComponent();
         DataContext = this;

         InitializeTray();
         LoadLeagueList();

         SingleInstance.PingedBySecondProcess += ( s, a ) => Dispatcher.Invoke( Show );
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

      public void RunOverlay()
      {
         if ( RecipeOverlay.IsOpen )
         {
            RecipeOverlay.Hide();
            RunOverlayButton.Content = "Run Overlay";
         }
         else
         {
            if ( CheckAllSettings( showError: true ) )
            {
               RecipeOverlay.Show();
               RunOverlayButton.Content = "Stop Overlay";
            }
         }
      }

      private void OnRunOverlayButtonClicked( object sender, RoutedEventArgs e ) => RunOverlay();

      private void OnWindowMouseDown( object sender, MouseButtonEventArgs e ) => _ = MainGrid.Focus();

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
         var selectedLeague = Properties.Settings.Default.League;
         LeagueComboBox.IsEnabled = false;
         LeagueList.Clear();

         var leagues = await _leagueGetter.GetLeaguesAsync();
         foreach ( var league in leagues )
         {
            LeagueList.Add( league );
         }

         if ( LeagueList.Count > 0 )
         {
            LeagueComboBox.IsEnabled = true;
            LeagueComboBox.SelectedIndex = Math.Max( LeagueList.IndexOf( selectedLeague ), 0 );
         }
      }

      private async Task LoadStashTabsAsync()
      {
         FetchStashTabsButton.IsEnabled = false;
         StashTabComboBox.IsEnabled = false;

         SelectedStashTab = null;
         var stashTabs = await ApiAdapter.FetchStashTabs();
         if ( stashTabs is not null )
         {
            StashTabList.Clear();
            foreach ( var tab in stashTabs )
            {
               StashTabList.Add( tab );
            }

            if ( stashTabs.Count > 0 )
            {
               var selectedStashTabName = Properties.Settings.Default.SelectedStashTabName;
               if ( !string.IsNullOrEmpty( selectedStashTabName ) )
               {
                  var previouslySelectedStashTab = StashTabList.FirstOrDefault( x => x.TabName == selectedStashTabName );
                  if ( previouslySelectedStashTab is not null )
                  {
                     SelectedStashTab = previouslySelectedStashTab;
                  }
               }

               if ( SelectedStashTab is null )
               {
                  SelectedStashTab = StashTabList[0];
               }
            }
         }
         else
         {
            _ = MessageBox.Show( "Failed to fetch stash tabs", "Request Failed", MessageBoxButton.OK, MessageBoxImage.Error );
         }

         FetchStashTabsButton.IsEnabled = true;
         StashTabComboBox.IsEnabled = true;
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

      private void OnOverlayModeComboBoxSelectionChanged( object sender, SelectionChangedEventArgs e ) => RecipeOverlay.UpdateOverlayType();

      private void OnResetButtonClicked( object sender, RoutedEventArgs e )
      {
         switch ( MessageBox.Show( "This will reset all of your settings!", "Reset Settings", MessageBoxButton.YesNo ) )
         {
            case MessageBoxResult.Yes:
               LeagueComboBox.SelectedIndex = 0;
               Properties.Settings.Default.Reset();
               break;
            case MessageBoxResult.No:
               break;
         }
      }

      private void OnShowNumbersComboBoxSelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         RecipeOverlay.AmountsVisibility = Properties.Settings.Default.ShowItemAmount != 0 ? Visibility.Visible : Visibility.Hidden;
      }

      private void OnLeagueSelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         StashTabList.Clear();
      }

      #region INotifyPropertyChanged implementation
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
