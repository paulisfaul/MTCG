using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Repositories.Interfaces
{
    public interface IPackageRepository
    {
        //CREATE
        Task<OperationResult<Package>> Create(Package package);
        //READ
        Task<OperationResult<Package>> GetRandom();
        Task<OperationResult<IEnumerable<Package>>> GetAll();

        Task<OperationResult<Package>> GetByUserId(Guid userId);

        Task<OperationResult<bool>> Update(Package package);
        Task<OperationResult<bool>> Delete(Package package);
    }
}
