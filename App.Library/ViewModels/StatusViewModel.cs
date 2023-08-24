using System;
using System.Threading.Tasks;

namespace App.Library.ViewModels
{
    internal class StatusViewModel : BaseViewModel
    {
        public StatusViewModel(MainViewModel owner) : base(owner)
        {

        }

        protected override bool CanNavigateNextAsync()
        {
            return false;
        }

        protected override Task NavigateNextAsync()
        {
            throw new NotImplementedException();
        }

        public override bool ShowNavigatePrevious => false;
        public override bool ShowNavigateNext => false;
        public override bool ShowMenu => true;
        public override bool ShowHelp => true;
    }
}
