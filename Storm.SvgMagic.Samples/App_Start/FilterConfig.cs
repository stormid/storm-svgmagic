using System.Web;
using System.Web.Mvc;

namespace Storm.SvgMagic.Samples
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
