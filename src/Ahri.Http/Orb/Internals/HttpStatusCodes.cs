using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Orb.Internals
{
    public class HttpStatusCodes : Dictionary<int, string>
    {
        private HttpStatusCodes()
        {
            var Table = Resources.HttpStatusCodes.Split('\n')
                .Select(X => X.Trim(' ', '\t', '\r', '\n'))
                .Where(X => !string.IsNullOrWhiteSpace(X))
                .Select(X => X.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries))
                .Where(X => X.Length > 1)
                .Select(X => (Code: int.Parse(X.First()), Phrase: X.LastOrDefault() ?? "Unknown"));

            foreach(var Each in Table)
                this[Each.Code] = Each.Phrase;
        }

        /// <summary>
        /// Status Code Table.
        /// </summary>
        public static HttpStatusCodes Table { get; } = new HttpStatusCodes();
    }
}
