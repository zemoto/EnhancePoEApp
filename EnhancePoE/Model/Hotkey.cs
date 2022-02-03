using System.Text;
using System.Windows.Input;

namespace EnhancePoE
{
   public class Hotkey
   {
      public Key Key { get; }

      public ModifierKeys Modifiers { get; }

      public Hotkey( Key key, ModifierKeys modifiers )
      {
         Key = key;
         Modifiers = modifiers;
      }

      public override string ToString()
      {
         var str = new StringBuilder();

         if ( Modifiers.HasFlag( ModifierKeys.Control ) )
            _ = str.Append( "Ctrl + " );
         if ( Modifiers.HasFlag( ModifierKeys.Shift ) )
            _ = str.Append( "Shift + " );
         if ( Modifiers.HasFlag( ModifierKeys.Alt ) )
            _ = str.Append( "Alt + " );
         if ( Modifiers.HasFlag( ModifierKeys.Windows ) )
            _ = str.Append( "Win + " );

         _ = str.Append( Key );

         return str.ToString();
      }
   }
}
