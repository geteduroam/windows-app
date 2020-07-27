using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
    class PasswordTextBox : HintTextBox
    {


        public PasswordTextBox() : base()
        {
        }

        protected override void txt_Enter(object sender, EventArgs e)
        {
            base.txt_Enter(sender, e);
            PasswordChar = '*';

        }

        protected override void txt_Leave(object sender, EventArgs e)
        {
            base.txt_Leave(sender, e);
            if (HintActive)
            {
                PasswordChar = '\0';
            }
        }
    }


}
