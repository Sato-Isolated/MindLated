using System.Net;
using System.Reflection;

namespace MindLated.Protection.String
{
    internal class OnlineString
    {
        public static string Decoder(string encrypted)
        {
            if (Assembly.GetExecutingAssembly() != Assembly.GetCallingAssembly()) return "MindLated.png";
            var webClient = new WebClient();
            return webClient.DownloadString("https://communitykeyv1.000webhostapp.com/Decoder4.php?string=" + encrypted);
        }
    }
}