using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ICartService cartService;
    private readonly IUnitOfWork unit;

    public PaymentService(IConfiguration config, ICartService cartService,
        IUnitOfWork unit)
    {
        this.cartService = cartService;
        this.unit = unit;
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];
    }

    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        ShoppingCart cart = await cartService.GetCartAsync(cartId)
            ?? throw new Exception("Cart unavailable");

        long shippingPrice = await GetShippingPriceAsync(cart) ?? 0;

        await ValidateCartItemsInCartAsync(cart);

        long subtotal = CalculateSubtotal(cart);

        if (cart.Coupon != null)
        {
            subtotal = await ApplyDiscountAsync(cart.Coupon, subtotal);
        }

        long total = subtotal + shippingPrice;

        await CreateUpdatePaymentIntentAsync(cart, total);

        _ = await cartService.SetCartAsync(cart);

        return cart;
    }

    public async Task<string> RefundPayment(string paymentIntentId)
    {
        RefundCreateOptions refundOptions = new()
        {
            PaymentIntent = paymentIntentId
        };

        RefundService refundService = new();
        Refund result = await refundService.CreateAsync(refundOptions);

        return result.Status;
    }

    private async Task CreateUpdatePaymentIntentAsync(ShoppingCart cart, long total)
    {
        PaymentIntentService service = new();

        if (string.IsNullOrEmpty(cart.PaymentIntentId))
        {
            PaymentIntentCreateOptions options = new()
            {
                Amount = total,
                Currency = "usd",
                PaymentMethodTypes = ["card"]
            };
            PaymentIntent intent = await service.CreateAsync(options);
            cart.PaymentIntentId = intent.Id;
            cart.ClientSecret = intent.ClientSecret;
        }
        else
        {
            PaymentIntentUpdateOptions options = new()
            {
                Amount = total
            };
            _ = await service.UpdateAsync(cart.PaymentIntentId, options);
        }
    }

    private async Task<long> ApplyDiscountAsync(AppCoupon appCoupon, long amount)
    {
        Stripe.CouponService couponService = new();

        Coupon coupon = await couponService.GetAsync(appCoupon.CouponId);

        if (coupon.AmountOff.HasValue)
        {
            amount -= (long)coupon.AmountOff * 100;
        }

        if (coupon.PercentOff.HasValue)
        {
            decimal discount = amount * (coupon.PercentOff.Value / 100);
            amount -= (long)discount;
        }

        return amount;
    }

    private long CalculateSubtotal(ShoppingCart cart)
    {
        decimal itemTotal = cart.Items.Sum(x => x.Quantity * x.Price * 100);
        return (long)itemTotal;
    }

    private async Task ValidateCartItemsInCartAsync(ShoppingCart cart)
    {
        foreach (CartItem item in cart.Items)
        {
            Core.Entities.Product productItem = await unit.Repository<Core.Entities.Product>()
                .GetByIdAsync(item.ProductId) ?? throw new Exception("Problem getting product in cart");

            if (item.Price != productItem.Price)
            {
                item.Price = productItem.Price;
            }
        }
    }

    private async Task<long?> GetShippingPriceAsync(ShoppingCart cart)
    {
        if (cart.DeliveryMethodId.HasValue)
        {
            DeliveryMethod deliveryMethod = await unit.Repository<DeliveryMethod>()
                .GetByIdAsync((int)cart.DeliveryMethodId)
                    ?? throw new Exception("Problem with delivery method");

            return (long)deliveryMethod.Price * 100;
        }

        return null;
    }
}