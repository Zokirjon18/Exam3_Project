namespace ExamProject.Extentions;

public class FileHelper
{
    public static string ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }
        return File.ReadAllText(filePath);
    }

    public static void WriteToFile(string filePath, string content)
    {
        File.WriteAllText(filePath, content);
    }
}