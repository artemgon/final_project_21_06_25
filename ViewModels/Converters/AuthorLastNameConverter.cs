using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BookLibrary.Domain.Entities;

namespace ViewModels.Converters
{
    public class AuthorLastNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<Author> authors && authors.Any())
            {
                // Get the first author's last name, or show multiple if there are more
                var firstAuthor = authors.First();
                if (authors.Count == 1)
                {
                    return string.IsNullOrWhiteSpace(firstAuthor.LastName) ? "Unknown" : firstAuthor.LastName.Trim();
                }
                else
                {
                    var lastName = string.IsNullOrWhiteSpace(firstAuthor.LastName) ? "Unknown" : firstAuthor.LastName.Trim();
                    return $"{lastName} (+{authors.Count - 1} more)";
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
