using System;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class TermsOfUseViewModel : BaseViewModel
    {
        public TermsOfUseViewModel(MainViewModel mainViewModel, string text)
            : base(mainViewModel)
        {
            this.Text = text;
        }

        public string Text { get; }

        protected override bool CanGoNext()
        {
            throw new NotImplementedException();
        }

        protected override Task GoNextAsync()
        {
            throw new NotImplementedException();
        }
    }
}