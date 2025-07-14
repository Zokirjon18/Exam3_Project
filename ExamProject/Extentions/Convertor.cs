using ExamProject.Enums;

namespace ExamProject.Extentions;

public static class Convertor
{
    public static List<Dish> ToDishe(this string text)
    {
        List<Dish> dishes = new List<Dish>();
        string[] lines = text.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))  continue;
                string[] parts = line.Split(',');
                var id = int.Parse(parts[0]);
                var name = parts[1];
                var ingredientParts = parts[2].Split(',');
                var ingredients = ingredientParts.Select<string, object>(p =>
                {
                    var pair=p.Split(':');
                    return new Ingredient
                    {
                        Name = pair[0],
                        Amount = double.Parse(pair[1]),
                        Unit = Enum.Parse<Unit>(pair[2]),
                    };
                }).ToList();
                TimeSpan ready=TimeSpan.Parse(parts[3]);
                int categoryId=int.Parse(parts[4]);
                
                dishes.Add(new Dish()
                {
                    Id = id,
                    Name = name,
                    Ingredients = ingredients,
                    ReadyIn = ready,
                    CategoryId = categoryId
                    
                });
        }
        return dishes;
    }
}