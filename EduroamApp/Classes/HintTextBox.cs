using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace EduroamApp
{
    class HintTextBox : TextBox
    {

        private string hint;
        public string Hint 
        {
            get => hint;
            set
            {
                hint = value;
                ActivateHint();
            }
        }
  
        public bool HintActive;
        public bool IgnoreChange;


        public HintTextBox() : base()
        {
            this.Enter += new EventHandler(this.txt_Enter);
            this.Leave += new EventHandler(this.txt_Leave);
            Hint = "";
            ActivateHint();
        }



        // removes helping text when field is in focus
        protected virtual void txt_Enter(object sender, EventArgs e)
        {
            if (HintActive)
            {
                DeactivateHint();
            }
        }

        protected virtual void txt_Leave(object sender, EventArgs e)
        {
            if (Text == "")
            {
                ActivateHint();
            }

        }

        private void ActivateHint()
        {
            IgnoreChange = true;
            Text = Hint;
            IgnoreChange = false;
            ForeColor = SystemColors.GrayText;
            HintActive = true;
        }

        private void DeactivateHint()
        {
            IgnoreChange = true;
            Text = "";
            IgnoreChange = false;
            ForeColor = SystemColors.ControlText;
            HintActive = false;
        }







    }





}
