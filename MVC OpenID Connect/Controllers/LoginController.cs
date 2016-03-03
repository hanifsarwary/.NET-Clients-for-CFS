using DotNetOpenAuth.AspNet;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace MVC_OpenID_Connect
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                string url = Url.Action("Callback", "Login", null, Uri.UriSchemeHttp);
                Global.OpenIdConnectClient.RequestAuthentication(HttpContext, new Uri(url, UriKind.RelativeOrAbsolute));
                return null;
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View();
            }
        }

        public ActionResult Callback()
        {
            try
            {
                string callbackUrl = Url.Action("Callback", "Login", null, Uri.UriSchemeHttp);
                AuthenticationResult result = Global.OpenIdConnectClient.VerifyAuthentication(HttpContext, new Uri(callbackUrl, UriKind.RelativeOrAbsolute));

                if (result.IsSuccessful)
                {
                    FormsAuthentication.SetAuthCookie(result.UserName, false);
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Message = "The authentication failed.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View("Index");
        }

        [Authorize]
        public ActionResult Logout()
        {
            try
            {
                FormsAuthentication.SignOut();
            }
            catch (Exception) { }

            return RedirectToAction("Index", "Home");
        }
    }
}