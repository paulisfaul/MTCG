using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Helper.HttpServer;

namespace MonsterTradingCardsGame.Services
{
    public class CardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly UserService _userService;

        public CardService(ICardRepository cardRepository, UserService userService)
        {
            _cardRepository = cardRepository;
            _userService = userService;
        }

        public async Task<OperationResult<IEnumerable<Card>>> GetCardsByUser(User user)
        {
           return await _cardRepository.GetCardsByUserId(user.Id);
        }

        public async Task<OperationResult<Card>> GetCardById(Guid cardId)
        {
            return await _cardRepository.GetCardById(cardId);
        }

        public async Task<OperationResult<Deck>> GetDeckByUser(User user)
        {
            return await _cardRepository.GetDeckCardsByUserId(user.Id);
        }

        public async Task AddCard(Card card)
        {
            // Add any business logic or validation here if needed
            throw new NotImplementedException();
        }

        public async Task<OperationResult<bool>> TransferCardToUserFromPackage(Package package, User user)
        {
            if (package == null || user == null)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "Package and/or User was null.");
            }


            var result = await _cardRepository.Update(package, user);
            if (!result.Success)
            {
                return result;
            }
            user.Coins -= 5;

            return await _userService.UpdateUser(user);
        }

        public async Task<OperationResult<bool>> TransferCardToUser(Card card, Guid userId)
        {
            if (card == null || userId == Guid.Empty)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "Card and/or User was null.");
            }

            return await _cardRepository.Update(card, userId);



        }

        public async Task<OperationResult<bool>> ConfigureDeck(List<Guid> cardIds, User user, bool fromBattle = false)
        {
            if (!cardIds.Any() || user == null)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "CardIds dürfen nicht leer und User darf nicht null sein.");
            }
            // Überprüfen, ob die Karten-IDs dem Benutzer gehören
            if (!fromBattle)
            {
                foreach (var cardId in cardIds)
                {
                    if ((await CheckOwnershipOfCard(cardId)).Data != user.Id)
                    {
                        return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST,
                            "Card gehört nicht dem Benutzer.");
                    }
                }
            }

            // Speichere die Karten die aktuell im Deck des Benutzers sind
            var result = await _cardRepository.GetDeckCardsByUserId(user.Id);
            Deck currentDeck = result.Data;
            IEnumerable<Guid> currentCardIdsInDeck = currentDeck?.Select(card => card.Id);

            try
            {
                // Konfiguriere das neue Deck
                var resultConfigure = (await _cardRepository.ConfigureDeck(cardIds, user.Id));
                if (!resultConfigure.Success)
                {
                    if (currentCardIdsInDeck.Any())
                    {
                        // Wiederherstellen des alten Decks im Fehlerfall
                        await _cardRepository.ConfigureDeck(currentCardIdsInDeck, user.Id);
                    }
                    return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "Deck konnte nicht konfiguriert werden.");
                }

                return resultConfigure;
            }
            catch (Exception)
            {
                if (currentCardIdsInDeck.Any())
                {
                    // Wiederherstellen des alten Decks im Fehlerfall
                    await _cardRepository.ConfigureDeck(currentCardIdsInDeck, user.Id);
                }
                return new OperationResult<bool>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, "Deck konnte nicht konfiguriert werden.");
            }
        }

        public async Task<OperationResult<Guid>> CheckOwnershipOfCard(Guid cardId)
        { 
            return await _cardRepository.GetOwnerIdOfCardById(cardId);
        }

        public async Task<OperationResult<bool>> CreateDeck(User user)
        {
            Deck deck = new Deck(user.Id);
            var result = _cardRepository.CreateDeck(deck, user.Id).Result;

            if (result.Success)
            {
                return new OperationResult<bool>(true, HttpStatusCode.CREATED, "Deck erstellt.");

            }
            return new OperationResult<bool>(false, result.Code, result.Message);

        }
    }
}