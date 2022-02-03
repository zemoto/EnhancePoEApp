using System.Windows;
using System.Windows.Controls;

namespace EnhancePoE.UserControls
{
   public partial class MainOverlayContent : UserControl
   {
      public MainOverlayContent()
      {
         InitializeComponent();
      }

      private void OpenStashTabOverlay_Click( object sender, RoutedEventArgs e )
      {
         MainWindow.RunStashTabOverlay();
      }

      private void RefreshButton_Click_1( object sender, RoutedEventArgs e )
      {
         MainWindow.overlay.RunFetching();
      }

      private void ReloadFilterButton_Click( object sender, RoutedEventArgs e )
      {
         MainWindow.overlay.ReloadItemFilter();
      }
   }
}
