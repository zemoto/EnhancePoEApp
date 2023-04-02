using System.Windows;

namespace EnhancePoE.UI
{
   internal partial class StashTabGridControl
   {
      public static readonly DependencyProperty IsQuadProperty = DependencyProperty.Register( 
         nameof( IsQuad ),
         typeof( bool ),
         typeof( StashTabGridControl ),
         new PropertyMetadata( false ) );
      public bool IsQuad
      {
         get => (bool)GetValue( IsQuadProperty );
         set => SetValue( IsQuadProperty, value );
      }

      public StashTabGridControl() => InitializeComponent();
   }
}
