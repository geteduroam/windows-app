using App.Library.Command;

using System;
using System.Threading.Tasks;

using SharedResources = EduRoam.Localization.Resources;

namespace App.Library.ViewModels
{
    internal class ConfirmViewModel : BaseViewModel
    {
        private readonly Action onConfirm;
        private readonly Action onDeny;
        public string ConfirmText { get; set; }
        public bool ShowDenyButton { get; set; }
        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand DenyCommand { get; }

        public ConfirmViewModel(MainViewModel owner, string textBlockText, bool confirmOnly, Action onConfirm, Action onDeny) : base(owner)
        {
            this.ConfirmText = textBlockText;
            this.ShowDenyButton = !confirmOnly;
            this.onConfirm = onConfirm;
            this.onDeny = onDeny;
            this.ConfirmCommand = new DelegateCommand(this.Confirm);
            this.DenyCommand = new DelegateCommand(this.Deny);
        }

        public override string PageTitle { get; }
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
        public override bool ShowLogo => true;

        private void Confirm()
        {
            // Invoke the confirm callback
            this.onConfirm?.Invoke();
        }

        private void Deny()
        {
            // Invoke the deny callback
            this.onDeny?.Invoke();
        }

        public string ConfirmButtonText => this.ShowDenyButton ? SharedResources.YesText : SharedResources.OkeText;


    }
}
