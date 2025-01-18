using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Repositories.Interfaces
{
    public interface ITradingRepository
    {
        //CREATE
        Task<OperationResult<TradingOffer>> Create(TradingOffer tradingOffer);
        Task<OperationResult<IEnumerable<TradingOffer>>> GetOpenTradingOffers(User user);
        Task<OperationResult<TradingOffer>> GetTradingOfferById(Guid tradingOfferId);
        Task<OperationResult<bool>> Update(TradingOffer offer);
        Task<OperationResult<bool>> Delete(Guid tradingId);
        Task<OperationResult<Guid>> GetOwnerIdOfTradingById(Guid tradingId);
    }
}
