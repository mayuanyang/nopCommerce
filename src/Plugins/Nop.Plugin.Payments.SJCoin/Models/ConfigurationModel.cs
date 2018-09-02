using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.SJCoin.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payment.SJCoin.ContractAddress")]
        public string ContractAddress { get; set; }

        [NopResourceDisplayName("Plugins.Payment.SJCoin.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Plugins.Payment.SJCoin.AccountPrivateKey")]
        public string AccountPrivateKey { get; set; }

        [NopResourceDisplayName("Plugins.Payment.SJCoin.TransactionReceiptCheckIntervalInSeconds")]
        public int TransactionReceiptCheckIntervalInSeconds { get; set; }

        [NopResourceDisplayName("Plugins.Payment.SJCoin.TransactionDetailsBaseUrl")]
        public string TransactionDetailsBaseUrl { get; set; }
    }
}
