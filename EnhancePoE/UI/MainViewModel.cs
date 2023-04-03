using EnhancePoE.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZemotoCommon.UI;

namespace EnhancePoE.UI;

internal sealed class MainViewModel : ViewModelBase
{

   private bool _initialized;

   public MainViewModel( ISelectedStashTabHandler selectedStashTabHandler )
   {
      SelectedStashTabHandler = selectedStashTabHandler;
      Settings.PropertyChanged += OnSettingsChanged;
   }

   private void OnSettingsChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
   {
      if ( e.PropertyName == nameof( Settings.League ) && _initialized )
      {
         StashTabList.Clear();
         SelectedStashTabHandler.SelectedStashTab = null;
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
   public ISelectedStashTabHandler SelectedStashTabHandler { get; }

   public ObservableCollection<string> LeagueList { get; } = new();
   public ObservableCollection<StashTab> StashTabList { get; } = new();


   private bool _fetchingStashTabs;
   public bool FetchingStashTabs
   {
      get => _fetchingStashTabs;
      set => SetProperty( ref _fetchingStashTabs, value );
   }
}
