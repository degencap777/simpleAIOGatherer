using Ennui.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ennui.Script.Official
{
    public class Configuration
    {
        public int GatherAttemptsTimeout { get; set; } = 6;
        public string GatherClusterName { get; set; }
        public IArea<float> GatherArea { get; set; }

    }
}
