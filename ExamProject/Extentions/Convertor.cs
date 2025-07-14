using ExamProject.Constants;
using ExamProject.Domain;
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

            var parts = line.Split(',');

            var ingredients = parts[2]
           .Split('|', StringSplitOptions.RemoveEmptyEntries)
           .Select(name => new Ingredient { Name = name.Trim() })
           .ToList();

            dishes.Add(new Dish
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
                Ingredients = ingredients,
                ReadyIn = TimeSpan.Parse(parts[3]),
                CategoryId = int.Parse(parts[4])
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
                Name = parts[1]
            });
        }

        return categories;
    }
    public static DishViewModel ToDishViewModel(this Dish dish)
    {
        string categoryText = FileHelper.ReadFromFile(PathHolder.CategoryPath);
        List<Category> categories = categoryText.ToCategories();

        var category = categories.FirstOrDefault(c => c.Id == dish.CategoryId);

        if (category == null)
            throw new ArgumentException("Category not found for Dish ID: " + dish.Id);

        return new DishViewModel
        {
            DishName = dish.Name,
            CategoryName = category.Name,
            PriceFormatted = string.Join(", ", dish.Ingredients.Select(i => i.Name)) + $" | Ready in: {dish.ReadyIn.TotalMinutes} min"
        };
    }
}
