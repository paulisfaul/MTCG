using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Repositories.Interfaces
{
    public interface IUserRepository
    {
        //CREATE
        Task<OperationResult<User>> Create(User user, string hashedPassword);
        //READ
        Task<OperationResult<IEnumerable<User>>> GetAll();

        Task<OperationResult<User>> GetById(Guid id);
        //Task<User> GetByName(string name);
        Task<OperationResult<(User user, string hashedPassword)>> GetByUsername(string username);
        //UPDATE
        Task<OperationResult<bool>> Update(User user);

        //DELETE
        Task<OperationResult<bool>> Delete(Guid userId);

        Task<OperationResult<IEnumerable<User>>> GetHighestElo(int top);
    }
}
    