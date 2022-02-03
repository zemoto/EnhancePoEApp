﻿using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EnhancePoE.Model
{
   public class LogWatcher
   {
      public static Thread WorkerThread { get; set; }
      public static string LastZone { get; set; } = "";
      public static string NewZone { get; set; } = "";

      private static readonly int cooldown = 120;

      private static bool FetchAllowed { get; set; } = true;

      public LogWatcher()
      {
         Trace.WriteLine( "logwatcher created" );

         var wh = new AutoResetEvent( false );
         var fsw = new FileSystemWatcher( Path.GetDirectoryName( @"" + Properties.Settings.Default.LogLocation ) )
         {
            Filter = "Client.txt",
            EnableRaisingEvents = true
         };
         fsw.Changed += ( s, e ) => wh.Set();

         var fs = new FileStream( Properties.Settings.Default.LogLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
         fs.Position = fs.Length;
         WorkerThread = new Thread( () =>
          {
             using ( var sr = new StreamReader( fs ) )
             {
                var s = "";
                while ( true )
                {
                   s = sr.ReadLine();
                   if ( s != null )
                   {
                      string[] phrase = GetPhraseTranslation();
                      string hideout = GetHideoutTranslation();
                      if ( s.Contains( phrase[0] ) && s.Contains( phrase[1] ) && !s.Contains( hideout ) )
                      {
                         LastZone = NewZone;
                         NewZone = s.Substring( s.IndexOf( phrase[0] ) );
                         Trace.WriteLine( NewZone );
                         FetchIfPossible();
                      }
                   }
                   else
                   {
                      _ = wh.WaitOne( 1000 );
                   }
                }
             }
          } );

         StartWatchingLogFile();
      }

      public static string[] GetPhraseTranslation()
      {
         string[] ret = new string[2];
         ret[1] = "";
         switch ( Properties.Settings.Default.Language )
         {
            //english
            case 0:
               ret[0] = "You have entered";
               break;
            //french
            case 1:
               ret[0] = "Vous êtes à présent dans";
               break;
            //german
            case 2:
               ret[0] = "Ihr habt";
               ret[1] = "betreten.";
               break;
            //portuguese
            case 3:
               ret[0] = "Você entrou em";
               break;
            //russian
            case 4:
               ret[0] = "Вы вошли в область";
               break;
            //spanish
            case 5:
               ret[0] = "Has entrado a";
               break;
            //chinese
            case 6:
               ret[0] = "진입했습니다";
               break;
         }
         return ret;
      }

      public static string GetHideoutTranslation()
      {
         switch ( Properties.Settings.Default.Language )
         {
            case 0:
               return "Hideout";
            case 1:
               return "Repaire";
            case 2:
               return "Versteck";
            case 3:
               return "Refúgio";
            case 4:
               return "Убежище";
            case 5:
               return "Guarida";
            case 6:
               return "은신처";
            default:
               return "";
         }
      }

      public static void StartWatchingLogFile()
      {
         WorkerThread.Start();
         Trace.WriteLine( "starting watching" );
      }

      public static void StopWatchingLogFile()
      {
         WorkerThread.Abort();
         Trace.WriteLine( "stop watch" );
      }

      public async void FetchIfPossible()
      {
         if ( FetchAllowed )
         {
            FetchAllowed = false;
            try
            {
               MainWindow.overlay.RunFetching();
               await Task.Delay( cooldown * 1000 ).ContinueWith( _ =>
                  {
                     FetchAllowed = true;
                     Trace.WriteLine( "allow fetch" );
                  } );
            }
            catch
            {
               FetchAllowed = true;
            }
         }
      }
   }
}
