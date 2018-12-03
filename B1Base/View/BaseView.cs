using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbouiCOM;
using System.Timers;

namespace B1Base.View
{
    public abstract class BaseView
    {
        public string FormUID { get; private set; }
        public string FormType { get; private set; }

        Timer m_timerInitialize = new Timer(1000);

        public BaseView(string formUID, string formType)
        {
            FormUID = formUID;
            FormType = formType;

            m_timerInitialize.Elapsed += Initialize;
            m_timerInitialize.Enabled = true;

            SAPForm.Freeze(true);
        }

        
        public delegate void ButtonClickEventHandler();
        public delegate void ButtonPressEventHandler();
        public delegate void FolderSelectEventHandler();
        public delegate void ChooseFromEventHandler(params string[] values);
        public delegate void MatixRowClickEventHandler(int row, string column);
        public delegate void MatrixRowRemoveEventHandler(int row);
        public delegate void EditValidateEventHandler();
        public delegate void CheckEventHandler();

        protected Form SAPForm
        {
            get
            {
                return Controller.ConnectionController.Instance.Application.Forms.Item(FormUID);
            }
        }

        private void Initialize(object sender, ElapsedEventArgs e)
        {
            
            try
            {
                m_timerInitialize.Enabled = false;

                CreateControls();

                Form mainForm = Controller.ConnectionController.Instance.Application.Forms.GetForm("0", 1);

                SAPForm.Top = (System.Windows.Forms.SystemInformation.WorkingArea.Height - 115 - SAPForm.Height) / 2;
                SAPForm.Left = (mainForm.ClientWidth - SAPForm.Width) / 2;                

            }
            finally
            {
                SAPForm.Freeze(false);
            }
        }

        protected virtual Dictionary<string, ButtonClickEventHandler> ButtonClickEvents { get { return new Dictionary<string, ButtonClickEventHandler>(); } }

        protected virtual Dictionary<string, ButtonPressEventHandler> ButtonPressEvents { get { return new Dictionary<string, ButtonPressEventHandler>(); } }

        protected virtual Dictionary<string, ChooseFromEventHandler> ChooseFromEvents { get { return new Dictionary<string, ChooseFromEventHandler>(); } }

        protected virtual Dictionary<string, MatixRowClickEventHandler> MatrixRowClickEvents { get { return new Dictionary<string, MatixRowClickEventHandler>(); } }

        protected virtual Dictionary<string, MatrixRowRemoveEventHandler> MatrixRowRemoveEvents { get { return new Dictionary<string, MatrixRowRemoveEventHandler>(); } }

        protected virtual Dictionary<string, FolderSelectEventHandler> FolderSelectEvents { get { return new Dictionary<string, FolderSelectEventHandler>(); } }

        protected virtual Dictionary<string, EditValidateEventHandler> EditValidateEvents { get { return new Dictionary<string, EditValidateEventHandler>(); } }

        protected virtual Dictionary<string, CheckEventHandler> CheckEvents { get { return new Dictionary<string, CheckEventHandler>(); } }

        protected virtual void CreateControls() { }

        protected void ControlMenus(bool enableInsert, bool enableSearch, bool enableNavigation)
        {
            SAPForm.EnableMenu("1282", enableInsert);
            SAPForm.EnableMenu("1281", enableSearch);
            SAPForm.EnableMenu("1283", enableNavigation);
            SAPForm.EnableMenu("1284", enableNavigation);
            SAPForm.EnableMenu("1285", enableNavigation);
            SAPForm.EnableMenu("1286", enableNavigation);
        }

        protected void LoadCombo(ComboBox combo, string sqlScript, params string[] variables)
        {
            List<KeyValuePair<int, string>> validValues = AddOn.Instance.ConnectionController.ExecuteSqlForList<KeyValuePair<int, string>>(sqlScript, variables);

            foreach (KeyValuePair<int, string> validValue in validValues)
            {
                combo.ValidValues.Add(validValue.Key.ToString(), validValue.Value);
            }
        }

