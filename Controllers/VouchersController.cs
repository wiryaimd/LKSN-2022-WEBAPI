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
    public class VouchersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VouchersController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VoucherResDto>>> GetVouchers()
        {
            return _context.Vouchers.OrderBy(v => v.Code).Select(delegate(Voucher vc) {

                DateTime now = DateTime.Now;
                return new VoucherResDto()
                {
                    Code = vc.Code,
                    ActivatedAt = vc.ActivatedAt,
                    Cost = vc.Cost,
                    Description = vc.Description,
                    ExpiredAt = vc.ExpiredAt,
                    IsActive = now.CompareTo(vc.ExpiredAt) == 1 ? false : true,
                    Limit = vc.Limit,
                    Name = vc.Name
                };
            }).ToList();
        }

        [Authorize(Roles = "0")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVoucher(int id, Voucher voucher)
        {
            if (id != voucher.Id)
            {
                return BadRequest();
            }

            _context.Entry(voucher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoucherExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest();
                }
            }

            return NoContent();
        }

        [Authorize(Roles = "0")]
        [HttpPost]
        public async Task<ActionResult<Voucher>> PostVoucher(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch {
                return BadRequest();
            }

            return CreatedAtAction("GetVoucher", new { id = voucher.Id }, voucher);
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(e => e.Id == id);
        }
    }
}
