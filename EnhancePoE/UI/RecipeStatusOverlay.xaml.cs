using EnhancePoE.Model;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EnhancePoE.UI;

internal partial class RecipeStatusOverlay
{
   private const string _setsFullText = "Sets full!";
   private const int fetchCooldown = 30;

   public bool IsOpen { get; private set; }

   private readonly StashTabWindow _stashTabOverlay;
   private readonly ItemSetManager _itemSetManager;
   private readonly StashTabGetter _stashTabGetter;

   private readonly RecipeStatusOverlayViewModel _model;

   public RecipeStatusOverlay( ItemSetManager itemSetManager, StashTabGetter stashTabGetter )
   {
      _itemSetManager = itemSetManager;
      _stashTabGetter = stashTabGetter;
      DataContext = _model = new RecipeStatusOverlayViewModel( _itemSetManager );
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
      else if ( e.PropertyName == nameof( Properties.Settings.Sets ) || e.PropertyName == nameof( Properties.Settings.SelectedStashTabName ) )
      {
         CheckForFullSets();
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

      if ( _itemSetManager.SelectedStashTab is null )
      {
         _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please select a Stash Tab." );
         return;
      }

      FetchDataAsync(); // Fire and forget async
   }

   private async void FetchDataAsync()
   {
      _model.WarningMessage = string.Empty;

      _model.ShowProgress = true;
      _model.FetchButtonEnabled = false;

      if ( await _stashTabGetter.GetItemsAsync( _itemSetManager.SelectedStashTab ) )
      {
         await Task.Run( _itemSetManager.UpdateData );
         _model.ShowProgress = false;
         await Task.Delay( fetchCooldown * 1000 );
      }
      else if ( RateLimit.RateLimitExceeded )
      {
         _model.WarningMessage = "Rate Limit Exceeded! Waiting...";
         await Task.Delay( RateLimit.GetSecondsToWait() * 1000 );
         RateLimit.RequestCounter = 0;
         RateLimit.RateLimitExceeded = false;
      }
      else if ( RateLimit.BanTime > 0 )
      {
         _model.WarningMessage = "Temporary Ban! Waiting...";
         await Task.Delay( RateLimit.BanTime * 1000 );
         RateLimit.BanTime = 0;
      }

      _model.ShowProgress = false;
      _model.FetchButtonEnabled = true;

      CheckForFullSets();
   }

   private void CheckForFullSets()
   {
      if ( _itemSetManager.SelectedStashTab.FullSets >= Properties.Settings.Default.Sets )
      {
         _model.WarningMessage = _setsFullText;
      }
      else if ( _model.WarningMessage == _setsFullText )
      {
         _model.WarningMessage = string.Empty;
      }
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
