using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Repositories.Interfaces
{
    public interface IDeckRepository
    {
        //CREATE
        Task<OperationResult<Deck>> Create(Deck deck);
        //READ
        Task<OperationResult<IEnumerable<Deck>>> GetAll();
        //UPDATE
        Task<OperationResult<bool>> Update(Deck deck);
        //DELETE
        Task<OperationResult<bool>> Delete(Guid deckId);

    }
}