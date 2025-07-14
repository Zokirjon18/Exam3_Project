using ExamProject.Constants;
using ExamProject.Extentions;

namespace ExamProject.Domain
{
    public class Category
    {
        public Category()
        {
            Id = GeneratorHelper.GenerateId(PathHolder.CategoryPath);
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public long ChatId { get; set; }
    }
}
