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
               GenerateStashTabs();
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

      private static void GenerateStashTabs()
      {
         var ret = new List<StashTab>();

         // mode = ID
         if ( Properties.Settings.Default.StashtabMode == 0 )
         {
            StashTabList.GetStashTabIndices();
            if ( PropsList != null )
            {
               foreach ( var p in PropsList.Tabs )
               {
                  for ( int i = StashTabList.StashTabIndices.Count - 1; i > -1; i-- )
                  {
                     if ( StashTabList.StashTabIndices[i] == p.I )
                     {
                        StashTabList.StashTabIndices.RemoveAt( i );
                        ret.Add( new StashTab( p.N, p.I ) );
                     }
                  }
               }
               StashTabList.StashTabs = ret;
               GetAllTabNames();
            }
         }
         // mode = Name
         else
         {
            if ( PropsList != null )
            {
               string stashName = Properties.Settings.Default.StashTabName;
               foreach ( var p in PropsList.Tabs )
               {
                  if ( p.N.StartsWith( stashName ) )
                  {
                     ret.Add( new StashTab( p.N, p.I ) );
                  }
               }
               StashTabList.StashTabs = ret;
            }
         }
         Trace.WriteLine( StashTabList.StashTabs.Count, "stash tab count" );
      }

      private static void GenerateStashtabUris( string accName, string league )
      {
         foreach ( var i in StashTabList.StashTabs )
         {
            string stashTab = i.TabIndex.ToString();
            i.StashTabUri = new Uri( $"https://www.pathofexile.com/character-window/get-stash-items?accountName={accName}&tabIndex={stashTab}&league={league}" );
         }
      }

      private static void GetAllTabNames()
      {
         foreach ( var s in StashTabList.StashTabs )
         {
            foreach ( var p in PropsList.Tabs )
            {
               if ( s.TabIndex == p.I )
               {
                  s.TabName = p.N;
               }
            }
         }
      }

      private static async Task<bool> GetProps( string accName, string league )
      {
         //Trace.WriteLine(IsFetching, "isfetching props");
         if ( IsFetching )
         {
            return false;
         }
         if ( Properties.Settings.Default.SessionId == "" )
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
            //Trace.WriteLine("is here");
            // add user agent
            client.DefaultRequestHeaders.Add( "User-Agent", $"EnhancePoEApp/v{Assembly.GetExecutingAssembly().GetName().Version}" );
            using ( var res = await client.GetAsync( propsUri ) )
            {
               //Trace.WriteLine("is NOT here");
               if ( res.IsSuccessStatusCode )
               {
                  //Trace.WriteLine("is not herre");
                  using ( var content = res.Content )
                  {
                     string resContent = await content.ReadAsStringAsync();
                     //Trace.Write(resContent);
                     PropsList = JsonSerializer.Deserialize<StashTabPropsList>( resContent );

                     Trace.WriteLine( res.Headers, "res headers" );

                     // get new rate limit values
                     //RateLimit.IncreaseRequestCounter();
                     string rateLimit = res.Headers.GetValues( "X-Rate-Limit-Account" ).FirstOrDefault();
                     string rateLimitState = res.Headers.GetValues( "X-Rate-Limit-Account-State" ).FirstOrDefault();
                     string responseTime = res.Headers.GetValues( "Date" ).FirstOrDefault();
                     RateLimit.DeserializeRateLimits( rateLimit, rateLimitState );
                     RateLimit.DeserializeResponseSeconds( responseTime );
                  }
               }
               else
               {
                  if ( res.StatusCode == HttpStatusCode.Forbidden )
                  {
                     _ = MessageBox.Show( "Connection forbidden. Please check your Accountname and POE Session ID. You may have to refresh your POE Session ID sometimes.", "Error fetching data", MessageBoxButton.OK, MessageBoxImage.Error );
                  }
                  else
                  {
                     _ = MessageBox.Show( res.ReasonPhrase, "Error fetching data", MessageBoxButton.OK, MessageBoxImage.Error );
                  }
                  FetchError = true;
                  return false;
               }
            }
         }

         //await Task.Delay(1000);
         IsFetching = false;
         return true;
      }

      public async static Task<bool> GetItems()
      {
         if ( IsFetching )
         {
            Trace.WriteLine( "already fetching" );
            return false;
         }
         if ( Properties.Settings.Default.SessionId == "" )
         {
            _ = MessageBox.Show( "Missing Settings!" + Environment.NewLine + "Please set PoE Session Id." );
            return false;
         }
         if ( FetchError )
         {
            return false;
         }
         // check rate limit
         if ( RateLimit.RateLimitState[0] >= RateLimit.MaximumRequests - StashTabList.StashTabs.Count - 4 )
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
            foreach ( var i in StashTabList.StashTabs )
            {
               // check rate limit ban
               if ( RateLimit.CheckForBan() )
               {
                  return false;
               }
               if ( !usedUris.Contains( i.StashTabUri ) )
               {
                  cookieContainer.Add( i.StashTabUri, new Cookie( "POESESSID", sessionId ) );
                  using ( var res = await client.GetAsync( i.StashTabUri ) )
                  {
                     usedUris.Add( i.StashTabUri );
                     if ( res.IsSuccessStatusCode )
                     {
                        using ( var content = res.Content )
                        {
                           // get new rate limit values
                           string rateLimit = res.Headers.GetValues( "X-Rate-Limit-Account" ).FirstOrDefault();
                           string rateLimitState = res.Headers.GetValues( "X-Rate-Limit-Account-State" ).FirstOrDefault();
                           string responseTime = res.Headers.GetValues( "Date" ).FirstOrDefault();
                           RateLimit.DeserializeRateLimits( rateLimit, rateLimitState );
                           RateLimit.DeserializeResponseSeconds( responseTime );

                           // deserialize response
                           string resContent = await content.ReadAsStringAsync();
                           var deserializedContent = JsonSerializer.Deserialize<ItemList>( resContent );
                           i.ItemList = deserializedContent.Items;
                           i.Quad = deserializedContent.QuadLayout;

                           i.CleanItemList();
                        }
                     }
                     else
                     {
                        FetchError = true;
                        _ = MessageBox.Show( res.ReasonPhrase, "Error fetching data", MessageBoxButton.OK, MessageBoxImage.Error );
                        return false;
                     }
                  }
               }
            }
         }

         IsFetching = false;
         FetchingDone = true;
         return true;
      }
   }
}
