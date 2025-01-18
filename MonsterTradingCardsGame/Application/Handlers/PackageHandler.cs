using System;
using System.Text.Json;
using MonsterTradingCardsGame.Application.Handlers.Interfaces;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Models.RequestModels;
using MonsterTradingCardsGame.Services;

namespace MonsterTradingCardsGame.Application.Handlers
{
    /// <summary>This class implements a handler for package-specific requests.</summary>
    public class PackageHandler : Handler, IHandler
    {
        private readonly PackageService _packageService;
        private readonly AuthenticationService _authenticationService;

        /// <summary>
        /// Initializes the used services and Handle-Functions/Methods
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="authenticationService"></param>
        public PackageHandler(PackageService packageService, AuthenticationService authenticationService)
        {
            _packageService = packageService;
            _authenticationService = authenticationService;
            _finalRouteHandlerFunctions.Add("/", (HandlePackages, authenticationService.AuthorizeAdmin));
        }

        /// <summary>
        /// Handles an incoming HTTP request regarding all packages.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="user">Logged in User on the client. Used for querying.</param>
        /// <returns></returns>
        public async Task<bool> HandlePackages(HttpSvrEventArgs e, User user)
        {
            if (e.Method == "POST")
            {
                try
                {
                    var packageCreateRequest = JsonSerializer.Deserialize<PackageRequestDto>(e.Payload);
                    if (packageCreateRequest != null)
                    {

                        var package = new Package(packageCreateRequest);

                        var result = await _packageService.CreatePackage(package);
                        e.Reply(result.Code, result.Message);
                        return result.Success;
                    }

                    e.Reply(HttpStatusCode.BAD_REQUEST, "Ungültige Packagedaten.");
                    return false;
                }
                catch (JsonException)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Ungültiges JSON-Format.");
                    return false;
                }
            }

            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Methode nicht erlaubt.");
            return false;
        }

       
    }
}