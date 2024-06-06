using MemberShip.Data;
using MemberShip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberShip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("make-payment")]
        public async Task<IActionResult> MakePayment([FromBody] PaymentModel model)
        {
            // Check if the UserId exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == model.UserId);
            if (!userExists)
            {
                return NotFound(new { message = "User not found" });
            }

            var payment = new Payment
            {
                UserId = model.UserId,
                PackageId = model.PackageId,
                AmountPaid = model.AmountPaid,
                PaymentDate = model.PaymentDate
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // Fetch the saved payment with related Package
            var savedPayment = await _context.Payments
                .Include(p => p.Package)
                .FirstOrDefaultAsync(p => p.Id == payment.Id);

            if (savedPayment == null)
            {
                return NotFound(new { message = "Payment not found after saving" });
            }

            var balance = savedPayment.Balance;

            return Ok(new
            {
                Id = savedPayment.Id,
                UserId = savedPayment.UserId,
                PackageId = savedPayment.PackageId,
                AmountPaid = savedPayment.AmountPaid,
                PaymentDate = savedPayment.PaymentDate,
                Balance = balance
            });
        }

    }
}
