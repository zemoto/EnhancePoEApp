namespace EnhancePoE.UserControls
{
   public partial class DynamicGridControl
   {
      public int Size { get; }

      public DynamicGridControl( int size )
      {
         Size = size;
         InitializeComponent();
      }
   }
}
