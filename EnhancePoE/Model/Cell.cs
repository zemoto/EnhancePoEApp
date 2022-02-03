using System.ComponentModel;
using System.Windows.Controls;

namespace EnhancePoE.Model
{
   public class Cell : INotifyPropertyChanged
   {
      private bool _active;
      public bool Active
      {
         get => _active;
         set
         {
            _active = value;
            OnPropertyChanged( nameof( Active ) );
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged( string propertyName )
      {
         PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
      }

      public int XIndex { get; set; }
      public int YIndex { get; set; }
      public string ItemID { get; set; }

      public string CellName { get; set; }

      private string _buttonName { get; set; }
      public string ButtonName
      {
         get => _buttonName;
         set
         {
            _buttonName = value;
            OnPropertyChanged( nameof( ButtonName ) );
         }
      }

      public Button CellButton { get; set; }
      public Item CellItem { get; set; }
      public int TabIndex { get; set; }

      public Cell()
      {
         CellButton = new Button
         {
            Content = ButtonName
         };
      }
   }
}
