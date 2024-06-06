using MemberShip.Data;
using MemberShip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberShip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PackageController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetPackages()
        {
            var packages = _context.Packages.ToList();
            return Ok(packages);
        }
    }
}
