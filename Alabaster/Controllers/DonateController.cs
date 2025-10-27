using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Alabaster.Controllers
{
    public class DonateController : Controller
    {
        public IActionResult Index() => View();

        // ---------- CONFIG ----------
        private const string PayfastSandboxUrl = "https://sandbox.payfast.co.za/eng/process";
        private const string PayfastMerchantId = "10043096";     
        private const string PayfastMerchantKey = "azzzkjjvc17zx"; 
        private const string payfastPassphrase = "";             

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartPayfast([FromForm] string amount, string donorName, string reference)
        {
           
            if (!decimal.TryParse(amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount))
            {
                if (!decimal.TryParse(amount, NumberStyles.Number, CultureInfo.CurrentCulture, out parsedAmount))
                {
                    
                    TempData["PayfastError"] = "Invalid amount format. Please enter a valid number (e.g. 100.00).";
                    return RedirectToAction("Index");
                }
            }

            if (parsedAmount <= 0)
            {
                TempData["PayfastError"] = "Invalid amount. Please enter an amount greater than zero.";
                return RedirectToAction("Index");
            }

          
            var fields = new SortedDictionary<string, string>()
    {
        {"merchant_id", PayfastMerchantId},
        {"merchant_key", PayfastMerchantKey},
        {"return_url", Url.Action("PayfastReturn", "Donate", null, Request.Scheme)},
        {"cancel_url", Url.Action("PayfastCancel", "Donate", null, Request.Scheme)},
        {"notify_url", Url.Action("Notify", "Donate", null, Request.Scheme)},
        {"m_payment_id", Guid.NewGuid().ToString()},
        {"amount", parsedAmount.ToString("0.00", CultureInfo.InvariantCulture)},
        {"item_name", "Donation to Alabaster Box"},
        {"name_first", donorName ?? ""},
    
        {"custom_str1", reference ?? ""}
    };

            var cleaned = new SortedDictionary<string, string>(fields.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                                                                     .ToDictionary(kv => kv.Key, kv => kv.Value));

            var signature = GeneratePayfastSignature(cleaned, payfastPassphrase);

            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8'><title>Redirecting to PayFast...</title></head><body>");
            sb.AppendLine("<p>Redirecting to PayFast — please wait...</p>");
            sb.AppendLine($"<form id='pf_form' action='{PayfastSandboxUrl}' method='post'>");

            foreach (var kv in cleaned)
            {
                sb.AppendLine($"<input type='hidden' name='{WebUtility.HtmlEncode(kv.Key)}' value='{WebUtility.HtmlEncode(kv.Value)}'/>");
            }

            sb.AppendLine($"<input type='hidden' name='signature' value='{WebUtility.HtmlEncode(signature)}'/>");
            sb.AppendLine("</form>");
            sb.AppendLine("<script>document.getElementById('pf_form').submit();</script>");
            sb.AppendLine("</body></html>");

            return Content(sb.ToString(), "text/html", Encoding.UTF8);
        }


        [HttpPost]
        public async Task<IActionResult> Notify()
        {
            var form = Request.HasFormContentType ? Request.Form : null;
            if (form == null) return BadRequest();

            var posted = form.ToDictionary(x => x.Key, x => x.Value.ToString());

            string returnedSignature = posted.ContainsKey("signature") ? posted["signature"] : "";
            var variablesForSignature = new SortedDictionary<string, string>(
                posted.Where(kv => kv.Key != "signature" && !string.IsNullOrWhiteSpace(kv.Value))
                      .ToDictionary(kv => kv.Key, kv => kv.Value)
            );

            var computed = GeneratePayfastSignature(variablesForSignature, payfastPassphrase);

            if (!string.Equals(computed, returnedSignature, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("PayFast ITN: signature mismatch.");
                return Content("INVALID_SIGNATURE", "text/plain");
            }

           

            return Ok("OK");
        }

        public IActionResult PayfastReturn()
        {
            // 
            TempData["PayfastMessage"] = "Thank you — your payment was received by PayFast. We'll confirm and send a receipt after server verification.";
            return RedirectToAction("Index");
        }

        public IActionResult PayfastCancel()
        {
            TempData["PayfastError"] = "Payment was cancelled or not completed.";
            return RedirectToAction("Index");
        }


        private static string GeneratePayfastSignature(SortedDictionary<string, string> fields, string passphrase = "")
        {
            var sb = new StringBuilder();
            foreach (var kv in fields)
            {
                if (string.IsNullOrEmpty(kv.Value)) continue;
                sb.Append(kv.Key).Append("=").Append(WebUtility.UrlEncode(kv.Value)).Append("&");
            }

            if (!string.IsNullOrWhiteSpace(passphrase))
            {
                sb.Append("passphrase=").Append(WebUtility.UrlEncode(passphrase));
            }
            else if (sb.Length > 0 && sb[sb.Length - 1] == '&')
            {
                sb.Length -= 1;
            }

            var paramString = sb.ToString();

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(paramString));
                var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return hex;
            }
        }
    }
}
