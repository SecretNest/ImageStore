using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    static class SimilarFileHelper
    {
        public static string IgnoredModeToString(IgnoredMode ignoredMode)
        {
            if (ignoredMode == IgnoredMode.Effective)
                return "";
            else if (ignoredMode == IgnoredMode.HiddenButConnected)
                return "Yes";
            else
                return "Disconnected";
        }
    }
}
