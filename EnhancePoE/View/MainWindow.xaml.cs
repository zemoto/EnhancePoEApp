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

      private System.Windows.Forms.NotifyIcon _trayIcon;

      public static bool SettingsComplete { get; set; }

      public static ChaosRecipeEnhancer overlay = new ChaosRecipeEnhancer();

      public static StashTabWindow stashTabOverlay = new StashTabWindow();

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

      private bool _closingFromTrayIcon;

      public static MainWindow instance;

      public MainWindow()
      {
         instance = this;
         InitializeComponent();
         DataContext = this;

         if ( !string.IsNullOrEmpty( Properties.Settings.Default.ItemPickupSoundFileLocation ) && !ItemPickupLocationDialog.Content.Equals( "Default Sound" ) )
         {
            Data.PlayerSet.Open( new Uri( Properties.Settings.Default.ItemPickupSoundFileLocation ) );
         }
         else
         {
            Data.PlayerSet.Open( new Uri( Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), @"Sounds\itemsPickedUp.mp3" ) ) );
         }

         InitializeColors();
         InitializeTray();
         LoadModeVisibility();

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

         _trayIcon = new System.Windows.Forms.NotifyIcon
         {
            Icon = Properties.Resources.coin,
            Visible = true,
            ContextMenu = contextMenu
         };
         _trayIcon.MouseClick += ( s, a ) => Show();
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

      private void ColorStashPicker_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e )
      {
         Properties.Settings.Default.ColorStash = ColorStashPicker.SelectedColor.ToString();
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
