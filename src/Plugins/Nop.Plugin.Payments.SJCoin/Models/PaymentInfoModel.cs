using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.SJCoin.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Payment.RecipientWalletAddress")]
        public string RecipientWalletAddress { get; }

        public PaymentInfoModel(string recipientWalletAddress)
        {
            RecipientWalletAddress = recipientWalletAddress;
        }
    }
}
