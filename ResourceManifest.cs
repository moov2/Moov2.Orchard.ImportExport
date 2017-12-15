using Orchard.UI.Resources;

namespace Moov2.Orchard.ImportExport
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            builder.Add().DefineScript("ImportExport.ExportContentAdmin").SetUrl("exportcontent.admin.js").SetDependencies("JQuery");
            builder.Add().DefineStyle("ImportExport.ExportContentAdmin").SetUrl("exportcontent.admin.css");
        }
    }
}