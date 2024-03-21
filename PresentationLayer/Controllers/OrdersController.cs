﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusinessObject.Models;
using eStore.Models;

namespace PresentationLayer.Controllers
{
    public class OrdersController : Controller
    {
        private readonly SalesManagementContext _context;

        public OrdersController(SalesManagementContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var salesManagementContext = _context.Orders.Include(o => o.Member);
            return View(await salesManagementContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Member)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            // Lấy thông tin chi tiết đơn hàng từ bảng OrderDetails
            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == id)
                .ToListAsync();


            // Tạo danh sách để lưu thông tin chi tiết sản phẩm
            var productDetails = new List<ProductDetail>();

            // Lấy thông tin chi tiết sản phẩm từ bảng Product dựa trên ProductId
            foreach (var detail in orderDetails)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);

                if (product != null)
                {
                    var productDetail = new ProductDetail
                    {
                        ProductName = product.ProductName,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice
                    };

                    productDetails.Add(productDetail);
                }
            }
            // Tạo một ViewModel để chứa cả thông tin về đơn hàng và danh sách chi tiết đơn hàng
            var orderViewModel = new OrderViewModel
            {
                Order = order,
                ProductDetails = productDetails
            };

            return View(orderViewModel);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "CompanyName");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,OrderDate,RequiredDate,MemberId,ShippedDate,Freight")] Order order)
        {
            Console.WriteLine("OrderId: " + order.OrderId);
            Console.WriteLine("OrderDate: " + order.OrderDate);
            Console.WriteLine("RequiredDate: " + order.RequiredDate);
            Console.WriteLine("MemberId: " + order.MemberId);
            Console.WriteLine("ShippedDate: " + order.ShippedDate);
            Console.WriteLine("Freight: " + order.Freight);


            try
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "CompanyName", order.MemberId);
                return View(order);
            }
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "City", order.MemberId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderDate,RequiredDate,MemberId,ShippedDate,Freight")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "City", order.MemberId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Member)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'SalesManagementContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
          return (_context.Orders?.Any(e => e.OrderId == id)).GetValueOrDefault();
        }
    }
}
