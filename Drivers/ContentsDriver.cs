using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System.Collections.Generic;

namespace Moov2.Orchard.ImportExport.Drivers
{
    public class ContentsDriver : ContentPartDriver<ContentPart>
    {
        protected override DriverResult Editor(ContentPart part, dynamic shapeHelper)
        {
            var results = new List<DriverResult> { ContentShape("Content_ExportButton", exportButton => exportButton) };
            return Combined(results.ToArray());
        }
    }
}