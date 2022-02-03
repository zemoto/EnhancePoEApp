using System.Windows;
using System.Windows.Media;

namespace EnhancePoE.Model
{
   internal static class Utility
   {
      public static T GetChild<T>( DependencyObject obj ) where T : DependencyObject
      {
         if ( obj != null )
         {
            DependencyObject child = null;
            for ( int i = 0; i < VisualTreeHelper.GetChildrenCount( obj ); i++ )
            {
               child = VisualTreeHelper.GetChild( obj, i );
               if ( child != null && child.GetType() == typeof( T ) )
               {
                  break;
               }
               else if ( child != null )
               {
                  child = GetChild<T>( child );
                  if ( child != null && child.GetType() == typeof( T ) )
                  {
                     break;
                  }
               }
            }
            return child as T;
         }
         return null;
      }

      public static object GetPropertyValue( object src, string propertyName )
      {
         return src.GetType().GetProperty( propertyName ).GetValue( src );
      }
   }
}
