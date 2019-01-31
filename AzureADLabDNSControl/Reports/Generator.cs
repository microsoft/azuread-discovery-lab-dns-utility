using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace AzureADLabDNSControl.Reports
{
    public static class Generator
    {
        public static byte[] GenReport(string html)
        {
            Byte[] res = null;
            using (var stream = new MemoryStream())
            {
                var doc = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.Letter);
                doc.Save(stream);
                res = stream.ToArray();
            }
            return res;
        }
    }
}
