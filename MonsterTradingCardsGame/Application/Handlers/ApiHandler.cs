using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Application.Handlers.Interfaces;
using MonsterTradingCardsGame.Helper.HttpServer;

namespace MonsterTradingCardsGame.Application.Handlers
{
    public class ApiHandler: Handler, IHandler
    {
        /// <summary>
        /// Adds the different handler classes to the unfinalRouteHandlers Dictionary
        /// </summary>
        /// <param name="userHandler"></param>
        /// <param name="authenticationHandler"></param>
        /// <param name="packageHandler"></param>
        /// <param name="transactionHandler"></param>
        /// <param name="cardHandler"></param>
        /// <param name="deckHandler"></param>
        public ApiHandler(UserHandler userHandler, AuthenticationHandler authenticationHandler, PackageHandler packageHandler, TransactionHandler transactionHandler, CardHandler cardHandler, DeckHandler deckHandler, BattleHandler battleHandler, ScoreboardHandler scoreboardHandler, StatsHandler statsHandler,TradingHandler tradingHandler)
        {
            _unfinalRouteHandlers.Add("/users", userHandler);
            _unfinalRouteHandlers.Add("/auth", authenticationHandler);
            _unfinalRouteHandlers.Add("/packages", packageHandler);
            _unfinalRouteHandlers.Add("/transactions", transactionHandler);
            _unfinalRouteHandlers.Add("/cards", cardHandler);
            _unfinalRouteHandlers.Add("/deck", deckHandler);
            _unfinalRouteHandlers.Add("/battle", battleHandler);
            _unfinalRouteHandlers.Add("/scoreboard", scoreboardHandler);
            _unfinalRouteHandlers.Add("/stats", statsHandler);
            _unfinalRouteHandlers.Add("/tradings", tradingHandler);
        }
    }
}
