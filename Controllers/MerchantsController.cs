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
    public class MerchantsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MerchantsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MerchantResDto>>> GetMerchants()
        {
            return _context.Merchants.OrderBy(m => m.Location).ThenBy(m => m.Name).Select(delegate(Merchant mc) {
                MerchantResDto dto = new MerchantResDto() {
                    Name = mc.Name,
                    Location = mc.Location,
                    Description = mc.Description,
                    Multiplier = mc.Multiplier
                };
                return dto;
            }).ToList();
        }

        [Authorize(Roles = "0")]
        [HttpPut]
        public async Task<IActionResult> PutMerchant(Merchant merchant)
        {
            Merchant? m = await _context.Merchants.FindAsync(merchant.Id);
            if (m == null)
            {
                return BadRequest();
            }

            m.Name = merchant.Name;
            m.Multiplier = merchant.Multiplier;
            m.Description = merchant.Description;
            m.Location = merchant.Location;
            m.CreatedAt = merchant.CreatedAt;

            _context.Entry(m).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

            return Ok();
        }

        [Authorize(Roles = "0")]
        [HttpPost]
        public async Task<ActionResult<Merchant>> PostMerchant(Merchant merchant)
        {
            _context.Merchants.Add(merchant);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest();
            }

            return CreatedAtAction("GetMerchant", new { id = merchant.Id }, merchant);
        }

        [Authorize(Roles = "0")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMerchant(int id)
        {
            var merchant = await _context.Merchants.FindAsync(id);
            if (merchant == null)
            {
                return NotFound();
            }

            _context.Merchants.Remove(merchant);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
