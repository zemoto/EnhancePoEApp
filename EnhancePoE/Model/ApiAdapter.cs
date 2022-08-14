using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using EnhancePoE.Model;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace EnhancePoE
{
   public static class ApiAdapter
   {
      public static bool IsFetching { get; set; }
      private static StashTabPropsList PropsList { get; set; }
      public static bool FetchError { get; set; }
      public static bool FetchingDone { get; set; }

      public static List<StashTab> StashTabList { get; private set; }

      public static async Task<bool> GenerateUri()
      {
         FetchError = false;
         FetchingDone = false;
         Trace.WriteLine( "generating uris!!" );
         if ( Properties.Settings.Default.accName != ""
             && Properties.Settings.Default.League != "" )
         {
            string accName = Properties.Settings.Default.accName.Trim();
            string league = Properties.Settings.Default.League.Trim();

            if ( await GetProps( accName, league ) && !FetchError )
            {
               StashTabList = PropsList.tabs.ConvertAll( x => new StashTab( x.n, x.i ) );
               GenerateStashtabUris( accName, league );
               return true;
            }
         }
         else
         {
            _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please set Accountname, Stash Tab and League." );
         }
         IsFetching = false;
         return false;
      }

      private static void GenerateStashtabUris( string accName, string league )
      {
         if ( StashTabList is null )
         {
            return;
         }

         for ( int i = 0; i < StashTabList.Count; i++ )
         {
            StashTabList[i].StashTabUri = new Uri( $"https://www.pathofexile.com/character-window/get-stash-items?accountName={accName}&tabIndex={i}&league={league}" );
         }
      }

      private static async Task<bool> GetProps( string accName, string league )
      {
         if ( IsFetching )
         {
            return false;
         }
         if ( string.IsNullOrEmpty( Properties.Settings.Default.SessionId ) )
         {
            _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please set PoE Session Id." );
            return false;
         }
         // check rate limit
         if ( RateLimit.CheckForBan() )
         {
            return false;
         }
         // -1 for 1 request + 3 times if ratelimit high exceeded
         if ( RateLimit.RateLimitState[0] >= RateLimit.MaximumRequests - 4 )
         {
            RateLimit.RateLimitExceeded = true;
            return false;
         }
         IsFetching = true;
         var propsUri = new Uri( $"https://www.pathofexile.com/character-window/get-stash-items?accountName={accName}&tabs=1&league={league}" );

         string sessionId = Properties.Settings.Default.SessionId;

         var cC = new CookieContainer();
         cC.Add( propsUri, new Cookie( "POESESSID", sessionId ) );

         using ( var handler = new HttpClientHandler() { CookieContainer = cC } )
         using ( var client = new HttpClient( handler ) )
         {
            // add user agent
            client.DefaultRequestHeaders.Add( "User-Agent", $"EnhancePoEApp/v{Assembly.GetExecutingAssembly().GetName().Version}" );
            using var res = await client.GetAsync( propsUri );
            if ( res.IsSuccessStatusCode )
            {
               using var content = res.Content;
               string resContent = await content.ReadAsStringAsync();
               PropsList = JsonSerializer.Deserialize<StashTabPropsList>( resContent );

               Trace.WriteLine( res.Headers, "res headers" );

               // get new rate limit values
               string rateLimit = res.Headers.GetValues( "X-Rate-Limit-Account" ).FirstOrDefault();
               string rateLimitState = res.Headers.GetValues( "X-Rate-Limit-Account-State" ).FirstOrDefault();
               string responseTime = res.Headers.GetValues( "Date" ).FirstOrDefault();
               RateLimit.DeserializeRateLimits( rateLimit, rateLimitState );
               RateLimit.DeserializeResponseSeconds( responseTime );
            }
            else
            {
               _ = MessageBox.Show( res.StatusCode == HttpStatusCode.Forbidden ?
                  "Connection forbidden. Please check your Accountname and POE Session ID. You may have to refresh your POE Session ID sometimes." :
                  res.ReasonPhrase, "Error fetching data", MessageBoxButton.OK, MessageBoxImage.Error );
               FetchError = true;
               return false;
            }
         }

         IsFetching = false;
         return true;
      }

      public static async Task<bool> GetItems()
      {
         if ( IsFetching )
         {
            Trace.WriteLine( "already fetching" );
            return false;
         }

         if ( string.IsNullOrEmpty( Properties.Settings.Default.SessionId ) )
         {
            _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please set PoE Session Id." );
            return false;
         }

         if ( FetchError )
         {
            return false;
         }

         // check rate limit
         if ( RateLimit.RateLimitState[0] >= RateLimit.MaximumRequests - 5 )
         {
            RateLimit.RateLimitExceeded = true;
            return false;
         }

         IsFetching = true;
         var usedUris = new List<Uri>();

         string sessionId = Properties.Settings.Default.SessionId;

         var cookieContainer = new CookieContainer();
         using ( var handler = new HttpClientHandler() { CookieContainer = cookieContainer } )
         using ( var client = new HttpClient( handler ) )
         {
            // add user agent
            client.DefaultRequestHeaders.Add( "User-Agent", $"EnhancePoEApp/v{Assembly.GetExecutingAssembly().GetName().Version}" );

            // check rate limit ban
            if ( RateLimit.CheckForBan() )
            {
               return false;
            }

            var stashTab = MainWindow.Instance.SelectedStashTab;

            if ( !usedUris.Contains( stashTab.StashTabUri ) )
            {
               cookieContainer.Add( stashTab.StashTabUri, new Cookie( "POESESSID", sessionId ) );
               using var res = await client.GetAsync( stashTab.StashTabUri );
               usedUris.Add( stashTab.StashTabUri );
               if ( res.IsSuccessStatusCode )
               {
                  using var content = res.Content;
                  // get new rate limit values
                  string rateLimit = res.Headers.GetValues( "X-Rate-Limit-Account" ).FirstOrDefault();
                  string rateLimitState = res.Headers.GetValues( "X-Rate-Limit-Account-State" ).FirstOrDefault();
                  string responseTime = res.Headers.GetValues( "Date" ).FirstOrDefault();
                  RateLimit.DeserializeRateLimits( rateLimit, rateLimitState );
                  RateLimit.DeserializeResponseSeconds( responseTime );

                  // deserialize response
                  string resContent = await content.ReadAsStringAsync();
                  var deserializedContent = JsonSerializer.Deserialize<ItemList>( resContent );
                  stashTab.ItemList = deserializedContent.items;
                  stashTab.Quad = deserializedContent.quadLayout;

                  stashTab.CleanItemList();
               }
               else
               {
                  FetchError = true;
                  _ = MessageBox.Show( res.ReasonPhrase, "Error fetching data", MessageBoxButton.OK, MessageBoxImage.Error );
                  return false;
               }
            }
         }

         IsFetching = false;
         FetchingDone = true;
         return true;
      }
   }
}
