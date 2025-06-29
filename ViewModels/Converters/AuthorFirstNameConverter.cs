using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BookLibrary.Domain.Entities;

namespace ViewModels.Converters
{
    public class AuthorFirstNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<Author> authors && authors.Any())
            {
                // Get the first author's first name, or show multiple if there are more
                var firstAuthor = authors.First();
                if (authors.Count == 1)
                {
                    return string.IsNullOrWhiteSpace(firstAuthor.FirstName) ? "Unknown" : firstAuthor.FirstName.Trim();
                }
                else
                {
                    var firstName = string.IsNullOrWhiteSpace(firstAuthor.FirstName) ? "Unknown" : firstAuthor.FirstName.Trim();
                    return $"{firstName} (+{authors.Count - 1} more)";
                }
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
