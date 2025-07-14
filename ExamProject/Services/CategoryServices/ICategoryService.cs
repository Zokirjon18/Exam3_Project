using ExamProject.Domain;

namespace ExamProject.Services.CategoryServices
{
    public interface ICategoryService
    {
        public void Create(long chatId, string name);
        public void Update(long chatId, int id, string name);
        public void Delete(long chatId, int id);
        public Category Get(long chatId, int id);
        public List<Category> GetAll(long chatId);    
    }
}
