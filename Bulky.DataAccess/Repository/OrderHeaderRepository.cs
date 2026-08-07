using Bulky.DataAccess.Data;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Bulky.DataAccess.Repository.IRepository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null) 
            {
                orderFromDb.OrderStatus = orderStatus; 
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateRazorPaymentId(int id, string razorpayOrderId, string razorpayPaymentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(razorpayOrderId))
            {
                orderFromDb.RazorpayOrderId = razorpayOrderId;
            }
            if (!string.IsNullOrEmpty(razorpayPaymentId))
            {
                orderFromDb.RazorpayPaymentId = razorpayPaymentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }

    }
}
