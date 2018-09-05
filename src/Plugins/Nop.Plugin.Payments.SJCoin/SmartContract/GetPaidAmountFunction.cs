using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;

namespace Nop.Plugin.Payments.SJCoin.SmartContract
{
    [Function("getPaidAmount", "uint256")]
    public class GetPaidAmountFunction : ContractMessage
    {
        [Parameter("string", "uint256", 1)]
        public string CorrelationId { get; set; }
    }
}
