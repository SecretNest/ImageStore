using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretNest.ImageStore
{
    public class DoubleBufferedListView : System.Windows.Forms.ListView
    {
        private Timer changeDelayTimer = null;
        public DoubleBufferedListView()
           : base()
        {
            // Set common properties for our listviews
            if (!SystemInformation.TerminalServerSession)
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.ResizeRedraw, true);
            }
        }

        /// <summary>
        /// Make sure to properly dispose of the timer
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && changeDelayTimer != null)
            {
                changeDelayTimer.Tick -= ChangeDelayTimerTick;
                changeDelayTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Hack to avoid lots of unnecessary change events by marshaling with a timer:
        /// http://stackoverflow.com/questions/86793/how-to-avoid-thousands-of-needless-listview-selectedindexchanged-events
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (changeDelayTimer == null)
            {
                changeDelayTimer = new Timer();
                changeDelayTimer.Tick += ChangeDelayTimerTick;
                changeDelayTimer.Interval = 40;
            }
            // When a new SelectedIndexChanged event arrives, disable, then enable the
            // timer, effectively resetting it, so that after the last one in a batch
            // arrives, there is at least 40 ms before we react, plenty of time 
            // to wait any other selection events in the same batch.
            changeDelayTimer.Enabled = false;
            changeDelayTimer.Enabled = true;
        }

        private void ChangeDelayTimerTick(object sender, EventArgs e)
        {
            changeDelayTimer.Enabled = false;
            base.OnSelectedIndexChanged(new EventArgs());
        }
    }
}
