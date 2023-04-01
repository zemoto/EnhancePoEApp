using EnhancePoE.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EnhancePoE.UI
{
   internal partial class RecipeStatusOverlay : Window, INotifyPropertyChanged
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

      public ItemSetData Data => _itemSetManager.Data;

      public int FullSets { get; set; }

      private readonly StashTabWindow _stashTabOverlay;
      private readonly ItemSetManager _itemSetManager = new();

      public RecipeStatusOverlay()
      {
         InitializeComponent();
         DataContext = this;

         _stashTabOverlay = new StashTabWindow( _itemSetManager );
      }

      private async void FetchData()
      {
         if ( FetchingActive )
         {
            return;
         }

         _itemSetManager.Data.WarningMessage = string.Empty;
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
                       _itemSetManager.CheckActives();
                       SetOpacity();
                       CalculationActive = false;
                       _ = Dispatcher.Invoke( () => IsIndeterminate = false );
                    }, _itemSetManager.CancelTokenSource.Token );
                   await Task.Delay( fetchCooldown * 1000 ).ContinueWith( _ =>
                      {
                         Trace.WriteLine( "waited fetchcooldown" );

                      } );
                }
                catch ( OperationCanceledException ex ) when ( ex.CancellationToken == _itemSetManager.CancelTokenSource.Token )
                {
                   Trace.WriteLine( "abort" );
                }
             }
             if ( RateLimit.RateLimitExceeded )
             {
               _itemSetManager.Data.WarningMessage = "Rate Limit Exceeded! Waiting...";
                await Task.Delay( RateLimit.GetSecondsToWait() * 1000 );
                RateLimit.RequestCounter = 0;
                RateLimit.RateLimitExceeded = false;
             }
             if ( RateLimit.BanTime > 0 )
             {
                _itemSetManager.Data.WarningMessage = "Temporary Ban! Waiting...";
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
            _itemSetManager.CancelTokenSource.Cancel();
            FetchingActive = false;
         }
         else
         {
            if ( !ApiAdapter.IsFetching )
            {
               _itemSetManager.ResetCancelToken();
               if ( _stashTabOverlay.IsOpen )
               {
                  _stashTabOverlay.Hide();
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
             if ( !_itemSetManager.Data.HelmetActive )
             {
                HelmetOpacity = deactivatedOpacity;
             }
             else
             {
                HelmetOpacity = activatedOpacity;
             }
             if ( !_itemSetManager.Data.GlovesActive )
             {
                GlovesOpacity = deactivatedOpacity;
             }
             else
             {
                GlovesOpacity = activatedOpacity;
             }
             if ( !_itemSetManager.Data.BootsActive )
             {
                BootsOpacity = deactivatedOpacity;
             }
             else
             {
                BootsOpacity = activatedOpacity;
             }
             if ( !_itemSetManager.Data.WeaponActive )
             {
                WeaponsOpacity = deactivatedOpacity;
             }
             else
             {
                WeaponsOpacity = activatedOpacity;
             }
             if ( !_itemSetManager.Data.ChestActive )
             {
                ChestsOpacity = deactivatedOpacity;
             }
             else
             {
                ChestsOpacity = activatedOpacity;
             }
             if ( !_itemSetManager.Data.RingActive )
             {
                RingsOpacity = deactivatedOpacity;
             }
             else
             {
                RingsOpacity = activatedOpacity;
             }
             if ( !_itemSetManager.Data.AmuletActive )
             {
                AmuletsOpacity = deactivatedOpacity;
             }
             else
             {
                AmuletsOpacity = activatedOpacity;
             }
             if ( !_itemSetManager.Data.BeltActive )
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
         _stashTabOverlay.Hide();
         base.Hide();
      }

      public new virtual void Show()
      {
         IsOpen = true;
         base.Show();
      }

      public void UpdateOverlayType()
      {
         if ( Properties.Settings.Default.OverlayMode == 0 )
         {
            MainOverlayContentControl.Content = new MainOverlayContent( this );
         }
         else if ( Properties.Settings.Default.OverlayMode == 1 )
         {
            MainOverlayContentControl.Content = new MainOverlayContentMinified( this );
         }
      }

      public void RunStashTabOverlay()
      {
         if ( _stashTabOverlay.IsOpen )
         {
            _stashTabOverlay.Hide();
         }
         else
         {
            _stashTabOverlay.Show();
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
