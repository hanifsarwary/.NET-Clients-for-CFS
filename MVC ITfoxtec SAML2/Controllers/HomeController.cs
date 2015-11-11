using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Web.Mvc;
using ITfoxtec.Saml2;
using ITfoxtec.Saml2.Bindings;
using ITfoxtec.Saml2.Mvc;
using ITfoxtec.Saml2.Schemas;
using ITfoxtec.Saml2.Util;

namespace MVC_ITfoxtec_SAML2.Controllers
{
    public class HomeController : Controller
    {
        private const string relayStateReturnUrl = "ReturnUrl";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Claims(string returnUrl)
        {
            if (Request.IsAuthenticated)
                return View();

            // Generate the SAML 2 Authentication Request
            
            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl } });

            var authRequest = new Saml2AuthnRequest
            {
                //ForceAuthn = true,
                //NameIdPolicy = new NameIdPolicy { AllowCreate = true, Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" },
                RequestedAuthnContext = new RequestedAuthnContext
                {
                    Comparison = AuthnContextComparisonTypes.Exact,
                    AuthnContextClassRef = new string[] { AuthnContextClassTypes.PasswordProtectedTransport.OriginalString },
                },
                Issuer = new EndpointReference(Configuration.ISSUER),
                Destination = new EndpointAddress(Configuration.CFS_ENDPOINT),
                AssertionConsumerServiceUrl = new EndpointAddress(Configuration.ISSUER + "/Home/AssertionConsumerService")
            };

            return binding.Bind(authRequest).ToActionResult();
        }

        public ActionResult AssertionConsumerService()
        {
            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse();

            X509Certificate2 certificate = CertificateUtil.Load(Configuration.PATH_TO_CERTIFICATE);
            binding.Unbind(Request, saml2AuthnResponse, certificate);
            saml2AuthnResponse.CreateSession();

            var returnUrl = binding.GetRelayStateQuery()[relayStateReturnUrl];
            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Claims");
        }

        [HttpPost]
        public ActionResult Logout()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            var logoutRequest = new Saml2LogoutRequest
            {
                Issuer = new EndpointReference(Configuration.ISSUER),
                Destination = new EndpointAddress(Configuration.CFS_ENDPOINT)
            };

            var binding = new Saml2PostBinding();
            return binding.Bind(logoutRequest).ToActionResult();
        }

        public ActionResult LoggedOut()
        {
            var response = new Saml2LogoutResponse();
            response.DeleteSession();

            return RedirectToAction("Index");
        }
    }
}