using PlayFab;
using PlayFab.EconomyModels;
using ProductMigration.Utils.Title;
using ProductMigration.Services.CatalogsV2;
using ProductMigration.Services;

namespace ProductMigrationTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Out put to screen some fancy playfab jazz
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(".oO={ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("PlayFab Migration Tool");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" }=Oo.\n");
            Console.ForegroundColor = ConsoleColor.White;

            while (true)
            {
                Console.Write("\nEnter command ('exit' to quit): ");
                var command = Console.ReadLine();

                if (command.ToLower() == "exit")
                {
                    break;
                }

                if (command.Length < 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"That's an invalid command!");
                    continue;
                }

                var commandParts = command.Split(' ');
                bool bVerbose = commandParts.Contains(" -v");
                var cmd = commandParts[0].ToLower();
                string context = commandParts[1];

                // TODO: for now we have only a few commands, should we spend time and create a better design with a dispatcher or something?
                if (cmd == "ls" && commandParts.Length >= 3)
                {
                    if (context == "catalogv2")
                    {
                        string titleId = commandParts[2];
                        string titleDevSecret = commandParts[3];

                        await ListCatalogV2Items(titleId, titleDevSecret);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"I'm sorry, this context ({context}) is not implemented yet!");
                    }
                }
                else if (cmd == "cp" && commandParts.Length >= 5)
                {
                    if (context == "catalogv2")
                    {
                        string sourceTitleId = commandParts[2];
                        string sourceTitleSecret = commandParts[3];
                        string targetTitleId = commandParts[4];
                        string targetTitleSecret = commandParts[5];

                        CatalogV2MigrationService catalogV2MigrationService = new CatalogV2MigrationService(sourceTitleId, sourceTitleSecret, targetTitleId, targetTitleSecret, bVerbose);
                        await catalogV2MigrationService.Setup();
                        await catalogV2MigrationService.CopyCatalogV2();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"I'm sorry, this context ({context}) is not implemented yet!");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write($"Ops! My advanced AI capabilities couldn't understand what you want with {cmd}... please, try again.");
                }
            }
        }

        static async Task ListCatalogV2Items(string titleId, string titleDevSecret)
        {
            var titleSettings = new PlayFabApiSettings
            {
                TitleId = titleId,
                DeveloperSecretKey = titleDevSecret
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityId = titleSettings.TitleId,
                EntityType = "title",
                EntityToken = await TitleAuthUtil.GetTitleEntityToken(titleSettings)
            };

            CatalogV2Service catalogService = new CatalogV2Service(titleSettings, authContext);
            List<CatalogItem> catalogItems = await catalogService.SearchItems();
            CatalogV2Service.PrintCatalogItems(catalogItems);
        }
    }
}
