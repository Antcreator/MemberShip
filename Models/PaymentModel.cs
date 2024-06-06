namespace MemberShip.Models
{
    public class PaymentModel
    {
        public string UserId { get; set; }
        public int PackageId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
