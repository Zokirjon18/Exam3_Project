namespace ExamProject.Constants
{
    internal class PathHolder
    {
       // private static readonly string parentRoot = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        //public static readonly string CategoryPath = Path.Combine(parentRoot, "Datas", "categories.txt");
       // public static readonly string DishPath = Path.Combine(parentRoot, "Datas", "dishes.txt");

        public static string GetProjectRoot()
        {
            var current = Directory.GetCurrentDirectory();
            while (!Directory.GetFiles(current, "*.csproj").Any())
            {
                current = Directory.GetParent(current).FullName;
            }
            return current;
        }

        public static readonly string CategoryPath = Path.Combine(GetProjectRoot(), "Datas", "categories.txt");
        public static readonly string DishPath = Path.Combine(GetProjectRoot(), "Datas", "dishes.txt");
    }
}
