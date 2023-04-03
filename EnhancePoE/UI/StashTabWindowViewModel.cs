using ZemotoCommon.UI;

namespace EnhancePoE.UI;

internal sealed class StashTabWindowViewModel : ViewModelBase
{
   public StashTabWindowViewModel( ISelectedStashTabHandler selectedStashTabHandler ) => SelectedStashTabHandler = selectedStashTabHandler;

   private bool _isEditing;
   public bool IsEditing
   {
      get => _isEditing;
      set => SetProperty( ref _isEditing, value );
   }

   public ISelectedStashTabHandler SelectedStashTabHandler { get; }
}
