public static class UpdateDishSession
{
    public static Dictionary<long, string> dishUpdateStep = new();
    public static Dictionary<long, DishUpdateModel> modelForUpdation = new();
    public static Dictionary<long, List<Ingredient>> tempIngredients = new();
}

