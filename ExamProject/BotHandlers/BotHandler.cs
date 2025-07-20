using System.Linq;
using System.Threading;
using ExamProject.botSessions;
using ExamProject.Constants;
using ExamProject.Domain;
using ExamProject.Enums;
using ExamProject.Extentions;
using ExamProject.Models;
using ExamProject.Services.CategoryServices;
using ExamProject.Services.DishServices;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExamProject.BotHandlers;

internal class BotHandler
{
    TelegramBotClient botClient;
    DishService dishService;
    CategoryService categoryService;
    private Dictionary<long, DishCreateModel> dishDrafts = new();

    public BotHandler()
    {
        botClient = new("7569582496:AAEONimHysHEis2dPBFgUSxhCzEJ4qeaOrg");
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

                // User selects a catagory to add a dish
                if (data.StartsWith("category:"))
                {
                    int categoryId = int.Parse(data.Split(':')[1]);

                    DishSession.HoldCallerChatId[fromChatId] = new DishCreateModel
                    {
                        ChatId = fromChatId,
                        categoryId = categoryId,
                        ingredients = new List<Ingredient>()
                    };

                    await client.SendMessage(fromChatId, "✅ Category selected. Now enter the *name* of the dish:", cancellationToken: token);
                    return;
                }

                // User selects a category to view recipes
                else if (data.StartsWith("category_"))
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
                        .Chunk(4)
                        .Select(row => row.ToList())
                        .ToList();

                    await client.SendMessage(fromChatId, "🍽 Choose a dish:", replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: token);
                    return;
                }

