using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Dynamic;

namespace OpenVid.Extensions
{
    public static class SiteMap
    {
        public static PageLocation Home { get; set; } = new PageLocation("Home", "Index");
        public static PageLocation Playback_Play { get; set; } = new PageLocation("Playback", "Play", "Index");
        public static PageLocation Playback_Search { get; set; } = new PageLocation("Playback", "Search", "Index");
        public static PageLocation Playback_Update { get; set; } = new PageLocation("Playback", "Update", "Index");
        public static PageLocation Playback_VideoList { get; set; } = new PageLocation("Playback", "VideoList", "Index");
        public static PageLocation Playback_TagGetTags { get; set; } = new PageLocation("Playback", "Tag", "GetTags");
        public static PageLocation Playback_Thumbnail { get; set; } = new PageLocation("Playback", "Thumbnail", "Index");
        public static PageLocation VideoManagement_Import { get; set; } = new PageLocation("VideoManagement", "Import", "Index");
        public static PageLocation VideoManagement_ImportSave { get; set; } = new PageLocation("VideoManagement", "Import", "Save");
        public static PageLocation VideoManagement_Destroy { get; set; } = new PageLocation("VideoManagement", "Destroy", "Index");
        public static PageLocation VideoManagement_UploadUI { get; set; } = new PageLocation("VideoManagement", "Upload", "Index");
        public static PageLocation VideoManagement_Upload { get; set; } = new PageLocation("VideoManagement", "Upload", "Upload");
        public static PageLocation FlagDelete { get; set; } = new PageLocation("VideoManagement", "FlagDelete", "Index"); 
        public static PageLocation Catalog_Import { get; set; } = new PageLocation("Catalog", "Import", "Index");
        public static PageLocation Catalog_Import_Upload { get; set; } = new PageLocation("Catalog", "Import", "Upload");
        public static PageLocation Catalog_Import_Queue { get; set; } = new PageLocation("Catalog", "Import", "Queue");
        public static PageLocation Catalog_Curation { get; set; } = new PageLocation("Catalog", "Curation", "Index");


        public static List<PageLocation> AdminMenu = new List<PageLocation>()
        {
            Catalog_Import,
            Catalog_Curation
        };

        public static string Action(this IUrlHelper urlHelper, PageLocation page)
        {
            return urlHelper.Action(page.Action, page.Controller, new { page.Area });
        }
        public static string Action(this IUrlHelper urlHelper, PageLocation page, object routeValues)
        {
            return urlHelper.Action(page.Action, page.Controller, MergeRouteValues(page, routeValues));
        }

        public static IHtmlContent ActionLink(this IHtmlHelper helper, string linkText, PageLocation page)
        {
            return helper.ActionLink(linkText, page.Action, page.Controller, page);
        }

        public static IHtmlContent ActionLink(this IHtmlHelper helper, string linkText, PageLocation page, object routeValues)
        {
            return helper.ActionLink(linkText, page.Action, page.Controller, MergeRouteValues(page, routeValues));
        }

        private static object MergeRouteValues(PageLocation page, object routeValues)
        {
            dynamic expando = new ExpandoObject();
            var result = expando as IDictionary<string, object>;
            foreach (System.Reflection.PropertyInfo fi in routeValues.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(routeValues, null);
            }
            result["Area"] = page.Area;

            return result;
        }
    }
}
