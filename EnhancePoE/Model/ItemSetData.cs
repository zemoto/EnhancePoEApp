using System;
using ZemotoCommon.UI;

namespace EnhancePoE.Model
{
   internal sealed class ItemSetData : ViewModelBase
   {
      private const string _setsFullText = "Sets full!";

      public ItemSetData() => Properties.Settings.Default.PropertyChanged += OnSettingsChanged;

      private void OnSettingsChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
      {
         if ( e.PropertyName == nameof( Properties.Settings.Sets ) )
         {
            UpdateDisplay();
         }
         else if ( e.PropertyName == nameof( Properties.Settings.ShowItemAmount ) )
         {
            UpdateDisplay();
            AmountsAreVisible = Properties.Settings.Default.ShowItemAmount != 0;
         }
      }

      // 0: rings
      // 1: amulets
      // 2: belts
      // 3: chests
      // 4: weaponsSmall
      // 5: weaponsBig
      // 6: gloves
      // 7: helmets
      // 8: boots
      public void UpdateAmounts( int[] amounts )
      {
         _ringsAmount = amounts[0];
         _amuletsAmount = amounts[1];
         _beltsAmount = amounts[2];
         _chestsAmount = amounts[3];
         _weaponsSmallAmount = amounts[4];
         _weaponsBigAmount = amounts[5];
         _glovesAmount = amounts[6];
         _helmetsAmount = amounts[7];
         _bootsAmount = amounts[8];
         UpdateDisplay();
      }

      private void UpdateDisplay()
      {
         OnPropertyChanged( nameof( RingsAmount ) );
         OnPropertyChanged( nameof( RingsActive ) );

         OnPropertyChanged( nameof( AmuletsAmount ) );
         OnPropertyChanged( nameof( AmuletsActive ) );

         OnPropertyChanged( nameof( BeltsAmount ) );
         OnPropertyChanged( nameof( BeltsActive ) );

         OnPropertyChanged( nameof( ChestsAmount ) );
         OnPropertyChanged( nameof( ChestsActive ) );

         OnPropertyChanged( nameof( WeaponsAmount ) );
         OnPropertyChanged( nameof( WeaponsActive ) );

         OnPropertyChanged( nameof( GlovesAmount ) );
         OnPropertyChanged( nameof( GlovesActive ) );

         OnPropertyChanged( nameof( HelmetsAmount ) );
         OnPropertyChanged( nameof( HelmetsActive ) );

         OnPropertyChanged( nameof( BootsAmount ) );
         OnPropertyChanged( nameof( BootsActive ) );

         CheckForFullSets();
      }

      private void CheckForFullSets()
      {
         if ( FullSets >= Properties.Settings.Default.Sets )
         {
            WarningMessage = _setsFullText;
         }
         else if ( WarningMessage == _setsFullText )
         {
            WarningMessage = string.Empty;
         }
      }

      private int _fullSets;
      public int FullSets
      {
         get => _fullSets;
         set
         {
            if ( SetProperty( ref _fullSets, value ) )
            {
               CheckForFullSets();
            }
         }
      }

      private bool ShowAmountNeeded => Properties.Settings.Default.ShowItemAmount == 2;

      private int _ringsAmount;
      public int RingsAmount => ShowAmountNeeded ? Math.Max( ( Properties.Settings.Default.Sets * 2 ) - _ringsAmount, 0 ) : _ringsAmount;
      public bool RingsActive => ( Properties.Settings.Default.Sets * 2 ) - _ringsAmount > 0;

      private int _amuletsAmount;
      public int AmuletsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _amuletsAmount, 0 ) : _amuletsAmount;
      public bool AmuletsActive => Properties.Settings.Default.Sets - _amuletsAmount > 0;

      private int _beltsAmount;
      public int BeltsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _beltsAmount, 0 ) : _beltsAmount;
      public bool BeltsActive => Properties.Settings.Default.Sets - _beltsAmount > 0;

      private int _chestsAmount;
      public int ChestsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _chestsAmount, 0 ) : _chestsAmount;
      public bool ChestsActive => Properties.Settings.Default.Sets - _chestsAmount > 0;

      private int _weaponsSmallAmount;
      private int _weaponsBigAmount;
      public int WeaponsAmount => ShowAmountNeeded ? Math.Max( ( Properties.Settings.Default.Sets * 2 ) - ( _weaponsSmallAmount + ( _weaponsBigAmount * 2 ) ), 0 ) : _weaponsSmallAmount + ( _weaponsBigAmount * 2 );
      public bool WeaponsActive => ( Properties.Settings.Default.Sets * 2 ) - ( _weaponsSmallAmount + ( _weaponsBigAmount * 2 ) ) > 0;

      private int _glovesAmount;
      public int GlovesAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _glovesAmount, 0 ) : _glovesAmount;
      public bool GlovesActive => Properties.Settings.Default.Sets - _glovesAmount > 0;

      private int _helmetsAmount;
      public int HelmetsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _helmetsAmount, 0 ) : _helmetsAmount;
      public bool HelmetsActive => Properties.Settings.Default.Sets - _helmetsAmount > 0;

      private int _bootsAmount;
      public int BootsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _bootsAmount, 0 ) : _bootsAmount;
      public bool BootsActive => Properties.Settings.Default.Sets - _bootsAmount > 0;

      private string _warningMessage;
      public string WarningMessage
      {
         get => _warningMessage;
         set => SetProperty( ref _warningMessage, value );
      }

      private bool _amountsVisibility = Properties.Settings.Default.ShowItemAmount != 0;
      public bool AmountsAreVisible
      {
         get => _amountsVisibility;
         private set => SetProperty( ref _amountsVisibility, value );
      }
   }
}
