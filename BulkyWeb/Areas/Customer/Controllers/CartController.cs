using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly RazorpayClient _razorpayClient;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork,RazorpayClient razorpayClient,IConfiguration configuration)
        {
            _razorpayClient = razorpayClient;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id ==  userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }


            return View(ShoppingCartVM);
        }



        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //regular customer account
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //it is company account 
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList) 
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }


            //if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //regular customer account and we need to capture payment.
                //stripe payment logic

                //Initiate order
                Dictionary<string, object> input = new Dictionary<string, object>();

                // Razorpay accepts amount in paise
                input.Add("amount", (int)(ShoppingCartVM.OrderHeader.OrderTotal * 100));
                input.Add("currency", "INR");
                input.Add("receipt", $"order_{ShoppingCartVM.OrderHeader.Id}");

                //Razorpay.Api.Order order = _razorpayClient.Order.Create(input);
                Razorpay.Api.Order order;
                try
                {
                    order = _razorpayClient.Order.Create(input);
                }
                catch (Exception ex)
                {
                    return BadRequest("Unable to create Razorpay order: " + ex.Message);
                }

                // Save Razorpay Order Id
                ShoppingCartVM.OrderHeader.RazorpayOrderId = order["id"].ToString();

                _unitOfWork.OrderHeader.Update(ShoppingCartVM.OrderHeader);
                _unitOfWork.Save();

                ViewBag.OrderId = ShoppingCartVM.OrderHeader.RazorpayOrderId;
                ViewBag.RazorpayKey = _configuration["RazorPay:KeyId"];
                ViewBag.Amount = (int)(ShoppingCartVM.OrderHeader.OrderTotal * 100);

                return View("Payment", ShoppingCartVM);
                //Initiate order end
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id});
        }
        [HttpGet]
        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id);

            if (orderHeader == null)
            {
                return NotFound();
            }

            var cartItems = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == orderHeader.ApplicationUserId);

            HttpContext.Session.Clear();

            _unitOfWork.ShoppingCart.removeRange(cartItems);
            _unitOfWork.Save();

            return View(id);
        }
        [HttpPost]
        public IActionResult OrderConfirmation(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
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

                HttpContext.Session.Clear();

                var cartItems = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == orderHeader.ApplicationUserId);

                _unitOfWork.ShoppingCart.removeRange(cartItems);
                _unitOfWork.Save();


                return View(orderHeader.Id);
            }
            catch (Exception)
            {
                return BadRequest("Payment verification failed");
            }
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked:true);
            if (cartFromDb.Count == 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart,
                        _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked:true);
            HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
