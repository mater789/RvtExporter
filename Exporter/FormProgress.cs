using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Exporter
{
    public partial class FormProgress : Form
    {
        public FormProgress()
        {
            InitializeComponent();
        }

        public void SetProgress(int nValue)
        {
            if (progressBar.Value != nValue)
            {
                progressBar.Value = nValue;
                progressBar.Update();
            }
        }
    }
}
