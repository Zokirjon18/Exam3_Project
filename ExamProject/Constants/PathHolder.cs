namespace ExamProject.Constants
{
    internal class PathHolder
    {
        private static readonly string parentRoot = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public static readonly string CategoryPath = Path.Combine(parentRoot, "Datas", "categorys.txt");
        public static readonly string DishPath = Path.Combine(parentRoot, "Datas", "dishes.txt");
    }
}
