namespace PC_Bali.Dtos
{
    public class TransactionDto
    {

        public string CustomerName { get; set; }
        public string MerchantName { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Price { get; set; }
        public decimal Points { get; set; }

    }
}
