namespace ExamProject.Extentions;

public static class ListExtention
{
    public static List<string> ToStrringList(this List<dynamic>models)
    {
        var result = new List<string>();
        foreach (var model in models)
        {
            if (model == null)
            {
                result.Add("null");
            }
            var properties=model.GetType().GetProperties();
            var values=new List<string>();
            foreach (var property in properties)
            {
                var value = property.GetValue(model);
                values.Add(value.ToString()?? "null");
            }
            string line=string.Join(",",values);
            result.Add(line);
        }
        return result;
    }
}