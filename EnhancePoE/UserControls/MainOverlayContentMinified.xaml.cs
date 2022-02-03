using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnhancePoE.UserControls
{
   public partial class MainOverlayContentMinified : UserControl
   {
      public MainOverlayContentMinified()
      {
         InitializeComponent();
      }
      private void OpenStashTabOverlay_Click( object sender, RoutedEventArgs e )
      {
         MainWindow.RunStashTabOverlay();
      }

      private void RefreshButton_Click_1( object sender, RoutedEventArgs e )
      {
         MainWindow.Overlay.RunFetching();
      }

      private void Border_MouseDown( object sender, MouseButtonEventArgs e )
      {
         MainWindow.RunStashTabOverlay();
      }
   }
}
