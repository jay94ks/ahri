using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals
{
    internal class OrpHelpers
    {
        /// <summary>
        /// Hash <paramref name="str"/> value by DJB-2 method.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static uint Hash(string str)
        {
            var Temp = Encoding.UTF8.GetBytes(str);
            uint Hash = 5381;

            for (var i = 0; i < Temp.Length; i++)
                Hash = ((Hash << 5) + Hash) + Temp[i];

            return Hash;
        }
    }
}
