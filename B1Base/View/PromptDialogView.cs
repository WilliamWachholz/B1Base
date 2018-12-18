using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace B1Base.View
{
    public class PromptDialogView
    {
        #region Funções da API

        private const int GW_ENABLEDPOPUP = 6;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        /// <summary>
        ///     Special window handles
        /// </summary>
        public enum SpecialWindowHandles
        {
            // ReSharper disable InconsistentNaming
            /// <summary>
            ///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
            /// </summary>
            HWND_TOP = 0,
            /// <summary>
            ///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
            /// </summary>
            HWND_BOTTOM = 1,
            /// <summary>
            ///     Places the window at the top of the Z order.
            /// </summary>
            HWND_TOPMOST = -1,
            /// <summary>
            ///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
            /// </summary>
            HWND_NOTOPMOST = -2
            // ReSharper restore InconsistentNaming
        }

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
            /// </summary>
            SWP_ASYNCWINDOWPOS = 0x4000,

            /// <summary>
            ///     Prevents generation of the WM_SYNCPAINT message.
            /// </summary>
            SWP_DEFERERASE = 0x2000,

            /// <summary>
            ///     Draws a frame (defined in the window's class description) around the window.
            /// </summary>
            SWP_DRAWFRAME = 0x0020,

            /// <summary>
            ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
            /// </summary>
            SWP_FRAMECHANGED = 0x0020,

            /// <summary>
            ///     Hides the window.
            /// </summary>
            SWP_HIDEWINDOW = 0x0080,

            /// <summary>
            ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOACTIVATE = 0x0010,

            /// <summary>
            ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
            /// </summary>
            SWP_NOCOPYBITS = 0x0100,

            /// <summary>
            ///     Retains the current position (ignores X and Y parameters).
            /// </summary>
            SWP_NOMOVE = 0x0002,

            /// <summary>
            ///     Does not change the owner window's position in the Z order.
            /// </summary>
            SWP_NOOWNERZORDER = 0x0200,

            /// <summary>
            ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            /// </summary>
            SWP_NOREDRAW = 0x0008,

            /// <summary>
            ///     Same as the SWP_NOOWNERZORDER flag.
            /// </summary>
            SWP_NOREPOSITION = 0x0200,

            /// <summary>
            ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            /// </summary>
            SWP_NOSENDCHANGING = 0x0400,

            /// <summary>
            ///     Retains the current size (ignores the cx and cy parameters).
            /// </summary>
            SWP_NOSIZE = 0x0001,

            /// <summary>
            ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOZORDER = 0x0004,

            /// <summary>
            ///     Displays the window.
            /// </summary>
            SWP_SHOWWINDOW = 0x0040,

            // ReSharper restore InconsistentNaming
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint flags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetWindowRect(System.Runtime.InteropServices.HandleRef hwnd, out RECT lpRect);

        #endregion Funções da API

        #region Atributos

        private string m_fileName;
        private string m_filter;
        private DialogResult m_result;

        private string m_title;

        private IntPtr m_dialogOwner = (IntPtr)null;
        private object m_lock = new object();

        protected IntPtr DialogOwner
        {
            get
            {
                lock (m_lock)
                {
                    return m_dialogOwner;
                }
            }
        }

        delegate void DialogFocusDelegate();
        private DialogFocusDelegate m_dialogFocusFunction;

        #endregion Atributos

        #region Construtor

        /// <summary> Construtor
        /// </summary>
        /// <param name="title">Título da Dialog</param>
        public PromptDialogView(string title = "Anexo")
        {
            m_filter = string.Empty;
            m_result = DialogResult.None;
            m_fileName = string.Empty;

            m_title = title;
        }

        #endregion Construtor

        #region Métodos Públicos

        /// <summary> Abre uma OpenFileDialog, permitindo que o usuário escolha o nome do arquivo a ser salvo, desde que esteja dentro das extensões permitidas
        /// </summary>
        /// <param name="fileName">Variável onde será colocado o nome do arquivo escolhido pelo usuário</param>
        /// <param name="extensions">Extensões permitidas</param>
        /// <returns>True = Escolheu Salvar, False = Não escolheu salvar</returns>
        public DialogResult OpenFilePrompt(out string fileName, params string[] extensions)
        {
            foreach (string extension in extensions)
            {
                m_filter = "Arquivos " + "(*" + extension + ")|" + "*" + extension + "|";
            }

            //retira último pipe
            m_filter = m_filter.Substring(0, m_filter.Length - 1);

            OpenFileDialog dialog = new OpenFileDialog();

            try
            {
                Thread t = new Thread(ShowDialog);
                t.SetApartmentState(ApartmentState.STA);

                t.Start(dialog);

                t.Join();

                fileName = m_fileName;
                return m_result;
            }
            catch (Exception)
            {
                fileName = string.Empty;
                return DialogResult.None;
            }
        }

        /// <summary> Abre uma SaveFileDialog, permitindo que o usuário escolha o nome do arquivo a ser salvo, desde que esteja dentro das extensões permitidas
        /// </summary>
        /// <param name="fileName">Variável onde será colocado o nome do arquivo escolhido pelo usuário</param>
        /// <param name="extensions">Extensões permitidas</param>
        /// <returns>True = Escolheu Salvar, False = Não escolheu salvar</returns>
        public DialogResult SaveFilePrompt(out string fileName, params string[] extensions)
        {
            foreach (string extension in extensions)
            {
                m_filter = "Arquivos " + "(*" + extension + ")|" + "*" + extension + "|";
            }

            //retira último pipe
            m_filter = m_filter.Substring(0, m_filter.Length - 1);

            SaveFileDialog dialog = new SaveFileDialog();

            try
            {
                Thread t = new Thread(ShowDialog);
                t.SetApartmentState(ApartmentState.STA);

                t.Start(dialog);

                t.Join();

                fileName = m_fileName;
                return m_result;
            }
            catch (Exception)
            {
                fileName = string.Empty;
                return DialogResult.None;
            }
        }

        /// <summary> Abre uma FolderBrowserDialog, permitindo que o usuário escolha uma pasta
        /// </summary>
        /// <param name="folderPath">Caminho da pasta</param>
        /// <returns>DialogResult do ShowDialog</returns>
        public DialogResult FolderSelectPrompt(out string folderPath)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            try
            {
                Thread t = new Thread(ShowDialog);
                t.SetApartmentState(ApartmentState.STA);

                t.Start(dialog);
                t.Join();

                folderPath = m_fileName;
                return m_result;
            }
            catch (Exception)
            {
                folderPath = string.Empty;
                return DialogResult.None;
            }
        }

        #endregion Métodos Públicos

        #region Métodos Privados

        private void SetDialogFocus()
        {
            while (m_dialogOwner != null)
            {
                IntPtr hWnd = GetWindow(m_dialogOwner, GW_ENABLEDPOPUP);

                if (hWnd != null)
                {
                    RECT rect;
                    GetWindowRect(new System.Runtime.InteropServices.HandleRef(this, hWnd), out rect);

                    var x1Pos = SystemInformation.PrimaryMonitorSize.Width / 2 - (rect.Right - rect.Left) / 2;
                    var x2Pos = rect.Right - rect.Left;
                    var y1Pos = SystemInformation.PrimaryMonitorSize.Height / 2 - (rect.Bottom - rect.Top) / 2;
                    var y2Pos = rect.Bottom - rect.Top;

                    SetWindowPos(hWnd, HWND_TOPMOST, x1Pos, y1Pos, x2Pos, y2Pos, SetWindowPosFlags.SWP_SHOWWINDOW);

                    if (SetForegroundWindow(hWnd))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary> Mostra a dialog
        /// </summary>
        private void ShowCommonDialog(CommonDialog dialog)
        {
            using (Form dummy = new Form())
            {
                dummy.TopMost = true;

                m_dialogFocusFunction = SetDialogFocus;
                m_dialogOwner = dummy.Handle;

                try
                {
                    m_dialogFocusFunction.BeginInvoke(null, null);
                    m_result = dialog.ShowDialog(dummy);
                }
                finally
                {
                    m_dialogOwner = (IntPtr)null;
                }
            }
        }

        /// <summary> Método Inicial da Thread para mostrar dialog's
        /// </summary>
        /// <param name="dialog">Dialog</param>
        private void ShowDialog(object dialog)
        {
            if (dialog is FolderBrowserDialog)
            {
                ShowDialog(dialog as FolderBrowserDialog);
            }
            else if (dialog is FileDialog)
            {
                ShowDialog(dialog as FileDialog);
            }
        }

        /// <summary> Mostra a Dialog
        /// </summary>
        /// <param name="dialog">Mostra a Dialog</param>
        private void ShowDialog(FolderBrowserDialog dialog)
        {
            SetDialogConfigurations(dialog);
            ShowCommonDialog(dialog);

            m_fileName = dialog.SelectedPath;
        }

        /// <summary> Mostra a Dialog
        /// </summary>
        /// <param name="dialog">Dialog</param>
        private void ShowDialog(FileDialog dialog)
        {
            SetDialogConfigurations(dialog);
            ShowCommonDialog(dialog);

            m_fileName = dialog.FileName;
        }

        /// <summary> Seta as Configurações da dialog
        /// </summary>
        /// <param name="dialog">Dialog</param>
        private void SetDialogConfigurations(FileDialog dialog)
        {
            dialog.Title = m_title;

            dialog.Filter = m_filter;
            dialog.AddExtension = true;
            dialog.CheckPathExists = true;
            dialog.SupportMultiDottedExtensions = false;
            dialog.ValidateNames = true;
        }

        /// <summary> Seta as configurações da dialog
        /// </summary>
        /// <param name="dialog">Dialog</param>
        private void SetDialogConfigurations(FolderBrowserDialog dialog)
        {
            dialog.Description = m_title;

            dialog.ShowNewFolderButton = true;
        }

        #endregion Métodos Privados
    }
}
