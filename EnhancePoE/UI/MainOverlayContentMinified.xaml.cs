using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnhancePoE.UI
{
   internal partial class MainOverlayContentMinified : UserControl
   {
      public MainOverlayContentMinified()
      {
         InitializeComponent();
      }

      private void OpenStashTabOverlay_Click( object sender, RoutedEventArgs e )
      {
         MainWindow.RunStashTabOverlay();
      }

      private void OnFetchButtonClicked( object sender, RoutedEventArgs e )
      {
         MainWindow.RecipeOverlay.RunFetching();
      }

      private void Border_MouseDown( object sender, MouseButtonEventArgs e )
      {
         MainWindow.RunStashTabOverlay();
      }
   }
}
