using Moov2.Orchard.ImportExport.Recipes.Providers.Builders;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System.Collections.Generic;

namespace Moov2.Orchard.ImportExport.Services
{
    public class ExportSpecificService : IExportSpecificService
    {
        private readonly IImportExportService _importExportService;
        private readonly IRecipeBuilder _recipeBuilder;
        private readonly ISpecificContentStep _specificContentStep;

        public ExportSpecificService(IImportExportService importExportService, IRecipeBuilder recipeBuilder, ISpecificContentStep specificContentStep)
        {
            _importExportService = importExportService;
            _recipeBuilder = recipeBuilder;
            _specificContentStep = specificContentStep;
        }

        public string Export(IList<int> contentItemIds)
        {
            var context = new ExportActionContext();
            _specificContentStep.DataContentIds = contentItemIds;
            _specificContentStep.VersionHistoryOptions = VersionHistoryOptions.Latest;
            context.RecipeDocument = _recipeBuilder.Build(new List<IRecipeBuilderStep> { _specificContentStep });
            return _importExportService.WriteExportFile(context.RecipeDocument);
        }

        public string Export(int contentItemId)
        {
            return Export(new List<int> { contentItemId });
        }
    }
}