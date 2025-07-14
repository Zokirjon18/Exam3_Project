namespace ExamProject.Models
{
    public class DishViewModel
    {
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public List<Ingredient> ingredients { get; set; }
        public TimeSpan ReadyIn {  get; set; }
    }
}
