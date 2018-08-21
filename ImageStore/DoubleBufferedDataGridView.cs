using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretNest.ImageStore
{
    public class DoubleBufferedDataGridView : System.Windows.Forms.DataGridView
    {
        private Timer changeDelayTimer = null;
        public event EventHandler SelectedIndexChanged;

        public DoubleBufferedDataGridView()
            : base()
        {
            if (!SystemInformation.TerminalServerSession)
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.ResizeRedraw, true);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && changeDelayTimer != null)
            {
                changeDelayTimer.Tick -= ChangeDelayTimerTick;
                changeDelayTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnRowStateChanged(int rowIndex, DataGridViewRowStateChangedEventArgs e)
        {
            base.OnRowStateChanged(rowIndex, e);

            if (e.StateChanged == DataGridViewElementStates.Selected)
            {
                if (changeDelayTimer == null)
                {
                    changeDelayTimer = new Timer();
                    changeDelayTimer.Tick += ChangeDelayTimerTick;
                    changeDelayTimer.Interval = 40;
                }
                changeDelayTimer.Enabled = false;
                changeDelayTimer.Enabled = true;
            }
        }

        private void ChangeDelayTimerTick(object sender, EventArgs e)
        {
            changeDelayTimer.Enabled = false;
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
