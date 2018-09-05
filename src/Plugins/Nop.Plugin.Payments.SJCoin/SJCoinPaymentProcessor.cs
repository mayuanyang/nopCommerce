using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using Microsoft.AspNetCore.Http;
using Nethereum.Web3;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Services.Localization;
using Nop.Services.Payments;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Nethereum.Contracts.CQS;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3.Accounts;
using Nito.AsyncEx.Synchronous;
using Nop.Plugin.Payments.SJCoin.SmartContract;
using Nop.Services.Configuration;

namespace Nop.Plugin.Payments.SJCoin
{
    public class SJCoinPaymentProcessor : BasePlugin, IPaymentMethod
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly SJCoinPaymentSettings _sjCoinPaymentSettings;

        public SJCoinPaymentProcessor(ILocalizationService localizationService, IWebHelper webHelper, ISettingService settingService, IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _storeContext = storeContext;

            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            _sjCoinPaymentSettings = _settingService.LoadSetting<SJCoinPaymentSettings>(storeScope);
        }
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var account = new Account(_sjCoinPaymentSettings.AccountPrivateKey);
            var web3 = new Web3(account, _sjCoinPaymentSettings.Url);
            var handler = web3.Eth.GetContractHandler(_sjCoinPaymentSettings.ContractAddress);

            var transaction = ProcessTransactionInBlockchain(web3, handler, processPaymentRequest).WaitAndUnwrapException();

            var transactionReceipt = WaitForBlockchainToProcessTransaction(web3, transaction).WaitAndUnwrapException();

            var getPaidAmountFunction = new GetPaidAmountFunction()
            {
                CorrelationId = processPaymentRequest.OrderGuid.ToString("N"),
            };

            var paidAmountBigInteger = handler.QueryAsync<GetPaidAmountFunction, BigInteger>(getPaidAmountFunction).WaitAndUnwrapException();
            
            var paidAmount = Web3.Convert.FromWeiToBigDecimal(paidAmountBigInteger);

            processPaymentRequest.CustomValues["Paid"] = paidAmount;

            if (transactionReceipt != null)
                processPaymentRequest.CustomValues.Add("Payment.TransactionReceipt", Path.Combine(_sjCoinPaymentSettings.TransactionDetailsBaseUrl, transactionReceipt.TransactionHash));
            
            return new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = true,
                NewPaymentStatus = paidAmount >= processPaymentRequest.OrderTotal ? PaymentStatus.Paid : PaymentStatus.Pending
            };
        }

        private async Task<BigInteger> GetAccountBalance(string walletAddress, string accountPrivateKey)
        {
            var account = new Account(accountPrivateKey);
            var web3 = new Web3(account, _sjCoinPaymentSettings.Url);

            var handler = web3.Eth.GetContractHandler(_sjCoinPaymentSettings.ContractAddress);
            var balanceOfFunction = new BalanceOfFunction()
            {
                Owner = walletAddress
            };

            return await handler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction);
        }

        private async Task<Transaction> ProcessTransactionInBlockchain(Web3 web3, ContractHandler handler, ProcessPaymentRequest processPaymentRequest)
        {
            var withdrawFrom = processPaymentRequest.CustomValues["Payment.BuyerWalletAddress"].ToString();
            var withdrawMessage = new WithdrawFunction()
            {
                From = withdrawFrom,
                Amount = new HexBigInteger(Web3.Convert.ToWei(processPaymentRequest.OrderTotal)),
                CorrelationId = processPaymentRequest.OrderGuid.ToString("N")
            };

            var transactionHash = await handler.SendRequestAsync(withdrawMessage);

            return await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionHash);
        }

        private async Task<TransactionReceipt> WaitForBlockchainToProcessTransaction(Web3 web3, Transaction transaction)
        {
            int loop = 0;
            while (loop < 5)
            {
                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction.TransactionHash);
                if (receipt != null)
                {
                    return receipt;
                }
                await Task.Delay(_sjCoinPaymentSettings.TransactionReceiptCheckIntervalInSeconds == 0 ? 5 : _sjCoinPaymentSettings.TransactionReceiptCheckIntervalInSeconds * 1000);
                loop++;
            }

            return null;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0m;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 10 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 10)
                return false;

            return true;
        }

        public string GetPublicViewComponentName()
        {
            return "PaymentSJCoin";
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentSJCoin/Configure";
        }

        public bool SupportCapture => false;
        public bool SupportPartiallyRefund => false;
        public bool SupportRefund => false;
        public bool SupportVoid => false;
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;
        public bool SkipPaymentInfo => false;

        public string PaymentMethodDescription => "Use SJ Coin to pay";

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentRequest = new ProcessPaymentRequest();
            //pass custom values to payment processor
            if (form.TryGetValue("BuyerWalletAddress", out StringValues buyerWalletAddress) && !StringValues.IsNullOrEmpty(buyerWalletAddress))
                paymentRequest.CustomValues.Add("Payment.BuyerWalletAddress", buyerWalletAddress.ToString());

            return paymentRequest;
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            //pass custom values to payment processor
            if (form.TryGetValue("BuyerWalletAddress", out StringValues buyerWalletAddress)
                && !StringValues.IsNullOrEmpty(buyerWalletAddress)
                && form.TryGetValue("BuyerWalletPrivateKey", out StringValues buyerWalletPrivateKey)
                && !StringValues.IsNullOrEmpty(buyerWalletPrivateKey))
            {
                var value = Web3.Convert.FromWeiToBigDecimal(GetAccountBalance(buyerWalletAddress, buyerWalletPrivateKey).WaitAndUnwrapException());
                
                if (decimal.Parse(value.ToString()) <= 0)
                {
                    return new List<string>(){"You don't have enough credit to pay this order"};
                }
            }
            return new List<string>();
        }

        public override void Install()
        {
            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Payment.BuyerWalletAddress", "Wallet address");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payment.SJCoin.ContractAddress", "The SJC contract address");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payment.SJCoin.Url", "Url of the blockchain that hosted SJC token");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payment.SJCoin.AccountPrivateKey", "Private key of the master account of SJC");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payment.SJCoin.TransactionReceiptCheckIntervalInSeconds", "Transaction check interval in seconds");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payment.SJCoin.TransactionDetailsBaseUrl", "Transaction check base Url");

            base.Install();
        }

    }
}
