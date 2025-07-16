using ExamProject.Constants;
using ExamProject.Domain;
using ExamProject.Extentions;

namespace ExamProject.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        public void Create(long chatId,string name)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);

            var convertedCategories = text.ToCategories();

            var existingCategory = convertedCategories.Find(x => x.Name == name);

            if (existingCategory != null)
            {
                throw new Exception($"Category with this name <{name}> already exists");
            }
            if (!string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            int newId = convertedCategories.Where(x => x.ChatId == chatId).Any()
                ? convertedCategories.Where(x => x.ChatId == chatId).Max(x => x.Id) + 1
                : 1;

            string content = $"{newId},{name},{chatId}";

            File.WriteAllText(PathHolder.CategoryPath, content);
        }

        public void Delete(long chatId,int id)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);
            List<Category> categories = text.ToCategories();

            var categoryToDelete = categories.Find(x => x.ChatId == chatId && x.Id == id);

            if (categoryToDelete == null)
            {
                throw new Exception($"Category with ID {id} was not found for this user.");
            }

            categories.Remove(categoryToDelete);

            FileHelper.WriteToFile(PathHolder.CategoryPath, categories.ConvertToString());
        }

        public Category Get(long chatId,int id)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);
            List<Category> categories = text.ToCategories();

            var existCategory = categories.Find(x => x.ChatId == chatId && x.Id == id)
                ?? throw new Exception($"Category with ID {id} was not found for this user.");

            return existCategory;
        }

        public List<Category> GetAll(long chatId)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);

            List<Category> allCategories = text.ToCategories();

            List<Category> userCategories = allCategories
                .Where(c => c.ChatId == chatId)
                .ToList();

            return userCategories;
        }

        public void Update(long chatId,int id, string name)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);
            List<Category> categories = text.ToCategories();

            var existCategory = categories.Find(x => x.ChatId == chatId && x.Id == id)
                ?? throw new Exception($"Category with ID {id} was not found for this user.");

            if (!string.IsNullOrEmpty(name))
            {
                throw new Exception();
            }

            var alreadyExistCategory = categories.Find(x => x.Name == name)
                ?? throw new Exception($"Category already exists with this name = {name}");

            existCategory.Name = name;
            FileHelper.WriteToFile(PathHolder.CategoryPath, categories.ConvertToString());
        }
    }
}


