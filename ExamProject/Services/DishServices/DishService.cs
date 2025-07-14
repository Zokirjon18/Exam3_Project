using ExamProject.Constants;
using ExamProject.Domain;
using ExamProject.Extentions;
using ExamProject.Models;
using ExamProject.Services.CategoryServices;

namespace ExamProject.Services.DishServices
{
    public class DishService : IDishService
    {
        public CategoryService categoryService;

        public DishService(CategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        public void Create(DishCreateModel dishCreateModel)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            List<Dish> filteredDishes = FilterByChatId(convertedDishes, dishCreateModel.ChatId);

            var existDish = filteredDishes.Find(x => x.Name == dishCreateModel.Name);

            if (existDish != null)
            {
                throw new Exception("Sorry a dish on this name already exists." +
                    "\nPlease change it OR add some adjustments like this DISH_NAME2");
            }

            var categories = categoryService.GetAll(dishCreateModel.ChatId);

            var existCategory = categories.Find(x => x.Id == dishCreateModel.categoryId)
                ?? throw new Exception("Category was not found.\nPlease select a valid category.");

            int newId = filteredDishes.Count > 0 ? filteredDishes.Max(d => d.Id) + 1 : 1;

            convertedDishes.Add(new Dish
            {
                Id = newId,
                Name = dishCreateModel.Name,
                Ingredients = dishCreateModel.ingredients,
                ReadyIn = dishCreateModel.ReadyIn,
                CategoryId = dishCreateModel.categoryId,
                ChatId = dishCreateModel.ChatId
            });

            FileHelper.WriteToFile(PathHolder.DishPath, convertedDishes);
        }

        public void Update(DishUpdateModel dishUpdateModel)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            List<Dish> filteredDishes = FilterByChatId(convertedDishes, dishUpdateModel.ChatId);

            var dishForUpdation = filteredDishes.Find(x => x.Id == dishUpdateModel.Id)
                ?? throw new Exception($"Dish could not be found with ID: {dishUpdateModel.Id}"); ;
            
            var existDish = filteredDishes.Find(x => x.Name == dishUpdateModel.Name && x.Id != dishUpdateModel.Id);

            if (existDish != null)
            {
                throw new Exception("Sorry a dish on this name already exists." +
                    "\nPlease change it OR add some adjustments like this DISH_NAME2");
            }

            var categories = categoryService.GetAll(dishUpdateModel.ChatId);

            var existCategory = categories.Find(x => x.Id == dishUpdateModel.categoryId)
                ?? throw new Exception("Category was not found.\nPlease select a valid category.");

            dishForUpdation.Name = dishUpdateModel.Name;
            dishForUpdation.Ingredients = dishUpdateModel.ingredients;
            dishForUpdation.ReadyIn = dishUpdateModel.ReadyIn;
            dishForUpdation.CategoryId = dishUpdateModel.categoryId;

            FileHelper.WriteToFile(PathHolder.DishPath, convertedDishes);
        }

        public void Delete(long chatId, int id)
        {
            throw new NotImplementedException();
        }

        public DishViewModel Get(long chatId, int id)
        {
            string Categorytext = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            List<Category> categories = Categorytext.ToCategories();

            throw new NotImplementedException();
        }

        public List<DishViewModel> GetAllByDishName(long chatId, string dishName)
        {
            throw new NotImplementedException();
        }

        public List<DishViewModel> GetAllByCategoryId(long chatId, int categoryId)
        {
            throw new NotImplementedException();
        }

        public List<DishViewModel> GetAllByIngredients(long chatId, List<Ingredient> ingredients)
        {
            throw new NotImplementedException();
        }

        private List<Dish> FilterByChatId(List<Dish> allDishes, long chatId)
        {
            List<Dish> filteredDishes = new List<Dish>();

            foreach (var dish in allDishes)
            {
                if (dish.ChatId == chatId)
                {
                    filteredDishes.Add(dish);
                }
            }

            return filteredDishes;
        }
    }
}


