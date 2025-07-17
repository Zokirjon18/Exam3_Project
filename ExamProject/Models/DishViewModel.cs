namespace ExamProject.Models
{
    public class DishViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public TimeSpan ReadyIn {  get; set; }
        public long ChatId { get; set; }
    }
}
