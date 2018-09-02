using Nop.Web.Framework.Controllers;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;

namespace Nop.Plugin.Payments.SJCoin.SmartContract
{
    [Function("withdraw", "bool")]
    public class WithdrawFunction : ContractMessage
    {
        [Parameter("address", "_from", 1)]
        public string From { get; set; }

        [Parameter("uint256", "_value", 2)]
        public BigInteger Amount { get; set; }

        [Parameter("string", "correlationId", 3)]
        public string CorrelationId { get; set; }
    }
}
