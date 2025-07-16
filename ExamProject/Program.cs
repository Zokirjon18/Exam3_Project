using ExamProject.BotHandlers;
using Telegram.Bot;

namespace ExamProject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            BotHandler botHandler = new BotHandler();
            await botHandler.Run();
        }
    }
}
