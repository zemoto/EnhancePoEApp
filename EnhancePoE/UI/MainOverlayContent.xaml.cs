using System.Windows;
using System.Windows.Controls;

namespace EnhancePoE.UI;

internal partial class MainOverlayContent : UserControl
{
   private readonly RecipeStatusOverlay _parent;

   public MainOverlayContent( RecipeStatusOverlay parent )
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
}
