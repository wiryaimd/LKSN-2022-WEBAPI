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
    public class TradesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TradesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TradeResDto>>> GetTrades(string? keyword = null)
        {
            List<Trade> tradeList = await _context.Trades.Include(t => t.Customer).Include(t => t.Voucher).ToListAsync();
            if (keyword != null) {
                tradeList = tradeList.Where(delegate (Trade t)
                {
                    string key = keyword.ToLower();
                    return t.Customer.Name.ToLower().Contains(key) || t.Voucher.Code.Contains(keyword);
                }).ToList();
            }

            return tradeList.OrderByDescending(t => t.CreatedAt).Select(delegate (Trade td)
            {
                TradeResDto dto = new TradeResDto()
                {
                    CustomerName = td.Customer.Name,
                    VoucherCode = td.Voucher.Code,
                    VoucherName = td.Voucher.Name,
                    IssuedAt = td.CreatedAt
                };
                return dto;
            }).ToList();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Trade>> GetTrade(int id)
        {
            var trade = await _context.Trades.FindAsync(id);

            if (trade == null)
            {
                return NotFound();
            }

            return trade;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Trade>> PostTrade(TradeReqDto trade)
        {
            Voucher? v = await _context.Vouchers.FindAsync(trade.VoucherId);
            if (v == null) {
                return Problem("VoucherId not found", statusCode: 400);
            }

            Customer? c = await _context.Customers.FindAsync(trade.CustomerId);
            if (c == null) {
                return Problem("CustomerId not found", statusCode: 400);
            }

            DateTime now = DateTime.Now;
            bool isActive = now.CompareTo(v.ExpiredAt) == 1 ? false : true;
            if (!isActive) {
                return Problem("Voucher is Expired", statusCode: 400);
            }

            if (c.TotalPoint < v.Cost) {
                return Problem("Point not enough", statusCode: 400);
            }
            //c.TotalPoint -= (int) v.Cost;

            Trade tradeNew = new Trade(){ 
                CustomerId = c.Id,
                VoucherId = v.Id,
                CreatedAt = DateTime.Now
            };

            _context.Trades.Add(tradeNew);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest();
            }

            return Ok(trade);
        }
    }
}
