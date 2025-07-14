using ExamProject.Domain;

namespace ExamProject.Services.CategoryServices
{
    public interface ICategoryService
    {
        void Create(string name);
        void Update(int id, string name);
        void Delete(int id);
        Category Get(int id);
        List<Category> GetAll();    
    }
}
