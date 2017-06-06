﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Diagnostics;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Locations;
using Microsoft.Templates.Core.Mvvm;
using Microsoft.Templates.UI.Controls;
using Microsoft.Templates.UI.Resources;
using Microsoft.Templates.UI.Services;
using Microsoft.Templates.UI.Views.NewItem;
using System.Windows.Controls;
using Microsoft.Templates.UI.ViewModels.Common;
using System.IO;
using System.Xml.Linq;

namespace Microsoft.Templates.UI.ViewModels.NewItem
{
    public class MainViewModel : Observable
    {
        //private bool _canGoBack;
        //private bool _canGoForward;
        //private bool _templatesReady;
        //private bool _hasValidationErrors;
        //private bool _templatesAvailable;

        public static MainViewModel Current;
        public MainView MainView;
        public TemplateType ConfigTemplateType;
        public string ConfigFramework;
        public string ConfigProjectType;

        private StatusViewModel _status = StatusControl.EmptyStatus;

        public StatusViewModel Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _wizardVersion;
        public string WizardVersion
        {
            get => _wizardVersion;
            set => SetProperty(ref _wizardVersion, value);
        }

        private string _templatesVersion;
        public string TemplatesVersion
        {
            get => _templatesVersion;
            set => SetProperty(ref _templatesVersion, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        //private Visibility _infoShapeVisibility = Visibility.Collapsed;
        //public Visibility InfoShapeVisibility
        //{
        //    get => _infoShapeVisibility;
        //    set => SetProperty(ref _infoShapeVisibility, value);
        //}

        private Visibility _loadingContentVisibility = Visibility.Visible;
        public Visibility LoadingContentVisibility
        {
            get => _loadingContentVisibility;
            set => SetProperty(ref _loadingContentVisibility, value);
        }

        private Visibility _loadedContentVisibility = Visibility.Collapsed;
        public Visibility LoadedContentVisibility
        {
            get => _loadedContentVisibility;
            set => SetProperty(ref _loadedContentVisibility, value);
        }

        private Visibility _noContentVisibility = Visibility.Collapsed;
        public Visibility NoContentVisibility
        {
            get => _noContentVisibility;
            set => SetProperty(ref _noContentVisibility, value);
        }

        //private Visibility _createButtonVisibility = Visibility.Collapsed;
        //public Visibility CreateButtonVisibility
        //{
        //    get => _createButtonVisibility;
        //    set => SetProperty(ref _createButtonVisibility, value);
        //}

        //private Visibility _nextButtonVisibility = Visibility.Visible;
        //public Visibility NextButtonVisibility
        //{
        //    get => _nextButtonVisibility;
        //    set => SetProperty(ref _nextButtonVisibility, value);
        //}

        //private Visibility _wizardInfoVisibility = Visibility.Collapsed;
        //public Visibility WizardInfoVisibility
        //{
        //    get => _wizardInfoVisibility;
        //    set => SetProperty(ref _wizardInfoVisibility, value);
        //}

        //private RelayCommand _cancelCommand;
        //private RelayCommand _goBackCommand;
        //private RelayCommand _nextCommand;
        //private RelayCommand _createCommand;

        //public RelayCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(OnCancel));
        //public RelayCommand BackCommand => _goBackCommand ?? (_goBackCommand = new RelayCommand(OnGoBack, () => _canGoBack));
        //public RelayCommand NextCommand => _nextCommand ?? (_nextCommand = new RelayCommand(OnNext, () => _templatesAvailable && _canGoForward));
        //public RelayCommand CreateCommand => _createCommand ?? (_createCommand = new RelayCommand(OnCreate, CanCreate));

        //private bool CanCreate()
        //{
        //if (!_templatesReady)
        //{
        //    return false;
        //}
        //if (_hasValidationErrors)
        //{
        //    return false;
        //}
        //if (String.IsNullOrEmpty(ProjectTemplates.HomeName))
        //{
        //    Status = new StatusViewModel(StatusType.Error, StringRes.ErrorNoHomePage);
        //    return false;
        //}
        //CleanStatus();
        //    return true;
        //}

        public NewItemSetupViewModel NewItemSetup { get; private set; } = new NewItemSetupViewModel();

        //public ProjectTemplatesViewModel ProjectTemplates { get; private set; } = new ProjectTemplatesViewModel();

        //public ObservableCollection<SummaryLicenseViewModel> SummaryLicenses { get; } = new ObservableCollection<SummaryLicenseViewModel>();

        public MainViewModel(MainView mainView)
        {
            MainView = mainView;
            Current = this;
        }        

        public async Task InitializeAsync(TemplateType templateType)
        {
            ConfigTemplateType = templateType;
            GetProjectFramework();


            GenContext.ToolBox.Repo.Sync.SyncStatusChanged += Sync_SyncStatusChanged;

            //SummaryLicenses.CollectionChanged += (s, o) => { OnPropertyChanged(nameof(SummaryLicenses)); };

            try
            {
                Title = String.Format(StringRes.NewItemTitle_SF, ConfigTemplateType.ToString().ToLower());
                await GenContext.ToolBox.Repo.SynchronizeAsync();

                TemplatesVersion = GenContext.ToolBox.TemplatesVersion;
                WizardVersion = GenContext.ToolBox.WizardVersion;
            }
            catch (Exception ex)
            {
                Status = new StatusViewModel(StatusType.Information, StringRes.ErrorSync, true);

                await AppHealth.Current.Error.TrackAsync(ex.ToString());
                await AppHealth.Current.Exception.TrackAsync(ex);
            }
            finally
            {
                LoadingContentVisibility = Visibility.Collapsed;
                LoadedContentVisibility = Visibility.Visible;
            }
        }

        private void GetProjectFramework()
        {
            var path = Path.Combine(GenContext.Current.ProjectPath, "Package.appxmanifest");
            var manifest = XElement.Load(path);

            var metadata = manifest.Descendants().FirstOrDefault(e => e.Name.LocalName == "Metadata");
            ConfigProjectType = metadata?.Descendants().FirstOrDefault(m => m.Attribute("Name").Value == "projectType")?.Attribute("Value")?.Value;
            ConfigFramework = metadata?.Descendants().FirstOrDefault(m => m.Attribute("Name").Value == "framework")?.Attribute("Value")?.Value;
        }

        public void UnsuscribeEventHandlers()
        {
            GenContext.ToolBox.Repo.Sync.SyncStatusChanged -= Sync_SyncStatusChanged;
        }

        //public void RebuildLicenses()
        //{
        //    var userSelection = CreateUserSelection();
        //    var genItems = GenComposer.Compose(userSelection);

        //    var genLicenses = genItems
        //                        .SelectMany(s => s.Template.GetLicenses())
        //                        .Distinct(new TemplateLicenseEqualityComparer())
        //                        .ToList();

        //    SyncLicenses(genLicenses);
        //}

        //public void SetTemplatesReadyForProjectCreation()
        //{
        //    _templatesReady = true;
        //    CreateCommand.OnCanExecuteChanged();
        //}

        //public void EnableGoForward()
        //{
        //    _canGoForward = true;
        //    NextCommand.OnCanExecuteChanged();
        //}

        private async void Sync_SyncStatusChanged(object sender, SyncStatus status)
        {

            Status = new StatusViewModel(StatusType.Information, GetStatusText(status), true);

            if (status == SyncStatus.Updated)
            {
                TemplatesVersion = GenContext.ToolBox.Repo.TemplatesVersion;
                CleanStatus();

                //_templatesAvailable = true;
                NewItemSetup.Initialize();
            }

            //if (status == SyncStatus.OverVersion)
            //{
            //    MainView.Dispatcher.Invoke(() =>
            //    {
            //        Status = new StatusViewModel(StatusType.Warning, StringRes.StatusOverVersionContent);
            //    });
            //}

            //if (status == SyncStatus.OverVersionNoContent)
            //{
            //    MainView.Dispatcher.Invoke(() =>
            //    {
            //        Status = new StatusViewModel(StatusType.Error, StringRes.StatusOverVersionNoContent);
            //        _templatesAvailable = false;
            //        NextCommand.OnCanExecuteChanged();
            //    });
            //}

            //if (status == SyncStatus.UnderVersion)
            //{
            //    MainView.Dispatcher.Invoke(() =>
            //    {
            //        Status = new StatusViewModel(StatusType.Error, StringRes.StatusLowerVersionContent);
            //        _templatesAvailable = false;
            //        NextCommand.OnCanExecuteChanged();
            //    });
            //}
        }

        private string GetStatusText(SyncStatus status)
        {
            switch (status)
            {
                case SyncStatus.Updating:
                    return StringRes.StatusUpdating;
                case SyncStatus.Updated:
                    return StringRes.StatusUpdated;
                case SyncStatus.Adquiring:
                    return StringRes.StatusAdquiring;
                case SyncStatus.Adquired:
                    return StringRes.StatusAdquired;
                case SyncStatus.Preparing:
                    return StringRes.StatusPreparing;
                case SyncStatus.Prepared:
                    return StringRes.StatusPrepared;
                default:
                    return string.Empty;
            }
        }

        private void OnCancel()
        {
            MainView.DialogResult = false;
            MainView.Result = null;

            MainView.Close();
        }

        private void OnNext()
        {
            //if (CheckProjectSetupChanged())
            //{
            //    ProjectTemplates.ResetSelection();

            //    CleanStatus();
            //}

            //NavigationService.Navigate(new ProjectTemplatesView());

            //_canGoBack = true;

            //BackCommand.OnCanExecuteChanged();

            //CreateButtonVisibility = Visibility.Visible;
            //NextButtonVisibility = Visibility.Collapsed;
        }

        //private void OnGoBack()
        //{
        //    NavigationService.GoBack();

        //    _canGoBack = false;
        //    _templatesReady = false;

        //    BackCommand.OnCanExecuteChanged();

        //    CreateButtonVisibility = Visibility.Collapsed;
        //    NextButtonVisibility = Visibility.Visible;
        //}

        //private bool CheckProjectSetupChanged()
        //{
        //    if (ProjectTemplates.HasTemplatesAdded && (FrameworkChanged || ProjectTypeChanged))
        //    { 
        //        return true;
        //    }
        //    return false;
        //}

        //private bool FrameworkChanged => ProjectTemplates.ContextFramework.Name != ProjectSetup.SelectedFramework.Name;

        //private bool ProjectTypeChanged => ProjectTemplates.ContextProjectType.Name != ProjectSetup.SelectedProjectType.Name;

        //private void OnCreate()
        //{
        //    var userSelection = CreateUserSelection();

        //    MainView.DialogResult = true;
        //    MainView.Result = userSelection;

        //    MainView.Close();
        //}

        //private UserSelection CreateUserSelection()
        //{
        //    var userSelection = new UserSelection()
        //    {
        //        ProjectType = ProjectSetup.SelectedProjectType?.Name,
        //        Framework = ProjectSetup.SelectedFramework?.Name,
        //        HomeName = ProjectTemplates.HomeName
        //    };

        //    userSelection.Pages.AddRange(ProjectTemplates.SavedPages.Select(sp => sp.UserSelection));
        //    userSelection.Features.AddRange(ProjectTemplates.SavedFeatures.Select(sf => sf.UserSelection));

        //    return userSelection;
        //}

        //private void SyncLicenses(IEnumerable<TemplateLicense> licenses)
        //{
        //    var toRemove = new List<SummaryLicenseViewModel>();

        //    foreach (var summaryLicense in SummaryLicenses)
        //    {
        //        if (!licenses.Any(l => l.Url == summaryLicense.Url))
        //        {
        //            toRemove.Add(summaryLicense);
        //        }
        //    }

        //    foreach (var licenseToRemove in toRemove)
        //    {
        //        SummaryLicenses.Remove(licenseToRemove);
        //    }

        //    foreach (var license in licenses)
        //    {
        //        if (!SummaryLicenses.Any(l => l.Url == license.Url))
        //        {
        //            SummaryLicenses.Add(new SummaryLicenseViewModel(license));
        //        }
        //    }
        //}

        //public void SetValidationErrors(string errorMessage, StatusType statusType = StatusType.Error)
        //{
        //    Status = new StatusViewModel(statusType, errorMessage);
        //    _hasValidationErrors = true;
        //    CreateCommand.OnCanExecuteChanged();
        //}

        public void CleanStatus(bool cleanValidationError = false)
        {
            Status = StatusControl.EmptyStatus;
            if (cleanValidationError)
            {
                //_hasValidationErrors = false;
                //CreateCommand.OnCanExecuteChanged();
            }
        }
    }
}