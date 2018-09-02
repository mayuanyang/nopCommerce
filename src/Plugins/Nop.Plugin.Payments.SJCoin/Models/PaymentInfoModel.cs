using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.SJCoin.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Payment.BuyerWalletAddress")]
        public string BuyerWalletAddress { get; }

        public PaymentInfoModel(string buyerWalletAddress)
        {
            BuyerWalletAddress = buyerWalletAddress;
        }
    }
}
