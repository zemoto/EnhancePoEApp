using System.Windows;

namespace EnhancePoE
{
   public partial class HotkeyWindow : Window
   {
      private string type;
      private MainWindow mainWindow;

      public HotkeyWindow( MainWindow mainWindow, string hotkeyType )
      {
         this.mainWindow = mainWindow;

         type = hotkeyType;

         InitializeComponent();
      }

      private void SaveHotkeyButton_Click( object sender, RoutedEventArgs e )
      {
         if ( type == "refresh" )
         {
            HotkeysManager.RemoveHotkey( HotkeysManager.refreshModifier, HotkeysManager.refreshKey );
            if ( CustomHotkeyToggle.Hotkey == null )
            {
               Properties.Settings.Default.HotkeyRefresh = "< not set >";
            }
            else
            {
               Properties.Settings.Default.HotkeyRefresh = CustomHotkeyToggle.Hotkey.ToString();
               HotkeysManager.GetRefreshHotkey();
            }
            ReApplyHotkeys();
         }
         else if ( type == "toggle" )
         {
            HotkeysManager.RemoveHotkey( HotkeysManager.toggleModifier, HotkeysManager.toggleKey );
            if ( CustomHotkeyToggle.Hotkey == null )
            {
               Properties.Settings.Default.HotkeyToggle = "< not set >";
            }
            else
            {
               Properties.Settings.Default.HotkeyToggle = CustomHotkeyToggle.Hotkey.ToString();
               HotkeysManager.GetToggleHotkey();
            }
            ReApplyHotkeys();
         }
         else if ( type == "stashtab" )
         {
            HotkeysManager.RemoveHotkey( HotkeysManager.stashTabModifier, HotkeysManager.stashTabKey );
            if ( CustomHotkeyToggle.Hotkey == null )
            {
               Properties.Settings.Default.HotkeyStashTab = "< not set >";
            }
            else
            {
               Properties.Settings.Default.HotkeyStashTab = CustomHotkeyToggle.Hotkey.ToString();
               HotkeysManager.GetStashTabHotkey();
            }
            ReApplyHotkeys();
         }
         Close();
      }

      private void ReApplyHotkeys()
      {
         mainWindow.RemoveAllHotkeys();
         mainWindow.AddAllHotkeys();
      }
   }
}
