﻿using EnhancePoE.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EnhancePoE
{
   public partial class ChaosRecipeEnhancer : Window, INotifyPropertyChanged
   {
      // toggle fetch button
      public static bool FetchingActive { get; set; }
      // fetching and calculations currently active
      public static bool CalculationActive { get; set; }
      //public static System.Timers.Timer aTimer;

      private static readonly double deactivatedOpacity = .1;
      private static readonly double activatedOpacity = 1;

      private static readonly int fetchCooldown = 30;

      public static LogWatcher Watcher { get; set; }

      public bool IsOpen { get; set; }

      private string _warningMessage;
      public string WarningMessage
      {
         get => _warningMessage;
         set
         {
            _warningMessage = value;
            OnPropertyChanged( "WarningMessage" );
         }
      }

      private Visibility _warningMessageVisibility = Visibility.Hidden;
      public Visibility WarningMessageVisibility
      {
         get => _warningMessageVisibility;
         set
         {
            _warningMessageVisibility = value;
            OnPropertyChanged( "WarningMessageVisibility" );
         }
      }
      private double _shadowOpacity;
      public double ShadowOpacity
      {
         get => _shadowOpacity;
         set
         {
            _shadowOpacity = value;
            OnPropertyChanged( "ShadowOpacity" );
         }
      }

      private string _fullSetsText = "0";
      public string FullSetsText
      {
         get => _fullSetsText;
         set
         {
            _fullSetsText = value;
            OnPropertyChanged( "FullSetsText" );
         }
      }

      private bool _isIndeterminate;
      public bool IsIndeterminate
      {
         get => _isIndeterminate;
         set
         {
            _isIndeterminate = value;
            OnPropertyChanged( "IsIndeterminate" );
         }
      }

      private string _openStashOverlayButtonContent = "Stash";
      public string OpenStashOverlayButtonContent
      {
         get => _openStashOverlayButtonContent;
         set
         {
            _openStashOverlayButtonContent = value;
            OnPropertyChanged( "OpenStashOverlayButtonContent" );
         }
      }

      private double _helmetOpacity = activatedOpacity;
      public double HelmetOpacity
      {
         get => _helmetOpacity;
         set
         {
            _helmetOpacity = value;
            OnPropertyChanged( "HelmetOpacity" );
         }
      }
      private double _bootsOpacity = activatedOpacity;
      public double BootsOpacity
      {
         get => _bootsOpacity;
         set
         {
            _bootsOpacity = value;
            OnPropertyChanged( "BootsOpacity" );
         }
      }
      private double _glovesOpacity = activatedOpacity;
      public double GlovesOpacity
      {
         get => _glovesOpacity;
         set
         {
            _glovesOpacity = value;
            OnPropertyChanged( "GlovesOpacity" );
         }
      }
      private double _chestsOpacity = activatedOpacity;
      public double ChestsOpacity
      {
         get => _chestsOpacity;
         set
         {
            _chestsOpacity = value;
            OnPropertyChanged( "ChestsOpacity" );
         }
      }
      private double _weaponsOpacity = activatedOpacity;
      public double WeaponsOpacity
      {
         get => _weaponsOpacity;
         set
         {
            _weaponsOpacity = value;
            OnPropertyChanged( "WeaponsOpacity" );
         }
      }
      private double _ringsOpacity = activatedOpacity;
      public double RingsOpacity
      {
         get => _ringsOpacity;
         set
         {
            _ringsOpacity = value;
            OnPropertyChanged( "RingsOpacity" );
         }
      }
      private double _amuletsOpacity = activatedOpacity;
      public double AmuletsOpacity
      {
         get => _amuletsOpacity;
         set
         {
            _amuletsOpacity = value;
            OnPropertyChanged( "AmuletsOpacity" );
         }
      }
      private double _beltsOpacity = activatedOpacity;
      public double BeltsOpacity
      {
         get => _beltsOpacity;
         set
         {
            _beltsOpacity = value;
            OnPropertyChanged( "BeltsOpacity" );
         }
      }

      private ContentElement _mainOverlayContent;
      public ContentElement MainOverlayContent
      {
         get => _mainOverlayContent;
         set
         {
            _mainOverlayContent = value;
            OnPropertyChanged( "MainOverlayContent" );
         }
      }

      private SolidColorBrush _fetchButtonColor = Brushes.Green;
      public SolidColorBrush FetchButtonColor
      {
         get => _fetchButtonColor;
         set
         {
            _fetchButtonColor = value;
            OnPropertyChanged( "FetchButtonColor" );
         }
      }

      private int _ringsAmount;
      public int RingsAmount
      {
         get => _ringsAmount;
         set
         {
            _ringsAmount = value;
            OnPropertyChanged( "RingsAmount" );
         }
      }
      private int _amuletsAmount;
      public int AmuletsAmount
      {
         get => _amuletsAmount;
         set
         {
            _amuletsAmount = value;
            OnPropertyChanged( "AmuletsAmount" );
         }
      }
      private int _beltsAmount;
      public int BeltsAmount
      {
         get => _beltsAmount;
         set
         {
            _beltsAmount = value;
            OnPropertyChanged( "BeltsAmount" );
         }
      }
      private int _chestsAmount;
      public int ChestsAmount
      {
         get => _chestsAmount;
         set
         {
            _chestsAmount = value;
            OnPropertyChanged( "ChestsAmount" );
         }
      }
      private int _weaponsAmount;
      public int WeaponsAmount
      {
         get => _weaponsAmount;
         set
         {
            _weaponsAmount = value;
            OnPropertyChanged( "WeaponsAmount" );
         }
      }
      private int _glovesAmount;
      public int GlovesAmount
      {
         get => _glovesAmount;
         set
         {
            _glovesAmount = value;
            OnPropertyChanged( "GlovesAmount" );
         }
      }
      private int _helmetsAmount;
      public int HelmetsAmount
      {
         get => _helmetsAmount;
         set
         {
            _helmetsAmount = value;
            OnPropertyChanged( "HelmetsAmount" );
         }
      }
      private int _bootsAmount;
      public int BootsAmount
      {
         get => _bootsAmount;
         set
         {
            _bootsAmount = value;
            OnPropertyChanged( "BootsAmount" );
         }
      }

      private Visibility _amountsVisibility = Visibility.Hidden;
      public Visibility AmountsVisibility
      {
         get => _amountsVisibility;
         set
         {
            _amountsVisibility = value;
            OnPropertyChanged( "AmountsVisibility" );
         }
      }

      private bool _fetchButtonEnabled = true;
      public bool FetchButtonEnabled
      {
         get => _fetchButtonEnabled;
         set
         {
            _fetchButtonEnabled = value;
            OnPropertyChanged( "FetchButtonEnabled" );
         }
      }

      public static int FullSets { get; set; }
      public ChaosRecipeEnhancer()
      {
         InitializeComponent();
         DataContext = this;
         FullSetsText = "0";
      }

      private void DisableWarnings()
      {
         MainWindow.overlay.WarningMessage = "";
         MainWindow.overlay.ShadowOpacity = 0;
         MainWindow.overlay.WarningMessageVisibility = Visibility.Hidden;
      }

      private async void FetchData()
      {
         if ( FetchingActive )
         {
            return;
         }
         if ( !Properties.Settings.Default.ChaosRecipe && !Properties.Settings.Default.RegalRecipe && !Properties.Settings.Default.ExaltedRecipe )
         {
            _ = MessageBox.Show( "No recipes are enabled. Please pick a recipe.", "No Recipes", MessageBoxButton.OK, MessageBoxImage.Error );
            return;
         }

         DisableWarnings();
         FetchingActive = true;
         CalculationActive = true;

         Dispatcher.Invoke( () =>
          {
             IsIndeterminate = true;
             FetchButtonEnabled = false;
             FetchButtonColor = Brushes.DimGray;
          } );
         await Dispatcher.Invoke( async () =>
          {
             if ( await ApiAdapter.GenerateUri() && await ApiAdapter.GetItems() )
             {
                try
                {
                   await Task.Run( () =>
                    {
                       Data.CheckActives();
                       SetOpacity();
                       CalculationActive = false;
                       _ = Dispatcher.Invoke( () => IsIndeterminate = false );
                    }, Data.ct );
                   await Task.Delay( fetchCooldown * 1000 ).ContinueWith( _ =>
                      {
                         Trace.WriteLine( "waited fetchcooldown" );

                      } );
                }
                catch ( OperationCanceledException ex ) when ( ex.CancellationToken == Data.ct )
                {
                   Trace.WriteLine( "abort" );
                }
             }
             if ( RateLimit.RateLimitExceeded )
             {
                MainWindow.overlay.WarningMessage = "Rate Limit Exceeded! Waiting...";
                MainWindow.overlay.ShadowOpacity = 1;
                MainWindow.overlay.WarningMessageVisibility = Visibility.Visible;
                await Task.Delay( RateLimit.GetSecondsToWait() * 1000 );
                RateLimit.RequestCounter = 0;
                RateLimit.RateLimitExceeded = false;
             }
             if ( RateLimit.BanTime > 0 )
             {
                MainWindow.overlay.WarningMessage = "Temporary Ban! Waiting...";
                MainWindow.overlay.ShadowOpacity = 1;
                MainWindow.overlay.WarningMessageVisibility = Visibility.Visible;
                await Task.Delay( RateLimit.BanTime * 1000 );
                RateLimit.BanTime = 0;
             }
          } );

         CalculationActive = false;
         FetchingActive = false;
         Dispatcher.Invoke( () =>
          {
             IsIndeterminate = false;
             FetchButtonEnabled = true;
             FetchButtonColor = Brushes.Green;
             FetchingActive = false;
          } );
         Trace.WriteLine( "end of fetch function reached" );
      }

      public void RunFetching()
      {
         if ( MainWindow.SettingsComplete )
         {
            if ( !IsOpen )
            {
               return;
            }
            if ( Properties.Settings.Default.StashtabMode == 0 )
            {
               if ( Properties.Settings.Default.StashTabIndices == "" )
               {
                  _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please set Stashtab Indices." );
                  return;
               }
            }
            else if ( Properties.Settings.Default.StashtabMode == 1 )
            {
               if ( Properties.Settings.Default.StashTabName == "" )
               {
                  _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please set Stashtab Prefix." );
                  return;
               }
            }
            if ( CalculationActive )
            {
               Data.cs.Cancel();
               FetchingActive = false;
            }
            else
            {
               if ( !ApiAdapter.IsFetching )
               {
                  Data.cs = new System.Threading.CancellationTokenSource();
                  Data.ct = Data.cs.Token;
                  if ( MainWindow.stashTabOverlay.IsOpen )
                  {
                     MainWindow.stashTabOverlay.Hide();
                  }
                  FetchData();
                  FetchingActive = true;
               }
            }
         }
      }

      private void OnTimedEvent( object source, System.Timers.ElapsedEventArgs e )
      {
         FetchData();
      }

      private void Window_MouseDown( object sender, MouseButtonEventArgs e )
      {
         if ( e.ChangedButton == MouseButton.Left && !Properties.Settings.Default.LockOverlayPosition && Mouse.LeftButton == MouseButtonState.Pressed )
         {
            DragMove();
         }
      }

      private void SetOpacity()
      {
         Dispatcher.Invoke( () =>
          {
             if ( !Data.ActiveItems.HelmetActive )
             {
                HelmetOpacity = deactivatedOpacity;
             }
             else
             {
                HelmetOpacity = activatedOpacity;
             }
             if ( !Data.ActiveItems.GlovesActive )
             {
                GlovesOpacity = deactivatedOpacity;
             }
             else
             {
                GlovesOpacity = activatedOpacity;
             }
             if ( !Data.ActiveItems.BootsActive )
             {
                BootsOpacity = deactivatedOpacity;
             }
             else
             {
                BootsOpacity = activatedOpacity;
             }
             if ( !Data.ActiveItems.WeaponActive )
             {
                WeaponsOpacity = deactivatedOpacity;
             }
             else
             {
                WeaponsOpacity = activatedOpacity;
             }
             if ( !Data.ActiveItems.ChestActive )
             {
                ChestsOpacity = deactivatedOpacity;
             }
             else
             {
                ChestsOpacity = activatedOpacity;
             }
             if ( !Data.ActiveItems.RingActive )
             {
                RingsOpacity = deactivatedOpacity;
             }
             else
             {
                RingsOpacity = activatedOpacity;
             }
             if ( !Data.ActiveItems.AmuletActive )
             {
                AmuletsOpacity = deactivatedOpacity;
             }
             else
             {
                AmuletsOpacity = activatedOpacity;
             }
             if ( !Data.ActiveItems.BeltActive )
             {
                BeltsOpacity = deactivatedOpacity;
             }
             else
             {
                BeltsOpacity = activatedOpacity;
             }
          } );
      }

      public new virtual void Hide()
      {
         IsOpen = false;
         if ( LogWatcher.WorkerThread != null && LogWatcher.WorkerThread.IsAlive )
         {
            LogWatcher.StopWatchingLogFile();
         }
         base.Hide();
      }

      public new virtual void Show()
      {
         IsOpen = true;
         if ( Properties.Settings.Default.AutoFetch )
         {
            Watcher = new LogWatcher();
         }

         base.Show();
      }

      protected override void OnClosing( CancelEventArgs e )
      {
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
