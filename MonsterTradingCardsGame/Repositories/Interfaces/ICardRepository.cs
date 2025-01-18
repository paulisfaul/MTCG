using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Repositories.Interfaces
{
    public interface ICardRepository
    {
        // CREATE
        Task<OperationResult<Deck>> CreateDeck(Deck deck , Guid userId);
        Task<OperationResult<IEnumerable<Card>>> CreateCardsForPackage(Package package);
        Task<OperationResult<Card>> Create(Card card, Guid? packageId = null);

        // READ
        Task<OperationResult<IEnumerable<Card>>> GetCardsByUserId(Guid userId);
        Task<OperationResult<Guid>> GetOwnerIdOfCardById(Guid cardId);
        Task<OperationResult<Deck>> GetDeckCardsByUserId(Guid userId);
        Task<OperationResult<Card>> GetCardById(Guid cardId);


        // UPDATE
        Task<OperationResult<bool>> Update(Package package, User user);
        Task<OperationResult<bool>> ConfigureDeck(IEnumerable<Guid> cardIds, Guid userId);

        Task<OperationResult<bool>> Update(Card card, Guid userId);

        // DELETE
    }
}