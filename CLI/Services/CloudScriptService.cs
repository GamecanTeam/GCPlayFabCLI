using PlayFab;
using PlayFab.CloudScriptModels;

namespace ProductMigration.Services
{
    class GCCloudScriptFunction
    {
        public string FunctionName;
        public string ConnectionString;
        public string FunctionUrl;
        public string QueueName;
        public string TriggerType;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(TriggerType) && !string.IsNullOrEmpty(FunctionName);
        }
        public bool IsHttp() 
        {
            return IsValid() && TriggerType == "HTTP";
        }
        public bool IsQueued()
        {
            return IsValid() && TriggerType == "Queue";
        }

        
    }

    class CloudScriptService
    {
        private PlayFabCloudScriptInstanceAPI _playFabCloudScriptInstanceApi;
        private PlayFabApiSettings _currentSetting;

        public CloudScriptService(PlayFabApiSettings settings, PlayFabAuthenticationContext authContext)
        {
            _currentSetting = settings;

            _playFabCloudScriptInstanceApi = new PlayFabCloudScriptInstanceAPI(settings, authContext);
        }

        public string GetTitleId()
        {
            return _currentSetting.TitleId;
        }

        public async Task<List<FunctionModel>> ListFunctions()
        {
            var request = new ListFunctionsRequest();

            var response = await _playFabCloudScriptInstanceApi.ListFunctionsAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to list functions for title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                return null;
            }

            return response.Result.Functions;
        }

        public async Task<GetFunctionResult> GetFunction(string functionName)
        {
            var request = new GetFunctionRequest 
            { 
                FunctionName = functionName
            };

            var response = await _playFabCloudScriptInstanceApi.GetFunctionAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to list functions for title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                return null;
            }

            return response.Result;
        }

        public async Task<bool> UnregisterFunction(string functionName)
        {
            var request = new UnregisterFunctionRequest
            {
                FunctionName = functionName
            };

            var response = await _playFabCloudScriptInstanceApi.UnregisterFunctionAsync(request);

            if (response != null && response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to unregister function {functionName} for title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                return false;
            }

            return true;
        }

        public async Task<bool> RegisterHttpFunction(string functionName, string functionUrl)
        {
            var request = new RegisterHttpFunctionRequest
            {
                FunctionName = functionName,
                FunctionUrl = functionUrl
            };

            var response = await _playFabCloudScriptInstanceApi.RegisterHttpFunctionAsync(request);

            if (response != null && response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed register function {functionName} for title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                return false;
            }

            return true;
        }

        public async Task<bool> RegisterQueuedFunction(string functionName, string queueName, string connectionString)
        {
            var request = new RegisterQueuedFunctionRequest
            {
                FunctionName = functionName,
                QueueName = queueName,
                ConnectionString = connectionString
            };

            var response = await _playFabCloudScriptInstanceApi.RegisterQueuedFunctionAsync(request);

            if (response != null && response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed register function {functionName} for title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                return false;
            }

            return true;
        }

        public async Task<List<GetFunctionResult>> GetHttpFunctions(List<FunctionModel> functions)
        {
            List <GetFunctionResult> httpFunctions = new List<GetFunctionResult> ();

            foreach (var function in functions)
            {
                if (function.TriggerType == "HTTP")
                {
                    var func = await GetFunction(function.FunctionName);
                    if (func != null)
                    {
                        httpFunctions.Add(func);
                    }
                }
            }

            return httpFunctions;
        }

        public async Task<List<GetFunctionResult>> GetQueuedFunctions(List<FunctionModel> functions)
        {
            List<GetFunctionResult> httpFunctions = new List<GetFunctionResult>();

            foreach (var function in functions)
            {
                if (function.TriggerType == "Queue")
                {
                    var func = await GetFunction(function.FunctionName);
                    if (func != null)
                    {
                        httpFunctions.Add(func);
                    }
                }
            }

            return httpFunctions;
        }

        public async Task<List<GCCloudScriptFunction>> GetFunctions(List<FunctionModel> functions)
        {
            List<GCCloudScriptFunction> httpFunctions = new List<GCCloudScriptFunction>();

            foreach (var function in functions)
            {
                var func = await GetFunction(function.FunctionName);

                if (func != null)
                {
                    httpFunctions.Add(new GCCloudScriptFunction 
                    { 
                        FunctionName = function.FunctionName,
                        ConnectionString = func.ConnectionString,
                        FunctionUrl = func.FunctionUrl,
                        QueueName = func.QueueName,
                        TriggerType = func.TriggerType
                    });
                }
            }

            return httpFunctions;
        }

        public async Task DeleteFunctions(List<FunctionModel> functions)
        {
            foreach (var function in functions)
            {
                bool bResult = await UnregisterFunction(function.FunctionName);

                if (bResult)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"\nFunction {function.FunctionName} deleted successfully.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"\nFailed to delete function {function.FunctionName} ({function.TriggerType}) for title {GetTitleId()}. Check the logs for more info.");
                }
            }
        }

        public async Task CreateFunctions(List<GCCloudScriptFunction> functions)
        {
            foreach (var function in functions)
            {
                bool bResult = false;
                if (function.IsHttp())
                {
                    bResult = await RegisterHttpFunction(function.FunctionName, function.FunctionUrl);
                }
                else if (function.IsQueued())
                {
                    bResult = await RegisterQueuedFunction(function.FunctionName, function.QueueName, function.ConnectionString);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nFailed to create function {function.FunctionName} ({function.TriggerType}). Type not recognized as a valid type.");
                }

                // log result
                if (bResult)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"\nFunction {function.FunctionName} created successfully.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"\nFailed to create function {function.FunctionName} ({function.TriggerType}). Check the logs for more info.");
                }
            }
        }

        public static void PrintFunctions(List<FunctionModel> functions, string titleId)
        {
            if (functions == null || functions.Count == 0)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\n{functions.Count} Functions in title {titleId}");

            foreach (var func in functions)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\nFunction {func.FunctionName}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\n\tType {func.TriggerType} - Address: {func.FunctionAddress}");
            }
        }

        public static void PrintFunctions(List<GCCloudScriptFunction> functions, string titleId)
        {
            if (functions == null || functions.Count == 0)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\n{functions.Count} Functions in title {titleId}");

            foreach (var func in functions)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\nFunction {func.FunctionName} - Type: {func.TriggerType}");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                if (func.IsHttp())
                {
                    Console.WriteLine($"\n\tFunctionUrl: {func.FunctionUrl}");
                }
                else if (func.IsQueued())
                {
                    Console.WriteLine($"\n\tConnectionString: {func.ConnectionString}");
                    Console.WriteLine($"\n\tQueueName: {func.QueueName}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nFunction {func.FunctionName} with type {func.TriggerType} not recognized as a valid type.");
                }
            }
        }
    }
}
