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
                ?? throw new Exception($"Dish could not be found with ID: {dishUpdateModel.Id}");

            var existDish = filteredDishes.Find(x => x.Name == dishUpdateModel.Name && x.Id != dishUpdateModel.Id);

            if (existDish != null)
            {
                throw new Exception("Sorry a dish on this name already exists." +
                    "\nPlease change it OR add some adjustments like this DISH_NAME2");
            }

            var categories = categoryService.GetAll(dishUpdateModel.ChatId);

            var existCategory = categories.Find(x => x.Id == dishUpdateModel.CategoryId)
                ?? throw new Exception("Category was not found.\nPlease select a valid category.");

            if (string.IsNullOrWhiteSpace(dishUpdateModel.Name))
                dishForUpdation.Name = dishUpdateModel.Name;
            if (dishUpdateModel.Ingredients.Count > 0)
                dishForUpdation.Ingredients = dishUpdateModel.Ingredients;
            if (dishUpdateModel.ReadyIn.TotalSeconds > 0)
                dishForUpdation.ReadyIn = dishUpdateModel.ReadyIn;
            if (dishUpdateModel.CategoryId > 0)
                dishForUpdation.CategoryId = dishUpdateModel.CategoryId;

            FileHelper.WriteToFile(PathHolder.DishPath, convertedDishes);
        }

        public void Delete(long chatId, int id)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            var dishForDeletion = convertedDishes.Find(x => x.Id == id && x.ChatId == chatId);
            if (dishForDeletion == null) 
            { 
                throw new Exception($"Dish could not be found with ID: {id}"); 
            }

            convertedDishes.Remove(dishForDeletion);

            FileHelper.WriteToFile(PathHolder.DishPath, convertedDishes);
        }

        public void DeleteAllByCategoryId(long chatId, int categoryId)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            convertedDishes.RemoveAll(x => x.ChatId == chatId && x.CategoryId == categoryId);

            FileHelper.WriteToFile(PathHolder.DishPath, convertedDishes);
        }

        public DishViewModel Get(long chatId, int id)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            var dish = convertedDishes.Find(x => x.Id == id && x.ChatId == chatId || x.ChatId == 0)
                ?? throw new Exception($"Dish not found.");

            string Categorytext = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            List<Category> categories = Categorytext.ToCategories();

            return dish.ToDishViewModel(categories, chatId);
        }

        public Dish GetForUpdation(long chatId, int id)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            var dish = convertedDishes.Find(x => x.Id == id && x.ChatId == chatId)
                ?? throw new Exception($"Dish not found.");

            string Categorytext = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            List<Category> categories = Categorytext.ToCategories();

            return dish;
        }

        public List<DishViewModel> GetAllByDishName(long chatId, string dishName)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            List<Dish> userDishes = convertedDishes.FindAll(x => x.ChatId == chatId || x.ChatId == 0);

            if (userDishes.Count == 0)
                throw new Exception("You haven't added any dishes yet!");


            // for ToDishViewModel to avoid overreading from file
            string Categorytext = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            List<Category> categories = Categorytext.ToCategories();

            // for mapped models
            List<DishViewModel> matchedDishes = new List<DishViewModel>();

            foreach (var dish in userDishes)
            {
                if (dish.Name.Contains(dishName, StringComparison.OrdinalIgnoreCase))
                {
                    matchedDishes.Add(dish.ToDishViewModel(categories, chatId));
                }
            }

            if (matchedDishes.Count == 0)
                throw new Exception("No dishes found with the given name.");


            return matchedDishes;
        }

        public List<DishViewModel> GetAllByCategoryId(long chatId, int categoryId)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            List<Dish> userDishes = convertedDishes.FindAll(x => x.ChatId == chatId || x.ChatId == 0);

            if (userDishes.Count == 0)
                throw new Exception("There is no dishes yet!");


            // for ToDishViewModel to avoid overreading from file
            string categoryText = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            List<Category> categories = categoryText.ToCategories();

            // for mapped models
            List<DishViewModel> matchedDishes = new List<DishViewModel>();

            foreach (var dish in userDishes)
            {
                if (dish.CategoryId == categoryId)
                {
                    matchedDishes.Add(dish.ToDishViewModel(categories, chatId));
                }
            }

            if (matchedDishes.Count == 0)
                throw new Exception("No dishes found in this category");

            return matchedDishes;
        }

        public List<DishViewModel> GetAllByIngredients(long chatId, List<string> ingredientNames)
        {
            string text = FileHelper.ReadFromFile(PathHolder.DishPath);
            List<Dish> convertedDishes = text.ToDish();

            List<Dish> userDishes = convertedDishes.FindAll(x => x.ChatId == chatId || x.ChatId == 0);

            if (userDishes.Count == 0)
                throw new Exception("You haven't added any dishes yet!");


            // for ToDishViewModel to avoid overreading from file
            string Categorytext = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            List<Category> categories = Categorytext.ToCategories();


            // for mapped models
            List<DishViewModel> formatedDishes = new List<DishViewModel>();

            foreach (var dish in userDishes)
            {
                int matchCount = 0;

                foreach (var ingredientName in ingredientNames)
                {
                    foreach (var dishIngredient in dish.Ingredients)
                    {
                        if (dishIngredient.Name.Trim().ToLower() == ingredientName.Trim().ToLower())
                        {
                            matchCount++;
                            break;
                        }
                    }
                }

                if (matchCount == ingredientNames.Count)
                {
                    formatedDishes.Add(dish.ToDishViewModel(categories, chatId));
                }
            }

            if (formatedDishes.Count == 0)
                throw new Exception("No dishes found with these ingredients.");

            return formatedDishes;
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

