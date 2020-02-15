using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Swashbuckle.Application;

// ReSharper disable once CheckNamespace
namespace Shrooms.Presentation.Api.App_Start
{
    public static class SwaggerConfig
    {
        public static void Setup(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "ShroomsAPI")
                        .Description("Shrooms API documentation")
                        .Contact(cc => cc
                        .Name("Shrooms team")
                        .Email("shrooms@visma.com")
                        .Url("http://www.visma.com"));
                    c.IncludeXmlComments(GetXmlCommentsPath());
                    c.UseFullTypeNameInSchemaIds();
                    c.ResolveConflictingActions(api => api.First());
                    c.RootUrl(req => req.RequestUri.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/").TrimEnd('/'));
                })
                .EnableSwaggerUi();
        }

        private static string GetXmlCommentsPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
            var commentsFile = Path.Combine(baseDir, "bin", commentsFileName);
            return commentsFile;
        }
    }
}