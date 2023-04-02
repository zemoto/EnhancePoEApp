using System.Windows;
using ZemotoCommon.UI;

namespace EnhancePoE.UI
{
   internal sealed class StashTabWindowViewModel : ViewModelBase
   {
      private bool _isEditing;
      public bool IsEditing
      {
         get => _isEditing;
         set => SetProperty( ref _isEditing, value );
      }
   }
}
