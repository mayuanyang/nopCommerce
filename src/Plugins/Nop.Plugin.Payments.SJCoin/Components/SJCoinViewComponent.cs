using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.SJCoin.Components
{

    [ViewComponent(Name = "SJCoinViewComponent")]
    public class SJCoinViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.SJCoin/Views/PaymentInfo.cshtml");
        }
    }
}
