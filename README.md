# Moov2.Orchard.ImportExport

[Orchard](http://www.orchardproject.net/) module that enhances the functionality of the existing import export module. The primary goal of this module is to let content admins export specific content items instead of having to export all content of a particular type and then wrangle with XML to get the desired content items.

## Status

*Currently under development.*

## Getting Set Up

Download module source code and place within the "Modules" directory of your Orchard installation.

Alternatively, use the command below to add this module as a sub-module within your Orchard project.

    git submodule add git@github.com:moov2/Moov2.Orchard.ImportExport.git modules/Moov2.Orchard.ImportExport

# Usage

Enable the "Moov2.Orchard.ImportExport" module. 

Once enabled, the Import/Export section within the Admin dashboard will have an additional tab named "Export Content". This tab contains a similar view to the content list. However, it contains all content types (not only `Listable` content types) and has the ability to select items that can be bulk exported. Shown below is a demo of this new tab.

![alt text](https://raw.githubusercontent.com/moov2/Moov2.Orchard.ImportExport/master/docs/demo-export-content-tab.gif "Example of Export Content tab")

This module will also add actions to cater for export a specific content item. When viewing the content list within the admin area, this module will display an "Export" option in the summary admin menu for a content item. This option will only display if the user is authorised to export the content item.

![alt text](https://raw.githubusercontent.com/moov2/Moov2.Orchard.ImportExport/master/docs/screenshot-content-list-export-option.png "Screenshot of Export option on content list")

When editing a content item within the admin area, if the user is authorised, there is the option to export the content item. Any unsaved changes for the content item won't appear in the export until the content item is saved.

![alt text](https://raw.githubusercontent.com/moov2/Moov2.Orchard.ImportExport/master/docs/screenshot-edit-content-item-export-option.png "Screenshot of Export option on when editing content item.")