        protected void LoadCombo(string table, string codeField, string nameField, ComboBox combo)
        {
            LoadCombo(combo, "GetComboValues", table, codeField, nameField);
        }

        protected void LoadCombo(Enum valuesEnum, ComboBox combo)
        {

        }

        public virtual void GotFormData() { }

        public virtual void AddFormData() { }

        public virtual void MenuInsert() { }

        public virtual void MenuSearch() { }

        public virtual void MenuDuplicate() { }

        public virtual void Resize() { }

        public virtual void GotFocus() { }

        public virtual void Close() { }

        public void ButtonClick(string button)
        {
            if (ButtonClickEvents.ContainsKey(button))
            {
                ButtonClickEvents[button]();
            }
        }

        public void ButtonPress(string button)
        {
            if (ButtonPressEvents.ContainsKey(button))
            {
                ButtonPressEvents[button]();
            }
        }

        public void ChooseFrom(string choose, params string[] values)
        {
            if (ChooseFromEvents.ContainsKey(choose))
            {
                ChooseFromEvents[choose](values);
            }
        }

        public void FolderSelect(string folder)
        {
            if (FolderSelectEvents.ContainsKey(folder))
            {
                FolderSelectEvents[folder]();
            }
        }

        public void MatrixRowClick(string matrix, int row, string column)
        {
            if (MatrixRowClickEvents.ContainsKey(matrix))
            {
                MatrixRowClickEvents[matrix](row, column);
            }
        }

        public void MenuRightClick(string menu)
        {
            foreach (KeyValuePair<string, MatrixRowRemoveEventHandler> matrixRowRemoveEvent in MatrixRowRemoveEvents)
            {
                if (menu == string.Format("MNUREM{0}{1}{2}", SAPForm.TypeEx, SAPForm.UniqueID, matrixRowRemoveEvent.Key))
                {
                    Matrix matrix = (Matrix)SAPForm.Items.Item(MatrixRowRemoveEvents.Keys);

                    int row = matrix.GetCellFocus().rowIndex;

                    matrixRowRemoveEvent.Value(row);
                }
            }
        }

        public void RightClick(string item, int row, string col)
        {
            foreach (KeyValuePair<string, MatrixRowRemoveEventHandler> matrixRowRemoveEvent in MatrixRowRemoveEvents)
            {
                if (matrixRowRemoveEvent.Key == item)
                {
                    string menuID = string.Format("MNUREM{0}{1}{2}", SAPForm.TypeEx, SAPForm.UniqueID, item);

                    if (AddOn.Instance.ConnectionController.Application.Menus.Exists(menuID))
                        AddOn.Instance.ConnectionController.Application.Menus.RemoveEx(menuID);

                    SAPbouiCOM.Matrix matrix = (Matrix)SAPForm.Items.Item(item).Specific;

                    string colTitle = matrix.Columns.Item(col).Title;
                    string firstCol = matrix.Columns.Item(0).UniqueID;

                    if (row > 0 && row < matrix.RowCount && (col != firstCol || colTitle != "#" || colTitle != ""))
                    {
                        MenuItem menuItem = null;
                        Menus menu = null;
                        MenuCreationParams creationPackage = null;

                        creationPackage = ((MenuCreationParams)(AddOn.Instance.ConnectionController.Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams)));

                        creationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                        creationPackage.UniqueID = menuID;
                        creationPackage.String = "Eliminar linha";
                        creationPackage.Enabled = true;

                        menuItem = AddOn.Instance.ConnectionController.Application.Menus.Item("1280");
                        menu = menuItem.SubMenus;
                        menu.AddEx(creationPackage);
                    }
                }
            }
        }

        public void EditValidate(string edit)
        {
            if (EditValidateEvents.ContainsKey(edit))
            {
                EditValidateEvents[edit]();
            }
        }

        public void Checked(string check)
        {
            if (CheckEvents.ContainsKey(check))
            {
                CheckEvents[check]();
            }
        }
    }
    
}
