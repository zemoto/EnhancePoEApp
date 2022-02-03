using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using EnhancePoE.Model;
using EnhancePoE.View;
using System.IO;
using System.Reflection;
using EnhancePoE.Utils;
using System.Collections.ObjectModel;

namespace EnhancePoE
{
   public partial class MainWindow : Window, INotifyPropertyChanged
   {
      public static MainWindow Instance { get; private set; }
      public static ChaosRecipeEnhancer Overlay { get; private set; } = new ChaosRecipeEnhancer();
      public static StashTabWindow StashTabOverlay { get; private set; } = new StashTabWindow();

      public string AppVersionText { get; } = "v.1.2.7-zemoto";

      private Visibility _indicesVisible = Visibility.Hidden;
      public Visibility IndicesVisible
      {
         get => _indicesVisible;
         set
         {
            if ( _indicesVisible != value )
            {
               _indicesVisible = value;
               OnPropertyChanged( nameof( IndicesVisible ) );
            }
         }
      }

      private Visibility _nameVisible = Visibility.Hidden;
      public Visibility NameVisible
      {
         get => _nameVisible;
         set
         {
            if ( _nameVisible != value )
            {
               _nameVisible = value;
               OnPropertyChanged( nameof( NameVisible ) );
            }
         }
      }

      public ObservableCollection<string> LeagueList { get; } = new ObservableCollection<string>();

      private bool _closingFromTrayIcon;

      private readonly LeagueGetter _leagueGetter = new LeagueGetter();
      private readonly System.Windows.Forms.NotifyIcon _trayIcon = new System.Windows.Forms.NotifyIcon();

      public MainWindow()
      {
         Instance = this;
         InitializeComponent();
         DataContext = this;

         if ( !string.IsNullOrEmpty( Properties.Settings.Default.ItemPickupSoundFileLocation ) && !ItemPickupLocationButton.Content.Equals( "Default Sound" ) )
         {
            Data.Player.Open( new Uri( Properties.Settings.Default.ItemPickupSoundFileLocation ) );
         }
         else
         {
            Data.Player.Open( new Uri( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), @"Sounds\notificationSound.mp3" ) ) );
         }

         InitializeColors();
         InitializeTray();
         LoadModeVisibility();
         LoadLeagueList();

