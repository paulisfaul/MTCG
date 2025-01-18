using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Application.Configurations
{
    public static class DatabaseConfig
    {
        public const string ConnectionString = "Host=127.0.0.1;Port=5432;Database=MTCG_DB;Username=admin;Password=admin";
        public const string SchemaName = "DBSchema_MonsterTradingCardsGame";
    }
}
