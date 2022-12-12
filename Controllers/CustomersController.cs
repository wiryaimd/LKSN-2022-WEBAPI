using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PC_Bali.Dtos;
using PC_Bali.Models;

namespace PC_Bali.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "0")]
        public async Task<ActionResult<IEnumerable<CustomerResDto>>> GetCustomers(string? keyword = null)
        {
            List<Customer> customerList = await _context.Customers.Include(c => c.Loyalty).ToListAsync();

            if (keyword != null)
            {
                customerList = customerList.Where(delegate (Customer c)
                {
                    string key = keyword.ToLower();
                    return c.Email.ToLower().Contains(key) || c.Name.ToLower().Contains(key) || c.PhoneNumber.Contains(key) || c.Address.ToLower().Contains(key) || c.Loyalty.Name.ToLower().Contains(key);
                }).ToList();
            }

            return customerList.OrderBy(c => c.Name).ThenBy(c => c.Email).Select(delegate(Customer cs) {
                CustomerResDto dto = new CustomerResDto() {
                    Name = cs.Name,
                    Email = cs.Email,
                    Gender = cs.Gender,
                    Address = cs.Address,
                    PhoneNumber = cs.PhoneNumber,
                    LoyalityName = cs.Loyalty.Name,
                    DateOfBirth = cs.DateOfBirth,
                    TotalPoints = cs.TotalPoint
                };
                return dto;
            }).ToList();
        }

        [HttpGet("{email}")]
        [Authorize(Roles = "0")]
        public async Task<ActionResult<Customer>> GetCustomer(string email)
        {
            Customer? customer = _context.Customers.Include(c => c.Loyalty).Where(c => c.Email == email).FirstOrDefault();

            if (customer == null)
            {
                return NotFound();
            }

            //CustomerResDto dto = new CustomerResDto()
            //{
            //    Name = customer.Name,
            //    Email = customer.Email,
            //    Gender = customer.Gender,
            //    Address = customer.Address,
            //    PhoneNumber = customer.PhoneNumber,
            //    LoyalityName = customer.Loyalty.Name,
            //    DateOfBirth = customer.DateOfBirth,
            //    TotalPoints = customer.TotalPoint
            //};

            return customer;
        }

        [HttpPut("{email}")]
        [Authorize(Roles = "0")]
        public async Task<IActionResult> PutCustomer(string email, Customer updateCustomer)
        {
            Customer? customer = _context.Customers.Include(c => c.Loyalty).Where(c => c.Email == email).FirstOrDefault();
            if (customer == null)
            {
                return NotFound();
            }

            if (customer.Id != updateCustomer.Id) {
                return BadRequest();
            }

            _context.Entry(updateCustomer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(email))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest();
                }
            }

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "0")]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest();
            }

            return CreatedAtAction("GetCustomer", new { id = customer.Id }, customer);
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.Email.Equals(id));
        }
    }
}
