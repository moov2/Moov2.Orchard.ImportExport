using Orchard.ImportExport;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Moov2.Orchard.ImportExport
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }

        public string MenuName => "admin";

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("importexport")
                .Add(T("Import/Export"), "42", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu)
        {
            menu.Add(T("Export Content"), "0", item => item.Action("ExportContent", "Admin", new { area = "Moov2.Orchard.ImportExport" }).Permission(Permissions.Export).LocalNav());
        }
    }
}