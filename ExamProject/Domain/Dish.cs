public class Dish
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Ingredient> Ingredients { get; set; }
    public TimeSpan ReadyIn { get; set; }
    public int CategoryId { get; set; } 
}
