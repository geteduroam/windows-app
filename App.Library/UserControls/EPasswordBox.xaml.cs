using System.Windows;
using System.Windows.Controls;

namespace App.Library.UserControls
{
    public partial class EPasswordBox : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(EPasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool IsPasswordRevealed
        {
            get => (bool)GetValue(IsPasswordRevealedProperty);
            set => SetValue(IsPasswordRevealedProperty, value);
        }

        public static readonly DependencyProperty IsPasswordRevealedProperty =
            DependencyProperty.Register(
                nameof(IsPasswordRevealed),
                typeof(bool),
                typeof(EPasswordBox),
                new PropertyMetadata(false, OnIsPasswordRevealedChanged));

        public bool IsPasswordHidden => !IsPasswordRevealed;

        public EPasswordBox()
        {
            InitializeComponent();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EPasswordBox)d;
            var newValue = e.NewValue as string ?? string.Empty;
            if (control.IsPasswordRevealed)
            {
                control.TextBox.Text = newValue;
            }
            else
            {
                control.PasswordBox.Password = newValue;
            }
        }

        private static void OnIsPasswordRevealedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EPasswordBox)d;
            if ((bool)e.NewValue)
            {
                control.TextBox.Text = control.PasswordBox.Password;
            }
            else
            {
                control.PasswordBox.Password = control.TextBox.Text;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Value = PasswordBox.Password;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            IsPasswordRevealed = !IsPasswordRevealed;
        }
    }
}