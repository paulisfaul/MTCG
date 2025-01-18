using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Helper.HttpServer
{
    /// <summary>This enumeration defines HTTP status codes that are used by
    ///          the <see cref="HttpSvr"/> implementation.</summary>
    public static class HttpStatusCode
    {
        /// <summary>Status code OK.</summary>
        public const int OK = 200;

        public const int CREATED = 201;

        /// <summary>Status code BAD REQUEST.</summary>
        public const int BAD_REQUEST = 400;

        public const int UNAUTHORIZED = 401;

        /// <summary>Status code NOT FOUND.</summary>
        public const int NOT_FOUND = 404;

        /// <summary>Status code METHOD NOT ALLOWED.</summary>

        public const int METHOD_NOT_ALLOWED = 405;

        /// <summary>Status code CONFLICT.</summary>

        public const int CONFLICT = 409;

        /// <summary>
        /// Status code INTERNAL_SERVER_ERROR
        /// </summary>
        public const int INTERNAL_SERVER_ERROR = 500;
    }
}
