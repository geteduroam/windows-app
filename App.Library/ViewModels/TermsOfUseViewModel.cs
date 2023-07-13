using System;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    public class TermsOfUseViewModel : BaseViewModel
    {
        public TermsOfUseViewModel(MainViewModel owner, string text)
            : base(owner)
        {
            this.Text = text;
        }

        public string Text { get; }

        protected override bool CanNavigateNextAsync()
        {
            return false;
        }

        protected override Task NavigateNextAsync()
        {
            throw new NotImplementedException();
        }

        protected override bool CanNavigatePrevious()
        {
            return false;
        }

        protected override Task NavigatePreviousAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}