using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Navigation;
using EnhancePoE.Model;
using EnhancePoE.View;
using System.IO;
using System.Reflection;
using EnhancePoE.Utils;

namespace EnhancePoE
{
   public partial class MainWindow : Window, INotifyPropertyChanged
   {
      private static readonly string appVersion = "1.2.6.0";
      public static string AppVersionText { get; set; } = "v." + appVersion;

      private System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
      private System.Windows.Forms.ContextMenu contextMenu;
      private System.Windows.Forms.MenuItem menuItem;
      private System.Windows.Forms.MenuItem menuItemUpdate;
      private IContainer components;

      public static bool SettingsComplete { get; set; }

      public static ChaosRecipeEnhancer overlay = new ChaosRecipeEnhancer();

      public static StashTabWindow stashTabOverlay = new StashTabWindow();

      private static string RunButtonContent { get; set; } = "Run Overlay";

      private Visibility _indicesVisible = Visibility.Hidden;
      public Visibility IndicesVisible
      {
         get => _indicesVisible;
         set
         {
            if ( _indicesVisible != value )
            {
               _indicesVisible = value;
               OnPropertyChanged( "IndicesVisible" );
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
               OnPropertyChanged( "NameVisible" );
            }
         }
      }

      public Visibility LootfilterFileDialogVisible => Properties.Settings.Default.LootfilterOnline
            ? Visibility.Collapsed
            : Visibility.Visible;

      public Visibility LootfilterOnlineFilterNameVisible => Properties.Settings.Default.LootfilterOnline
          ? Visibility.Visible
          : Visibility.Collapsed;

      private bool trayClose;

      public static MainWindow instance;

      public MainWindow()
      {
         instance = this;
         InitializeComponent();
         DataContext = this;

         if ( !string.IsNullOrEmpty( Properties.Settings.Default.FilterChangeSoundFileLocation ) && !FilterSoundLocationDialog.Content.Equals( "Default Sound" ) )
         {
            Data.Player.Open( new Uri( Properties.Settings.Default.FilterChangeSoundFileLocation ) );
         }
         else
         {
            Data.Player.Open( new Uri( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), @"Sounds\filterchanged.mp3" ) ) );
         }

