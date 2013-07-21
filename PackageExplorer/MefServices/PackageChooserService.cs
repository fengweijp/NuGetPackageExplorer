﻿using System;
using System.ComponentModel.Composition;
using NuGet;
using NuGetPackageExplorer.Types;
using PackageExplorerViewModel;

namespace PackageExplorer
{
    [Export(typeof(IPackageChooser))]
    internal class PackageChooserService : IPackageChooser
    {
        private PackageChooserDialog _dialog;
        private PackageChooserDialog _pluginDialog;
        private PackageChooserViewModel _pluginViewModel;
        private PackageChooserViewModel _viewModel;

        [Import]
        public IPackageViewModelFactory ViewModelFactory { get; set; }

        [Import]
        public IUIServices UIServices { get; set; }

        [Import]
        public IPackageDownloader PackageDownloader { get; set; }

        [Import]
        public Lazy<MainWindow> Window { get; set; }

        #region IPackageChooser Members

        public PackageInfo SelectPackage(string searchTerm)
        {
            if (_dialog == null)
            {
                _viewModel = ViewModelFactory.CreatePackageChooserViewModel(null);
                _dialog = new PackageChooserDialog(_viewModel)
                          {
                              Owner = Window.Value
                          };
                _dialog.PackageDownloadRequested += OnPackageDownloadRequested;
            }

            _dialog.ShowDialog(searchTerm);
            return _dialog.SelectedPackage;
        }

        private async void OnPackageDownloadRequested(object sender, EventArgs e)
        {
            var dialog = (PackageChooserDialog)sender;
            PackageInfo packageInfo = dialog.SelectedPackage;
            if (packageInfo != null)
            {
                string selectedFilePath;
                int selectedIndex;

                string packageName = packageInfo.Id + "." + packageInfo.Version.ToString() + NuGet.Constants.PackageExtension;
                string title = "Save " + packageName;
                const string filter = "NuGet package file (*.nupkg)|*.nupkg|All files (*.*)|*.*";

                bool accepted = UIServices.OpenSaveFileDialog(
                    title,
                    packageName,
                    null,
                    filter,
                    overwritePrompt: true,
                    selectedFilePath: out selectedFilePath,
                    selectedFilterIndex: out selectedIndex);

                if (accepted)
                {
                    if (selectedIndex == 1 &&
                        !selectedFilePath.EndsWith(NuGet.Constants.PackageExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedFilePath += NuGet.Constants.PackageExtension;
                    }

                    await PackageDownloader.Download(selectedFilePath, packageInfo.DownloadUrl, packageInfo.Id, packageInfo.Version);                    
                }
            }
        }

        public PackageInfo SelectPluginPackage()
        {
            if (_pluginDialog == null)
            {
                _pluginViewModel = ViewModelFactory.CreatePackageChooserViewModel(NuGetConstants.PluginFeedUrl);
                _pluginDialog = new PackageChooserDialog(_pluginViewModel)
                                {
                                    Owner = Window.Value
                                };
            }

            _pluginDialog.ShowDialog();
            return _pluginDialog.SelectedPackage;
        }

        public void Dispose()
        {
            if (_dialog != null)
            {
                _dialog.ForceClose();
                _viewModel.Dispose();
            }
        }

        #endregion
    }
}