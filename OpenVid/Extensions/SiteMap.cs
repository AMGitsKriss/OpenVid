using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Dynamic;

namespace OpenVid.Extensions
{
    public static class SiteMap
    {
        public static PageLocation PlaybackPlay { get; set; } = new PageLocation("Playback", "Play", "Index");
        public static PageLocation PlaybackUpdate { get; set; } = new PageLocation("Playback", "Update", "Index");
        public static PageLocation PlaybackVideoList { get; set; } = new PageLocation("Playback", "VideoList", "Index");

        public static string Action(this IUrlHelper urlHelper, PageLocation page)
        {
            return urlHelper.Action(page.Action, page.Controller, new { page.Area });
        }
        public static string Action(this IUrlHelper urlHelper, PageLocation page, object routeValues)
        {
            dynamic expando = new ExpandoObject();
            var result = expando as IDictionary<string, object>;
            foreach (System.Reflection.PropertyInfo fi in routeValues.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(routeValues, null);
            }
            result["Area"] = page.Area;

            return urlHelper.Action(page.Action, page.Controller, result);
        }
    }
}
