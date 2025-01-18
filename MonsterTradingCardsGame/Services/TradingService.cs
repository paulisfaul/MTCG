using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Repositories.Interfaces;
using HttpStatusCode = MonsterTradingCardsGame.Helper.HttpServer.HttpStatusCode;

namespace MonsterTradingCardsGame.Services
{
    public class TradingService
    {
        private readonly ITradingRepository _tradingRepository;
        private readonly CardService _cardService;

        public TradingService(ITradingRepository tradingRepository, CardService cardService)
        {
            _tradingRepository = tradingRepository;
            _cardService = cardService;
        }

        public async Task<OperationResult<bool>> CreateTradingOffer(TradingOffer tradingOffer, User user)
        {
            if (tradingOffer == null)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST,
                    "TradingOffer cannot be null.");
            }

            //UNIQUE FEATURE: if trading offer has automatic accept true, checks if there is a trading offer with required

            if (tradingOffer.AutomaticAccept)
            {


                var result_get_all = await GetOpenTradingOffers(user);

                if (!result_get_all.Success)
                {
                    return new OperationResult<bool>(false, result_get_all.Code, result_get_all.Message);
                }

                IEnumerable<TradingOffer> offers = result_get_all.Data;


                var result_off_card = await _cardService.GetCardById(tradingOffer.OfferedCardId);

                if (!result_off_card.Success)
                {
                    return new OperationResult<bool>(false, result_off_card.Code, result_off_card.Message);
                }

                Card offeredCard = result_off_card.Data;

                //Iterate over all offers
                foreach (var otherOffer in offers)
                {
                    //check if your own offered Card meets the requirments of the offer
                    if ((otherOffer.RequestedCardTypeEnum == CardTypeEnum.Monster && offeredCard is not MonsterCard) ||
                        (otherOffer.RequestedCardTypeEnum == CardTypeEnum.Spell && offeredCard is not SpellCard))
                    {
                        continue;
                    }

                    if (otherOffer.RequestedMinimumDamage > offeredCard.Damage)
                    {
                        continue;
                    }
                    //if it meets the requirements, check if the other offered card meets your own requirements

                    var result_off_card_other = await _cardService.GetCardById(otherOffer.OfferedCardId);

                    if (!result_off_card_other.Success)
                    {
                        return new OperationResult<bool>(false, result_off_card_other.Code,
                            result_off_card_other.Message);
                    }

                    Card otherCard = result_off_card_other.Data;

                    if ((tradingOffer.RequestedCardTypeEnum == CardTypeEnum.Monster && otherCard is not MonsterCard) ||
                        (tradingOffer.RequestedCardTypeEnum == CardTypeEnum.Spell && otherCard is not SpellCard))
                    {
                        continue;
                    }

                    if (tradingOffer.RequestedMinimumDamage > otherCard.Damage)
                    {
                        continue;
                    }

                    //if it is the case, accept the trading offer;
                    var result_accept_trading = await AcceptTrading(user, otherOffer.Id, tradingOffer.OfferedCardId);

                    if (!result_accept_trading.Success)
                    {
                        return new OperationResult<bool>(false, result_off_card_other.Code,
                            result_off_card_other.Message);
                    }

                    return new OperationResult<bool>(true, HttpStatusCode.OK, "Trading offer automatically accepted.");

                }
            }

            //if no trading was found, create a trading

            var result_creation = await _tradingRepository.Create(tradingOffer);

            if (!result_creation.Success)
            {
                return new OperationResult<bool>(false, result_creation.Code, result_creation.Message);
            }

            return new OperationResult<bool>(true, HttpStatusCode.CREATED, "Trading offer was created.");
        }

        public async Task<OperationResult<IEnumerable<TradingOffer>>> GetOpenTradingOffers(User user)
        {
            return await _tradingRepository.GetOpenTradingOffers(user);
        }

        public async Task<OperationResult<bool>> AcceptTrading(User user, Guid tradingId, Guid cardId)
        {
            Guid requestedCardId = cardId;

            //first check if card belongs to the user
            var owner_id = (await _cardService.CheckOwnershipOfCard(cardId)).Data;
            if (owner_id != user.Id)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST,
                    "Offered card does not belong to user.");
            }

            var result_req_card = await _cardService.GetCardById(requestedCardId);

            if (!result_req_card.Success)
            {
                return new OperationResult<bool>(false, result_req_card.Code, result_req_card.Message);
            }

            Card requestedCard = result_req_card.Data;


            var result_trading_offer = await _tradingRepository.GetTradingOfferById(tradingId);

            if (!result_trading_offer.Success)
            {
                return new OperationResult<bool>(false, result_trading_offer.Code, result_trading_offer.Message);
            }

            TradingOffer offer = result_trading_offer.Data;

            if (offer.Open != true || offer.RequestedCardId != null)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "Trading Offer is already closed.");
            }


            // Überprüfen, ob die requestedCard den gleichen Typ wie offer.RequestedCardTypeEnum hat
            if ((offer.RequestedCardTypeEnum == CardTypeEnum.Monster && requestedCard is not MonsterCard) ||
                (offer.RequestedCardTypeEnum == CardTypeEnum.Spell && requestedCard is not SpellCard))
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST,
                    "Requested card type does not match the offer.");
            }

            if (offer.RequestedMinimumDamage > requestedCard.Damage)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST,
                    "Requested card has too low damage.");
            }

            Guid receiverOfRequestedCard = offer.Offerer;
            Guid receiverOfOfferedCard = user.Id;

            Guid offeredCardId = offer.OfferedCardId;

            var result_off_card = await _cardService.GetCardById(offeredCardId);

            if (!result_off_card.Success)
            {
                return new OperationResult<bool>(false, result_off_card.Code, result_off_card.Message);
            }

            Card offeredCard = result_off_card.Data;


            var result_transfer_1 = await _cardService.TransferCardToUser(offeredCard, receiverOfOfferedCard);
            var result_transfer_2 = await _cardService.TransferCardToUser(requestedCard, receiverOfRequestedCard);

            offer.RequestedCardId = requestedCardId;
            offer.Open = false;

            var result_update_tradingoffer = await _tradingRepository.Update(offer);

            if (!result_transfer_1.Success || !result_transfer_2.Success || !result_update_tradingoffer.Success)
            {
                return new OperationResult<bool>(false, HttpStatusCode.INTERNAL_SERVER_ERROR,
                    "There was an error. Need administrative assistance.");
            }

            return new OperationResult<bool>(true, HttpStatusCode.OK, "Trading was successful.");

        }

        public async Task<OperationResult<bool>> Delete(Guid tradingId, User user)
        {
            if ((await CheckOwnershipOfTrading(tradingId)).Data != user.Id)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST,
                    "Trading doesn't belong to the user.");
            }

            return await _tradingRepository.Delete(tradingId);
        }

        public async Task<OperationResult<Guid>> CheckOwnershipOfTrading(Guid tradingId)
        {
            return await _tradingRepository.GetOwnerIdOfTradingById(tradingId);

        }
    }
}