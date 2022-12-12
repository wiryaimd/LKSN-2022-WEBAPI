namespace PC_Bali.Dtos
{
    public class TransactionReqDto
    {

        public int CustomerId { get; set; }
        public int MerchantId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Price { get; set; }
    }
}
