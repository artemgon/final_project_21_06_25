// BookLibrary.ViewModels/Converters/AuthorsToStringConverter.cs
using Domain.Entities; // Required for Author
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data; // Required for IValueConverter

namespace BookLibrary.ViewModels.Converters
{
    public class AuthorsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<Author> authors && authors.Any())
            {
                return string.Join(", ", authors.Select(a => $"{a.FirstName} {a.LastName}"));
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Not needed for this scenario
        }
    }
}