using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.SJCoin
{
    public class SJCoinPaymentSettings : ISettings
    {
        public string ContractAddress { get; set; }
        public string Url { get; set; }
        public string AccountPrivateKey { get; set; }
        public int TransactionReceiptCheckIntervalInSeconds { get; set; }
        public string TransactionDetailsBaseUrl { get; set; }
    }
}
