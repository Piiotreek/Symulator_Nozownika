namespace Symulator_Nozownika.Models
{
    public class CoinWallet
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int Balance { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual UserAccount User { get; set; } = null!;
    }
}