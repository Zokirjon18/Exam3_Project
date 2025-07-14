namespace ExamProject.Models
{
    public class DishCreateModel
    {
        public string Name { get; set; }
        public int categoryId {  get; set; }
        public List<object> ingredients { get; set; }
        public TimeSpan ReadyIn {  get; set; }
        public long ChatId { get; set; }
    }
}
