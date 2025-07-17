using ExamProject.Domain;
using ExamProject.Enums;
using ExamProject.Models;

namespace ExamProject.Extentions;

public static class Convertor
{
    public static List<Dish> ToDish(this string text)
    {
        List<Dish> dishes = new List<Dish>();

        string[] lines = text.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',' ,StringSplitOptions.TrimEntries);

            //skips invalid lines because in our case is gonna cause unpleasing issues
            if (parts.Length != 6)
                    continue;

            var ingredients = ParseIngredientsPart(parts[2]);

            dishes.Add(new Dish
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
                Ingredients = ingredients,
                ReadyIn = TimeSpan.Parse(parts[3]),
                CategoryId = int.Parse(parts[4]),
                ChatId = Convert.ToInt64(parts[5])
            });
        }

        return dishes;
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
                ChatId = Convert.ToInt64(parts[2])
            });
        }

        return categories;
    }
    public static DishViewModel ToDishViewModel(this Dish dish,List<Category> categories, long chatId)
    {
        var category = categories.FirstOrDefault(c => c.Id == dish.CategoryId && chatId == c.ChatId || c.ChatId == 0);

        if (category == null)
            throw new ArgumentException($"Category was not found with ID: {dish.CategoryId}");

        return new DishViewModel
        {
            Id = dish.Id,
            Name = dish.Name,
            CategoryName = category.Name,
            Ingredients = dish.Ingredients,
            ReadyIn = dish.ReadyIn,
        };
    }

    private static List<Ingredient> ParseIngredientsPart(string ingredientText)
    {
        var ingredientStrings = ingredientText.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var ingredients = new List<Ingredient>();

        foreach (var ing in ingredientStrings)
        {
            var ingParts = ing.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ingParts.Length != 3)
                continue;

            ingredients.Add(new Ingredient
            {
                Name = ingParts[0].Trim(),
                Amount = double.Parse(ingParts[1].Trim()),
                Unit = Enum.Parse<Unit>(ingParts[2].Trim(), ignoreCase: true)
            });
        }

        return ingredients;
    }
}
