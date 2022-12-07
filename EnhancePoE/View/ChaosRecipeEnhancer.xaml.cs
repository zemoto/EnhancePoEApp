using EnhancePoE.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EnhancePoE
{
   public partial class ChaosRecipeEnhancer : Window, INotifyPropertyChanged
   {
      // toggle fetch button
      public static bool FetchingActive { get; set; }
      // fetching and calculations currently active
      public static bool CalculationActive { get; set; }
      //public static System.Timers.Timer aTimer;

      private const double deactivatedOpacity = .1;
      private const double activatedOpacity = 1;

      private const int fetchCooldown = 30;

      public bool IsOpen { get; set; }

      private string _warningMessage;
      public string WarningMessage
      {
         get => _warningMessage;
         set
         {
            _warningMessage = value;
            OnPropertyChanged( nameof( WarningMessage ) );
         }
      }

      private Visibility _warningMessageVisibility = Visibility.Collapsed;
      public Visibility WarningMessageVisibility
      {
         get => _warningMessageVisibility;
         set
         {
            _warningMessageVisibility = value;
            OnPropertyChanged( nameof( WarningMessageVisibility ) );
         }
      }
      private double _shadowOpacity;
      public double ShadowOpacity
      {
         get => _shadowOpacity;
         set
         {
            _shadowOpacity = value;
            OnPropertyChanged( nameof( ShadowOpacity ) );
         }
      }

      private string _fullSetsText = "0";
      public string FullSetsText
      {
         get => _fullSetsText;
         set
         {
            _fullSetsText = value;
            OnPropertyChanged( nameof( FullSetsText ) );
         }
      }

      private bool _isIndeterminate;
      public bool IsIndeterminate
      {
         get => _isIndeterminate;
         set
         {
            _isIndeterminate = value;
            OnPropertyChanged( nameof( IsIndeterminate ) );
         }
      }

      private string _openStashOverlayButtonContent = "Stash";
      public string OpenStashOverlayButtonContent
      {
         get => _openStashOverlayButtonContent;
         set
         {
            _openStashOverlayButtonContent = value;
            OnPropertyChanged( nameof( OpenStashOverlayButtonContent ) );
         }
      }

      private double _helmetOpacity = activatedOpacity;
      public double HelmetOpacity
      {
         get => _helmetOpacity;
         set
         {
            _helmetOpacity = value;
            OnPropertyChanged( nameof( HelmetOpacity ) );
         }
      }
      private double _bootsOpacity = activatedOpacity;
      public double BootsOpacity
      {
         get => _bootsOpacity;
         set
         {
            _bootsOpacity = value;
            OnPropertyChanged( nameof( BootsOpacity ) );
         }
      }
      private double _glovesOpacity = activatedOpacity;
      public double GlovesOpacity
      {
         get => _glovesOpacity;
         set
         {
            _glovesOpacity = value;
            OnPropertyChanged( nameof( GlovesOpacity ) );
         }
      }
      private double _chestsOpacity = activatedOpacity;
      public double ChestsOpacity
      {
         get => _chestsOpacity;
         set
         {
            _chestsOpacity = value;
            OnPropertyChanged( nameof( ChestsOpacity ) );
         }
      }
      private double _weaponsOpacity = activatedOpacity;
      public double WeaponsOpacity
      {
         get => _weaponsOpacity;
         set
         {
            _weaponsOpacity = value;
            OnPropertyChanged( nameof( WeaponsOpacity ) );
         }
      }
      private double _ringsOpacity = activatedOpacity;
      public double RingsOpacity
      {
         get => _ringsOpacity;
         set
         {
            _ringsOpacity = value;
            OnPropertyChanged( nameof( RingsOpacity ) );
         }
      }
      private double _amuletsOpacity = activatedOpacity;
      public double AmuletsOpacity
      {
         get => _amuletsOpacity;
         set
         {
            _amuletsOpacity = value;
            OnPropertyChanged( nameof( AmuletsOpacity ) );
         }
      }
      private double _beltsOpacity = activatedOpacity;
      public double BeltsOpacity
      {
         get => _beltsOpacity;
         set
         {
            _beltsOpacity = value;
            OnPropertyChanged( nameof( BeltsOpacity ) );
         }
      }

      private ContentElement _mainOverlayContent;
      public ContentElement MainOverlayContent
      {
         get => _mainOverlayContent;
         set
         {
            _mainOverlayContent = value;
            OnPropertyChanged( nameof( MainOverlayContent ) );
         }
      }

      private int _ringsAmount;
      public int RingsAmount
      {
         get => _ringsAmount;
         set
         {
            _ringsAmount = value;
            OnPropertyChanged( nameof( RingsAmount ) );
         }
      }
      private int _amuletsAmount;
      public int AmuletsAmount
      {
         get => _amuletsAmount;
         set
         {
            _amuletsAmount = value;
            OnPropertyChanged( nameof( AmuletsAmount ) );
         }
      }
      private int _beltsAmount;
      public int BeltsAmount
      {
         get => _beltsAmount;
         set
         {
            _beltsAmount = value;
            OnPropertyChanged( nameof( BeltsAmount ) );
         }
      }
      private int _chestsAmount;
      public int ChestsAmount
      {
         get => _chestsAmount;
         set
         {
            _chestsAmount = value;
            OnPropertyChanged( nameof( ChestsAmount ) );
         }
      }
      private int _weaponsAmount;
      public int WeaponsAmount
      {
         get => _weaponsAmount;
         set
         {
            _weaponsAmount = value;
            OnPropertyChanged( nameof( WeaponsAmount ) );
         }
      }
      private int _glovesAmount;
      public int GlovesAmount
      {
         get => _glovesAmount;
         set
         {
            _glovesAmount = value;
            OnPropertyChanged( nameof( GlovesAmount ) );
         }
      }
      private int _helmetsAmount;
      public int HelmetsAmount
      {
         get => _helmetsAmount;
         set
         {
            _helmetsAmount = value;
            OnPropertyChanged( nameof( HelmetsAmount ) );
         }
      }
      private int _bootsAmount;
      public int BootsAmount
      {
         get => _bootsAmount;
         set
         {
            _bootsAmount = value;
            OnPropertyChanged( nameof( BootsAmount ) );
         }
      }

      private Visibility _amountsVisibility = Visibility.Hidden;
      public Visibility AmountsVisibility
      {
         get => _amountsVisibility;
         set
         {
            _amountsVisibility = value;
            OnPropertyChanged( nameof( AmountsVisibility ) );
         }
      }

      private bool _fetchButtonEnabled = true;
      public bool FetchButtonEnabled
      {
         get => _fetchButtonEnabled;
         set
         {
            _fetchButtonEnabled = value;
            OnPropertyChanged( nameof( FetchButtonEnabled ) );
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
         MainWindow.Overlay.WarningMessage = "";
         MainWindow.Overlay.ShadowOpacity = 0;
         MainWindow.Overlay.WarningMessageVisibility = Visibility.Collapsed;
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
          } );
         await Dispatcher.Invoke( async () =>
          {
             if ( await ApiAdapter.GetItems() )
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
                MainWindow.Overlay.WarningMessage = "Rate Limit Exceeded! Waiting...";
                MainWindow.Overlay.ShadowOpacity = 1;
                MainWindow.Overlay.WarningMessageVisibility = Visibility.Visible;
                await Task.Delay( RateLimit.GetSecondsToWait() * 1000 );
                RateLimit.RequestCounter = 0;
                RateLimit.RateLimitExceeded = false;
             }
             if ( RateLimit.BanTime > 0 )
             {
                MainWindow.Overlay.WarningMessage = "Temporary Ban! Waiting...";
                MainWindow.Overlay.ShadowOpacity = 1;
                MainWindow.Overlay.WarningMessageVisibility = Visibility.Visible;
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
             FetchingActive = false;
          } );
         Trace.WriteLine( "end of fetch function reached" );
      }

      public void RunFetching()
      {
         if ( !IsOpen )
         {
            return;
         }
         if ( MainWindow.Instance.SelectedStashTab is null )
         {
            _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please select a Stash Tab." );
            return;
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
               if ( MainWindow.StashTabOverlay.IsOpen )
               {
                  MainWindow.StashTabOverlay.Hide();
               }
               FetchData();
               FetchingActive = true;
            }
         }
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
         base.Hide();
      }

      public new virtual void Show()
      {
         IsOpen = true;
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
