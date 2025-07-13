namespace ExamProject.Constants
{
    internal class PathHolder
    {
        private static readonly string parentRoot = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public static readonly string CategoryPath = Path.Combine(parentRoot, "Data", "categorys.txt");
        public static readonly string DishPath = Path.Combine(parentRoot, "Data", "dishes.txt");
    }
}
