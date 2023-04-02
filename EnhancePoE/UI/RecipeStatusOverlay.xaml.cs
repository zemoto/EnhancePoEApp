using EnhancePoE.Model;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EnhancePoE.UI
{
   internal partial class RecipeStatusOverlay
   {
      private const int fetchCooldown = 30;

      public bool IsOpen { get; private set; }

      private readonly StashTabWindow _stashTabOverlay;
      private readonly ItemSetManager _itemSetManager;

      private readonly RecipeStatusOverlayViewModel _model;

      public RecipeStatusOverlay( ItemSetManager itemSetManager )
      {
         _itemSetManager = itemSetManager;
         DataContext = _model = new RecipeStatusOverlayViewModel( _itemSetManager.Data );
         _stashTabOverlay = new StashTabWindow( _itemSetManager );

         InitializeComponent();

         UpdateOverlayType();
         Properties.Settings.Default.PropertyChanged += OnSettingsChanged;
      }

      private void OnSettingsChanged( object sender, PropertyChangedEventArgs e )
      {
         if ( e.PropertyName == nameof( Properties.Settings.OverlayMode ) )
         {
            UpdateOverlayType();
         }
      }

      private void UpdateOverlayType()
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

      public void RunFetching()
      {
         if ( !IsOpen )
         {
            return;
         }

         if ( _itemSetManager.Data.Tab is null )
         {
            _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please select a Stash Tab." );
            return;
         }

         if ( !ApiAdapter.IsFetching )
         {
            FetchData();
         }
      }

      private async void FetchData()
      {
         _itemSetManager.Data.WarningMessage = string.Empty;

         _model.ShowProgress = true;
         _model.FetchButtonEnabled = false;

         if ( await ApiAdapter.GetItems( _itemSetManager.Data.Tab ) )
         {
            await Task.Run( _itemSetManager.UpdateData );
            _model.ShowProgress = false;
            await Task.Delay( fetchCooldown * 1000 );
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

         _model.ShowProgress = false;
         _model.FetchButtonEnabled = true;
      }

      private void Window_MouseDown( object sender, MouseButtonEventArgs e )
      {
         if ( e.ChangedButton == MouseButton.Left && !Properties.Settings.Default.LockOverlayPosition && Mouse.LeftButton == MouseButtonState.Pressed )
         {
            DragMove();
         }
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
   }
}
