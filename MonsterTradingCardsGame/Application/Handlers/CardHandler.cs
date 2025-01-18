using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;

namespace MonsterTradingCardsGame.Application.Handlers
{
    /// <summary>This class implements a handler for card-specific requests.</summary>
    public class CardHandler: Handler
    {
        private readonly CardService _cardService;

        /// <summary>
        /// Initializes the used services and Handle-Functions/Methods
        /// </summary>
        /// <param name="cardService"></param>
        /// <param name="authenticationService"></param>
        public CardHandler(CardService cardService, AuthenticationService authenticationService)
        {
            _cardService = cardService;
            _finalRouteHandlerFunctions.Add("/", (HandleCards, authenticationService.AuthorizePlayer));
        }

        /// <summary>
        /// Handles an incoming HTTP request regarding all users.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="user">Logged in User on the client. Used for querying.</param>
        /// <returns></returns>

        public async Task<bool> HandleCards(HttpSvrEventArgs e, User user)
        {
            if (e.Method == "GET")
            {
                var cards = await _cardService.GetCardsByUser(user);
                e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(cards));
                return true;
            }
            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Methode nicht erlaubt.");
            return false;
        }
    }
}
