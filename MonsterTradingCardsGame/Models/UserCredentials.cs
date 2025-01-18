using MonsterTradingCardsGame.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Models
{
    public class UserCredentials
    {
        public string Username { get; private set; }
        public RoleEnum Role { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCredentials"/> class with username and password.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="role">The role of the user.</param>
        public UserCredentials(string username, RoleEnum role)
        {
            Username = username;
            Role = role;
        }

        public override string ToString()
        {
            return $@"
  Username: {Username}
  Role: {Role}";
        }
    }
}
