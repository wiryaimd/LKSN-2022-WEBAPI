namespace PC_Bali.Dtos
{
    public class VoucherResDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Cost { get; set; }
        public int Limit { get; set; }
        public DateTime ActivatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsActive { get; set; }
    }
}
