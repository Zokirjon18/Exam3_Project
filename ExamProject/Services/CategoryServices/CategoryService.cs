using ExamProject.Constants;
using ExamProject.Domain;
using ExamProject.Extentions;
using ExamProject.Services.DishServices;

namespace ExamProject.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {

        public void Create(long chatId,string name)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);

            var convertedCategories = text.ToCategories();

            var filteredCatagories = FilterByChatId(convertedCategories,chatId);

            
            var existingCategory = filteredCatagories.Find(x => x.Name == name);

            if (existingCategory != null)
            {
                throw new Exception($"Category with this name <{name}> already exists");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name must be entered!");
            }

            int newId = convertedCategories.Where(x => x.ChatId == chatId).Any()
                ? convertedCategories.Where(x => x.ChatId == chatId).Max(x => x.Id) + 1
                : 1;

            convertedCategories.Add(new Category
            {
                Id = newId,
                Name = name,
                ChatId = chatId
            });

            FileHelper.WriteToFile(PathHolder.CategoryPath, convertedCategories);
        }

        public void Update(long chatId, int id, string name)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);
            List<Category> categories = text.ToCategories();

            var existCategory = categories.Find(x => x.ChatId == chatId && x.Id == id)
                ?? throw new Exception($"Category with ID {id} was not found for this user.");


            var alreadyExistCategory = categories.Find(x => x.Name == name);
               if(alreadyExistCategory != null)
                throw new Exception($"Category already exists with this name = {name}");

            existCategory.Name = name;
            FileHelper.WriteToFile(PathHolder.CategoryPath, categories);
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

            FileHelper.WriteToFile(PathHolder.CategoryPath, categories);
        }

        public Category Get(long chatId,int id)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);
            List<Category> categories = text.ToCategories();

            var existCategory = categories.Find(x => (x.ChatId == chatId || x.ChatId == 0) && x.Id == id)
                ?? throw new Exception($"Category with ID {id} was not found for this user.");

            return existCategory;
        }

        public List<Category> GetAll(long chatId)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);

            List<Category> allCategories = text.ToCategories();

            List<Category> userCategories = FilterByChatId(allCategories, chatId);

            return userCategories;
        }


        private List<Category> FilterByChatId(List<Category> allCategories, long chatId)
        {
            List<Category> filteredCategories = new();

            foreach (var category in allCategories)
            {
                if (category.ChatId == chatId || category.ChatId == 0)
                {
                    filteredCategories.Add(category);
                }
            }

            return filteredCategories;
        }
    }
}


