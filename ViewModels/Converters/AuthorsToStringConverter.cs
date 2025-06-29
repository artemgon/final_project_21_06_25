// BookLibrary.ViewModels/Converters/AuthorsToStringConverter.cs
using Domain.Entities; // Required for Author
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BookLibrary.Domain.Entities; // Required for IValueConverter

namespace ViewModels.Converters
{
    public class AuthorsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<Author> authors && authors.Any())
            {
                return string.Join(", ", authors.Select(a => 
                {
                    var firstName = string.IsNullOrWhiteSpace(a.FirstName) ? "Unknown" : a.FirstName.Trim();
                    var lastName = string.IsNullOrWhiteSpace(a.LastName) ? "Unknown" : a.LastName.Trim();
                    return $"{firstName} {lastName}";
                }));
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Not needed for this scenario
        }
    }
}