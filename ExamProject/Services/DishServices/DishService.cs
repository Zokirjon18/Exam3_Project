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

            var existDish = convertedDishes.Find(x => x.Name == dishCreateModel.Name);

            if (existDish != null)
            {
                throw new Exception("Sorry a dish on this name already exists." +
                    "\nPlease change it OR add some adjustments like this DISH_NAME2");
            }

            var categorys = categoryService.GetAll();

            var existCategory = categorys.Find(x => x.Id == dishCreateModel.categoryId);

            if (existCategory == null)
            {
                throw new Exception("Could not find category");
            }






        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public DishViewModel Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<DishViewModel> GetAllByCategoryId(int categoryId)
        {
            throw new NotImplementedException();
        }

        public List<DishViewModel> GetAllByDishName(string dishName)
        {
            throw new NotImplementedException();
        }

        public List<DishViewModel> GetAllByIngredients(List<Ingredient> ingredients)
        {
            throw new NotImplementedException();
        }

        public void Update(DishUpdateModel dishUpdateModel)
        {
            throw new NotImplementedException();
        }
    }
}


