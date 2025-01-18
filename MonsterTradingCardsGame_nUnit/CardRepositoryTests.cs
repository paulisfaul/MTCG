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
    internal class CardRepositoryTests
    {
        private NpgsqlConnection _connection;
        private ICardRepository _cardRepository;
        private IUserRepository _userRepository;
        private IPackageRepository _packageRepository;
        private List<Guid> _createdCardIds;

        [SetUp]
        public void SetUp()
        {
            _connection = new NpgsqlConnection(DatabaseConfig.ConnectionString);
            _cardRepository = new CardRepository(_connection);
            _userRepository = new UserRepository(_connection);
            _packageRepository = new PackageRepository(_connection);
            _createdCardIds = new List<Guid>();
        }

        [TearDown]
        public async Task TearDown()
        {

            if (_connection.State == System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }

        private async Task<User> CreateUser()
        {
            //Arrange
            var uniqueUsername = GenerateUniqueUsername("testuser");

            var user = new User(uniqueUsername, RoleEnum.Admin);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("testuser");

            // Act
            var result_user = await _userRepository.Create(user, hashedPassword);

            return result_user.Data;
        }

        private async Task<Package> CreatePackage()
        {
            Guid guid = Guid.NewGuid();
            var package = new Package(Guid.NewGuid());
            package.AddCard(new MonsterCard("Test_Card1"+ guid, 1, ElementTypeEnum.Fire));
            package.AddCard(new MonsterCard("Test_Card2" + guid, 2, ElementTypeEnum.Water));
            package.AddCard(new MonsterCard("Test_Card3" + guid, 3, ElementTypeEnum.Fire));
            package.AddCard(new MonsterCard("Test_Card4" + guid, 4, ElementTypeEnum.Normal));
            package.AddCard(new MonsterCard("Test_Card5"+ guid, 5, ElementTypeEnum.Fire));

            var result_package = await _packageRepository.Create(package);

            var result_cards = await _cardRepository.CreateCardsForPackage(package);

            return result_package.Data;

        }

        private string GenerateUniqueUsername(string baseUsername)
        {
            return $"{baseUsername}_{Guid.NewGuid()}";
        }

        [Test]
        public async Task Create_ShouldReturnSuccess_WhenMonsterCardIsCreated()
        {
            // Arrange

            var card = new MonsterCard("TestMonsterCard", 10, ElementTypeEnum.Fire);

            // Act
            var result = await _cardRepository.Create(card);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.CREATED, result.Code);
            Assert.AreEqual(card, result.Data);
            Assert.AreEqual(card.Id, result.Data.Id);
        }

        [Test]
        public async Task Create_ShouldReturnSuccess_WhenSpellCardIsCreated()
        {
            // Arrange

            var card = new SpellCard("TestSpellCard", 10, ElementTypeEnum.Fire);

            // Act
            var result = await _cardRepository.Create(card);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.CREATED, result.Code);
            Assert.AreEqual(card, result.Data);
            Assert.AreEqual(card.Id, result.Data.Id);
        }


        [Test]
        public void Create_ShouldThrowArgumentNullException_WhenCardIsNull()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await _cardRepository.Create(null));
            Assert.AreEqual("card", ex.ParamName);
        }

        [Test]
        public async Task GetCardsByUserId_ShouldReturnCards_WhenUserIdIsValid()
        {
            //Arrange
            User user = await CreateUser();
            Package package = await CreatePackage();

            //Act
            var result_update = await _cardRepository.Update(package, user);


            //Act
            var result = await _cardRepository.GetCardsByUserId(user.Id);

            //Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.OK, result.Code);
            Assert.IsTrue(result.Data.Count() >= 5);
        }

        [Test]
        public async Task GetCardsByUserId_ShouldReturnBadRequest_WhenUserIdIsEmpty()
        { 
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await _cardRepository.GetCardsByUserId(Guid.Empty));
            Assert.AreEqual("userId", ex.ParamName);
        }

        [Test]
        public async Task GetOwnerIdOfCardById_ShouldReturnOwnerId_WhenCardIdIsValid()
        {

            //Arrange
            User user = await CreateUser();
            Package package = await CreatePackage();

            //Act
            var result_update = await _cardRepository.Update(package, user);
            Card card = package.FirstOrDefault();

            if (card == null)
            {
                Assert.Fail("No cards found in the package.");
            }

            //Act
            var result = await _cardRepository.GetOwnerIdOfCardById(card.Id);

            //Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.OK, result.Code);
            Assert.AreEqual(user.Id, result.Data);
        }

        [Test]
        public async Task GetOwnerIdOfCardById_ShouldReturnBadRequest_WhenCardIdIsEmpty()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await _cardRepository.GetOwnerIdOfCardById(Guid.Empty));
            Assert.AreEqual("cardId", ex.ParamName);
        }

        [Test]
        public async Task Update_ShouldReturnSuccess_WhenPackageAndUserAreValid()
        {

            User user = await CreateUser();
            Package package = await CreatePackage();

            //Act
            var result = await _cardRepository.Update(package, user);

            //Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.OK, result.Code);
            Assert.IsTrue(result.Data);
        }

        [Test]
        public async Task Update_ShouldReturnBadRequest_WhenPackageOrUserIsNull()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await _cardRepository.Update(null,null));
            Assert.AreEqual("package", ex.ParamName);


        }
    }
}