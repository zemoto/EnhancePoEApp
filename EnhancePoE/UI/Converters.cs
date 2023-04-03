﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EnhancePoE.UI;

[ValueConversion( typeof( string ), typeof( Color ) )]
internal sealed class StringColorConverter : IValueConverter
{
   public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
   {
      if ( value is string stringValue && !string.IsNullOrEmpty( stringValue ) )
      {

         return (Color)ColorConverter.ConvertFromString( stringValue );
      }

      return null;
   }

   public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
   {
      if ( value is Color colorValue )
      {
         return colorValue.ToString();
      }

      return string.Empty;
   }
}
