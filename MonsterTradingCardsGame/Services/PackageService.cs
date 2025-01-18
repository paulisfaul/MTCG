using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories.Interfaces;
using HttpStatusCode = MonsterTradingCardsGame.Helper.HttpServer.HttpStatusCode;

namespace MonsterTradingCardsGame.Services
{
    public class PackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly ICardRepository _cardRepository;
        private readonly CardService _cardService;
        private readonly UserService _userService;

        public PackageService(IPackageRepository packageRepository, ICardRepository cardRepository, CardService cardService, UserService userService)
        {
            _packageRepository = packageRepository;
            _cardRepository = cardRepository;
            _cardService = cardService;
            _userService = userService;
        }

        public async Task<OperationResult<bool>> CreatePackage(Package package)
        {
            var result = await _packageRepository.Create(package);
            if (!result.Success)
            {
                return new OperationResult<bool>(false, result.Code, result.Message);
            }

            var result2 = await _cardRepository.CreateCardsForPackage(package);

            return new OperationResult<bool>(true, HttpStatusCode.CREATED, "Package and Cards created.");
        }

        public async Task<OperationResult<bool>> AcquirePackage(User user)
        {
            //check if User has enough coins

            int coins = _userService.GetUserById(user.Id).Result.Data.Coins;

            if (coins < 5)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "User doesn't have enough coins.");
            }

            //select a random package
            var result = await _packageRepository.GetRandom();

            if (!result.Success)
            {
                return new OperationResult<bool>(false, result.Code, result.Message);
            }

            var result2 = await _cardService.TransferCardToUserFromPackage(result.Data, user);

            if (!result2.Success)
            {
                return new OperationResult<bool>(false, result2.Code, result2.Message);
            }
            //Delete Package
            var result3 = await _packageRepository.Delete( result.Data);

            if (!result3.Success)
            {
                return new OperationResult<bool>(false, result3.Code, result3.Message);
            }

            return new OperationResult<bool>(true, HttpStatusCode.OK, "Package acquired.");
        }

        //public async Task<IEnumerable<User>> GetAllCardsInPackage()
        //{
        //    return await _cardRepository.GetAllCardsByPackageId();
        //}
    }
}