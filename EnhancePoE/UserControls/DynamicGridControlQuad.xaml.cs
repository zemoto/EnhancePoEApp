using System.Windows.Controls;

namespace EnhancePoE.UserControls
{
   public partial class DynamicGridControlQuad : ItemsControl
   {
      public DynamicGridControlQuad()
      {
         InitializeComponent();
      }

      public Button GetButtonFromCell( object cell )
      {
         for ( int i = 0; i < Items.Count; i++ )
         {
            if ( Items[i] == cell )
            {
               var container = ItemContainerGenerator.ContainerFromIndex( i );
               return Model.Utility.GetChild<Button>( container );
            }
         }
         return null;
      }
   }
}
