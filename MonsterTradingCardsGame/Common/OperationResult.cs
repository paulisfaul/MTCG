using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Common
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        /// <summary>
        /// Initializes a result of an operation.
        /// </summary>
        /// <param name="success">Bool if operation was successful</param>
        /// <param name="code">Status code of the operation result</param>
        /// <param name="message">Message of the operation result</param>
        /// <param name="data">Optional data</param>
        public OperationResult(bool success, int code, string? message = null, T? data = default)
        {
            Success = success;
            Code = code;
            Message = message;
            Data = data;
        }
    }
}
