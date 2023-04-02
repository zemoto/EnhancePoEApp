using EnhancePoE.Model;
using ZemotoCommon.UI;

namespace EnhancePoE.UI
{
   internal sealed class StashTabWindowViewModel : ViewModelBase
   {
      public StashTabWindowViewModel( ItemSetData data ) => Data = data;

      private bool _isEditing;
      public bool IsEditing
      {
         get => _isEditing;
         set => SetProperty( ref _isEditing, value );
      }

      public ItemSetData Data { get; }
   }
}
