using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// This converter is used to show DateTime in short date format
    /// </summary>
    public class DateFormattingConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value != null)
                return ((DateTime)value).ToShortDateString();

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (string.IsNullOrEmpty((string)value))
                return null;

            string dateString = (string)value;

            // Append first month and day if just the year was entered
            if (dateString.Length == 4)
                dateString = "1/1/" + dateString;


            DateTime date;
            DateTime.TryParse(dateString, out date);
            return date;
        }

        #endregion
    }

    /// <summary>
    /// This converter is used to show possessive first name.
    /// </summary>
    public class FirstNamePossessiveFormConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (!string.IsNullOrEmpty(value.ToString()))
                {
                    // Simply add "'s".  This is the correct use of the posessive.
                    return value.ToString() + "'s ";
                }
                else
                    return string.Empty;


            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }

    /// <summary>
    /// Returns visible when a bool is true.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }

    /// <summary>
    /// Returns hidden when a bool is true.
    /// </summary>
    public class NotBoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }

    /// <summary>
    /// Returns false when a bool is true.
    /// </summary>
    public class NotConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }

    /// <summary>
    /// Returns true when a bool is true.
    /// </summary>
    public class BoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }

    public class ComposingConverter : IValueConverter
    {
        #region IValueCOnverter Members

        private List<IValueConverter> converters = new List<IValueConverter>();

        public Collection<IValueConverter> Converters
        {
            get { return new Collection<IValueConverter>(this.converters); }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            for (int i = 0; i < this.converters.Count; i++)
            {
                value = converters[i].Convert(value, targetType, parameter, culture);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            for (int i = this.converters.Count - 1; i >= 0; i--)
            {
                value = converters[i].ConvertBack(value, targetType, parameter, culture);
            }
            return value;
        }

        #endregion
    }

    /// <summary>
    /// Converts an filepath to a bitmap image.
    /// </summary>
    public class ImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var filePath = value as string;
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    // To save significant application memory, set the DecodePixelWidth or  
                    // DecodePixelHeight of the BitmapImage value of the image source to the desired 
                    // height or width of the rendered image. If you don't do this, the application will 
                    // cache the image as though it were rendered as its normal size rather then just 
                    // the size that is displayed.
                    // Note: In order to preserve aspect ratio, set DecodePixelWidth
                    // or DecodePixelHeight but not both.
                    // See http://msdn.microsoft.com/en-us/library/ms748873.aspx
                    bitmap.DecodePixelWidth = 200;
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.EndInit();

                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }

    /// <summary>
    /// Converts a radio button selection bound to an enum value to a bool.
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter.ToString() == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            if (Enum.Parse(value.GetType(), parameter.ToString()).Equals(value))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (parameter.ToString() == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameter.ToString());

        }

        #endregion
    }

    /// <summary>
    /// Converts an enum value to a localized string.
    /// </summary>  
    [ValueConversion(typeof(Enum), typeof(String))]
    public class EnumValueDescriptionConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {

                if (value is Enum == false)
                    return string.Empty;

                if (value.ToString() == "Foster")
                    return Properties.Resources.Fostered;

                else if (value.ToString() == "Adopted")
                    return Properties.Resources.Adopted;

                else if (value.ToString() == "Natural")
                    return Properties.Resources.Natural;

                else if (value.ToString() == "Current")
                    return Properties.Resources.Current;

                else if (value.ToString() == "Former")
                    return Properties.Resources.Former;

                else
                    return string.Empty;

            }
            catch { return string.Empty; }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }

}
