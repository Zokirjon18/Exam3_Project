using System.Reflection;
using ExamProject.Constants;
using ExamProject.Domain;
using ExamProject.Extentions;
using ExamProject.Helpers;
using ExamProject.Models;

namespace ExamProject.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        void ICategoryService.Create(string name)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);

            var convertedCategories = text.ToCategories();

            var existingCategory = convertedCategories.Find(x => x.Name == CategoryModel.Name);

            if (existingCategory != null)
            {
                throw new Exception($"Category with this name <{CategoryModel.Name}> already exists");
            }
            if (!string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            string content = $"{GeneratorHelper.GenerateID(PathHolder.CategoryPath)},{name}";

            File.WriteAllText(PathHolder.CategoryPath, content);
        }

        void ICategoryService.Delete(int id)
        {
            var text = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            var categories = text.ToCategories();
            var existStadium = categories.Find(x => x.Id == id)
                ?? throw new Exception("Category is not found");

            categories.Remove(existStadium);

            FileHelper.WriteToFile(PathHolder.CategoryPath, categories.ConvertToString());

        }

        Category ICategoryService.Get(int id)
        {
            var text = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            var categories = text.ToCategories();
            var existCategory = categories.Find(x => x.Id == id)
                ?? throw new Exception("Category is not found");

            return existCategory;
        }

        List<Category> ICategoryService.GetAll(string search)
        {
            var text = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            var stadiums = text.ToCategories();

            if (!string.IsNullOrEmpty(search))
            {
                stadiums = Search(search);
            }
            return stadiums;
        }

        void ICategoryService.Update(int id, string name)
        {
            var text = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            var categories = text.ToCategories();
            var existCategory = categories.Find(x => x.Id == id)
                ?? throw new Exception("Category is not found");

            var alreadyExistStadium = categories.Find(x => x.Name == name);

            if (alreadyExistStadium != null)
            {
                throw new Exception($"Category already exists with this name = {name}");
            }

            if(!string.IsNullOrEmpty(name))
            {
                throw new Exception();
            }

            existCategory.Name = name;
            FileHelper.WriteToFile(PathHolder.CategoryPath, categories.ConvertToString());
        }

        private List<Category> Search(string search)
        {
            var text = FileHelper.ReadFromFile(PathHolder.CategoryPath);
            var categories = text.ToCategories();
            var result = new List<Category>();

            if (!string.IsNullOrEmpty(search))
            {
                string trimedString = search.TrimStart(' ').ToLower();

                foreach (var category in categories)
                {
                    if (category.Name.ToLower().Contains(trimedString))
                    {
                        result.Add(category);
                    }
                }
            }

            return result;
        }

    }
}


