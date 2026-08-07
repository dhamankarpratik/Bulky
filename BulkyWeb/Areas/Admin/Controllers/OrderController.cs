using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.Diagnostics;
using System.Security.Claims;
using static System.Collections.Specialized.BitVector32;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly RazorpayClient _razorpayClient;
        [BindProperty]
        public OrderVM OrderVM {  get; set; }
        public OrderController(IUnitOfWork unitOfWork, IConfiguration configuration, RazorpayClient razorpayClient)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _razorpayClient = razorpayClient;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeader == null)
            {
                return NotFound();
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                try
                {
                        RazorpayClient client = new RazorpayClient(
                        _configuration["RazorPay:KeyId"],
                        _configuration["RazorPay:SecretKey"]);

                    Dictionary<string, object> refundRequest = new Dictionary<string, object>()
            {
                // Refund full amount (in paise)
                { "amount", (int)(orderHeader.OrderTotal * 100) },
                { "speed", "normal" } // or "optimum"
            };

                    Payment payment = client.Payment.Fetch(orderHeader.RazorpayPaymentId);
                    Refund refund = payment.Refund(refundRequest);

                    // Optional: Save refund id
                    // orderHeader.RazorpayRefundId = refund["id"].ToString();

                    orderHeader.PaymentStatus = SD.StatusRefunded;
                    orderHeader.OrderStatus = SD.StatusCancelled;

                    _unitOfWork.OrderHeader.Update(orderHeader);
                    _unitOfWork.Save();

                    TempData["Success"] = "Order cancelled and refunded successfully.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;

                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                TempData["Success"] = "Order cancelled successfully.";
            }

            return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });
        }


        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == OrderVM.OrderHeader.Id);


            RazorpayClient client = new RazorpayClient(
            _configuration["RazorPay:KeyId"],
            _configuration["RazorPay:SecretKey"]);

            Dictionary<string, object> input = new()
            {
                { "amount", (int)(OrderVM.OrderHeader.OrderTotal * 100) },
                { "currency", "INR" },
                { "receipt", $"order_{OrderVM.OrderHeader.Id}" }
            };

            var order = client.Order.Create(input);

            OrderVM.OrderHeader.RazorpayOrderId = order["id"].ToString();

            _unitOfWork.OrderHeader.Update(OrderVM.OrderHeader);
            _unitOfWork.Save();

            ViewBag.OrderId = OrderVM.OrderHeader.RazorpayOrderId;
            ViewBag.RazorpayKey = _configuration["RazorPay:KeyId"];
            ViewBag.Amount = (int)(OrderVM.OrderHeader.OrderTotal * 100);

            return View("Payment", OrderVM);
        }

        public IActionResult PaymentConfirmation(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
        {
            try
            {
                Dictionary<string, string> attributes = new()
                {
                    { "razorpay_order_id", razorpay_order_id },
                    { "razorpay_payment_id", razorpay_payment_id },
                    { "razorpay_signature", razorpay_signature }
                };


                Utils.verifyPaymentSignature(attributes);


                var orderHeader = _unitOfWork.OrderHeader.Get(
                    u => u.RazorpayOrderId == razorpay_order_id
                );


                orderHeader.RazorpayPaymentId = razorpay_payment_id;
                orderHeader.RazorpaySignature = razorpay_signature;

                orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                orderHeader.OrderStatus = SD.StatusApproved;
                orderHeader.PaymentDate = DateTime.Now;


                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();
                return View(orderHeader.Id);
            }
            catch (Exception)
            {
                return BadRequest("Payment verification failed");
            }
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;

            }

            return Json(new { data = objOrderHeaders });
        }

        #endregion
    }
}
