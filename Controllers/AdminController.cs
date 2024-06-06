using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf.Canvas.Draw;
using MemberShip.Data;
using MemberShip.Models;

namespace MemberShip.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetAllMembers()
        {
            var members = await _context.Users
                .Include(u => u.Payments)
                .ThenInclude(p => p.Package)
                .ToListAsync();

            return Ok(members);
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Payment)
                .ThenInclude(p => p.Package)
                .ToListAsync();

            return Ok(invoices);
        }

        [HttpGet("invoice/{userId}")]
        public async Task<IActionResult> GenerateUserInvoice(string userId)
        {
            // Fetch the user's payments including package details
            var payments = await _context.Payments
                .Include(p => p.Package)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            if (!payments.Any())
                return NotFound();

            // Fetch the username based on the userId
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var pdf = GeneratePdf(payments, user.UserName);

            return File(pdf, "application/pdf", $"Invoice_{user.UserName}.pdf");
        }

        private byte[] GeneratePdf(List<Payment> payments, string username)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Add document title
                document.Add(new Paragraph("Member Payment Invoice")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetBold());

                // Add line separator
                document.Add(new LineSeparator(new SolidLine()));

                // Add user-specific information
                document.Add(new Paragraph($"Username: {username}")
                    .SetFontSize(12));
                document.Add(new Paragraph($"Invoice Date: {DateTime.Now}")
                    .SetFontSize(12));

                document.Add(new LineSeparator(new SolidLine()));

                // Add table with payment details
                var table = new Table(new float[] { 2, 2, 2, 2, 2 });
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Add table header
                table.AddHeaderCell("Payment ID");
                table.AddHeaderCell("Package");
                table.AddHeaderCell("Amount Paid");
                table.AddHeaderCell("Payment Date");
                table.AddHeaderCell("Balance");

                // Add payment details to table
                foreach (var payment in payments)
                {
                    table.AddCell(payment.Id.ToString());
                    table.AddCell(payment.Package.Name);
                    table.AddCell($"kes {payment.AmountPaid.ToString("0.00")}");
                    table.AddCell(payment.PaymentDate.ToString("d"));
                    table.AddCell(payment.Balance > 0 ? $"kes {payment.Balance.ToString("0.00")}" : "kes 0.00");
                }

                document.Add(table);

                // Close document
                document.Close();

                return ms.ToArray();
            }
        }


    }
}
