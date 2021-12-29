using Ahri.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Networks
{
    internal static class Shared
    {
        public static readonly IServiceProvider Null = new ServiceCollection().BuildServiceProvider();

    }
}
