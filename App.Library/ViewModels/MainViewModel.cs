using App.Library.Command;
using App.Library.Language;

using EduRoam.Connect;

using System;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged, IDisposable
    {
        private readonly IdentityProviderDownloader idpDownloader;

        public MainViewModel()
        {
            this.LanguageText = new LanguageText(@"App.Library.Language.LanguageTexts.csv", "EN");
            this.NewProfileCommand = new DelegateCommand(this.NewProfileCommandAction, this.CanNewProfileCommandAction);
            this.idpDownloader = new IdentityProviderDownloader();
            this.State = new ApplicationState();

            this.IsLoading = true;

            Task.Run(
                async () =>
                {
                    await this.idpDownloader.LoadProviders(useGeodata: true);
                    this.IsLoading = false;
                    this.CallPropertyChanged(string.Empty);
                    this.NewProfileCommand.RaiseCanExecuteChanged();
                });
        }

        public ApplicationState State { get; private set; }

        public ILanguageText LanguageText { get; }

        public BaseViewModel ActiveContent { get; private set; }

        public DelegateCommand NewProfileCommand { get; protected set; }

        public bool IsLoading { get; private set; }

        private bool CanNewProfileCommandAction()
        {
            return this.idpDownloader.Loaded;
        }

        private void NewProfileCommandAction()
        {
            this.SetActiveContent(new SelectInstitutionViewModel(this, this.idpDownloader));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this.idpDownloader.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.idpDownloader.Dispose();
            }
        }

        public void SetActiveContent(BaseViewModel viewModel)
        {
            this.ActiveContent = viewModel;
            this.CallPropertyChanged(nameof(this.ActiveContent));
        }
    }
}