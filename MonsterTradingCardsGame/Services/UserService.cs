using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories.Interfaces;
using MonsterTradingCardsGame.Helper.HttpServer;


namespace MonsterTradingCardsGame.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<OperationResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _userRepository.GetAll();
        }

        public async Task<OperationResult<User?>> GetUserById(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new OperationResult<User?>(false, HttpStatusCode.BAD_REQUEST, "User ID cannot be empty.");
            }

            return await _userRepository.GetById(userId);
        }

        public async Task<OperationResult<Scoreboard>> GetScoreboard()
        {
            var result = await _userRepository.GetHighestElo(10);

            if (!result.Success)
            {
                return new OperationResult<Scoreboard>(false, result.Code, result.Message);
            }

            IEnumerable<User> users = result.Data;
            var scoreboardEntries = new List<ScoreboardEntry>();

            foreach (var user in users)
            {
                var entry = new ScoreboardEntry(user.UserCredentials.Username, user.UserStats);
                scoreboardEntries.Add(entry);
            }

            var scoreboard = new Scoreboard(scoreboardEntries);
            return new OperationResult<Scoreboard>(true, HttpStatusCode.OK, null, scoreboard);
        }

        public async Task<OperationResult<ScoreboardEntry>> GetStats(User u)
        {
            var result = await _userRepository.GetById(u.Id);

            if (!result.Success)
            {
                return new OperationResult<ScoreboardEntry>(false, result.Code, result.Message);
            }

            User user = result.Data;

            var stats = new ScoreboardEntry(user.UserCredentials.Username, user.UserStats);

            return new OperationResult<ScoreboardEntry>(true, HttpStatusCode.OK, null, stats);
        }

        public async Task<OperationResult<User?>> GetUserByUsername(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new OperationResult<User?>(false, HttpStatusCode.BAD_REQUEST, "Username cannot be empty.");
            }

            var result = await _userRepository.GetByUsername(name);
            return new OperationResult<User?>(result.Success, result.Code, result.Message, result.Data.user);
        }

        public async Task<OperationResult<bool>> UpdateUser(User user)
        {
            if (user.Id == Guid.Empty)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "User ID cannot be empty.");
            }

            return await _userRepository.Update(user);
        }

        public async Task<OperationResult<bool>> DeleteUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "User ID cannot be empty.");
            }

            return await _userRepository.Delete(userId);
        }
    }
}