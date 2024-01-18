using PlayFab;
using ProductMigration.Utils.Title;
using ProductMigration.Services;
using PlayFab.ServerModels;
using Exporter.Services;
using CLI.Models;

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
            Console.Write("Gamecan PlayFab CLI Tool");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" }=Oo.\n");
            Console.ForegroundColor = ConsoleColor.White;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\nEnter command ('exit' to quit): ");
                var command = Console.ReadLine();

                if (command.ToLower() == "exit" || command.ToLower() == "quit")
                {
                    break;
                }

                if (command.ToLower() == "help")
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"\n\nhahahahahaha"); // TODO: list of commands and its usage
                    continue;
                }

                var commandParts = command.Split(' ');
                if (commandParts.Length < 2)
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write($"What do you mean by {command}? Try again!");
                    continue;
                }

                bool bVerbose = commandParts.Contains("-v");
                var cmd = commandParts[0].ToLower();
                string context = commandParts[1];

                // TODO: for now we have only a few commands, should we spend time and create a better design with a dispatcher or something?
                #region listing
                if (cmd == "ls" && commandParts.Length >= 3)
                {
                    string titleId = commandParts[2];
                    string titleDevSecret = commandParts[3];

                    if (context == "catalogv2")
                    {
                        await ListCatalogV2Items(titleId, titleDevSecret);
                    }
                    else if (context == "functions")
                    {
                        await ListCloudScriptFunctions(titleId, titleDevSecret);
                    }
                    else if (context == "rules")
                    {
                        // TODO:
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"\nI'm sorry, this context ({context}) is not implemented yet but it is in our TODO list, once playfab engineers add the respective API for that...");
                    }
                    else if (context == "titledata")
                    {
                        await ListServerTitleData(titleId, titleDevSecret, false);
                    }
                    else if (context == "titleinternaldata")
                    {
                        await ListServerTitleData(titleId, titleDevSecret, true);
                    }
                    else if (context == "matchmakingqueues")
                    {
                        await ListMatchmakingQueueConfig(titleId, titleDevSecret);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"I'm sorry, this context ({context}) is not implemented yet!");
                    }
                }
                #endregion
                #region copying
                else if (cmd == "cp" && commandParts.Length >= 5)
                {
                    string sourceTitleId = commandParts[2];
                    string sourceTitleSecret = commandParts[3];
                    string targetTitleId = commandParts[4];
                    string targetTitleSecret = commandParts[5];

                    if (context == "catalogv2")
                    {
                        CatalogV2MigrationService catalogV2MigrationService = new CatalogV2MigrationService(sourceTitleId, sourceTitleSecret, targetTitleId, targetTitleSecret, bVerbose);
                        await catalogV2MigrationService.Login();
                        await catalogV2MigrationService.CopyCatalogV2();
                    }
                    else if (context == "functions")
                    {
                        CloudScriptMigrationService cloudScriptMigrationService = new CloudScriptMigrationService(sourceTitleId, sourceTitleSecret, targetTitleId, targetTitleSecret, bVerbose);
                        await cloudScriptMigrationService.Login();
                        await cloudScriptMigrationService.CopyFunctions();
                    }
                    else if (context == "rules")
                    {
                        // TODO:
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"\nI'm sorry, this context ({context}) is not implemented yet but it is in our TODO list, once playfab engineers add the respective API for that......");
                    }
                    else if (context == "titledata")
                    {
                        ServerTitleDataMigrationService serverTitleDataMigrationService = new ServerTitleDataMigrationService(sourceTitleId, sourceTitleSecret, targetTitleId, targetTitleSecret, bVerbose);
                        await serverTitleDataMigrationService.CopyData();
                    }
                    else if (context == "titleinternaldata")
                    {
                        ServerTitleDataMigrationService serverTitleDataMigrationService = new ServerTitleDataMigrationService(sourceTitleId, sourceTitleSecret, targetTitleId, targetTitleSecret, bVerbose);
                        await serverTitleDataMigrationService.CopyData(true);
                    }
                    else if (context == "matchmakingqueues")
                    {
                        MatchmakingQueuesMigrationService matchmakingQueuesMigrationService = new MatchmakingQueuesMigrationService(sourceTitleId, sourceTitleSecret, targetTitleId, targetTitleSecret, bVerbose);
                        await matchmakingQueuesMigrationService.LoginAsync();
                        await matchmakingQueuesMigrationService.CopyMatchmakingQueuesConfigAsync();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"\n\nI'm sorry, this context ({context}) is not implemented yet!");
                    }
                }
                #endregion
                #region exporting
                else if (cmd == "export" && commandParts.Length >= 6)
                {
                    if (context == "player")
                    {
                        string dataContext = commandParts[2];
                        string segmentId = commandParts[3];
                        string titleId = commandParts[4];
                        string titleDevSecret = commandParts[5];
                        string filePath = commandParts[6];

                        if (dataContext == "feedback")
                        {
                            PlayerExporterService playerExporterService = new PlayerExporterService(titleId, titleDevSecret, bVerbose);
                            await playerExporterService.Login();
                            List<UserProfileDTO> usersProfiles = await playerExporterService.GetAllPlayersUserProfilesAsync(segmentId);
                            PlayerExporterService.ExportFeedbackToCsv(usersProfiles, filePath);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write($"\n\nI'm sorry, this data context ({dataContext}) is not implemented yet!");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"\n\nI'm sorry, this context ({context}) is not implemented yet!");
                    }
                }
                #endregion
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write($"\n\nOps! My advanced AI capabilities couldn't understand what you want with {cmd}... are you sure you can type? you can always ask for 'help'");
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
            List<PlayFab.EconomyModels.CatalogItem> catalogItems = await catalogService.SearchItems();
            CatalogV2Service.PrintCatalogItems(catalogItems);
        }

        static async Task ListCloudScriptFunctions(string titleId, string titleDevSecret)
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

            CloudScriptService cloudScriptService = new CloudScriptService(titleSettings, authContext);
            var functions = await cloudScriptService.ListFunctions();
            CloudScriptService.PrintFunctions(functions, titleId);
        }

        static async Task ListServerTitleData(string titleId, string titleDevSecret, bool bInternalData = false)
        {
            var titleSettings = new PlayFabApiSettings
            {
                TitleId = titleId,
                DeveloperSecretKey = titleDevSecret
            };

            ServerTitleDataService serverTitleDataService = new ServerTitleDataService(titleSettings);
            GetTitleDataResult titleDataResult = bInternalData 
                ? await serverTitleDataService.GetTitleInternalData(null) 
                : await serverTitleDataService.GetTitleData(null);

            if (titleDataResult != null && titleDataResult.Data != null)
            {
                foreach (var kvp in titleDataResult.Data)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"\n\n{kvp.Key}: ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"{kvp.Value}");
                }
            }
        }

        static async Task ListMatchmakingQueueConfig(string titleId, string titleDevSecret)
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

            MatchmakingQueuesService matchmakingQueuesService = new MatchmakingQueuesService(titleSettings, authContext);

            var mmQueueConfigs = await matchmakingQueuesService.ListMatchmakingQueuesAsync();
            MatchmakingQueuesService.PrintMatchmakingQueuesConfig(mmQueueConfigs);
        }
    }
}
