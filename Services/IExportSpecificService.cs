using Orchard;
using System.Collections.Generic;

namespace Moov2.Orchard.ImportExport.Services
{
    public interface IExportSpecificService : IDependency
    {
        string Export(IList<int> contentItemIds);
        string Export(int contentItemId);
    }
}
