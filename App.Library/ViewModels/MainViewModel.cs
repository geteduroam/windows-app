using App.Library.Language;

namespace App.Library.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        public ILanguageText LanguageText { get; }

        public NotifyPropertyChanged ActiveContent { get; }

        public MainViewModel()
        {
            this.LanguageText = new LanguageText(@"App.Library.Language.LanguageTexts.csv", "EN");

            this.ActiveContent = new DiscoveryViewModel(this.LanguageText);
        }
    }
}