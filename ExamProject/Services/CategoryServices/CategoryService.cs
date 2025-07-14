using ExamProject.Constants;
using ExamProject.Domain;
using ExamProject.Extentions;

namespace ExamProject.Services.CategoryServices
{
    
    public class CategoryService : ICategoryService
    {

        private readonly string path = PathHolder.CategoryPath;
        
        public void Create(long chatId,string name)
        {
            throw new NotImplementedException();
        }

        public void Delete(long chatId,int id)
        {
            throw new NotImplementedException();
        }

        public Category Get(long chatId,int id)
        {
            var text = FileHelper.ReadFromFile(path);
            var categories = text.ToCategories();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public List<Category> GetAll(long chatId)
        {
            throw new NotImplementedException();
        }

        public void Update(long chatId,int id, string name)
        {
            throw new NotImplementedException();
        }
    }
}


