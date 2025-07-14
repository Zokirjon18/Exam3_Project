using ExamProject.Constants;
using ExamProject.Domain;
using ExamProject.Extentions;

namespace ExamProject.Services.CategoryServices
{
    
    public class CategoryService : ICategoryService
    {
        private readonly string path = PathHolder.CategoryPath;
        public void Create(string name)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Category Get(int id)
        {
            var text = FileHelper.ReadFromFile(path);
            var categories = text.ToCategory();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public List<Category> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Update(int id, string name)
        {
            throw new NotImplementedException();
        }
    }
}


