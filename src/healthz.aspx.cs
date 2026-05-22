using System;
using System.Web.UI;

namespace Loom
{
    /// <summary>
    /// App-scope liveness probe served at <c>/healthz</c>. Returns a plain
    /// <c>200 OK</c> with a short body. Reserved name <c>healthz</c> bypasses
    /// tenant resolution.
    /// </summary>
    public partial class Healthz : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.StatusCode = 200;
            Response.ContentType = "text/plain";
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Write("OK");
            Response.End();
        }
    }
}
