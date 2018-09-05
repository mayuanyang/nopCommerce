using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;

namespace Nop.Plugin.Payments.SJCoin.SmartContract
{
    [Function("balanceOf", "uint256")]
    public class BalanceOfFunction : ContractMessage
    {
        [Parameter("address", "_owner", 1)]
        public string Owner { get; set; }
    }
}