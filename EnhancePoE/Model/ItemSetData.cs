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
         if ( e.PropertyName == nameof( Properties.Settings.Sets ) || e.PropertyName == nameof( Properties.Settings.ShowItemAmount ) )
         {
            UpdateDisplayAmounts();
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
         UpdateDisplayAmounts();
      }

      private void UpdateDisplayAmounts()
      {
         OnPropertyChanged( nameof( RingsAmount ) );
         OnPropertyChanged( nameof( AmuletsAmount ) );
         OnPropertyChanged( nameof( BeltsAmount ) );
         OnPropertyChanged( nameof( ChestsAmount ) );
         OnPropertyChanged( nameof( WeaponsAmount ) );
         OnPropertyChanged( nameof( GlovesAmount ) );
         OnPropertyChanged( nameof( HelmetsAmount ) );
         OnPropertyChanged( nameof( BootsAmount ) );

         CheckForFullSets();
      }

      private void CheckForFullSets()
      {
         if ( FullSets == Properties.Settings.Default.Sets )
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

      private int _amuletsAmount;
      public int AmuletsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _amuletsAmount, 0 ) : _amuletsAmount;

      private int _beltsAmount;
      public int BeltsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _beltsAmount, 0 ) : _beltsAmount;

      private int _chestsAmount;
      public int ChestsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _chestsAmount, 0 ) : _chestsAmount;

      private int _weaponsSmallAmount;
      private int _weaponsBigAmount;
      public int WeaponsAmount => ShowAmountNeeded ? Math.Max( ( Properties.Settings.Default.Sets * 2 ) - ( _weaponsSmallAmount + ( _weaponsBigAmount * 2 ) ), 0 ) : _weaponsSmallAmount + ( _weaponsBigAmount * 2 );

      private int _glovesAmount;
      public int GlovesAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _glovesAmount, 0 ) : _glovesAmount;

      private int _helmetsAmount;
      public int HelmetsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _helmetsAmount, 0 ) : _helmetsAmount;

      private int _bootsAmount;
      public int BootsAmount => ShowAmountNeeded ? Math.Max( Properties.Settings.Default.Sets - _bootsAmount, 0 ) : _bootsAmount;

      private bool _glovesActive = true;
      public bool GlovesActive
      {
         get => _glovesActive;
         set => SetProperty( ref _glovesActive, value );
      }

      private bool _helmetActive = true;
      public bool HelmetActive
      {
         get => _helmetActive;
         set => SetProperty( ref _helmetActive, value );
      }

      private bool _bootsActive = true;
      public bool BootsActive
      {
         get => _bootsActive;
         set => SetProperty( ref _bootsActive, value );
      }

      private bool _chestActive = true;
      public bool ChestActive
      {
         get => _chestActive;
         set => SetProperty( ref _chestActive, value );
      }

      private bool _weaponActive = true;
      public bool WeaponActive
      {
         get => _weaponActive;
         set => SetProperty( ref _weaponActive, value );
      }

      private bool _ringActive = true;
      public bool RingActive
      {
         get => _ringActive;
         set => SetProperty( ref _ringActive, value );
      }

      private bool _amuletActive = true;
      public bool AmuletActive
      {
         get => _amuletActive;
         set => SetProperty( ref _amuletActive, value );
      }

      private bool _beltActive = true;
      public bool BeltActive
      {
         get => _beltActive;
         set => SetProperty( ref _beltActive, value );
      }

      private string _warningMessage;
      public string WarningMessage
      {
         get => _warningMessage;
         set => SetProperty( ref _warningMessage, value );
      }
   }
}