                // User selects a category for choosing a dish for deletion
                else if (data.StartsWith("categoryForDeletingDish_"))
                {
                    int categoryId = int.Parse(data.Split('_')[1]);
                    var dishes = dishService.GetAllByCategoryId(fromChatId, categoryId);

                    if (dishes.Count == 0)
                    {
                        await client.SendMessage(fromChatId, "📭 No dishes found in this category.", cancellationToken: token);
                        return;
                    }

                    var buttons = dishes
                        .Select(d => InlineKeyboardButton.WithCallbackData(d.Name, $"dish={d.Id}"))
                        .Chunk(4)
                        .Select(row => row.ToList())
                        .ToList();

                    await client.SendMessage(fromChatId, "🍽 Choose a dish:", replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: token);
                    return;
                }
                // dish selection and deletion part
                else if (data.StartsWith("dish="))
                {
                    int dishId = int.Parse(data.Split('=')[2]);

                    var dish = dishService.Get(fromChatId, dishId);


                    var confirmButtons = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅ Yes", $"confirm-delete-dish-{dishId}"),
                            InlineKeyboardButton.WithCallbackData("❌ No", "cancel-delete-dish")
                        }
                    });

                    await client.SendMessage(
                        fromChatId,
                        $"Are you sure you want to delete *{dish.Name}* recipe?",
                        replyMarkup: confirmButtons,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: token);
                    return;
                }
                else if (data.StartsWith("confirm-delete-dish-"))
                {
                    int dishId = int.Parse(data.Split('-')[3]);

                    dishService.Delete(fromChatId, dishId);

                    await client.SendMessage(fromChatId, "✅ Dish deleted successfully.", cancellationToken: token);

                    return;
                }
                else if (data == "cancel-delete-dish")
                {
                    await client.SendMessage(fromChatId, "❌ Deletion cancelled.", cancellationToken: token);
                    return;
                }

                // category deletion part
                else if (data.StartsWith("delete-cat-"))
                {
                    int categoryId = int.Parse(data.Split('-')[2]);

                    var category = categoryService.Get(fromChatId, categoryId);

                    var confirmButtons = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅ Yes", $"confirm-delete-category:{categoryId}"),
                            InlineKeyboardButton.WithCallbackData("❌ No", "cancel-delete-category")
                        }
                    });

                    await client.SendMessage(
                        fromChatId,
                        $"📂 Note: all dishes in this category will be *deleted* too!\n" +
                        $"Are you sure you want to delete *{category.Name}* category?",
                        replyMarkup: confirmButtons,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: token);
                    return;
                }
                else if (data.StartsWith("confirm-delete-category:"))
                {
                    if (int.TryParse(data.Split(':')[1], out int categoryId))
                    {
                        categoryService.Delete(fromChatId, categoryId);
                        categoryService.Delete(fromChatId, categoryId);
                        dishService.DeleteAllByCategoryId(fromChatId, categoryId);

                        await client.SendMessage(fromChatId, "✅ Category deleted successfully.", cancellationToken: token);
                    }
                    return;
                }
                else if (data == "cancel-delete-category")
                {
                    await client.SendMessage(fromChatId, "❌ Deletion cancelled.", cancellationToken: token);
                    return;
                }

                // update category part
                else if (data.StartsWith("update-cat-"))
                {
                    int categoryId = int.Parse(data.Split('-')[2]);

                    // Store the session to track which category to update
                    UpdateCategorySession.Session[fromChatId] = $"update-category-{categoryId}";

                    await client.SendMessage(
                        fromChatId,
                        "✏️ Send the new name for the category:",
                        cancellationToken: token);

                    return;
                }


                else if (data.StartsWith("update-dish-"))
                {
                    int dishId = int.Parse(data.Split('-')[2]);
                    var dish = dishService.GetForUpdation(fromChatId, dishId);

                    UpdateDishSession.Session[fromChatId] = new DishUpdateModel
                    {
                        Id = dishId,
                        ChatId = fromChatId,
                        Name = dish.Name,
                        Ingredients = dish.Ingredients,
                        ReadyIn = dish.ReadyIn,
                        CategoryId = dish.CategoryId
                        
                    };

                    UpdateDishSession.dishUpdateStep[fromChatId] = "name";

                    await client.SendMessage(fromChatId,
                        $"✏️ Current name: {dish.Name}\nSend new name or type /skip:",
                        cancellationToken: token);
                    return;
                }
                else if (data.StartsWith("upd-dishCategory-"))
                {
                    if (data.EndsWith("skip"))
                    {
                        // User chose to skip category update
                        var dishsh = UpdateDishSession.Session[fromChatId];
                        await FinalizeDishUpdate(client, fromChatId, dishsh);
                        return;
                    }

                    int categoryId = int.Parse(data.Split('-')[2]);

                    if (UpdateDishSession.Session.TryGetValue(fromChatId, out var dish))
                    {
                        dish.CategoryId = categoryId;
                        await FinalizeDishUpdate(client, fromChatId, dish);
                    }
                    return;
                }

                // User selects a specific dish to view
                else if (data.StartsWith("dish_"))
                {
                    int dishId = int.Parse(data.Split('_')[1]);

                    var viewModel = dishService.Get(fromChatId, dishId);

                    string ingredientList = string.Join("\n",
                                        viewModel.Ingredients.Select(
                                            i => $"- {i.Name}: {i.Amount} {i.Unit}"));

                    string dishRecipe = $"🍽 *{viewModel.Name}*\n" +
                                        $"📂 Category: {viewModel.CategoryName}\n" +
                                        $"⏱ Ready in: {viewModel.ReadyIn} minutes\n\n" +
                                        $"📋 *Ingredients:*\n{ingredientList}";

                    await client.SendMessage(
                        fromChatId,
                        dishRecipe,
                        ParseMode.Markdown,
                        cancellationToken: token);

                    return;
                }

                // User Selects a specific dish found by filtering by ingredients to view
                else if (data.StartsWith("dish:"))
                {
                    int dishId = int.Parse(data.Split(':')[1]);

                    var viewModel = dishService.Get(fromChatId, dishId);

                    string ingredientList = string.Join("\n",
                                      viewModel.Ingredients.Select(
                                          i => $"- {i.Name}: {i.Amount} {i.Unit}"));

                    string dishRecipe = $"🍽 *{viewModel.Name}*\n" +
                                        $"📂 Category: {viewModel.CategoryName}\n" +
                                        $"⏱ Ready in: {viewModel.ReadyIn} minutes\n\n" +
                                        $"📋 *Ingredients:*\n{ingredientList}";

                    await client.SendMessage(fromChatId, dishRecipe,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: token);

                    return;
                }

                // User selects a specific dish found by filtering by dish name to view
                else if (data.StartsWith("dish-"))
                {
                    int dishId = int.Parse(data.Split("-")[1]);

                    DishViewModel viewModel = dishService.Get(fromChatId, dishId);

                    string ingredientList = string.Join("\n",
                                      viewModel.Ingredients.Select(
                                          i => $"- {i.Name}: {i.Amount} {i.Unit}"));

                    string dishRecipe = $"🍽 *{viewModel.Name}*\n" +
                                        $"📂 Category: {viewModel.CategoryName}\n" +
                                        $"⏱ Ready in: {viewModel.ReadyIn} minutes\n\n" +
                                        $"📋 *Ingredients:*\n{ingredientList}";
                    await client.SendMessage(
                          fromChatId,
                          dishRecipe,
                          ParseMode.Markdown,
                          cancellationToken: token);
                }
            }


            if (update.Message == null || update.Message.Text == null) return;

            long chatId = update.Message.Chat.Id;
            var message = update.Message;
            var text = message.Text.Trim();

            if (text.ToLower() == "/start")
            {
                DishSession.HoldCallerChatId.Remove(chatId);
                CategorySession.HoldCallerChatId.Remove(chatId);
                SearchByDishNameSession.HoldCallerChatId.Remove(chatId);
                SearchByIngSession.HoldCallerChatId.Remove(chatId);
                UpdateCategorySession.Session.Remove(chatId);
                UpdateDishSession.Session.Remove(chatId);
                UpdateDishSession.dishUpdateStep.Remove(chatId);
                UpdateDishSession.tempIngredients.Remove(chatId);

                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                new[] { new KeyboardButton("➕ Add Dish"), new KeyboardButton("📖 View Recipes") },
                new[] { new KeyboardButton("➕ Add Category"), new KeyboardButton("🗒️ Search By Dish_Name") },
                new[] { new KeyboardButton("⚔️ Delete Category"), new KeyboardButton("🧾 Search by Ingredients") },
                new[] { new KeyboardButton("⚔️ Delete Dish"), new KeyboardButton("🖊️ Edit Category") },
                new[] /*{ new KeyboardButton("⚔️ Delete Dish"),*/ {new KeyboardButton("🖊️ Edit Dish") }
            })
                {
                    ResizeKeyboard = true
                };

                await client.SendMessage(chatId,
                    "👩‍🍳 Welcome to the Cooking Assistant Bot!\nThis bot helps you ease your life in the kitchen!",
                    replyMarkup: keyboard, cancellationToken: token);
                return;
            }

            // add dish feature
            if (text == "➕ Add Dish")
            {
                var categories = categoryService.GetAll(chatId);
                var buttons = categories.Select(c =>
                     InlineKeyboardButton.WithCallbackData(c.Name, $"category:{c.Id}"))
                    .Chunk(2)
                    .Select(row => row.ToList())
                    .ToList();

                await client.SendMessage(chatId,
                    "Please select a category for your dish:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: token);
                return;
            }

            if (DishSession.HoldCallerChatId.ContainsKey(chatId))
            {
                var model = DishSession.HoldCallerChatId[chatId];


                if (string.IsNullOrEmpty(model.Name))
                {
                    model.Name = text;
                    await client.SendMessage(chatId,
                        "🍅 Now enter ingredients one by one in this format: `name amount unit`\nType /done when finished.",
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
                    var parts = text.Split(' ');

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
                            await client.SendMessage(
                                chatId,
                                $"❌ Invalid unit. Use one of: " +
                                $"{string.Join(" ", Enum.GetNames(typeof(Unit)).Select(u => u.ToLower()))}",
                                cancellationToken: token);
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
                        DishSession.HoldCallerChatId.Remove(chatId);

                        await client.SendMessage(chatId, "🎉 Dish saved successfully!", cancellationToken: token);
                    }
                    else
                    {
                        await client.SendMessage(chatId,
                            "❌ Invalid input. Use `name amount unit` or /done to move to time.",
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


            // view recipes feature
            else if (text == "📖 View Recipes")
            {
                var categories = categoryService.GetAll(chatId);
                if (categories.Count == 0)
                {
                    await client.SendMessage(chatId, "📂 No categories found. Please add one first.", cancellationToken: token);
                    return;
                }

                var buttons = categories
                    .Select(c => InlineKeyboardButton.WithCallbackData(c.Name, $"category_{c.Id}"))
                    .Chunk(4)
                    .Select(row => row.ToList())
                    .ToList();

                await client.SendMessage(chatId, "📂 Choose a category:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: token);

                return;
            }


            // search by dish name 
            else if (text == "🗒️ Search By Dish_Name")
            {
                SearchByDishNameSession.HoldCallerChatId.Add(chatId);

                await client.SendMessage(
                    chatId, "🖋️ Enter dish name:", cancellationToken: token);

                return;
            }

            if (SearchByDishNameSession.HoldCallerChatId.Contains(chatId))
            {
                SearchByDishNameSession.HoldCallerChatId.Remove(chatId);

                List<DishViewModel> filteredDishes;

                try
                {
                    filteredDishes = dishService.GetAllByDishName(chatId, text.ToLower());
                }
                catch (Exception ex)
                {
                    await client.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: token);
                    return;
                }

                var buttons = filteredDishes
                    .Select(d => InlineKeyboardButton.WithCallbackData(d.Name, $"dish-{d.Id}"))
                    .Chunk(4)
                    .Select(row => row.ToList())
                    .ToList();

                await client.SendMessage(chatId, "🍽 Recipes found. Choose one:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: token);
            }


            // search by ing feature
            else if (text == "🧾 Search by Ingredients")
            {
                SearchByIngSession.HoldCallerChatId.Add(chatId);

                await client.SendMessage(
                    chatId,
                    "🧾 Add ingredients seperating by commas\n(e.g. flour,sour cream)",
                    cancellationToken: token);

                return;
            }

            else if (SearchByIngSession.HoldCallerChatId.Contains(chatId))
            {
                SearchByIngSession.HoldCallerChatId.Remove(chatId);

                // Parse ingredients
                var ingredientNames = text
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

                List<DishViewModel> filteredDishes;

                try
                {
                    filteredDishes = dishService.GetAllByIngredients(chatId, ingredientNames);
                }
                catch (Exception ex)
                {
                    await client.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: token);
                    return;
                }

                var buttons = filteredDishes
                    .Select(d => InlineKeyboardButton.WithCallbackData(d.Name, $"dish:{d.Id}"))
                    .Chunk(4)
                    .Select(row => row.ToList())
                    .ToList();

                await client.SendMessage(chatId, "🍽 Recipes found. Choose one:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: token);
            }


            // category deletion feature
            else if (text == "⚔️ Delete Category")
            {
                var categories = categoryService.GetAll(chatId);

                if (categories.Count == 0)
                {
                    await client.SendMessage(chatId, "📂 No categories found.", cancellationToken: token);
                    return;
                }

                var buttons = categories
                    .Select(c => InlineKeyboardButton.WithCallbackData(c.Name, $"delete-cat-{c.Id}"))
                    .Chunk(4)
                    .Select(row => row.ToList())
                    .ToList();

                await client.SendMessage(chatId, "📂 Choose a category to delete:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: token);

                return;
            }

            // dish deletion feature
            else if (text == "⚔️ Delete Dish")
            {
                var categories = categoryService.GetAll(chatId);

                if (categories.Count == 0)
                {
                    await client.SendMessage(chatId, "📂 No categories found.", cancellationToken: token);
                    return;
                }
                var buttons = categories
                                    .Select(c => InlineKeyboardButton.WithCallbackData(
                                        c.Name,
                                        $"categoryForDeletingDish_{c.Id}"))
                                    .Chunk(4)
                                    .Select(row => row.ToList())
                                    .ToList();

                await client.SendMessage(chatId, "📂 Choose a category:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: token);

                return;
            }

            else if (text == "🖊️ Edit Category")
            {
                var categories = categoryService.GetAll(chatId);

                if (categories.Count == 0)
                {
                    await client.SendMessage(chatId, "📂 No categories found.", cancellationToken: token);
                    return;
                }

                var buttons = categories
                    .Select(c => InlineKeyboardButton.WithCallbackData(c.Name, $"update-cat-{c.Id}"))
                    .Chunk(4)
                    .Select(row => row.ToList())
                    .ToList();

                await client.SendMessage(chatId, "📂 Choose a category to edit:",
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    cancellationToken: token);

                return;
            }

            else if (UpdateCategorySession.Session.ContainsKey(chatId))
            {
                int categoryId = int.Parse(UpdateCategorySession.Session[chatId].Split('-')[2]);

                categoryService.Update(chatId, categoryId, text);

                UpdateCategorySession.Session.Remove(chatId);

                await client.SendMessage(
                    chatId,
                    "✅ Category updated successfully.",
                    cancellationToken: token);

                return;
            }
           
            else if (text == "🖊️ Edit Dish")
            {
                // Start the edit dish process
                UpdateDishSession.dishUpdateStep[chatId] = "start";
                await client.SendMessage(chatId, "🖋️ Enter the name of the dish you want to edit:",
                    cancellationToken: token);
                return;
            }

            else if (UpdateDishSession.dishUpdateStep.ContainsKey(chatId))
            {
                var currentStep = UpdateDishSession.dishUpdateStep[chatId];

                // Step 1: Find dish to edit
                if (currentStep == "start")
                {

                    List<DishViewModel> filteredDishes;
                    try
                    {
                        filteredDishes = dishService.GetAllByDishName(chatId, text.ToLower());

                        if (filteredDishes.Count == 0)
                        {
                            await client.SendMessage(chatId, "❌ No dishes found with that name. Try again:",
                                cancellationToken: token);
                            return;
                        }

  
                        var buttons = filteredDishes
                                    .Select(d => InlineKeyboardButton.WithCallbackData(d.Name, $"update-dish-{d.Id}"))
                                    .Chunk(4)
                                    .Select(row => row.ToList())
                                    .ToList();

                        await client.SendMessage(chatId, "🍽 Recipes found. Choose one to edit:",
                            replyMarkup: new InlineKeyboardMarkup(buttons),
                            cancellationToken: token);
                    }
                    catch (Exception ex)
                    {
                        await client.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: token);
                        return;
                    }
                    return;
                }

                if (UpdateDishSession.Session.TryGetValue(chatId, out var dish))
                {
                    if (currentStep == "name")
                    {
                        if (text != "/skip")
                            dish.Name = text;

                        await client.SendMessage(
                            chatId,
                            "🧂 Send ingredients one by one in format: 'name amount unit'\n" +
                            "Example: tomato 2 kg\n" +
                            "Send /done when finished or /skip to keep current ingredients");

                        UpdateDishSession.dishUpdateStep[chatId] = "ingredients";
                        UpdateDishSession.tempIngredients[chatId] = new List<Ingredient>(); 
                        return;
                    }

                   
                    else if (currentStep == "ingredients")
                    {
                        if (text == "/done")
                        {
                            if (UpdateDishSession.tempIngredients[chatId].Count > 0)
                            {
                                dish.Ingredients = UpdateDishSession.tempIngredients[chatId];
                            }

                            await client.SendMessage(chatId, "⏱️ Now send the cooking time in format hh:mm or type /skip:");
                            UpdateDishSession.dishUpdateStep[chatId] = "time";
                            return;
                        }
                        else if (text != "/skip")
                        {
                            try
                            {
                                var parts = text.Split(' ');
                                if (parts.Length != 3)
                                    throw new FormatException("Invalid format. Use: name amount unit");

                                var ingredient = new Ingredient
                                {
                                    Name = parts[0],
                                    Amount = double.Parse(parts[1]),
                                    Unit = Enum.Parse<Unit>(parts[2], true)
                                };

                                UpdateDishSession.tempIngredients[chatId].Add(ingredient);

                                await client.SendMessage(chatId,
                                    $"✅ Added: {ingredient.Name} {ingredient.Amount} {ingredient.Unit}\n" +
                                    "Send next ingredient or /done when finished");
                                return;
                            }
                            catch (Exception ex)
                            {
                                await client.SendMessage(chatId,
                                    $"❌ Error: {ex.Message}\nPlease try again or send /done");
                                return;
                            }
                        }
                    }

                    else if (currentStep == "time")
                    {
                        if (text != "/skip")
                        {
                            if (TimeSpan.TryParse(text, out var time))
                            {
                                dish.ReadyIn = time;
                            }
                            else
                            {
                                await client.SendMessage(chatId,
                                    "❌ Invalid time format. Please use hh:mm format or /skip",
                                    cancellationToken: token);
                                return;
                            }
                        }

                        var categories = categoryService.GetAll(chatId);
                        var categoryButtons = categories
                            .Select(c => InlineKeyboardButton.WithCallbackData(c.Name, $"upd-dishCategory-{c.Id}"))
                            .Chunk(3)
                            .Select(c => c.ToList())
                            .ToList();

                        categoryButtons.Add(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData("Skip", "upd-dishCategory-skip")
            });

                        await client.SendMessage(chatId, "📂 Choose new category:",
                            replyMarkup: new InlineKeyboardMarkup(categoryButtons));
                        UpdateDishSession.dishUpdateStep[chatId] = "category";
                        return;
                    }
                }
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
    private async Task FinalizeDishUpdate(ITelegramBotClient client, long chatId, DishUpdateModel dish)
    {
        try
        {
            dishService.Update(dish);

            // More comprehensive cleanup
            UpdateDishSession.Session.Remove(chatId);
            UpdateDishSession.dishUpdateStep.Remove(chatId);
            if (UpdateDishSession.tempIngredients.ContainsKey(chatId))
                UpdateDishSession.tempIngredients.Remove(chatId);

            await client.SendMessage(chatId, "✅ Dish successfully updated.");
        }
        catch (Exception ex)
        {
            await client.SendMessage(chatId, $"❌ Error updating dish: {ex.Message}");

            // Cleanup even if update fails
            UpdateDishSession.Session.Remove(chatId);
            UpdateDishSession.dishUpdateStep.Remove(chatId);
            if (UpdateDishSession.tempIngredients.ContainsKey(chatId))
                UpdateDishSession.tempIngredients.Remove(chatId);
        }
    }
}
