using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions(string? keyword = null)
        {
            List<Transaction> transactionList = await _context.Transactions.Include(t => t.Customer).Include(t => t.Merchant).ToListAsync();

            if (keyword != null)
            {
                transactionList = transactionList.Where(delegate (Transaction t)
                {
                    string key = keyword.ToLower();
                    return t.Customer.Name.ToLower().Contains(key) || t.Merchant.Name.ToLower().Contains(key) || t.TransactionDate.ToString("yyyy-MM-dd").Contains(keyword);
                }).ToList();
            }

            return transactionList.OrderByDescending(t => t.TransactionDate).Select(delegate(Transaction t) {
                TransactionDto dto = new TransactionDto()
                {
                    CustomerName = t.Customer.Name,
                    MerchantName = t.Merchant.Name,
                    Points = t.Point,
                    Price = t.Price,
                    TransactionDate = t.TransactionDate
                };

                return dto;
            }).ToList();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            Transaction? transaction = _context.Transactions.Include(t => t.Merchant).Include(t => t.Customer).Where(t => t.Id == id).FirstOrDefault();

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Transaction>> PostTransaction(TransactionReqDto transactionDto) {
            Customer? customer = await _context.Customers.FindAsync(transactionDto.CustomerId);
            if (customer == null) {
                return Problem("CustomerId not found", statusCode: 400);
            }

            Merchant? merchant = await _context.Merchants.FindAsync(transactionDto.MerchantId);
            if (merchant == null)
            {
                return Problem("MerchantId not found", statusCode: 400);
            }

            List<Transaction> transactionList = _context.Transactions.Include(t => t.Customer).Where(t => t.Customer.Id == customer.Id).ToList();
            Transaction tNew;

            decimal price = transactionDto.Price / 300;
            if (transactionList.Count < 2)
            {
                tNew = new Transaction()
                {
                    CustomerId = customer.Id,
                    MerchantId = merchant.Id,
                    CreatedAt = DateTime.Now,
                    Point = price,
                    Price = transactionDto.Price,
                    TransactionDate = DateTime.Now
                };
            }
            else {
                string now = DateTime.Now.ToString("yyyy-MM-dd");
                Console.WriteLine(now);
                int count = 0;
                for (int i = 0; i < transactionList.Count; i++)
                {
                    Console.WriteLine("cs: " + transactionList[i].TransactionDate.ToString("yyyy-MM-dd"));
                    if (now.Equals(transactionList[i].TransactionDate.ToString("yyyy-MM-dd")))
                    {
                        count += 1;
                        if (count >= 2)
                        {
                            return Problem("Cannot insert more than 2 times for every same Merchant in the same day", statusCode: 409);
                        }
                    }
                }
                tNew = new Transaction()
                {
                    CustomerId = customer.Id,
                    MerchantId = merchant.Id,
                    CreatedAt = DateTime.Now,
                    Point = price,
                    Price = transactionDto.Price,
                    TransactionDate = DateTime.Now
                };
            }

            customer.TotalPoint = customer.TotalPoint + (int) price;
            List<Loyalty> loyaltyList = _context.Loyalties.ToList();

            for (int i = 0; i < loyaltyList.Count; i++) {
                if (customer.TotalPoint >= loyaltyList[i].RequiredPoint) {
                    customer.LoyaltyId = loyaltyList[i].Id;
                }
            }

            try
            {
                _context.Entry(customer).State = EntityState.Modified;
                _context.Transactions.Add(tNew);
                await _context.SaveChangesAsync();

                Console.WriteLine("update sucess");
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest();
            }

            return Ok(tNew);
        }

        [HttpGet("ExportCSV")]
        [Authorize(Roles = "0")]
        public async Task<ActionResult> ExportCSV() {
            List<Transaction> transactionList = await _context.Transactions.Include(t => t.Customer).Include(t => t.Merchant).ToListAsync();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Customer Name;Merchant Name;Transaction Date;Points");
            for (int i = 0; i < transactionList.Count; i++) {
                Transaction t = transactionList[i];
                sb.AppendLine(t.Customer.Name + ";" + t.Merchant.Name + ";" + t.TransactionDate + ";" + t.Point);
            }

            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            return File(data, "text/csv");
        }

    }
}
