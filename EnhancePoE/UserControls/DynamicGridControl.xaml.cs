using EnhancePoE.Model;
using System.Windows.Controls;

namespace EnhancePoE.UserControls
{
   public partial class DynamicGridControl : ItemsControl
   {
      public DynamicGridControl()
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
               return Utility.GetChild<Button>( container );
            }
         }
         return null;
      }
   }
}
