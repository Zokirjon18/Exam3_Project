using System.Reflection;

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
                string stringValue = "";

                if (value is List<Ingredient> ingredients)
                {
                    var ingredientStrings = new List<string>();

                    foreach (var ingredient in ingredients)
                    {
                        string perIngredient = $"{ingredient.Name},{ingredient.Amount},{ingredient.Unit}";
                        ingredientStrings.Add(perIngredient);
                    }

                    stringValue = string.Join("|", ingredientStrings);
                }
                else
                {
                    stringValue = value != null ? value.ToString() : "null";
                }

                if (stringValue.Contains(",") || stringValue.Contains("\""))
                    stringValue = $"\"{stringValue.Replace("\"", "\"\"")}\"";

                values.Add(stringValue);
            }

            string line = string.Join(",", values);
            result.Add(line);
        }

        return result;
    }

}