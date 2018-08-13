using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.SJCoin
{
    public class SJCoinPaymentSettings : ISettings
    {
        public string RecipientWalletAddress { get; set; }
    }
}
