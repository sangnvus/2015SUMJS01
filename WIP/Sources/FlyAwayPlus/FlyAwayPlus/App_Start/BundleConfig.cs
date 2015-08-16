using System.Web;
using System.Web.Optimization;

namespace FlyAwayPlus
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.10.2.min.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*",
                        "~/Scripts/common.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                      "~/Scripts/sweetalert.min.js",
                      "~/Scripts/blocksit.min.js",
                      "~/Scripts/bootstrap.dropdowns-enhancement.js",
                      "~/Scripts/jquery.simpleWeather.min.js",
                      "~/Scripts/images-loaded.min.js",
                      "~/Scripts/Fancy/jquery.fancybox.pack.js",
                      "~/Scripts/jquery.elastic.source.js",
                      "~/Scripts/jquery.signalR-2.2.0.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/datepicker").Include("~/Scripts/bootstrap-datepicker.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/Site.css",
                      "~/Content/common.css",
                      "~/Content/Fancy/jquery.fancybox.css",
                      "~/Content/bootstrap.dropdowns-enhancement.min.css",
                      "~/Content/sweetalert.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/bootstrap-social.css",
                      "~/Content/bootflat.css",
                      "~/Content/datepicker.css"));
        }
    }
}
