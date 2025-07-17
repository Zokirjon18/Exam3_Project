using System.Threading;
using ExamProject.Enums;
using ExamProject.Models;
using ExamProject.Services.CategoryServices;
using ExamProject.Services.DishServices;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExamProject.BotHandlers;

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
            if (update.CallbackQuery?.Data != null)
            {
                string data = update.CallbackQuery.Data;
                long fromChatId = update.CallbackQuery.Message.Chat.Id;

                if (data.StartsWith("category:"))
                {
                    int categoryId = int.Parse(data.Split(':')[1]);

                    DishSession.TempDishData[fromChatId] = new DishCreateModel
                    {
                        ChatId = fromChatId,
                        categoryId = categoryId,
                        ingredients = new List<Ingredient>()
                    };

                    await client.SendMessage(fromChatId, "✅ Category selected. Now enter the *name* of the dish:", cancellationToken: token);
                    return;
                }

                // 📖 User selects a category to view recipes
                if (data.StartsWith("category_"))
                {
                    int categoryId = int.Parse(data.Split('_')[1]);
                    var dishes = dishService.GetAllByCategoryId(fromChatId, categoryId);

                    if (dishes.Count == 0)
                    {
                        await client.SendMessage(fromChatId, "📭 No dishes found in this category.", cancellationToken: token);
                        return;
                    }

                    var buttons = dishes
                        .Select(d => InlineKeyboardButton.WithCallbackData(d.Name, $"dish_{d.Id}"))
                        .Chunk(2)
                        .Select(row => row.ToList())
                        .ToList();

                    await client.SendMessage(fromChatId, "🍽 Choose a dish:", replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: token);
                    return;
                }

                // 📋 User selects a specific dish to view
                if (data.StartsWith("dish_"))
                {
                    int dishId = int.Parse(data.Split('_')[1]);

                    var dish = dishService.Get(fromChatId, dishId);
                    if (dish == null)
                    {
                        await client.SendMessage(fromChatId, "❌ Dish was not found.", cancellationToken: token);
                        return;
                    }

                    string ingredientList = dish.Ingredients?.Any() == true
                        ? string.Join("\n", dish.Ingredients.Select(i => $"- {i.Name}: {i.Amount} {i.Unit}"))
                        : "No ingredients listed.";

                    string dishRecipe = $"🍽 *{dish.Name}*\n" +
                                        $"📂 Category: `{dish.CategoryName}`\n" +
                                        $"⏱ Ready in: `{dish.ReadyIn}` minutes\n\n" +
                                        $"📋 *Ingredients:*\n{ingredientList}";

                    await client.SendMessage(fromChatId, dishRecipe, Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: token);
                    return;
                }
            }


            if (update.Message == null || update.Message.Text == null) return;

            long chatId = update.Message.Chat.Id;
            var message = update.Message;
            var text = message.Text.Trim();

            if (text.ToLower() == "/start")
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
            new[] { new KeyboardButton("➕ Add Dish"), new KeyboardButton("📖 View Recipes") },
            new[] { new KeyboardButton("➕ Add Category"), new KeyboardButton("🧾 Get List By Dish Name") },
            new[] { new KeyboardButton("⚔️ Delete Category"), new KeyboardButton("⚔️ Delete Dish") },
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

            if (text == "➕ Add Dish")
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

            if (DishSession.TempDishData.ContainsKey(chatId))
            {
                var model = DishSession.TempDishData[chatId];

                // Dish Name
                if (string.IsNullOrEmpty(model.Name))
                {
                    model.Name = text;
                    await client.SendMessage(chatId,
                        "🍅 Now enter ingredients one by one in this format: `name,amount,unit`\nType /done when finished.",
                        cancellationToken: token);
                    return;
                }

                if (text.ToLower() == "/done")
                {
                    await client.SendMessage(chatId,
                        "⏱ How long does it take to prepare? (e.g., 00:30 for 30 minutes)",
                        cancellationToken: token);
                    return;
                }

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
                            $"✅ Added: {ingName} ({amount} {unit}).\nType another or /done",
                            cancellationToken: token);
                        return;
                    }

                    if (TimeSpan.TryParse(text, out TimeSpan readyTime))
                    {
                        model.ReadyIn = readyTime;
                        dishService.Create(model);
                        DishSession.TempDishData.Remove(chatId);

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


            // add category feature
            if (text == "➕ Add Category")
            {
                CategorySession.HoldCallerChatId.Add(chatId);

                await client.SendMessage(
                        chatId: chatId,
                        text: "📂 Please enter a name for the new category:",
                        cancellationToken: token
                    );
                return;
            }

            if (CategorySession.HoldCallerChatId.Contains(chatId))
            {
                string categoryName = text.Trim();

                try
                {
                    categoryService.Create(chatId, categoryName);
                    CategorySession.HoldCallerChatId.Remove(chatId);

                    await client.SendMessage(chatId, $"✅ Category *{categoryName}* added successfully!", cancellationToken: token);
                }
                catch (Exception ex)
                {
                    await client.SendMessage(chatId, $"❌ Failed to add category: {ex.Message}", cancellationToken: token);
                }

                return;
            }


            // view recipes fature
            else if (text == "📖 View Recipes")
            {
                var categories = categoryService.GetAll(chatId); 
                if (categories.Count == 0)
                {
                    await client.SendMessage(chatId, "📂 No categories found. Please add one first.", cancellationToken: token);
                    
                }

                var buttons = categories
                    .Select(c => InlineKeyboardButton.WithCallbackData(c.Name, $"category_{c.Id}"))
                    .Chunk(2)
                    .Select(row => row.ToList())
                    .ToList();

                await client.SendMessage(chatId, "📂 Choose a category:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: token);
                
                return;
            }
            else if (text == "🧾 Get List By Dish Name")
            {
                await client.SendMessage(chatId, "by dish name.", cancellationToken: token);
            }
            else if (text == "⚔️ Delete Category")
            {
                await client.SendMessage(chatId, "delete ca.", cancellationToken: token);
            }
            else if (text == "⚔️ Delete Dish")
            {
                await client.SendMessage(chatId, "delete d.", cancellationToken: token);
            }
            else if (text == "❓ Help")
            {
                await client.SendMessage(chatId, "help", cancellationToken: token);
            }
            else if (text == "🧠 Smart Search")
            {
                await client.SendMessage(chatId, "smart", cancellationToken: token);
            }
            else if (text.ToLower() == "/units")
            {
                await client.SendMessage(chatId, "        kg,\r\n        gram,\r\n        ml,\r\n        pieces", cancellationToken: token);
            }
            else
            {
                await client.SendMessage(chatId, "Sorry i didn't understand that", cancellationToken: token);
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
