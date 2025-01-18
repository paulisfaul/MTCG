using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;

namespace MonsterTradingCardsGame.Repositories.Helpers
{
    public static class DatabaseHelper
    {
        public static async Task<int> ExecuteNonQueryAsync(NpgsqlConnection connection, string query, Action<NpgsqlCommand> addParameters)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            addParameters(cmd);
            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<T> ExecuteReaderAsync<T>(NpgsqlConnection connection, string query, Action<NpgsqlCommand> addParameters, Func<NpgsqlDataReader, Task<T>> readData)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            if (addParameters != null)
            {
                addParameters(cmd);
            }
            using var reader = await cmd.ExecuteReaderAsync();
            var x = await readData(reader);
            return x;
        }

        public static void AddParameter(NpgsqlCommand command, string parameterName, object value)
        {
                command.Parameters.AddWithValue(parameterName, value ?? DBNull.Value);
        }
    }
}