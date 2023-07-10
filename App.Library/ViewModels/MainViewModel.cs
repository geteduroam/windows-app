using System.Windows.Input;

using App.Library.Command;
using App.Library.Language;

namespace App.Library.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        public ILanguageText LanguageText { get; }

        public NotifyPropertyChanged ActiveContent { get; private set; }

        public MainViewModel()
        {
            this.LanguageText = new LanguageText(@"App.Library.Language.LanguageTexts.csv", "EN");
            this.NewProfileCommand = new DelegateCommand(this.NewProfileCommandAction, () => true);
        }

        public ICommand NewProfileCommand { get; protected set; }

        private void NewProfileCommandAction()
        {
            this.ActiveContent = new SelectInstitutionViewModel(this.LanguageText);
            this.CallPropertyChanged(string.Empty);
        }
    }
}