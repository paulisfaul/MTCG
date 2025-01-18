using MonsterTradingCardsGame.Application.Configurations;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Repositories.Interfaces;
using Npgsql;
using MonsterTradingCardsGame.Application.Configurations;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Repositories.Interfaces;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MonsterTradingCardsGame_nUnit
{
    [TestFixture]
    internal class TradingRepositoryTests
    {
        private NpgsqlConnection _connection;
        private ITradingRepository _tradingRepository;

        [SetUp]
        public void SetUp()
        {
            _connection = new NpgsqlConnection(DatabaseConfig.ConnectionString);
            _tradingRepository = new TradingRepository(_connection);
        }

        [TearDown]
        public async Task TearDown()
        {

            if (_connection.State == System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }

        [Test]
        public async Task Create_ShouldReturnError_WhenGuidDoesntExist()
        {
            // Arrange

            var tradingOffer = new TradingOffer(Guid.NewGuid(), Guid.NewGuid(), CardTypeEnum.Monster, 10, true);

            // Act
            var result = await _tradingRepository.Create(tradingOffer);

            // Assert
            Assert.IsTrue(!result.Success);
            Assert.AreEqual(HttpStatusCode.INTERNAL_SERVER_ERROR, result.Code);
        }
    }
}