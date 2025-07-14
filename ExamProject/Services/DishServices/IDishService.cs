using ExamProject.Models;

namespace ExamProject.Services.DishServices
{
    public interface IDishService
    {
        void Create(DishCreateModel dishCreateModel);
        void Update(DishUpdateModel dishUpdateModel);
        void Delete(int id);
        DishViewModel Get(int id);
        List<DishViewModel> GetAllByDishName(string dishName);
        List<DishViewModel> GetAllByCategoryId(int categoryId);
        List<DishViewModel> GetAllByIngredients(List<Ingredient> ingredients);
    }
}
