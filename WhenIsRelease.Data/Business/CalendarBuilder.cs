using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using WhenIsRelease.Data.Utils;
using WhenIsRelease.Models;

namespace WhenIsRelease.Data.Business
{
    public class CalendarBuilder
    {
        private readonly string _companyName;
        private readonly string _productName;

        public CalendarBuilder()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            _companyName = fvi.CompanyName;
            _productName = fvi.ProductName;
        }

        internal string Build(IEnumerable<IRelease> releaseItems)
        {
            StringBuilder sb = new StringBuilder();
            var DateFormat = "yyyyMMdd";
            var DateTimeFormat = "yyyyMMddTHHmmss";
            var now = DateTime.Now.ToUniversalTime().ToString(DateFormat);
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine($"PRODID:-//{_companyName}//{_productName}//EN");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("METHOD:PUBLISH");
            foreach (var item in releaseItems)
            {
                var lastModified = item.LastUpdate > DateTime.MinValue && item.LastUpdate < DateTime.MaxValue
                    ? item.LastUpdate
                    : DateTime.Now;

                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"DTSTART;VALUE=DATE:{item.ReleaseDate.ToString(DateFormat)}");
                sb.AppendLine($"DTSTAMP:{now}");
                sb.AppendLine($"UID:{Guid.NewGuid()}");
                sb.AppendLine($"CREATED:{now}");
                sb.AppendLine($"X-ALT-DESC;FMTTYPE=text/html:{ReleaseHelper.GetReleaseDescription(item)}");
                sb.AppendLine($"LAST-MODIFIED:{lastModified.ToString(DateTimeFormat)}");
                sb.AppendLine("SEQUENCE:0");
                sb.AppendLine("STATUS:CONFIRMED");
                sb.AppendLine($"SUMMARY:{ReleaseHelper.GetReleaseTitle(item)}");
                sb.AppendLine("TRANSP:OPAQUE");
                sb.AppendLine("END:VEVENT");
            }
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }
    }
}
