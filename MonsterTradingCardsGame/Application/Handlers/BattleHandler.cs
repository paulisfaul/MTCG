using MonsterTradingCardsGame.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using System.Text.Json;
using MonsterTradingCardsGame.Models.RequestModels;
using MonsterTradingCardsGame.Repositories.Interfaces;

namespace MonsterTradingCardsGame.Application.Handlers
{
    public class BattleHandler: Handler
    {
        private readonly GameService _lobbyService;
        private readonly CardService _cardService;

        /// <summary>
        /// Initializes the used services and Handle-Functions/Methods
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="authenticationService"></param>
        /// <param name="cardService"></param>
        public BattleHandler(GameService lobbyService, CardService cardService, AuthenticationService authenticationService)
        {
            _lobbyService = lobbyService;
            _cardService = cardService;
            _finalRouteHandlerFunctions.Add("/", (HandleBattles, authenticationService.AuthorizePlayer));
            //_variableRoute = HandleSingleUserByUsername;
        }

        public async Task<bool> HandleBattles(HttpSvrEventArgs e, User user)
        
        {
            if (e.Method == "POST")
            {
                var result = await _cardService.GetDeckByUser(user);

                if (!result.Success)
                {
                    e.Reply(result.Code, result.Message);
                    return false;
                }

                user.Deck = result.Data;

                if (user.Deck.Count() > 4)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Zu viele Karten im Deck. Bitte rekonfigurieren Sie ihr Deck.");
                    return false;
                }

                var battle = await _lobbyService.StartGameAsync(user);

                e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(battle));
                return true;
            }

            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Methode nicht erlaubt.");
            return false;
        }
    }
}
