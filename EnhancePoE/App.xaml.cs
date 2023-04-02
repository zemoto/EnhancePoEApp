﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using ZemotoCommon;

namespace EnhancePoE
{
   public partial class App : Application
   {
      private SingleInstance _singleInstance = new( "EnhancePoE" );

      public App()
      {
         if ( !_singleInstance.Claim() )
         {
            Shutdown();
         }

         SetupUnhandledExceptionHandling();
         _singleInstance.PingedByOtherProcess += ( s, e ) => Dispatcher.Invoke( UI.MainWindow.Instance.Show );
      }

      private void SetupUnhandledExceptionHandling()
      {
         // Catch exceptions from all threads in the AppDomain.
         AppDomain.CurrentDomain.UnhandledException += ( sender, args ) => ShowUnhandledException( args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false );

         // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
         TaskScheduler.UnobservedTaskException += ( sender, args ) => ShowUnhandledException( args.Exception, "TaskScheduler.UnobservedTaskException", false );

         // Catch exceptions from a single specific UI dispatcher thread.
         Dispatcher.UnhandledException += ( sender, args ) =>
         {
            // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
            if ( !Debugger.IsAttached )
            {
               args.Handled = true;
               ShowUnhandledException( args.Exception, "Dispatcher.UnhandledException", true );
            }
         };
      }

      private void ShowUnhandledException( Exception e, string unhandledExceptionType, bool promptUserForShutdown )
      {
         var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
         var messageBoxMessage = $"The following exception occurred:\n\n{e}";
         var messageBoxButtons = MessageBoxButton.OK;

         if ( promptUserForShutdown )
         {
            messageBoxMessage += "\n\n\nPlease report this issue on github or discord :)";
            messageBoxButtons = MessageBoxButton.OK;
         }

         // Let the user decide if the app should die or not (if applicable).
         if ( MessageBox.Show( messageBoxMessage, messageBoxTitle, messageBoxButtons ) == MessageBoxResult.Yes )
         {
            Current.Shutdown();
         }
      }

      private void OnStartup( object sender, StartupEventArgs e ) => UI.MainWindow.Instance.Show();
   }
}
