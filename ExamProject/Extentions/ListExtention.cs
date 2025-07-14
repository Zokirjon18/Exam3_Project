using System.Reflection;
using ExamProject.Domain;

namespace ExamProject.Extentions;

public static class ListExtention
{
    public static List<string> ToStringList<T>(this List<T> models)
    {
        var result = new List<string>();
        if (models == null) return result;

        var properties = typeof(T).GetProperties();

        foreach (var model in models)
        {
            if (model == null) continue;

            var values = new List<string>();
            foreach (var property in properties)
            {
                var value = property.GetValue(model);
                string stringValue = value != null ? value.ToString() : "null";

                if (stringValue.Contains(",") || stringValue.Contains("\""))
                    stringValue = $"\"{stringValue.Replace("\"", "\"\"")}\"";
              
                values.Add(stringValue);
            }

            string line = string.Join(",", values);
            result.Add(line);
        }
        return result;
    }

    public static List<Category> ToCategories(this string text)
    {
        List<Category> categories = new List<Category>();

        string[] lines = text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');

            categories.Add(new Category
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
            });
        }

        return categories;
    }

    public static List<string> ConvertToString(this List<Category> stadiums)
    {
        var convertedCategories = new List<string>();

        foreach (var stadium in stadiums)
        {
            convertedCategories.Add(stadium.ToString());
        }

        return convertedCategories;
    }
}