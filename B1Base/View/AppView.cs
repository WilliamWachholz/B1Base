using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace B1Base.View
{
    public abstract partial class AppView : Form
    {
        public AppView()
        {
            InitializeComponent();
        }

        protected virtual ProgressBar ProgressBar { get { return new ProgressBar(); } }

        protected abstract TextBox TextBoxLog { get; }

        public virtual void Ready() { }

        public void AddTextLog(string text)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                TextBoxLog.AppendText(string.Format("\r\n{0}", text));
                this.Refresh();
                this.Update();
            }));
        }

        public void ClearLog()
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                TextBoxLog.Clear();
                this.Refresh();
                this.Update();
            }));
        }

        public void SetProgressBarMax(int value)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                ProgressBar.Maximum = value;
            }));
        }

        public void IncrementProgressBar(int value)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                ProgressBar.Increment(value);
                this.Refresh();
                this.Update();
            }));
        }

        public void ClearProgressBar()
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                ProgressBar.Value = 0;
                this.Refresh();
                this.Update();
            }));
        }
    }
}
