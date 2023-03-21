using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockUrlApp
{
   public partial class BlockUrlForm : Form
   {

      private readonly BlockUrlController blockUrlController;

      public BlockUrlForm()
      {
         InitializeComponent();

         Application.ApplicationExit += OnApplicationExit;
         AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;

         WindowState = FormWindowState.Minimized;
         ShowInTaskbar = false;
         Visible = false;
         blockUrlController = new BlockUrlController();
         blockUrlController.Start();
      }

      private void OnApplicationExit(object sender, EventArgs e)
      {
         blockUrlController?.Stop();
         blockUrlController?.Dispose();
      }
   }
}
