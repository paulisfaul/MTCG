using System;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Application.Configurations;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Repositories.Interfaces;
using Npgsql;
using NUnit.Framework;

namespace MonsterTradingCardsGame_nUnit
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private NpgsqlConnection _connection;
        private IUserRepository _userRepository;
        private List<Guid> _createdUserIds;

        [SetUp]
        public void SetUp()
        {
            _connection = new NpgsqlConnection(DatabaseConfig.ConnectionString);
            _userRepository = new UserRepository(_connection);
            _createdUserIds = new List<Guid>();
        }

        [TearDown]
        public async Task TearDown()
        {
            foreach (var userId in _createdUserIds)
            {
                await _userRepository.Delete(userId);
            }

            if (_connection.State == System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }

        private string GenerateUniqueUsername(string baseUsername)
        {
            return $"{baseUsername}_{Guid.NewGuid()}";
        }

        [Test]
        public async Task Create_ShouldReturnSuccess_WhenUserIsCreated()
        {
            // Arrange
            var uniqueUsername = GenerateUniqueUsername("testuser");
            var user = new User(uniqueUsername, RoleEnum.Admin);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("testuser");

            // Act
            var result = await _userRepository.Create(user, hashedPassword);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.CREATED, result.Code);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(user.Id, result.Data.Id);

            // Benutzer-ID zur Aufräumliste hinzufügen
            _createdUserIds.Add(user.Id);
        }

        [Test]
        public async Task GetAll_ShouldReturnAllUsers()
        {
            // Arrange
            var uniqueUsername = GenerateUniqueUsername("testuser_getall");
            var user = new User(uniqueUsername, RoleEnum.Admin);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("testuser");
            await _userRepository.Create(user, hashedPassword);

            // Benutzer-ID zur Aufräumliste hinzufügen
            _createdUserIds.Add(user.Id);

            // Act
            var result = await _userRepository.GetAll();

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.OK, result.Code);
            Assert.IsNotNull(result.Data);
            Assert.IsInstanceOf<IEnumerable<User>>(result.Data);
            Assert.IsTrue(result.Data.Any(u => u.Id == user.Id && u.UserCredentials.Username == user.UserCredentials.Username));
        }

        // Weitere Tests hier, die ebenfalls _createdUserIds hinzufügen

        [Test]
        public async Task GetById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var uniqueUsername = GenerateUniqueUsername("testuser_getbyid");
            var user = new User(uniqueUsername, RoleEnum.Admin);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("testuser");
            await _userRepository.Create(user, hashedPassword);

            // Benutzer-ID zur Aufräumliste hinzufügen
            _createdUserIds.Add(user.Id);

            // Act
            var result = await _userRepository.GetById(user.Id);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.OK, result.Code);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(user.Id, result.Data.Id);
        }

        [Test]
        public async Task GetByUsername_ShouldReturnUser_WhenUsernameExists()
        {
            // Arrange
            var uniqueUsername = GenerateUniqueUsername("testuser_getbyusername");
            var user = new User(uniqueUsername, RoleEnum.Admin);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("testuser");
            await _userRepository.Create(user, hashedPassword);

            // Benutzer-ID zur Aufräumliste hinzufügen
            _createdUserIds.Add(user.Id);

            // Act
            var result = await _userRepository.GetByUsername(uniqueUsername);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.OK, result.Code);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(uniqueUsername, result.Data.user.UserCredentials.Username);
        }

        [Test]
        public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _userRepository.GetById(userId);

            //Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(HttpStatusCode.NOT_FOUND, result.Code);
        }


        [Test]
        public async Task GetByUsername_ShouldReturnNotFound_WhenUsernameDoesNotExist()
        {
            //Arrange
            var username = "nonexistentuser";

            //Act
            var result = await _userRepository.GetByUsername(username);

            //Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(HttpStatusCode.NOT_FOUND, result.Code);
        }


        [Test]
        public async Task Update_ShouldReturnSuccess_WhenUserIsUpdated()
        {
            //Arrange
            var uniqueUsername = GenerateUniqueUsername("testuser");
            var user = new User(uniqueUsername, RoleEnum.Admin);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("testuser");
            await _userRepository.Create(user, hashedPassword);

            int updated_coins = 12;
            string updated_username = "updatedusername";
            user.Coins = updated_coins;
            user.SetUsername(updated_username);

            //Act
            var result = await _userRepository.Update(user);

            var result_getbyid = await _userRepository.GetById(user.Id);

            //Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.OK, result.Code);
            Assert.AreEqual(updated_coins, result_getbyid.Data.Coins);
            Assert.AreEqual(updated_username, result_getbyid.Data.UserCredentials.Username);
        }


        [Test]
        public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            //Arrange
            var user = new User("nonexistentuser", RoleEnum.Admin);

            user.SetUsername("updatedusername");

            //Act
            var result = await _userRepository.Update(user);

            //Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(HttpStatusCode.NOT_FOUND, result.Code);
        }

        [Test]
        public async Task Delete_ShouldReturnSuccess_WhenUserIsDeleted()
        {

            //Arrange
            var uniqueUsername = GenerateUniqueUsername("testuser");
            var user = new User(uniqueUsername, RoleEnum.Admin);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("testuser");
            await _userRepository.Create(user, hashedPassword);
            //Act
            var result = await _userRepository.Delete(user.Id);

            //Assert
            var result_getbyid = await _userRepository.GetById(user.Id);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(HttpStatusCode.OK, result.Code);
            Assert.IsFalse(result_getbyid.Success);
            Assert.AreEqual(HttpStatusCode.NOT_FOUND, result_getbyid.Code);
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            //Arrange
            var userId = Guid.NewGuid();

            //Act
            var result = await _userRepository.Delete(userId);

            //Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(HttpStatusCode.NOT_FOUND, result.Code);
        }

    }
}