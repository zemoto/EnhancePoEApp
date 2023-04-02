using EnhancePoE.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZemotoCommon.UI;

namespace EnhancePoE.UI
{
   internal sealed class MainViewModel : ViewModelBase
   {
      public const string AppVersionText = "v.1.5.1-zemoto";

      private bool _initialized;

      public MainViewModel() => Settings.PropertyChanged += OnSettingsChanged;

      private void OnSettingsChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
      {
         if ( e.PropertyName == nameof( Settings.League ) && _initialized )
         {
            StashTabList.Clear();
            SelectedStashTab = null;
         }
      }

      public void UpdateLeagueList( IEnumerable<string> leagueList )
      {
         var selectedLeague = Settings.League;
         LeagueList.Clear();
         foreach ( var league in leagueList )
         {
            LeagueList.Add( league );
         }

         if ( string.IsNullOrEmpty( selectedLeague ) )
         {
            Settings.League = LeagueList.FirstOrDefault();
         }
         else
         {
            Settings.League = selectedLeague;
         }

         _initialized = true;
      }

      public Properties.Settings Settings { get; } = Properties.Settings.Default;

      public ObservableCollection<string> LeagueList { get; } = new();
      public ObservableCollection<StashTab> StashTabList { get; } = new();

      private StashTab _selectedStashTab;
      public StashTab SelectedStashTab
      {
         get => _selectedStashTab;
         set
         {
            if ( SetProperty( ref _selectedStashTab, value ) && _selectedStashTab is not null )
            {
               Settings.SelectedStashTabName = _selectedStashTab.TabName;
            }
         }
      }

      private bool _fetchingStashTabs;
      public bool FetchingStashTabs
      {
         get => _fetchingStashTabs;
         set => SetProperty( ref _fetchingStashTabs, value );
      }
   }
}
