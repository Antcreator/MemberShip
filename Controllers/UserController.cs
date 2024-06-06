using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas.Draw;
using MemberShip.Data;
using MemberShip.Models;
using Microsoft.AspNetCore.Identity;

namespace MemberShip.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserModel> _userManager;

        public UserController(AppDbContext context, UserManager<UserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("select-package")]
        public async Task<IActionResult> SelectPackage([FromBody] int packageId)
        {
            var userEmail = User.Identity?.Name;
            if (userEmail == null) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return Unauthorized();

            var package = await _context.Packages.FindAsync(packageId);
            if (package == null) return NotFound();

            return Ok(new { PackageId = package.Id, PackageName = package.Name, PackagePrice = package.Price });
        }

        [HttpPost("make-payment")]
        public async Task<IActionResult> MakePayment([FromBody] PaymentModel model)
        {
            var userEmail = User.Identity?.Name;
            if (userEmail == null) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return Unauthorized();

            var payment = new Payment
            {
                UserId = user.Id,
                PackageId = model.PackageId,
                AmountPaid = model.AmountPaid,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(payment);
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetUserInvoices()
        {
            var userEmail = User.Identity?.Name;
            if (userEmail == null) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return Unauthorized();

            var payments = await _context.Payments
                .Include(p => p.Package)
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            if (!payments.Any())
                return NotFound();

            var pdf = GeneratePdf(payments);

            return File(pdf, "application/pdf", $"Invoice_{user.Id}.pdf");
        }

        private byte[] GeneratePdf(List<Payment> payments)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                foreach (var payment in payments)
                {
                    document.Add(new Paragraph($"Payment ID: {payment.Id}"));
                    document.Add(new Paragraph($"Package: {payment.Package.Name}"));
                    document.Add(new Paragraph($"Amount Paid: {payment.AmountPaid}"));
                    if (payment.Balance > 0)
                    {
                        document.Add(new Paragraph($"Balance: {payment.Balance}"));
                    }
                    document.Add(new Paragraph($"Payment Date: {payment.PaymentDate}"));
                    document.Add(new Paragraph($"Invoice Date: {DateTime.Now}"));
                    document.Add(new LineSeparator(new SolidLine()));
                }

                document.Close();
                return ms.ToArray();
            }
        }
    }
}
