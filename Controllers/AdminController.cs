using Moov2.Orchard.ImportExport.Services;
using Moov2.Orchard.ImportExport.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.ViewModels;
using Orchard.DisplayManagement;
using Orchard.ImportExport;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Moov2.Orchard.ImportExport.Controllers
{
    public class AdminController : Controller
    {
        public Localizer T { get; set; }

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly ICultureFilter _cultureFilter;
        private readonly ICultureManager _cultureManager;
        private readonly IExportSpecificService _exportSpecificService;
        private readonly ISiteService _siteService;

        public AdminController(IContentDefinitionManager contentDefinitionManager, IContentManager contentManager, IExportSpecificService exportSpecificService, ISiteService siteService, ICultureFilter cultureFilter, ICultureManager cultureManager, IShapeFactory shapeFactory, IOrchardServices orchardServices)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _cultureFilter = cultureFilter;
            _cultureManager = cultureManager;
            _exportSpecificService = exportSpecificService;
            _siteService = siteService;

            Shape = shapeFactory;
            Services = orchardServices;

            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }

        public ActionResult ExportContent(ListExportContentsViewModel model, PagerParameters pagerParameters)
        {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var versionOptions = VersionOptions.Latest;
            switch (model.Options.ContentsStatus)
            {
                case ContentsStatus.Published:
                    versionOptions = VersionOptions.Published;
                    break;
                case ContentsStatus.Draft:
                    versionOptions = VersionOptions.Draft;
                    break;
                case ContentsStatus.AllVersions:
                    versionOptions = VersionOptions.AllVersions;
                    break;
                default:
                    versionOptions = VersionOptions.Latest;
                    break;
            }

            var query = _contentManager.Query(versionOptions, GetListableTypes(false).Select(ctd => ctd.Name).ToArray());
            if ("--content--".Equals(model.TypeName, StringComparison.InvariantCultureIgnoreCase))
            {
                model.HasCommonPartOrdering = true;
                model.TypeDisplayName = "--content--";
            }
            else if (!string.IsNullOrEmpty(model.TypeName))
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.TypeName);
                if (contentTypeDefinition == null)
                    return HttpNotFound();

                model.TypeDisplayName = !string.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;
                query = query.ForType(model.TypeName);
                model.HasCommonPartOrdering = contentTypeDefinition.Parts.Any(x => "CommonPart".Equals(x.PartDefinition.Name));
            }
            else
            {
                model.HasCommonPartOrdering = false;
            }

            if (!String.IsNullOrWhiteSpace(model.Options.SelectedCulture))
            {
                query = _cultureFilter.FilterCulture(query, model.Options.SelectedCulture);
            }

            if (model.HasCommonPartOrdering)
            {
                switch (model.Options.OrderBy)
                {
                    case ContentsOrder.Modified:
                        query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                        break;
                    case ContentsOrder.Published:
                        query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                        break;
                    case ContentsOrder.Created:
                        query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                        break;
                }
            }

            model.Options.SelectedFilter = model.TypeName;
            model.Options.FilterOptions = GetListableTypes(false)
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            model.Options.Cultures = _cultureManager.ListCultures();

            var maxPagedCount = _siteService.GetSiteSettings().MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;
            var pagerShape = Shape.Pager(pager).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));

            var viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .TypeDisplayName(model.TypeDisplayName ?? "")
                .HasCommonPartOrdering(model.HasCommonPartOrdering);

            return View(viewModel);
        }

        private IEnumerable<ContentTypeDefinition> GetListableTypes(bool andContainable)
        {
            var types = _contentDefinitionManager.ListTypeDefinitions().Where(x =>
            {
                if (x.Settings.TryGetValue("Stereotype", out string stereotype))
                {
                    return !"widget".Equals(stereotype, StringComparison.InvariantCultureIgnoreCase);
                }
                else
                {
                    return true;
                }
            });
            return types;
        }

        [HttpPost, ActionName("ExportContent")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ExportContentFilterPOST(ContentOptions options)
        {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null)
            {
                routeValues["Options.SelectedCulture"] = options.SelectedCulture; //todo: don't hard-code the key
                routeValues["Options.OrderBy"] = options.OrderBy; //todo: don't hard-code the key
                routeValues["Options.ContentsStatus"] = options.ContentsStatus; //todo: don't hard-code the key
                if ("--content--".Equals(options.SelectedFilter, StringComparison.OrdinalIgnoreCase) || GetListableTypes(false).Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase)))
                {
                    routeValues["id"] = options.SelectedFilter;
                }
                else
                {
                    routeValues.Remove("id");
                }
            }

            return RedirectToAction("ExportContent", routeValues);
        }

        [HttpPost, ActionName("ExportContent")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ExportContentPOST(ContentOptions options, IEnumerable<int> itemIds, string returnUrl)
        {
            if (itemIds != null)
            {
                var checkedContentItems = _contentManager.GetMany<ContentItem>(itemIds, VersionOptions.Latest, QueryHints.Empty);
                foreach (var content in checkedContentItems)
                {
                    if (!Services.Authorizer.Authorize(Permissions.Export, content, T("Couldn't export selected content.")))
                    {
                        return new HttpUnauthorizedResult();
                    }
                }

                var exportPath = _exportSpecificService.Export(itemIds.ToList()); ;
                return File(exportPath, "text/xml", "export.xml");
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("ExportContent"));
        }

        public ActionResult ExportSingle(int itemId)
        {
            var content = _contentManager.Get(itemId);
            if (content != null)
            {
                if (!Services.Authorizer.Authorize(Permissions.Export, content, T("Couldn't export selected content.")))
                {
                    return new HttpUnauthorizedResult();
                }
                var exportPath = _exportSpecificService.Export(itemId);
                return File(exportPath, "text/xml", "export.xml");
            }
            return HttpNotFound();
        }
    }
}