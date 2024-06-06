namespace MemberShip.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public UserModel? User { get; set; }
        public int PaymentId { get; set; }
        public Payment? Payment { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
}