         MouseHook.MouseAction += Coordinates.Event;
         SingleInstance.PingedBySecondProcess += ( s, a ) => Dispatcher.Invoke( Show );
      }

      private void InitializeColors()
      {
         if ( Properties.Settings.Default.ColorStash != "" )
         {
            ColorStashPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorStash );
         }
         if ( Properties.Settings.Default.StashTabBackgroundColor != "" )
         {
            ColorStashBackgroundPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.StashTabBackgroundColor );
         }
      }

      // creates tray icon with menu
      private void InitializeTray()
      {
         var menuItem = new System.Windows.Forms.MenuItem { Text = "Close" };
         menuItem.Click += OnTrayItemMenuClicked;

         var contextMenu = new System.Windows.Forms.ContextMenu();
         _ = contextMenu.MenuItems.Add( menuItem );

         _trayIcon.Icon = new System.Drawing.Icon( "coin.ico" );
         _trayIcon.Visible = true;
         _trayIcon.ContextMenu = contextMenu;
         _trayIcon.MouseClick += ( s, a ) =>
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
         if ( Overlay.IsOpen )
         {
            Overlay.Hide();
            if ( StashTabOverlay.IsOpen )
            {
               StashTabOverlay.Hide();
            }
            RunOverlayButton.Content = "Run Overlay";
         }
         else
         {
            if ( CheckAllSettings() )
            {
               Overlay.Show();
               RunOverlayButton.Content = "Stop Overlay";
            }
         }
      }

      private void OnRunOverlayButtonClicked( object sender, RoutedEventArgs e ) => RunOverlay();

      public static void RunStashTabOverlay()
      {
         if ( CheckAllSettings() )
         {
            if ( StashTabOverlay.IsOpen )
            {
               StashTabOverlay.Hide();
            }
            else
            {
               StashTabOverlay.Show();
            }
         }
      }

      private string GetSoundFilePath()
      {
         var open = new System.Windows.Forms.OpenFileDialog
         {
            Filter = "MP3|*.mp3"
         };
         var res = open.ShowDialog();

         return res == System.Windows.Forms.DialogResult.OK ? open.FileName : null;
      }

      private void ColorStashPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorStash = ColorStashPicker.SelectedColor.ToString();
      }

      private void OnWindowMouseDown( object sender, MouseButtonEventArgs e ) => _ = MainGrid.Focus();

      public static bool CheckAllSettings()
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
         if ( Properties.Settings.Default.StashtabMode == 0 )
         {
            if ( string.IsNullOrEmpty( Properties.Settings.Default.StashTabIndices ) )
            {
               missingSettings.Add( "- StashTab Index" );
            }
         }
         else if ( Properties.Settings.Default.StashtabMode == 1 )
         {
            if ( string.IsNullOrEmpty( Properties.Settings.Default.StashTabName ) )
            {
               missingSettings.Add( "- StashTab Name" );
            }
         }

         if ( missingSettings.Count == 0 )
         {
            return true;
         }

         foreach ( string setting in missingSettings )
         {
            errorMessage += setting;
         }

         _ = MessageBox.Show( errorMessage, "Missing Settings", MessageBoxButton.OK, MessageBoxImage.Error );
         return false;
      }

      private void OnVolumeSliderPreviewMouseUp( object sender, MouseButtonEventArgs e ) => Data.PlayNotificationSound();

      private void OnColorStashBackgroundColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.StashTabBackgroundColor = ColorStashBackgroundPicker.SelectedColor.ToString();
      }

      private void ComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e ) => LoadModeVisibility();

      private void LoadModeVisibility()
      {
         if ( Properties.Settings.Default.StashtabMode == 0 )
         {
            IndicesVisible = Visibility.Visible;
            NameVisible = Visibility.Hidden;
         }
         else
         {
            NameVisible = Visibility.Visible;
            IndicesVisible = Visibility.Hidden;
         }
      }

      private async void LoadLeagueList()
      {
         var selectedLeague = Properties.Settings.Default.League;
         LeagueComboBox.IsEnabled = false;
         LeagueList.Clear();

         var leagues = await _leagueGetter.GetLeaguesAsync();
         foreach( var league in leagues )
         {
            LeagueList.Add( league );
         }

         if ( LeagueList.Count > 0 )
         {
            LeagueComboBox.IsEnabled = true;
            LeagueComboBox.SelectedIndex = Math.Max( LeagueList.IndexOf( selectedLeague ), 0 );
         }
      }

      private void OnRefreshLeaguesButtonClicked( object sender, RoutedEventArgs e ) => LoadLeagueList();

      private void OnTabHeaderGapSliderValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
      {
         StashTabOverlay.TabHeaderGap = new Thickness( Properties.Settings.Default.TabHeaderGap, 0, Properties.Settings.Default.TabHeaderGap, 0 );
      }

      private void OnTabHeaderWidthSliderValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
      {
         if ( StashTabList.StashTabs.Count > 0 )
         {
            foreach ( var s in StashTabList.StashTabs )
            {
               s.TabHeaderWidth = new Thickness( Properties.Settings.Default.TabHeaderWidth, 2, Properties.Settings.Default.TabHeaderWidth, 2 );
            }
         }
      }

      private void OnTabHeaderMarginSliderValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
      {
         StashTabOverlay.TabMargin = new Thickness( Properties.Settings.Default.TabMargin, 0, 0, 0 );
      }

      public static void GenerateNewOverlay()
      {
         Overlay = new ChaosRecipeEnhancer();
      }

      public static void GenerateNewStashtabOverlay()
      {
         StashTabOverlay = new StashTabWindow();
      }

      private void OnSaveButtonClicked( object sender, RoutedEventArgs e )
      {
         Properties.Settings.Default.Save();
      }

      private void OnOverlayModeComboBoxSelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         if ( Properties.Settings.Default.OverlayMode == 0 )
         {
            Overlay.MainOverlayContentControl.Content = new UserControls.MainOverlayContent();
         }
         else if ( Properties.Settings.Default.OverlayMode == 1 )
         {
            Overlay.MainOverlayContentControl.Content = new UserControls.MainOverlayContentMinified();
         }
         else if ( Properties.Settings.Default.OverlayMode == 2 )
         {
            Overlay.MainOverlayContentControl.Content = new UserControls.MainOverlayOnlyButtons();
         }
      }

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

      private void OnChaosRecipeCheckBoxChecked( object sender, RoutedEventArgs e )
      {
         Properties.Settings.Default.RegalRecipe = false;
      }

      private void OnRegalRecipeCheckBoxChecked( object sender, RoutedEventArgs e )
      {
         Properties.Settings.Default.ChaosRecipe = false;
      }

      private void OnShowNumbersComboBoxSelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         Overlay.AmountsVisibility = Properties.Settings.Default.ShowItemAmount != 0 ? Visibility.Visible : Visibility.Hidden;
      }

      private void OnItemPickupLocationButtonClicked( object sender, RoutedEventArgs e )
      {
         var soundFilePath = GetSoundFilePath();

         if ( soundFilePath != null )
         {
            Properties.Settings.Default.ItemPickupSoundFileLocation = soundFilePath;
            ItemPickupLocationButton.Content = soundFilePath;
            Data.Player.Open( new Uri( soundFilePath ) );

            Data.PlayNotificationSound();
         }
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
