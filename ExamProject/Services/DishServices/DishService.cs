using ExamProject.Models;

namespace ExamProject.Services.DishServices
{
    public class DishService : IDishService
    {
        public void Create(DishCreateModel dishCreateModel)
        {
            throw new NotImplementedException();
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


