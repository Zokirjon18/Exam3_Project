namespace ExamProject.Helpers;

public static class GeneratorHelper
{
    public static int GenerateID(string path)
    {
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);

            int maxId = 0;
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line)) continue;
                string[] parts = line.Split(',');
                string id = parts[0];

                if (maxId < Convert.ToInt32(id))
                {
                    maxId = Convert.ToInt32(id);
                }
            }

            return ++maxId;
        }
        else
        {
            return 1;
        }
    }
}
