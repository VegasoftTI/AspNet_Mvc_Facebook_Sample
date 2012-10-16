using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using System.Threading;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace FB.Controllers
{
    public class HomeController : Controller
    {
        public string AppId = "374918325879397";
        public string AppSecret = "54c997ce7eb78b85f906db62e758e368";
        public string local_url = "http://fb.local";
        public string redirect_uri = "http://www.facebook.com/mmdevel/app_374918325879397";
        public string ExtendedPermissions = "";
        public int ContestID = 7;
        public string lang = "en_US";

        //
        // GET: /Home/

        public ActionResult Index()
        {
            //GETS SIGNED REQUEST FROM FACEBOOK WHEN APPLICATION IS LOADED
            if (Request.Params["signed_request"] != null)
            {
                string payload = Request.Params["signed_request"].Split('.')[1];

                //STORE A STRING VERSION OF SIGNED REQUEST INTO A SESSION VARIABLE SO APPLICATION CAN USE DATA
                Session["signed_request"] = payload;

                var encoding = new UTF8Encoding();
                var decodedJson = payload.Replace("=", string.Empty).Replace('-', '+').Replace('_', '/');
                var base64JsonArray = Convert.FromBase64String(decodedJson.PadRight(decodedJson.Length + (4 - decodedJson.Length % 4) % 4, '='));
                var json = encoding.GetString(base64JsonArray);
                var o = JObject.Parse(json);

                string PageID = Convert.ToString(o.SelectToken("page.id")).Replace("\"", "");
                string oauth_token = Convert.ToString(o.SelectToken("oauth_token")).Replace("\"", "");
                string algorithm = Convert.ToString(o.SelectToken("algorithm")).Replace("\"", "");
                string PageLiked = Convert.ToString(o.SelectToken("page.liked")).Replace("\"", "");
                string fbuid = Convert.ToString(o.SelectToken("user_id")).Replace("\"", "");
                string Country = Convert.ToString(o.SelectToken("user.country")).Replace("\"", "");
                string locale = Convert.ToString(o.SelectToken("user.locale")).Replace("\"", "");
                string lang = "en-CA";

                if (locale.Substring(0, 2) == "fr")
                {
                    lang = "fr-CA";
                }
                Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

                //SET USER ID
                Session["fbuid"] = fbuid;
                Session["lang"] = lang;

                //IF USER LIKES THE PAGE, THEN SEE IF THEY ARE AUTHENTICATED WITH THE APPLICATION
                if (PageLiked == "True")
                {

                    //IF SIGNED REQUEST HAS NO ACCESS TOKEN, THEN APPLICATION IS NOT AUTHENTICATED
                    if (oauth_token == "")
                    {
                        dynamic parameters = new ExpandoObject();
                        parameters.client_id = AppId;
                        parameters.client_secret = AppSecret;
                        parameters.redirect_uri = local_url;

                        // The requested response: an access token (token), an authorization code (code), or both (code token).
                        parameters.response_type = "token";

                        // add the 'scope' parameter only if we have extendedPermissions.
                        if (!string.IsNullOrWhiteSpace(ExtendedPermissions))
                            parameters.scope = ExtendedPermissions;

                        // generate the login url
                        var fb = new FacebookClient();
                        var loginUrl = fb.GetLoginUrl(parameters);

                        string oauth_url = "https://www.facebook.com/dialog/oauth/?client_id=" + AppId + "&redirect_uri=" + local_url + "&scope=" + ExtendedPermissions;

                        Response.Write("<script>");
                        Response.Write("var oauth_url = '" + oauth_url + "';");
                        Response.Write("window.top.location = oauth_url;");
                        Response.Write("</script>");

                    }
                    else
                    {
                        //IF SIGNED REQUEST HAS AN AUTHENTICATED TOKEN, THEN PROCEED WITH THE APPLICATION
                        //CHECK TO SEE IF THE USER HAS ENTERED THE CONTEST
                        ViewBag.x = "SIGNED REQUEST IS AUTHENTICATED!!!!";
                        ViewBag.PageID = PageID;
                        ViewBag.oauth_token = oauth_token;
                        ViewBag.algorithm = algorithm;
                        ViewBag.PageLiked = PageLiked;
                        ViewBag.fbuid = fbuid;
                        ViewBag.Country = Country;
                        ViewBag.locale = locale;
                        ViewBag.lang = lang;

                    }

                }
                //END PAGE LIKE CHECK

            }
            //END SIGNED REQUEST CHECK

            //NO SIGNED REQUEST AVAILABLE BECAUSE FACEBOOK AUTHENTICATED REDIRECTED USE TO WEB HOST
            //THIS WILL HANDLE THE RE-DIRECT FROM THE WEB HOST BACK TO FACEBOOK AFTER THE USER AUTHENTICATES THE APPLICATION.
            if (Request.Params["code"] != null)
            {
                Response.Redirect(redirect_uri, true);
            }

            return View();
        }

    }
}
