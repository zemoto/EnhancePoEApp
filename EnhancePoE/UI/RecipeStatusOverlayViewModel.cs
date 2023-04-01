using EnhancePoE.Model;
using ZemotoCommon.UI;

namespace EnhancePoE.UI
{
   internal sealed class RecipeStatusOverlayViewModel : ViewModelBase
   {
      public RecipeStatusOverlayViewModel( ItemSetData data )
      {
         Data = data;
      }

      public ItemSetData Data { get; }

      private bool _showProgress;
      public bool ShowProgress
      {
         get => _showProgress;
         set => SetProperty( ref _showProgress, value );
      }

      private bool _fetchButtonEnabled = true;
      public bool FetchButtonEnabled
      {
         get => _fetchButtonEnabled;
         set => SetProperty( ref _fetchButtonEnabled, value );
      }
   }
}