         if ( !string.IsNullOrEmpty( Properties.Settings.Default.ItemPickupSoundFileLocation ) && !ItemPickupLocationDialog.Content.Equals( "Default Sound" ) )
         {
            Data.PlayerSet.Open( new Uri( Properties.Settings.Default.ItemPickupSoundFileLocation ) );
         }
         else
         {
            Data.PlayerSet.Open( new Uri( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), @"Sounds\itemsPickedUp.mp3" ) ) );
         }

         InitializeColors();
         InitializeHotkeys();
         InitializeTray();
         LoadModeVisibility();
         // add Action to MouseHook
         MouseHook.MouseAction += Coordinates.Event;

         SingleInstance.PingedBySecondProcess += ( s, a ) => Dispatcher.Invoke( Show );
      }

      private void InitializeHotkeys()
      {
         HotkeysManager.SetupSystemHook();
         HotkeysManager.GetRefreshHotkey();
         HotkeysManager.GetToggleHotkey();
         HotkeysManager.GetStashTabHotkey();
         AddAllHotkeys();
      }

      private void InitializeColors()
      {
         if ( Properties.Settings.Default.ColorBoots != "" )
         {
            ColorBootsPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorBoots );
         }
         if ( Properties.Settings.Default.ColorChest != "" )
         {
            ColorChestPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorChest );
         }
         if ( Properties.Settings.Default.ColorWeapon != "" )
         {
            ColorWeaponsPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorWeapon );
         }
         if ( Properties.Settings.Default.ColorGloves != "" )
         {
            ColorGlovesPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorGloves );
         }
         if ( Properties.Settings.Default.ColorHelmet != "" )
         {
            ColorHelmetPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorHelmet );
         }
         if ( Properties.Settings.Default.ColorStash != "" )
         {
            ColorStashPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorStash );
         }
         if ( Properties.Settings.Default.StashTabBackgroundColor != "" )
         {
            ColorStashBackgroundPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.StashTabBackgroundColor );
         }
         if ( Properties.Settings.Default.ColorRing != "" )
         {
            ColorRingPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorRing );
         }
         if ( Properties.Settings.Default.ColorAmulet != "" )
         {
            ColorAmuletPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorAmulet );
         }
         if ( Properties.Settings.Default.ColorBelt != "" )
         {
            ColorBeltPicker.SelectedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString( Properties.Settings.Default.ColorBelt );
         }
      }

      // creates tray icon with menu
      private void InitializeTray()
      {
         ni.Icon = Properties.Resources.coin;
         ni.Visible = true;
         ni.DoubleClick +=
             ( object sender, EventArgs args ) =>
             {
                Show();
                WindowState = WindowState.Normal;
             };

         components = new Container();
         contextMenu = new System.Windows.Forms.ContextMenu();
         menuItem = new System.Windows.Forms.MenuItem();
         menuItemUpdate = new System.Windows.Forms.MenuItem();

         // Initialize contextMenu1
         contextMenu.MenuItems.AddRange( new System.Windows.Forms.MenuItem[] { menuItem, menuItemUpdate } );

         // Initialize menuItem1
         menuItem.Index = 1;
         menuItem.Text = "E&xit";
         menuItem.Click += MenuItem_Click;

         // Initialize menuItemUpdate
         menuItemUpdate.Index = 0;
         menuItemUpdate.Text = "C&eck for Updates";
         menuItemUpdate.Click += CheckForUpdates_Click;

         ni.ContextMenu = contextMenu;
      }

      private void CheckForUpdates_Click( object Sender, EventArgs e )
      {
      }

      // Close the form, which closes the application.
      private void MenuItem_Click( object Sender, EventArgs e )
      {
         trayClose = true;
         Close();
      }

      // Minimize to system tray when application is closed.
      protected override void OnClosing( CancelEventArgs e )
      {
         // if hideOnClose
         // setting cancel to true will cancel the close request
         // so the application is not closed
         if ( Properties.Settings.Default.hideOnClose && !trayClose )
         {
            e.Cancel = true;
            Hide();
            base.OnClosing( e );
         }

         if ( !Properties.Settings.Default.hideOnClose || trayClose )
         {
            ni.Visible = false;
            MouseHook.Stop();
            HotkeysManager.ShutdownSystemHook();
            Properties.Settings.Default.Save();
            if ( LogWatcher.WorkerThread != null && LogWatcher.WorkerThread.IsAlive )
            {
               LogWatcher.StopWatchingLogFile();
            }
            Application.Current.Shutdown();
         }
      }

      public void RunOverlay()
      {
         if ( overlay.IsOpen )
         {
            overlay.Hide();
            if ( stashTabOverlay.IsOpen )
            {
               stashTabOverlay.Hide();
            }
            RunButton.Content = "Run Overlay";
         }
         else
         {
            if ( CheckAllSettings() )
            {
               overlay.Show();
               RunButton.Content = "Stop Overlay";
            }
         }
      }

      private void RunButton_Click( object sender, RoutedEventArgs e )
      {
         RunOverlay();
      }

      public static void RunStashTabOverlay()
      {
         bool ready = CheckAllSettings();
         if ( ready )
         {
            if ( stashTabOverlay.IsOpen )
            {
               stashTabOverlay.Hide();
            }
            else
            {
               stashTabOverlay.Show();
            }
         }
      }

      public void AddAllHotkeys()
      {
         if ( Properties.Settings.Default.HotkeyRefresh != "< not set >" )
         {
            HotkeysManager.AddHotkey( HotkeysManager.refreshModifier, HotkeysManager.refreshKey, overlay.RunFetching );
         }
         if ( Properties.Settings.Default.HotkeyToggle != "< not set >" )
         {
            HotkeysManager.AddHotkey( HotkeysManager.toggleModifier, HotkeysManager.toggleKey, RunOverlay );
         }
         if ( Properties.Settings.Default.HotkeyStashTab != "< not set >" )
         {
            HotkeysManager.AddHotkey( HotkeysManager.stashTabModifier, HotkeysManager.stashTabKey, RunStashTabOverlay );
         }
      }

      public void RemoveAllHotkeys()
      {
         HotkeysManager.RemoveRefreshHotkey();
         HotkeysManager.RemoveStashTabHotkey();
         HotkeysManager.RemoveToggleHotkey();
      }

      private string GetSoundFilePath()
      {
         var open = new System.Windows.Forms.OpenFileDialog
         {
            Filter = "MP3|*.mp3"
         };
         var res = open.ShowDialog();

         if ( res == System.Windows.Forms.DialogResult.OK )
         {
            return open.FileName;
         }

         return null;
      }

      private void ColorBootsPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorBoots = ColorBootsPicker.SelectedColor.ToString();
      }

      private void ColorGlovesPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorGloves = ColorGlovesPicker.SelectedColor.ToString();
      }

      private void ColorHelmetPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorHelmet = ColorHelmetPicker.SelectedColor.ToString();
      }

      private void ColorChestPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorChest = ColorChestPicker.SelectedColor.ToString();
      }

      private void ColorWeaponsPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorWeapon = ColorWeaponsPicker.SelectedColor.ToString();
      }

      private void ColorStashPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorStash = ColorStashPicker.SelectedColor.ToString();
      }

      private void ColorRingPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorRing = ColorRingPicker.SelectedColor.ToString();
      }

      private void ColorAmuletPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorAmulet = ColorAmuletPicker.SelectedColor.ToString();
      }

      private void ColorBeltPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorBelt = ColorBeltPicker.SelectedColor.ToString();
      }

      private void Window_MouseDown( object sender, MouseButtonEventArgs e )
      {
         _ = MainGrid.Focus();
      }

      public static bool CheckAllSettings()
      {
         string accName = Properties.Settings.Default.accName;
         string sessId = Properties.Settings.Default.SessionId;
         string league = Properties.Settings.Default.League;
         string lootfilterLocation = Properties.Settings.Default.LootfilterLocation;
         bool lootfilterOnline = Properties.Settings.Default.LootfilterOnline;
         string lootfilterOnlineName = Properties.Settings.Default.LootfilterOnlineName;
         bool lootfilterActive = Properties.Settings.Default.LootfilterActive;
         string logLocation = Properties.Settings.Default.LogLocation;
         bool autoFetch = Properties.Settings.Default.AutoFetch;

         var missingSettings = new List<string>();
         string errorMessage = "Please add: \n";

         if ( accName == "" )
         {
            missingSettings.Add( "- Account Name \n" );
         }
         if ( sessId == "" )
         {
            missingSettings.Add( "- PoE Session ID \n" );
         }
         if ( league == "" )
         {
            missingSettings.Add( "- League \n" );
         }
         if ( lootfilterActive )
         {
            if ( !lootfilterOnline && lootfilterLocation == "" )
            {
               missingSettings.Add( "- Lootfilter Location \n" );
            }

            if ( lootfilterOnline && lootfilterOnlineName == "" )
            {
               missingSettings.Add( "- Lootfilter Name \n" );
            }
         }
         if ( autoFetch && logLocation == "" )
         {
            missingSettings.Add( "- Log File Location \n" );
         }
         if ( Properties.Settings.Default.StashtabMode == 0 )
         {
            if ( Properties.Settings.Default.StashTabIndices == "" )
            {
               missingSettings.Add( "- StashTab Index" );
            }
         }
         else if ( Properties.Settings.Default.StashtabMode == 1 )
         {
            if ( Properties.Settings.Default.StashTabName == "" )
            {
               missingSettings.Add( "- StashTab Name" );
            }
         }

         if ( missingSettings.Count > 0 )
         {
            SettingsComplete = false;
         }
         else
         {
            SettingsComplete = true;
            return true;
         }

         foreach ( string setting in missingSettings )
         {
            errorMessage += setting;
         }

         _ = MessageBox.Show( errorMessage, "Missing Settings", MessageBoxButton.OK, MessageBoxImage.Error );
         return false;
      }

      private void VolumeSlider_PreviewMouseUp( object sender, MouseButtonEventArgs e )
      {
         Data.PlayNotificationSound();
      }
      private void ColorStashBackgroundPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.StashTabBackgroundColor = ColorStashBackgroundPicker.SelectedColor.ToString();
      }

      private void CustomHotkeyToggle_Click( object sender, RoutedEventArgs e )
      {
         bool isWindowOpen = false;
         foreach ( Window w in Application.Current.Windows )
         {
            if ( w is HotkeyWindow )
            {
               isWindowOpen = true;
            }
         }

         if ( !isWindowOpen )
         {
            var hotkeyDialog = new HotkeyWindow( this, "toggle" );
            hotkeyDialog.Show();
         }
      }

      private void RefreshHotkey_Click( object sender, RoutedEventArgs e )
      {
         bool isWindowOpen = false;
         foreach ( Window w in Application.Current.Windows )
         {
            if ( w is HotkeyWindow )
            {
               isWindowOpen = true;
            }
         }

         if ( !isWindowOpen )
         {
            var hotkeyDialog = new HotkeyWindow( this, "refresh" );
            hotkeyDialog.Show();
         }
      }

      private void StashTabHotkey_Click( object sender, RoutedEventArgs e )
      {
         bool isWindowOpen = false;
         foreach ( Window w in Application.Current.Windows )
         {
            if ( w is HotkeyWindow )
            {
               isWindowOpen = true;
            }
         }

         if ( !isWindowOpen )
         {
            var hotkeyDialog = new HotkeyWindow( this, "stashtab" );
            hotkeyDialog.Show();
         }
      }

      private void ReloadFilterHotkey_Click( object sender, RoutedEventArgs e )
      {
         bool isWindowOpen = false;
         foreach ( Window w in Application.Current.Windows )
         {
            if ( w is HotkeyWindow )
            {
               isWindowOpen = true;
            }
         }

         if ( !isWindowOpen )
         {
            var hotkeyDialog = new HotkeyWindow( this, "reloadFilter" );
            hotkeyDialog.Show();
         }
      }

      private void LootfilterFileDialog_Click( object sender, RoutedEventArgs e )
      {
         var open = new System.Windows.Forms.OpenFileDialog
         {
            Filter = "Lootfilter|*.filter"
         };
         var res = open.ShowDialog();
         if ( res == System.Windows.Forms.DialogResult.OK )
         {
            string filename = open.FileName;
            Properties.Settings.Default.LootfilterLocation = filename;
            LootfilterFileDialog.Content = filename;
         }
      }

      private void ComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         LoadModeVisibility();
      }

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

      private void TabHeaderGapSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
      {
         stashTabOverlay.TabHeaderGap = new Thickness( Properties.Settings.Default.TabHeaderGap, 0, Properties.Settings.Default.TabHeaderGap, 0 );
      }

      private void TabHeaderWidthSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
      {
         if ( StashTabList.StashTabs.Count > 0 )
         {
            foreach ( var s in StashTabList.StashTabs )
            {
               s.TabHeaderWidth = new Thickness( Properties.Settings.Default.TabHeaderWidth, 2, Properties.Settings.Default.TabHeaderWidth, 2 );
            }
         }
      }

      private void TabHeaderMarginSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
      {
         stashTabOverlay.TabMargin = new Thickness( Properties.Settings.Default.TabMargin, 0, 0, 0 );
      }

      public static void GenerateNewOverlay()
      {
         overlay = new ChaosRecipeEnhancer();
      }
      public static void GenerateNewStashtabOverlay()
      {
         stashTabOverlay = new StashTabWindow();
      }

      private void SaveButton_Click_1( object sender, RoutedEventArgs e )
      {
         Properties.Settings.Default.Save();
      }

      private void OverlayModeComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         if ( Properties.Settings.Default.OverlayMode == 0 )
         {
            overlay.MainOverlayContentControl.Content = new UserControls.MainOverlayContent();
         }
         else if ( Properties.Settings.Default.OverlayMode == 1 )
         {
            overlay.MainOverlayContentControl.Content = new UserControls.MainOverlayContentMinified();
         }
         else if ( Properties.Settings.Default.OverlayMode == 2 )
         {
            overlay.MainOverlayContentControl.Content = new UserControls.MainOverlayOnlyButtons();
         }
      }

      private void ResetButton_Click( object sender, RoutedEventArgs e )
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

      private void ChaosRecipeCheckBox_Checked( object sender, RoutedEventArgs e )
      {
         Properties.Settings.Default.RegalRecipe = false;
      }

      private void RegalRecipeCheckBox_Checked( object sender, RoutedEventArgs e )
      {
         Properties.Settings.Default.ChaosRecipe = false;
      }

      private void LogLocationDialog_Click( object sender, RoutedEventArgs e )
      {
         var open = new System.Windows.Forms.OpenFileDialog
         {
            Filter = "Text|Client.txt"
         };
         var res = open.ShowDialog();
         if ( res == System.Windows.Forms.DialogResult.OK )
         {
            string filename = open.FileName;
            Properties.Settings.Default.LogLocation = filename;
            LogLocationDialog.Content = filename;
         }
      }

      private void AutoFetchCheckBox_Checked( object sender, RoutedEventArgs e )
      {
      }

      private void AutoFetchCheckBox_Unchecked( object sender, RoutedEventArgs e )
      {
         if ( LogWatcher.WorkerThread != null && LogWatcher.WorkerThread.IsAlive )
         {
            LogWatcher.WorkerThread.Abort();
         }
      }

      private void ShowNumbersComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
      {
         if ( Properties.Settings.Default.ShowItemAmount != 0 )
         {
            overlay.AmountsVisibility = Visibility.Visible;
         }
         else
         {
            overlay.AmountsVisibility = Visibility.Hidden;
         }
      }

      private void LootfilterOnlineCheckbox_Checked( object sender, RoutedEventArgs e )
      {
         OnPropertyChanged( nameof( LootfilterFileDialogVisible ) );
         OnPropertyChanged( nameof( LootfilterOnlineFilterNameVisible ) );
      }

      private void Hyperlink_RequestNavigateByAccName( object sender, RequestNavigateEventArgs e )
      {
         if ( string.IsNullOrWhiteSpace( Properties.Settings.Default.accName ) )
         {
            const string messageBoxText = "You first need enter your account name";
            _ = MessageBox.Show( messageBoxText, "Missing Settings", MessageBoxButton.OK, MessageBoxImage.Error );
         }
         else
         {
            string url = string.Format( e.Uri.ToString(), Properties.Settings.Default.accName );
            _ = System.Diagnostics.Process.Start( url );
         }
      }

      private void FilterSoundLocationDialog_OnClick( object sender, RoutedEventArgs e )
      {
         var soundFilePath = GetSoundFilePath();

         if ( soundFilePath != null )
         {
            Properties.Settings.Default.FilterChangeSoundFileLocation = soundFilePath;
            FilterSoundLocationDialog.Content = soundFilePath;
            Data.Player.Open( new Uri( soundFilePath ) );

            Data.PlayNotificationSound();
         }
      }

      private void ItemPickupLocationDialog_OnClick( object sender, RoutedEventArgs e )
      {
         var soundFilePath = GetSoundFilePath();

         if ( soundFilePath != null )
         {
            Properties.Settings.Default.ItemPickupSoundFileLocation = soundFilePath;
            ItemPickupLocationDialog.Content = soundFilePath;
            Data.PlayerSet.Open( new Uri( soundFilePath ) );

            Data.PlayNotificationSoundSetPicked();
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
