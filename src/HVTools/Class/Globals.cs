namespace HVTools.Class
{
    internal class Globals
    {
        internal class ToolName
        {
            internal static string ShortName = "HVTools";
            internal static string FullName = "HVTools GUI";
        }

        internal class MsgBox
        {
            internal static string Warning = "Warning";
            internal static string Error = "Error";
        }

        internal class ToolStates
        {
            internal static string CodeSignedBuild = @"Signed build";
            internal static string CodeSignedBuildMichael = @"Signed build (by Michael Morten Sonne)";
            internal static string NotCodeSignedBuild = @"Unsigned build";
            internal static string MichaelCodeSignThumbprintOffline = "D6A630B8F65C473C19F8B694491130073FCCDB32";
        }

        internal class ToolStings
        {
            internal static string UrlMyBlog = @"https://blog.sonnes.cloud";
            internal static string UrlMyWebPage = @"https://sonnes.cloud";
            internal static string UrlBuyMeaCoffie = @"https://buymeacoffee.com/sonnes";
            internal static string UrlLinkedIn = @"https://www.linkedin.com/in/michaelmsonne/";
            internal static string UrlGitHub = @"https://github.com/michaelmsonne/";
            internal static string UrlGitHubDownload = @"https://github.com/michaelmsonne/HVTools/releases/";
        }

        internal class ToolProperties
        {
            internal static string ToolVersion = Application.ProductVersion.Split('+')[0];
        }

        internal static async Task<string> FetchCurrentCertificateThumbprintAsync()
        {
            const string url = "https://raw.githubusercontent.com/michaelmsonne/michaelmsonne/main/Trusted_Publisher_Certificate/CurrentCertificateThumbprint.txt";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string fetchedThumbprint = await client.GetStringAsync(url);
                    return fetchedThumbprint.Trim();
                }
            }
            catch
            {
                // Return the hardcoded thumbprint if unable to fetch online
                return ToolStates.MichaelCodeSignThumbprintOffline;
            }
        }
    }
}
