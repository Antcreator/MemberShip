namespace MemberShip.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public UserModel? User { get; set; }
        public int PackageId { get; set; }
        public Package Package { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Balance => Package != null ? Package.Price - AmountPaid : 0;
    }
}
