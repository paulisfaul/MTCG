using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame.Models.RequestModels
{
    /// <summary>
    /// Repräsentiert eine Anfrage zum Registrieren eines neuen Benutzers.
    /// </summary>
    public class UserRegisterRequestDto
    {
        /// <summary>
        /// Ruft den Benutzernamen des neuen Benutzers ab oder legt diesen fest.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Ruft das Passwort des neuen Benutzers ab oder legt dieses fest.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Ruft die Rolle des neuen Benutzers ab oder legt diese fest.
        /// </summary>
        public string Role { get; set; }
    }

    /// <summary>
    /// Repräsentiert eine Anfrage zum Einloggen eines Benutzers.
    /// </summary>
    public class UserLoginRequestDto
    {
        /// <summary>
        /// Ruft den Benutzernamen des Benutzers ab oder legt diesen fest.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Ruft das Passwort des Benutzers ab oder legt dieses fest.
        /// </summary>
        public string Password { get; set; }
    }

    public class UserUpdateRequestDto
    {
        public string Name { get; set; }

        public string Bio { get; set; }

        public string Image {get; set; }
    }

    /// <summary>
    /// Repräsentiert eine Anfrage zum Erstellen eines Pakets, das eine Liste von Karten enthält.
    /// </summary>
    public class PackageRequestDto : List<CardRequestDto>
    {
    }

    /// <summary>
    /// Repräsentiert eine Anfrage zum Erstellen oder Aktualisieren einer Karte.
    /// </summary>
    public class CardRequestDto
    {
        /// <summary>
        /// Ruft den Namen der Karte ab oder legt diesen fest.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ruft den Schadenswert der Karte ab oder legt diesen fest.
        /// </summary>
        public float Damage { get; set; }

        /// <summary>
        /// Ruft den Elementtyp der Karte ab oder legt diesen fest.
        /// </summary>
        public string ElementType { get; set; }

        /// <summary>
        /// Ruft den Kartentyp der Karte ab oder legt diesen fest.
        /// </summary>
        public string CardType { get; set; }
    }

    /// <summary>
    /// Repräsentiert eine Anfrage zum Erstellen oder Aktualisieren eines Decks, das eine Liste von Karten-IDs enthält.
    /// </summary>
    public class DeckRequestDto : List<CardIdRequestDto>
    {
    }

    /// <summary>
    /// Repräsentiert eine Anfrage, die eine Karten-ID enthält.
    /// </summary>
    public class CardIdRequestDto
    {
        /// <summary>
        /// Ruft die eindeutige Kennung der Karte ab oder legt diese fest.
        /// </summary>
        public Guid CardId { get; set; }
    }

    /// <summary>
    /// Repräsentiert eine Anfrage, die eine TradingOffer repräsentiert
    /// </summary>
    public class TradingOfferRequestDto
    {
        public Guid CardId { get; set; }

        public string CardType { get; set; }

        public float MinDmg { get; set; }

        public bool AutomaticAccept { get; set; }
    }

    public class TradingAcceptRequestDto
    {
        public Guid CardId { get; set; }
    }
}
