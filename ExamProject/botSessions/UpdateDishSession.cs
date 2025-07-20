public static class UpdateDishSession
{
    public static Dictionary<long, string> dishUpdateStep = new();
    public static Dictionary<long, DishUpdateModel> Session = new();
    public static Dictionary<long, List<Ingredient>> tempIngredients = new();
}

public class DishUpdateModel
{
    public long ChatId { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Ingredient> Ingredients { get; set; }
    public TimeSpan ReadyIn { get; set; }
    public int CategoryId { get; set; }
}