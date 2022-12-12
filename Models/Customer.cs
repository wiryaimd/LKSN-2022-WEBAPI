using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PC_Bali.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Trades = new HashSet<Trade>();
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Role { get; set; }
        public int LoyaltyId { get; set; }
        public DateTime LoyaltyExpiredDate { get; set; }
        public string? PhotoPath { get; set; }
        public int TotalPoint { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public virtual Loyalty? Loyalty { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<Trade> Trades { get; set; }

        [JsonIgnore]
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
