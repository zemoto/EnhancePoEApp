using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnhancePoE.UI
{
   internal partial class MainOverlayContentMinified : UserControl
   {
      private readonly RecipeStatusOverlay _parent;

      public MainOverlayContentMinified( RecipeStatusOverlay parent )
      {
         _parent = parent;
         InitializeComponent();
      }

      private void OpenStashTabOverlay_Click( object sender, RoutedEventArgs e )
      {
         _parent.RunStashTabOverlay();
      }

      private void OnFetchButtonClicked( object sender, RoutedEventArgs e )
      {
         _parent.RunFetching();
      }

      private void Border_MouseDown( object sender, MouseButtonEventArgs e )
      {
         _parent.RunStashTabOverlay();
      }
   }
}
