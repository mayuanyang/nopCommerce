using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.SJCoin.Models;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.SJCoin.Components
{

    [ViewComponent(Name = "PaymentSJCoin")]
    public class PaymentSjCoinViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;

        public PaymentSjCoinViewComponent(IStoreContext storeContext)
        {
            _storeContext = storeContext;
        }
        public IViewComponentResult Invoke()
        {
            var scopedStore = _storeContext.ActiveStoreScopeConfiguration;
            return View("~/Plugins/Payments.SJCoin/Views/PaymentInfo.cshtml", new PaymentInfoModel(""));
        }
    }
}
