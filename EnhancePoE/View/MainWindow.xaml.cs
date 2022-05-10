using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using EnhancePoE.Model;
using EnhancePoE.View;
using EnhancePoE.Utils;
using System.Collections.ObjectModel;
using Color = System.Windows.Media.Color;

namespace EnhancePoE
{
   public partial class MainWindow : Window, INotifyPropertyChanged
   {
      private static MainWindow _instance;
      public static MainWindow Instance => _instance ??= new MainWindow();

      public static ChaosRecipeEnhancer Overlay { get; } = new ChaosRecipeEnhancer();
      public static StashTabWindow StashTabOverlay { get; } = new StashTabWindow();

      public string AppVersionText { get; } = "v.1.2.8-zemoto";

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
         InitializeComponent();
         DataContext = this;

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
            ColorStashPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorStash );
         }
         if ( Properties.Settings.Default.StashTabBackgroundColor != "" )
         {
            ColorStashBackgroundPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.StashTabBackgroundColor );
         }
         if ( Properties.Settings.Default.ColorBoots != "" )
         {
            ColorBootsPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorBoots );
         }
         if ( Properties.Settings.Default.ColorChest != "" )
         {
            ColorChestPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorChest );
         }
         if ( Properties.Settings.Default.ColorWeapon != "" )
         {
            ColorWeaponsPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorWeapon );
         }
         if ( Properties.Settings.Default.ColorGloves != "" )
         {
            ColorGlovesPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorGloves );
         }
         if ( Properties.Settings.Default.ColorHelmet != "" )
         {
            ColorHelmetPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorHelmet );
         }
         if ( Properties.Settings.Default.ColorRing != "" )
         {
            ColorRingPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorRing );
         }
         if ( Properties.Settings.Default.ColorAmulet != "" )
         {
            ColorAmuletPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorAmulet );
         }
         if ( Properties.Settings.Default.ColorBelt != "" )
         {
            ColorBeltPicker.SelectedColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorBelt );
         }
      }

      // creates tray icon with menu
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

      private void OnColorGlovesPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.ColorGloves = ColorGlovesPicker.SelectedColor.ToString();
      }

      private void OnColorBootsPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.ColorBoots = ColorBootsPicker.SelectedColor.ToString();
      }

      private void OnColorHelmetPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.ColorHelmet = ColorHelmetPicker.SelectedColor.ToString();
      }

      private void OnColorChestPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.ColorChest = ColorChestPicker.SelectedColor.ToString();
      }

      private void OnColorWeaponsPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.ColorWeapon = ColorWeaponsPicker.SelectedColor.ToString();
      }

      private void OnColorStashBackgroundColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.StashTabBackgroundColor = ColorStashBackgroundPicker.SelectedColor.ToString();
      }

      private void OnColorRingPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.ColorRing = ColorRingPicker.SelectedColor.ToString();
      }

      private void OnColorAmuletPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.ColorAmulet = ColorAmuletPicker.SelectedColor.ToString();
      }

      private void OnColorBeltPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
      {
         Properties.Settings.Default.ColorBelt = ColorBeltPicker.SelectedColor.ToString();
      }

      private void OnColorStashPickerSelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
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

      private void OnStashtabModeComboBoxSelectionChanged( object sender, SelectionChangedEventArgs e ) => LoadModeVisibility();

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
