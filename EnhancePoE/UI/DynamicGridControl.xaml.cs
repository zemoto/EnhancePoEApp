namespace EnhancePoE.UI
{
   internal partial class DynamicGridControl
   {
      public int Size { get; }

      public DynamicGridControl( int size )
      {
         Size = size;
         InitializeComponent();
      }
   }
}
