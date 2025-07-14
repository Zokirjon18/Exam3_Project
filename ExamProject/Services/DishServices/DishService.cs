using ExamProject.Constants;
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

            List<Dish> filteredDishes = FilterByChatId(dishCreateModel.ChatId);

            var existDish = filteredDishes.Find(x => x.Name == dishCreateModel.Name);

            if (existDish != null)
            {
                throw new Exception("Sorry a dish on this name already exists." +
                    "\nPlease change it OR add some adjustments like this DISH_NAME2");
            }

            var categories = categoryService.GetAll(dishCreateModel.ChatId);

            var existCategory = categories.Find(x => x.Id == dishCreateModel.categoryId)
                ?? throw new Exception("Category was not found.\nPlease select a valid category.");

            convertedDishes.Add(new Dish
            {
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
            
        }

        public void Delete(long chatId, int id)
        {
            throw new NotImplementedException();
        }

        public DishViewModel Get(long chatId, int id)
        {
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

        private List<Dish> FilterByChatId(long chatId)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            List<Dish> filteredDishes = new List<Dish>();

            foreach (var dish in convertedDishes)
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


