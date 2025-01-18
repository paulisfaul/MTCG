using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Models.RequestModels;
using MonsterTradingCardsGame.Services;

namespace MonsterTradingCardsGame.Application.Handlers
{
    /// <summary>This class implements a handler for deck-specific requests.</summary>

    public class DeckHandler: Handler
    {
        private readonly CardService _cardService;

        /// <summary>
        /// Initializes the used services and Handle-Functions/Methods
        ///</summary>
        /// <param name="cardService"></param>
        /// <param name="authenticationService"></param>
        public DeckHandler(CardService cardService, AuthenticationService authenticationService)
        {
            _cardService = cardService;
            _finalRouteHandlerFunctions.Add("/", (HandleDeck, authenticationService.AuthorizePlayer));
        }
        /// <summary>
        /// Handles an incoming HTTP request regarding all users.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="user">Logged in User on the client. Used for querying.</param>
        /// <returns></returns>
        public async Task<bool> HandleDeck(HttpSvrEventArgs e, User user)
        {
            if (e.Method == "GET")
            {
                var result = await _cardService.GetDeckByUser(user);
                if (result.Success)
                {
                    e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(result.Data));
                    return true;
                }
                e.Reply(result.Code, result.Message);
                return false;
            }
            if (e.Method == "PUT")
            {
                try
                {
                    var deckConfigureRequest = JsonSerializer.Deserialize<DeckRequestDto>(e.Payload);
                    if (deckConfigureRequest != null)
                    {
                        List<Guid> cardIds = deckConfigureRequest.Select(card => card.CardId).ToList();


                        if (cardIds.Count != 4)
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Es müssen genau 4 CardIds angegeben werden.");
                            return false;
                        }

                        if (cardIds.Distinct().Count() != cardIds.Count)
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "CardIds dürfen keine Duplikate enthalten.");
                            return false;
                        }
                         var result = await _cardService.ConfigureDeck(cardIds, user);
                         e.Reply(result.Code, result.Message);
                         return result.Success;

                    }
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Ungültige CardIds.");
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
