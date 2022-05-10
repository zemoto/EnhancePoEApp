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

      private void OnFetchButtonClicked( object sender, RoutedEventArgs e )
      {
         MainWindow.Overlay.RunFetching();
      }
   }
}
