using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Models
{
    public class UserData
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserData"/> class.
        /// Sets the CreatedAt property to the current date and time.
        /// </summary>
        public UserData()
        {
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserData"/> class with name, bio, image, lastLoginAt and createdAt.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <param name="bio">The biography of the user.</param>
        /// <param name="image">The image URL of the user.</param>
        /// <param name="lastLoginAt">The last login date and time of the user, if available.</param>
        /// <param name="createdAt">The creation date and time of the user.</param>
        /// <param name="elo">The elo points of the user.</param>
        public UserData(string name, string bio, string image, DateTime? lastLoginAt, DateTime createdAt, int elo)
        {
            Name = name;
            Bio = bio;
            Image = image;
            LastLoginAt = lastLoginAt;
            CreatedAt = createdAt;
        }

        public override string ToString()
        {
            return $@"
  Name: {Name ?? "N/A"}
  Bio: {Bio ?? "N/A"}
  Image: {Image ?? "N/A"}
  Created At: {CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}
  Last Login: {LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}";
        }
    }
}