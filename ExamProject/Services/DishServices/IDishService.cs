using ExamProject.Models;

namespace ExamProject.Services.DishServices
{
    public interface IDishService
    {
        void Create(DishCreateModel dishCreateModel);
        void Update(DishUpdateModel dishUpdateModel);
        void Delete(long chatId,int id);
        DishViewModel Get(long chatId,int id);
        List<DishViewModel> GetAllByDishName(long chatId,string dishName);
        List<DishViewModel> GetAllByCategoryId(long chatId,int categoryId);
        List<DishViewModel> GetAllByIngredients(long chatId,List<Ingredient> ingredients);
    }
}
