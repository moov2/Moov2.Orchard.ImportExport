using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Moov2.Orchard.ImportExport.Recipes.Providers.Builders
{
    // Modified version of Orchard.Recipes.Providers.Builders.ContentStep which allows filtering to specific content
    public class SpecificContentStep : RecipeBuilderStep, ISpecificContentStep
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentDefinitionWriter _contentDefinitionWriter;

        public SpecificContentStep(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            IContentDefinitionWriter contentDefinitionWriter)
        {

            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _contentDefinitionWriter = contentDefinitionWriter;

            VersionHistoryOptions = VersionHistoryOptions.Published;
            DataContentIds = new List<int>();
        }

        public override string Name
        {
            get { return "SpecificContent"; }
        }

        public override LocalizedString DisplayName
        {
            get { return T("Specific Content and Content Definition"); }
        }

        public override LocalizedString Description
        {
            get { return T("Exports specific content items and content item definitions."); }
        }

        public override bool IsVisible
        {
            get { return false; }
        }

        public override int Priority { get { return 20; } }
        public override int Position { get { return 20; } }

        public IList<int> DataContentIds { get; set; }
        public VersionHistoryOptions VersionHistoryOptions { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory)
        {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater)
        {
            return null;
        }

        public override void Configure(RecipeBuilderStepConfigurationContext context)
        {
            var dataContentIds = context.ConfigurationElement.Attr("DataContentIds");
            var versionHistoryOptions = context.ConfigurationElement.Attr<VersionHistoryOptions>("VersionHistoryOptions");

            if (!String.IsNullOrWhiteSpace(dataContentIds))
                DataContentIds = dataContentIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => int.TryParse(x, out int test)).Select(x => int.Parse(x)).ToList();

            VersionHistoryOptions = versionHistoryOptions;
        }

        public override void ConfigureDefault()
        {
            DataContentIds = new List<int>();
            VersionHistoryOptions = VersionHistoryOptions.Published;
        }

        public override void Build(BuildContext context)
        {
            var dataContentIds = DataContentIds;
            var exportVersionOptions = GetContentExportVersionOptions(VersionHistoryOptions);
            var contentItems = dataContentIds.Any()
                ? _orchardServices.ContentManager.GetManyByVersionId(dataContentIds, QueryHints.Empty)
                : Enumerable.Empty<ContentItem>();

            var schemaContentTypes = contentItems.Select(x => x.ContentType).Distinct();

            if (schemaContentTypes.Any())
                context.RecipeDocument.Element("Orchard").Add(ExportMetadata(schemaContentTypes));

            if (contentItems.Any())
                context.RecipeDocument.Element("Orchard").Add(ExportData(schemaContentTypes, contentItems));
        }

        private XElement ExportMetadata(IEnumerable<string> contentTypes)
        {
            var typesElement = new XElement("Types");
            var partsElement = new XElement("Parts");
            var typesToExport = _contentDefinitionManager.ListTypeDefinitions()
                .Where(typeDefinition => contentTypes.Contains(typeDefinition.Name))
                .ToList();
            var partsToExport = new Dictionary<string, ContentPartDefinition>();

            foreach (var contentTypeDefinition in typesToExport.OrderBy(x => x.Name))
            {
                foreach (var contentPartDefinition in contentTypeDefinition.Parts.OrderBy(x => x.PartDefinition.Name))
                {
                    if (partsToExport.ContainsKey(contentPartDefinition.PartDefinition.Name))
                    {
                        continue;
                    }
                    partsToExport.Add(contentPartDefinition.PartDefinition.Name, contentPartDefinition.PartDefinition);
                }
                typesElement.Add(_contentDefinitionWriter.Export(contentTypeDefinition));
            }

            foreach (var part in partsToExport.Values.OrderBy(x => x.Name))
            {
                partsElement.Add(_contentDefinitionWriter.Export(part));
            }

            return new XElement("ContentDefinition", typesElement, partsElement);
        }

        private XElement ExportData(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems)
        {
            var data = new XElement("Content");

            var orderedContentItemsQuery =
                from contentItem in contentItems
                let identity = _orchardServices.ContentManager.GetItemMetadata(contentItem).Identity.ToString()
                orderby identity
                select contentItem;

            var orderedContentItems = orderedContentItemsQuery.ToList();

            foreach (var contentType in contentTypes.OrderBy(x => x))
            {
                var type = contentType;
                var items = orderedContentItems.Where(i => i.ContentType == type);
                foreach (var contentItem in items)
                {
                    var contentItemElement = ExportContentItem(contentItem);
                    if (contentItemElement != null)
                        data.Add(contentItemElement);
                }
            }

            return data;
        }

        private VersionOptions GetContentExportVersionOptions(VersionHistoryOptions versionHistoryOptions)
        {
            switch (versionHistoryOptions)
            {
                case VersionHistoryOptions.Draft:
                    return VersionOptions.Draft;
                case VersionHistoryOptions.Latest:
                    return VersionOptions.Latest;
                case VersionHistoryOptions.Published:
                default:
                    return VersionOptions.Published;
            }
        }

        private XElement ExportContentItem(ContentItem contentItem)
        {
            return _orchardServices.ContentManager.Export(contentItem);
        }
    }

    public interface ISpecificContentStep : IRecipeBuilderStep
    {
        IList<int> DataContentIds { get; set; }
        VersionHistoryOptions VersionHistoryOptions { get; set; }
    }
}