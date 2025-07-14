using ExamProject.Constants;
using ExamProject.Domain;
using ExamProject.FileHelper;
using ExamProject.Models;
using ExamProject.Extentions;

namespace ExamProject.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        void ICategoryService.Create(string name)
        {
            string text = File.ReadAllText(PathHolder.CategoryPath);

            var convertedCategories = text.ToStringList();

            var existingCategory = convertedCategories.Find(x => x.Name == CategoryModel.Name);

            if (existingCategory != null)
            {
                throw new Exception($"Stadium with this name <{CategoryModel.Name}> already exists");
            }
            if (!string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            string content = $"{GeneratorHelper.GenerateID(PathHolder.CategoryPath)}";

            File.WriteAllText(PathHolder.CategoryPath, content);
        }

        void ICategoryService.Delete(int id)
        {
            throw new NotImplementedException();
        }

        Category ICategoryService.Get(int id)
        {
            throw new NotImplementedException();
        }

        List<Category> ICategoryService.GetAll()
        {
            throw new NotImplementedException();
        }

        void ICategoryService.Update(int id, string name)
        {
            throw new NotImplementedException();
        }

        List<Category> ToCategory(this string text)
        {
            List<Category> categories = new List<Category>();

            string[] lines = text.Split('\n');

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(',');

                categories.Add(new Category
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1],
                });
            }

            return categories;
        }
    }
}


