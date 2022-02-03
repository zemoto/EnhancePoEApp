using System.Windows;
using System.Windows.Controls;

namespace EnhancePoE.UserControls
{
   public partial class MainOverlayOnlyButtons : UserControl
   {
      public MainOverlayOnlyButtons()
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
   }
}
