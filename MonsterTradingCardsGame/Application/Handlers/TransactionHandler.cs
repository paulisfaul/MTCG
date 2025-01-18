using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Application.Handlers.Interfaces;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;

namespace MonsterTradingCardsGame.Application.Handlers
{
    /// <summary>This class implements a handler for transaction-specific requests.</summary>

    public class TransactionHandler : Handler, IHandler
    {
        private readonly PackageService _packageService;
        /// <summary>
        /// Initializes the used services and Handle-Functions/Methods
        /// </summary>
        /// <param name="packageService"></param>
        /// <param name="authenticationService"></param>
        public TransactionHandler(PackageService packageService, AuthenticationService authenticationService)
        {
            _packageService = packageService;
            _finalRouteHandlerFunctions.Add("/packages", (HandlePackageTransaction, authenticationService.AuthorizePlayer));
        }
        /// <summary>
        /// Handles an incoming HTTP request regarding the transaction of packages
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="user">Logged in User on the client. Used for querying.</param>
        /// <returns></returns>
        public async Task<bool> HandlePackageTransaction(HttpSvrEventArgs e, User user)
        {
            if (e.Method == "POST")
            {
                var result = await _packageService.AcquirePackage(user);
                e.Reply(result.Code, result.Message);
                return result.Success;
            }
            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Methode nicht erlaubt.");
            return false;
        }
    }
}
