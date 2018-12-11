using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.ComponentModel;
using System.Reflection;
using SAPbouiCOM;

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
        public delegate void EditValidateEventHandler(bool changed);
        public delegate void ComboSelectEventHandler(bool changed);
        public delegate void CheckEventHandler();

        public string LastEditValue { get; private set; }
        public string LastComboValue { get; private set; }

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

                LastEditValue = string.Empty;
                LastComboValue = string.Empty;
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

        protected virtual Dictionary<string, ComboSelectEventHandler> ComboSelectEvents { get { return new Dictionary<string, ComboSelectEventHandler>(); } }

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

        protected void LoadCombo<T>(ComboBox combo)
        {
            var type = typeof(T);

            //if (!type.IsEnum)
            //{
            //    return;
            //}

            //var names = Enum.GetNames(type);
            //foreach (var name in names)
            //{
            //    var field = type.GetField(name);
            //    var attribute = field.GetCustomAttribute(typeof(DescriptionAttribute), true);
                
            //    if (attribute != null)
            //    {
            //        combo.ValidValues.Add(((int)field.GetValue(null)).ToString(), ((DescriptionAttribute)attribute).Description);
            //    }
            //    else
            //    {
            //        combo.ValidValues.Add(((int)field.GetValue(null)).ToString(), field.Name);
            //    }
            //}
            var enumValues = Enum.GetValues(type);

            foreach (var enumValue in enumValues)
            {
                combo.ValidValues.Add(enumValue.ToString(), Model.EnumOperation.GetEnumDescription(enumValue));
            }
        }

        protected dynamic GetValue(string item) 
        {
            if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_COMBO_BOX)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((ComboBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);

                if (userDataSource.DataType == BoDataType.dt_SHORT_NUMBER || userDataSource.DataType == BoDataType.dt_LONG_NUMBER)
                {
                    if (userDataSource.Value == string.Empty)
                        return 0;
                    else return Convert.ToInt32(userDataSource.Value);
                }
                else return userDataSource.Value;                
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EDIT)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                
                if (userDataSource.DataType == BoDataType.dt_SHORT_NUMBER || userDataSource.DataType == BoDataType.dt_LONG_NUMBER)
                {
                    if (userDataSource.Value == string.Empty)
                        return 0;
                    else return Convert.ToInt32(userDataSource.Value);
                }
                if (userDataSource.DataType == BoDataType.dt_MEASURE || userDataSource.DataType == BoDataType.dt_PERCENT ||
                    userDataSource.DataType == BoDataType.dt_PRICE || userDataSource.DataType == BoDataType.dt_QUANTITY ||
                    userDataSource.DataType == BoDataType.dt_RATE || userDataSource.DataType == BoDataType.dt_SUM)
                {
                    if (userDataSource.Value == string.Empty)
                        return 0;
                    else return Convert.ToDouble(userDataSource.Value);
                }
                else if (userDataSource.DataType == BoDataType.dt_DATE)
                {
                    if (userDataSource.Value == string.Empty)
                        return new DateTime(1990, 1, 1);
                    else return Convert.ToDateTime(userDataSource.Value);
                }
                else return userDataSource.Value;
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_CHECK_BOX)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((ComboBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);

                return userDataSource.Value == "Y";
            }
            else return string.Empty;
        }

        protected void SetValue(string item, dynamic value)
        {
            if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_COMBO_BOX)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((ComboBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);

                Type type = value.GetType();

                if (type.IsEnum)
                {
                    userDataSource.Value = ((int) value).ToString();
                }
                else
                {
                    userDataSource.Value = value.ToString();
                }                
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EDIT)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                userDataSource.Value = value.ToString();
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_CHECK_BOX)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((ComboBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                userDataSource.Value = value ? "Y" : "N";
            }
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
                if (menu.StartsWith(string.Format("MNUREM{0}{1}{2}", SAPForm.TypeEx, SAPForm.UniqueID, matrixRowRemoveEvent.Key)))
                {
                    Matrix matrix = (Matrix)SAPForm.Items.Item(matrixRowRemoveEvent.Key).Specific;

                    int row = Convert.ToInt16(menu.Split('-')[1]);

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
                    string menuID = string.Format("MNUREM{0}{1}{2}-{3}", SAPForm.TypeEx, SAPForm.UniqueID, item, row);

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

        public void Checked(string check)
        {
            if (CheckEvents.ContainsKey(check))
            {
                CheckEvents[check]();
            }
        }

        public void EditValidate(string edit)
        {
            if (EditValidateEvents.ContainsKey(edit))
            {
                //tratar matriz.edit
                try
                {
                    EditValidateEvents[edit](LastEditValue != ((EditText)SAPForm.Items.Item(edit).Specific).String);
                }
                catch
                {
                    EditValidateEvents[edit](true);
                }
            }
        }

        public void EditFocus(string edit)
        {
            if (EditValidateEvents.ContainsKey(edit))
            {
                try
                {
                    //tratar matriz.edit
                    LastEditValue = ((EditText)SAPForm.Items.Item(edit).Specific).String;
                }
                catch
                {
                    LastEditValue = string.Empty;
                }
            }
        }

        public void ComboSelect(string combo)
        {
            if (ComboSelectEvents.ContainsKey(combo))
            {
                //tratar matriz.combo
                try
                {
                    ComboSelectEvents[combo](LastComboValue !=
                        (((ComboBox)SAPForm.Items.Item(combo).Specific).Selected == null ? string.Empty :
                        ((ComboBox)SAPForm.Items.Item(combo).Specific).Selected.Value));
                }
                catch
                {
                    ComboSelectEvents[combo](true);
                }
            }
        }

        public void ComboFocus(string combo)
        {
            if (ComboSelectEvents.ContainsKey(combo))
            {
                try
                {
                    //tratar matriz.combo
                    LastComboValue = (((ComboBox)SAPForm.Items.Item(combo).Specific).Selected == null ? string.Empty :
                        ((ComboBox)SAPForm.Items.Item(combo).Specific).Selected.Value);
                }
                catch
                {
                    LastComboValue = string.Empty;
                }
            }
        }

//        baseview.showerrormesssage(string msg)
//baseview.showwarningmesssage(string msg)
//baseview.showsuccessmessage() = operacao completada com exito
//baseview.showdeleteconfirmation() = deseja realmente eliliminar esse registro?
//baseview.showabortconfirmatio()= dados nao salvos serao perdidos
    }
    
}
