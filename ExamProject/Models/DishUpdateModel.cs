namespace ExamProject.Models
{
    public class DishUpdateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int categoryId { get; set; }
        public List<Ingredient> ingredients { get; set; }
        public TimeSpan ReadyIn { get; set; }
        public long ChatId { get; set; }
    }
}
