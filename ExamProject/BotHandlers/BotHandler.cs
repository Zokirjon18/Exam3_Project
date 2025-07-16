using ExamProject.Enums;
using ExamProject.Models;
using ExamProject.Services.CategoryServices;
using ExamProject.Services.DishServices;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamProject.BotHandlers
{
    internal class BotHandler
    {
        TelegramBotClient botClient;
        DishService dishService;
        CategoryService categoryService;
        private Dictionary<long, DishCreateModel> dishDrafts = new();

        public BotHandler()
        {
            botClient = new("7760588415:AAGnGtWsikGYFVNIdoY0m5g5Ed5PX_W9j2E");
            this.categoryService = new CategoryService();
            this.dishService = new DishService(categoryService);
        }

        public async Task Run()
        {
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);

            Console.WriteLine("🤖 Bot is running... Press any key to stop.");
            Console.ReadKey();
        }



        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            try
            {
                // 1️⃣ Handle Category Selection (Callback)
                if (update.CallbackQuery != null && update.CallbackQuery.Data != null)
                {
                    string data = update.CallbackQuery.Data;
                    long fromChatId = update.CallbackQuery.Message.Chat.Id;

                    if (data.StartsWith("category:"))
                    {
                        int categoryId = int.Parse(data.Split(':')[1]);

                        DishSession.TempData[fromChatId] = new DishCreateModel
                        {
                            ChatId = fromChatId,
                            categoryId = categoryId,
                            ingredients = new List<Ingredient>()
                        };

                        await client.SendMessage(fromChatId, "✅ Category selected. Now enter the *name* of the dish:", cancellationToken: token);
                        return;
                    }
                }

                // 2️⃣ Text Messages
                if (update.Message == null || update.Message.Text == null)
                    return;

                long chatId = update.Message.Chat.Id;
                var message = update.Message;
                var text = message.Text.Trim();

                if (text.ToLower() == "/start")
                {
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                new[] { new KeyboardButton("➕ Add Dish"), new KeyboardButton("📖 View Recipes") },
                new[] { new KeyboardButton("⚔️ Delete"), new KeyboardButton("🧾 Get List By Name") },
                new[] { new KeyboardButton("❓ Help"), new KeyboardButton("🧠 Smart Search") }
            })
                    {
                        ResizeKeyboard = true
                    };

                    await client.SendMessage(chatId,
                        "👋 Welcome! 👩‍🍳 Welcome to the Cooking Assistant Bot!\nThis bot helps you ease your life in the kitchen!",
                        replyMarkup: keyboard, cancellationToken: token);
                    return;
                }

                if (text.ToLower() == "➕ add dish")
                {
                    var categories = categoryService.GetAll(chatId);
                    var buttons = categories.Select(c =>
                        new[] { InlineKeyboardButton.WithCallbackData(c.Name, $"category:{c.Id}") }).ToList();

                    await client.SendMessage(chatId,
                        "Please select a category for your dish:",
                        replyMarkup: new InlineKeyboardMarkup(buttons),
                        cancellationToken: token);
                    return;
                }

                // 3️⃣ Continue Dish Creation After Category
                if (DishSession.TempData.ContainsKey(chatId))
                {
                    var model = DishSession.TempData[chatId];

                    // Dish Name
                    if (string.IsNullOrEmpty(model.Name))
                    {
                        model.Name = text;
                        await client.SendMessage(chatId,
                            "🍅 Now enter ingredients one by one in this format: `name,amount,unit`\nType `done` when finished.",
                            cancellationToken: token);
                        return;
                    }

                    // ReadyIn
                    if (text.ToLower() == "done")
                    {
                        await client.SendMessage(chatId,
                            "⏱ How long does it take to prepare? (e.g., 00:30 for 30 minutes)",
                            cancellationToken: token);
                        return;
                    }

                    // Still adding ingredients
                    if (model.ReadyIn == default)
                    {
                        var parts = text.Split(',');

                        if (parts.Length == 3)
                        {
                            string ingName = parts[0].Trim();
                            bool amountOk = double.TryParse(parts[1].Trim(), out double amount);
                            string unitText = parts[2].Trim().ToLower();

                            if (!amountOk)
                            {
                                await client.SendMessage(chatId, "❌ Invalid amount. Use a number.", cancellationToken: token);
                                return;
                            }

                            bool unitOk = Enum.TryParse<Unit>(unitText, true, out Unit unit);
                            if (!unitOk)
                            {
                                await client.SendMessage(chatId, $"❌ Invalid unit. Use one of: {string.Join(", ", Enum.GetNames(typeof(Unit)).Select(u => u.ToLower()))}", cancellationToken: token);
                                return;
                            }

                            model.ingredients.Add(new Ingredient
                            {
                                Name = ingName,
                                Amount = amount,
                                Unit = unit
                            });

                            await client.SendMessage(chatId,
                                $"✅ Added: {ingName} ({amount} {unit}).\nType another or `done`.",
                                cancellationToken: token);
                            return;
                        }

                        // Try parsing ReadyIn if not ingredient
                        if (TimeSpan.TryParse(text, out TimeSpan readyTime))
                        {
                            model.ReadyIn = readyTime;
                            dishService.Create(model);
                            DishSession.TempData.Remove(chatId);

                            await client.SendMessage(chatId, "🎉 Dish saved successfully!", cancellationToken: token);
                        }
                        else
                        {
                            await client.SendMessage(chatId,
                                "❌ Invalid input. Use `name,amount,unit` or `done` to move to time.",
                                cancellationToken: token);
                        }

                        return;
                    }
                }

                // Other commands
                if (text.ToLower() == "📖 view recipes")
                {
                    await client.SendMessage(chatId, "Here are your recipes...", cancellationToken: token);
                }
                else if (text.ToLower() == "❓ help")
                {
                    await client.SendMessage(chatId, "This bot helps you store and view recipes.", cancellationToken: token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{update.Message?.Chat?.Id ?? 0}, ❌ Error: {ex.Message}");
            }
        }

        private async Task HandleErrorAsync(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
