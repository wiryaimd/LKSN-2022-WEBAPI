using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PC_Bali.Dtos;
using PC_Bali.Models;

namespace PC_Bali.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeController : ControllerBase
    {

        private readonly AppDbContext _context;

        public MeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Customer>> Me() {
            string email = User.Identity.Name;

            Customer? customer = _context.Customers.Where(c => c.Email.Equals(email)).FirstOrDefault();
            if (customer == null) {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult> Put(CustomerReqDto customerDto) 
        {
            string email = User.Identity.Name;

            Customer? customer = _context.Customers.Where(c => c.Email.Equals(email)).FirstOrDefault();
            if (customer == null)
            {
                return NotFound();
            }

            customer.Name = customerDto.Name;
            customer.Email = customerDto.Email;
            customer.Gender = customerDto.Gender;
            customer.DateOfBirth = customerDto.DateOfBirth;
            customer.PhoneNumber = customerDto.PhoneNumber;
            customer.Address = customerDto.Address;

            _context.Entry(customer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(1_000_000)]
        public async Task<ActionResult> Photo(IFormFile file) {
            string email = User.Identity.Name;

            Customer? customer = _context.Customers.Where(c => c.Email.Equals(email)).FirstOrDefault();
            if (customer == null)
            {
                return NotFound();
            }

            string name = Guid.NewGuid().ToString().Substring(0, 8);
            string ex = Path.GetExtension(file.FileName);
            Console.WriteLine(ex);

            if (!Directory.Exists(Path.GetFullPath(Environment.CurrentDirectory) + "/Photo")) { 
                Directory.CreateDirectory(Path.GetFullPath(Environment.CurrentDirectory) + "/Photo");
            }

            if (!(ex.Equals(".jpg") || ex.Equals(".png") || ex.Equals(".jpeg"))) {
                return Problem("Extension must jpg/png/jpeg", statusCode: 404);
            }

            using (FileStream fs = new FileStream(Path.GetFullPath(Environment.CurrentDirectory) + "/Photo/" + name + ex, FileMode.Create)) {
                file.CopyTo(fs);
                fs.Flush();
            }

            customer.PhotoPath = name + ex;

            _context.Entry(customer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
