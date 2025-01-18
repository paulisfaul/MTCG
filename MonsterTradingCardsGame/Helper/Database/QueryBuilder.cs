using System.Text;

namespace MonsterTradingCardsGame.Helper.Database
{
    public static class QueryBuilder
    {
        public static string BuildInsertQuery(string tableName, IEnumerable<string> columns)
        {
            var columnsList = string.Join(", ", columns);
            var parametersList = string.Join(", ", columns.Select(c => "@" + c));
            return $"INSERT INTO {tableName} ({columnsList}) VALUES ({parametersList})";
        }

        public static string BuildUpdateQuery(string tableName, IEnumerable<string> columns, string keyColumn)
        {
            var columnsList = columns.ToList();
            var keyColumnAlias = keyColumn;

            if (columnsList.Contains(keyColumn))
            {
                keyColumnAlias = keyColumn + "_kc";
            }

            var setClause = string.Join(", ", columnsList.Select(c => $"{c} = @{c}"));

            return $"UPDATE {tableName} SET {setClause} WHERE {keyColumn} = @{keyColumnAlias}";
        }

        public static string BuildDeleteQuery(string tableName, string keyColumn)
        {
            return $"DELETE FROM {tableName} WHERE {keyColumn} = @{keyColumn}";
        }

        public static string BuildSelectQuery(
            string tableName,
            IEnumerable<string> columns,
            string keyColumn = null,
            bool isNull = false,
            string orderByColumn = null,
            string orderByDirection = "ASC",
            bool orderByRandom = false,
            int limit = 0)
        {
            var columnsList = string.Join(", ", columns);
            var query = new StringBuilder($"SELECT {columnsList} FROM {tableName}");

            if (!string.IsNullOrEmpty(keyColumn))
            {
                if (isNull)
                {
                    query.Append($" WHERE {keyColumn} IS NULL");
                }
                else
                {
                    query.Append($" WHERE {keyColumn} = @{keyColumn}");
                }
            }

            if (orderByRandom)
            {
                query.Append(" ORDER BY RANDOM()");
            }
            else if (!string.IsNullOrEmpty(orderByColumn))
            {
                query.Append($" ORDER BY {orderByColumn} {orderByDirection}");
            }

            if (limit != 0)
            {
                query.Append($" LIMIT {limit}");
            }

            return query.ToString();
        }

        public static string BuildCustomSelectQuery(
            string tableName,
            IEnumerable<string> columns,
            string joinClause = null,
            string whereClause = null,
            string orderByClause = null,
            int? limit = null)
        {
            var columnsList = string.Join(", ", columns);
            var query = new StringBuilder($"SELECT {columnsList} FROM {tableName}");

            if (!string.IsNullOrEmpty(joinClause))
            {
                query.Append(" ");
                query.Append(joinClause);
            }

            if (!string.IsNullOrEmpty(whereClause))
            {
                query.Append(" WHERE ");
                query.Append(whereClause);
            }

            if (!string.IsNullOrEmpty(orderByClause))
            {
                query.Append(" ORDER BY ");
                query.Append(orderByClause);
            }

            if (limit.HasValue)
            {
                query.Append(" LIMIT ");
                query.Append(limit.Value);
            }

            return query.ToString();
        }
    }
}