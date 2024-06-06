using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using MemberShip.Data;
using MemberShip.Models;

namespace MemberShip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoiceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetInvoice(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Package)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return NotFound();

            var invoice = new Invoice
            {
                PaymentId = payment.Id,
                UserId = payment.UserId,
                Amount = payment.AmountPaid,
                InvoiceDate = DateTime.Now
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            byte[] pdf = GeneratePdf(invoice, payment);

            return File(pdf, "application/pdf", $"Invoice_{invoice.Id}.pdf");
        }

        private byte[] GeneratePdf(Invoice invoice, Payment payment)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph($"Invoice ID: {invoice.Id}"));
                document.Add(new Paragraph($"User ID: {invoice.UserId}"));
                document.Add(new Paragraph($"Payment ID: {payment.Id}"));
                document.Add(new Paragraph($"Package: {payment.Package.Name}"));
                document.Add(new Paragraph($"Amount Paid: {payment.AmountPaid}"));
                document.Add(new Paragraph($"Invoice Date: {invoice.InvoiceDate}"));

                document.Close();
                return ms.ToArray();
            }
        }
    }
}
