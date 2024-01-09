using PlayFab;
using PlayFab.EconomyModels;
using ProductMigration.Utils.Title;
using ProductMigration.Services.CatalogsV2;
using System.Text;

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

                var commandParts = command.Split(' ');
                var cmd = commandParts[0].ToLower();

                // TODO: for now we have only a few commands, should we spend time and create a better design with a dispatcher or something?
                if (cmd == "ls" && commandParts.Length >= 2)
                {
                    string titleId = commandParts[1];
                    string titleDevSecret = commandParts[2];

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
                    PrintCatalogItems(catalogItems);
                }
                else if (cmd == "cp" && commandParts.Length >= 4)
                {
                    string sourceTitleId = commandParts[1];
                    string sourceTitleSecret = commandParts[2];
                    string targetTitleId = commandParts[3];
                    string targetTitleSecret = commandParts[4];
                    bool bVerbose = commandParts.Length > 4;                    

                    // source title ("from")
                    var sourceTitleSettings = new PlayFabApiSettings
                    {
                        TitleId = sourceTitleId,
                        DeveloperSecretKey = sourceTitleSecret
                    };                    

                    var source_authContext = new PlayFabAuthenticationContext
                    {
                        EntityId = sourceTitleSettings.TitleId,
                        EntityType = "title",
                        EntityToken = await TitleAuthUtil.GetTitleEntityToken(sourceTitleSettings)
                    };

                    CatalogV2Service source_catalogV2Service = new CatalogV2Service(sourceTitleSettings, source_authContext);
                    List<CatalogItem> source_allItems = await source_catalogV2Service.SearchItems();
                    List<CatalogItem> source_catalogItems = source_allItems.Where(x => x.Type == "catalogItem").ToList(); // TODO: how to handle bundles and currency?

                    if (bVerbose)
                    {
                        Console.WriteLine($"\n\nNumber of catalog items available in the source title: {source_catalogItems.Count}");
                        PrintCatalogItems(source_catalogItems);
                    }

                    // target title ("to")
                    var targetTitleSettings = new PlayFabApiSettings
                    {
                        TitleId = targetTitleId,
                        DeveloperSecretKey = targetTitleSecret
                    };

                    var target_authContext = new PlayFabAuthenticationContext
                    {
                        EntityId = targetTitleSettings.TitleId,
                        EntityType = "title",
                        EntityToken = await TitleAuthUtil.GetTitleEntityToken(targetTitleSettings)
                    };

                    // NOTE: delete all existing items that is not present on source title
                    CatalogV2Service target_catalogV2Service = new CatalogV2Service(targetTitleSettings, target_authContext);
                    List<CatalogItem> target_allItems = await target_catalogV2Service.SearchItems();
                    List<CatalogItem> target_catatologItems = target_allItems.Where(x => x.Type == "catalogItem").ToList(); // TODO: how to handle bundles and currency?
                    // delete only items that the target has and the source doesn't have
                    List<CatalogItem> itemsToDelete = target_catatologItems.Where(targetItem => !source_catalogItems.Any(sourceItem => sourceItem.Id == targetItem.Id)).ToList();                    
                    // NOTE: create only items that target doesn't have it yet from the source title
                    List<CatalogItem> itemsToCreate = source_catalogItems.Where(sourceItem => !target_catatologItems.Any(targetItem => targetItem.Id == sourceItem.Id)).ToList();
                    if (bVerbose)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n\nNumber of catalog items to delete from the target title: {itemsToDelete.Count}\n");
                        PrintCatalogItems(itemsToDelete);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n\nNumber of catalog items to create in the target title: {itemsToCreate.Count}\n");
                        PrintCatalogItems(itemsToCreate);
                    }
                    await target_catalogV2Service.DeleteItems(itemsToDelete);
                    await target_catalogV2Service.CreateItems(itemsToCreate);
                }                
                else
                {
                    Console.Write($"Ops! My advanced AI capabilities couldn't understand what you want with {cmd}... please, try again.");
                }                
            }            
        }

        public static void PrintCatalogItems(List<CatalogItem> items)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\n{items.Count} items:");

            foreach (var item in items)
            {
                if (item.Type == "catalogItem")
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (item.Type == "currency")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (item.Type == "bundle")
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                }
                else if (item.Type == "store")
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                string friendlyId = item.AlternateIds.Where(obj => obj.Type == "FriendlyId").Aggregate(new StringBuilder(), (sb, obj) => sb.Append(obj.Value), sb => sb.ToString());
                Console.WriteLine($"\nType: {item.Type} - FriendlyId: {friendlyId} (StackId: {item.DefaultStackId})");
            }
        }
    }
}
