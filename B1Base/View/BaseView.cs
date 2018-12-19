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
        public delegate void ColChooseFromEventHandler(int row, Dictionary<string, string> values);        
        public delegate void MatixRowClickEventHandler(int row, string column);
        public delegate void MatrixRowRemoveEventHandler(int row);
        public delegate void MatrixSortEventHandler(string column);
        public delegate bool MatrixCanAddEventHandler(int row);
        public delegate void EditValidateEventHandler(bool changed);
        public delegate void ColumnValidateEventHandler(int row, bool changed);
        public delegate void ComboSelectEventHandler(bool changed);
        public delegate void ColumnSelectEventHandler(int row, bool changed);
        public delegate void CheckEventHandler();
        public delegate void ColumnCheckEventHandler(int row);

        public string LastEditValue { get; private set; }
        public string LastComboValue { get; private set; }
        public string LastColumnValue { get; private set; }
        public int LastSortedColPos { get; private set; }

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
                LastSortedColPos = 1;
            }
            finally
            {
                SAPForm.Freeze(false);
            }
        }

        protected virtual Dictionary<string, ButtonClickEventHandler> ButtonClickEvents { get { return new Dictionary<string, ButtonClickEventHandler>(); } }

        protected virtual Dictionary<string, ButtonPressEventHandler> ButtonPressEvents { get { return new Dictionary<string, ButtonPressEventHandler>(); } }

        protected virtual Dictionary<string, ChooseFromEventHandler> ChooseFromEvents { get { return new Dictionary<string, ChooseFromEventHandler>(); } }

        protected virtual Dictionary<string, ColChooseFromEventHandler> ColChooseFromEvents { get { return new Dictionary<string, ColChooseFromEventHandler>(); } }

        protected virtual Dictionary<string, MatixRowClickEventHandler> MatrixRowClickEvents { get { return new Dictionary<string, MatixRowClickEventHandler>(); } }

        protected virtual Dictionary<string, MatrixRowRemoveEventHandler> MatrixRowRemoveEvents { get { return new Dictionary<string, MatrixRowRemoveEventHandler>(); } }

        protected virtual Dictionary<string, MatrixSortEventHandler> MatrixSortEvents { get { return new Dictionary<string, MatrixSortEventHandler>(); } }

        protected virtual Dictionary<string, MatrixCanAddEventHandler> MatrixCanAddEvents { get { return new Dictionary<string, MatrixCanAddEventHandler>(); } }

        protected virtual Dictionary<string, FolderSelectEventHandler> FolderSelectEvents { get { return new Dictionary<string, FolderSelectEventHandler>(); } }

        protected virtual Dictionary<string, EditValidateEventHandler> EditValidateEvents { get { return new Dictionary<string, EditValidateEventHandler>(); } }

        protected virtual Dictionary<string, ColumnValidateEventHandler> ColumnValidateEvents { get { return new Dictionary<string, ColumnValidateEventHandler>(); } }

        protected virtual Dictionary<string, ComboSelectEventHandler> ComboSelectEvents { get { return new Dictionary<string, ComboSelectEventHandler>(); } }

        protected virtual Dictionary<string, ColumnSelectEventHandler> ColumnSelectEvents { get { return new Dictionary<string, ColumnSelectEventHandler>(); } }

        protected virtual Dictionary<string, CheckEventHandler> CheckEvents { get { return new Dictionary<string, CheckEventHandler>(); } }

        protected virtual Dictionary<string, ColumnCheckEventHandler> ColumnCheckEvents { get { return new Dictionary<string, ColumnCheckEventHandler>(); } }

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
            List<KeyValuePair<dynamic, string>> validValues = AddOn.Instance.ConnectionController.ExecuteSqlForList<KeyValuePair<dynamic, string>>(sqlScript, variables);

            foreach (KeyValuePair<dynamic, string> validValue in validValues)
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
            
            var enumValues = Enum.GetValues(type);

            foreach (var enumValue in enumValues)
            {
                combo.ValidValues.Add(((int)enumValue).ToString(), Model.EnumOperation.GetEnumDescription(enumValue));
            }
        }

        protected void FilterChoose(EditText edit, string column, BoConditionOperation operation, string value)
        {
            if (edit.ChooseFromListUID != string.Empty)
            {
                ChooseFromList choose = (ChooseFromList)SAPForm.ChooseFromLists.Item(edit.ChooseFromListUID);

                Conditions conditions = new Conditions();

                Condition condition = conditions.Add();

                condition.Alias = column;
                condition.Operation = operation;
                condition.CondVal = value;

                choose.SetConditions(conditions);
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
                
                EditText editText = (EditText)SAPForm.Items.Item(item).Specific;
                
                if (editText.ChooseFromListUID != string.Empty)
                {
                    try
                    {
                        userDataSource = SAPForm.DataSources.UserDataSources.Item("_" + editText.DataBind.Alias);
                    }
                    catch { }
                }
                
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
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((CheckBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);

                return userDataSource.Value == "Y";
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_PICTURE)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((PictureBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);

                if (userDataSource.Value == string.Empty)
                    return string.Empty;
                else return System.IO.Path.Combine(new Controller.AttachmentController().ImageFolder, userDataSource.Value);
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EXTEDIT)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);

                return userDataSource.Value;
            }

            else return string.Empty;
        }

        protected List<T> GetValue<T>(DataTable dataTable, Matrix matrix) where T : Model.BaseModel
        {
            List<T> result = new List<T>();

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            matrix.FlushToDataSource();

            for (int row = 0; row < dataTable.Rows.Count - 1; row++)
            {
                T model = (T)Activator.CreateInstance(type);

                foreach (var prop in props)
                {
                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        if (dataTable.Columns.Item(col).Name == prop.Name)
                        {
                            if (prop.PropertyType == typeof(Boolean))
                            {
                                prop.SetValue(model, dataTable.GetValue(col, row).ToString().Equals("Y"), null);
                            }
                            else if (prop.PropertyType.IsEnum)
                            {
                                prop.SetValue(model, Convert.ChangeType(dataTable.GetValue(col, row), Enum.GetUnderlyingType(prop.PropertyType)), null);
                            }
                            else
                            {
                                prop.SetValue(model, dataTable.GetValue(col, row));
                            }
                            break;
                        }
                    }
                }

                result.Add(model);
            }

            return result;
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
                else if (type == typeof(Int32))
                {
                    if (value == 0)
                        userDataSource.Value = string.Empty;
                    else
                        userDataSource.Value = value.ToString();
                }       
                else
                {
                    userDataSource.Value = value.ToString();
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EDIT)
            {
                EditText editText = (EditText)SAPForm.Items.Item(item).Specific;
                
                if (editText.ChooseFromListUID != string.Empty)
                {
                    try
                    {
                        ChooseFromList chooseFromList = SAPForm.ChooseFromLists.Item(editText.ChooseFromListUID);

                        string descValue = AddOn.Instance.ConnectionController.ExecuteSqlForObject<string>("GetChooseValue", chooseFromList.ObjectType, value.ToString());

                        UserDataSource codeDataSource = SAPForm.DataSources.UserDataSources.Item("_" + editText.DataBind.Alias);

                        if (codeDataSource.DataType == BoDataType.dt_SHORT_NUMBER || codeDataSource.DataType == BoDataType.dt_LONG_NUMBER ||
                            codeDataSource.DataType == BoDataType.dt_MEASURE || codeDataSource.DataType == BoDataType.dt_PERCENT ||
                            codeDataSource.DataType == BoDataType.dt_PRICE || codeDataSource.DataType == BoDataType.dt_QUANTITY ||
                            codeDataSource.DataType == BoDataType.dt_RATE || codeDataSource.DataType == BoDataType.dt_SUM)
                        {
                            if (value != 0)
                                codeDataSource.Value = value.ToString();
                            else
                                codeDataSource.Value = string.Empty;
                        }
                        else if (codeDataSource.DataType == BoDataType.dt_DATE)
                        {
                            if (value != DateTime.MinValue && value != new DateTime(1990, 1, 1))
                                codeDataSource.Value = value.ToString();
                            else
                                codeDataSource.Value = string.Empty;
                        }
                        else
                        {
                            codeDataSource.Value = value.ToString();
                        }

                        UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                        userDataSource.Value = descValue;
                    }
                    catch 
                    {
                        UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                        userDataSource.Value = value.ToString();
                    }
                }                
                else
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                    userDataSource.Value = value.ToString();
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_CHECK_BOX)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((CheckBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                userDataSource.Value = value ? "Y" : "N";
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_PICTURE)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((PictureBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                if (value.ToString() == string.Empty)
                    userDataSource.Value = string.Empty;
                else
                    userDataSource.Value = System.IO.Path.Combine(new Controller.AttachmentController().ImageFolder, value.ToString());
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EXTEDIT)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                userDataSource.Value = value;
            }
        }

        protected void SetValue<T>(DataTable dataTable, Matrix matrix, List<T> list) where T : Model.BaseModel
        {
            dataTable.Rows.Clear();

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            foreach (T model in list)
            {
                dataTable.Rows.Add();

                foreach (var prop in props)
                {
                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        if (dataTable.Columns.Item(col).Name == prop.Name)
                        {
                            if (prop.PropertyType == typeof(Boolean))
                            {
                                dataTable.SetValue(col, dataTable.Rows.Count - 1, (bool) prop.GetValue(model) ? "Y" : "N");
                            }
                            else if (prop.PropertyType.IsEnum)
                            {
                                dataTable.SetValue(col, dataTable.Rows.Count - 1, (int) prop.GetValue(model));;
                            }
                            else
                            {
                                dataTable.SetValue(col, dataTable.Rows.Count - 1, prop.GetValue(model));
                            }
                            
                            break;
                        }
                    }
                }
            }
            dataTable.Rows.Add();

            matrix.LoadFromDataSource();

            if (matrix.RowCount == 0)
                matrix.AddRow();
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

        public void ChooseFrom(string edit, params string[] values)
        {
            if (ChooseFromEvents.ContainsKey(edit))
            {
                try
                {
                    UserDataSource codeDataSource = SAPForm.DataSources.UserDataSources.Item("_" + ((EditText)SAPForm.Items.Item(edit).Specific).DataBind.Alias);
                    codeDataSource.Value = values[0];

                    UserDataSource valueDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(edit).Specific).DataBind.Alias);
                    valueDataSource.Value = values[1];
                }
                catch { }

                ChooseFromEvents[edit](values);
            }
        }

        public void ColChooseFrom(string matrix, int row, string column, Dictionary<string, string> values)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColChooseFromEvents.ContainsKey(key))
            {
                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                for (int col = 0; col < matrixItem.Columns.Count; col++)
                {
                    if (matrixItem.Columns.Item(col).Type == BoFormItemTypes.it_EDIT || matrixItem.Columns.Item(col).Type == BoFormItemTypes.it_LINKED_BUTTON)
                    {
                        EditText editText = (EditText)matrixItem.Columns.Item(col).Cells.Item(row).Specific;

                        string alias = string.Empty;

                        try
                        {
                            alias = matrixItem.Columns.Item(col).ChooseFromListAlias;
                        }
                        catch { }
                        
                        if (values.ContainsKey(alias))
                        {
                            try
                            {
                                editText.String = values[alias];
                            }
                            catch { }
                        }
                    }
                }

                ColChooseFromEvents[key](row, values);
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

        public void MatrixSort(string matrix, string column)
        {
            if (MatrixSortEvents.ContainsKey(matrix))
            {
                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                for (int colPos = 1; colPos < matrixItem.Columns.Count;  colPos++)
                {
                    if (matrixItem.Columns.Item(colPos).UniqueID == column)
                    {
                        LastSortedColPos = colPos;
                        break;
                    }
                }
                   
                MatrixSortEvents[matrix](column);
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

        public void ColumnChecked(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColumnCheckEvents.ContainsKey(key))
            {
                ColumnCheckEvents[key](row);
            }
        }

        public void EditValidate(string edit)
        {
            if (EditValidateEvents.ContainsKey(edit))
            {
                EditText editText = ((EditText)SAPForm.Items.Item(edit).Specific);
                if (editText.ChooseFromListUID != string.Empty && editText.String == string.Empty)
                {
                    try
                    {
                        UserDataSource codeDataSource = SAPForm.DataSources.UserDataSources.Item("_" + editText.DataBind.Alias);
                        codeDataSource.Value = "";

                        UserDataSource valueDataSource = SAPForm.DataSources.UserDataSources.Item(editText.DataBind.Alias);
                        valueDataSource.Value = "";
                    }
                    catch { }

                    if (ChooseFromEvents.ContainsKey(edit))
                        ChooseFromEvents[edit]("");
                }

                EditValidateEvents[edit](LastEditValue != editText.String);
            }
        }

        public void ColumnValidate(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColumnValidateEvents.ContainsKey(key))
            {
                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                EditText editText = (EditText)matrixItem.Columns.Item(column).Cells.Item(row).Specific;
                if (editText.ChooseFromListUID != string.Empty && editText.String == string.Empty)
                {
                    try
                    {
                        UserDataSource codeDataSource = SAPForm.DataSources.UserDataSources.Item("_" + editText.DataBind.Alias);
                        codeDataSource.Value = "";

                        UserDataSource valueDataSource = SAPForm.DataSources.UserDataSources.Item(editText.DataBind.Alias);
                        valueDataSource.Value = "";
                    }
                    catch { }

                    if (ChooseFromEvents.ContainsKey(key))
                        ChooseFromEvents[key]("");
                }

                ColumnValidateEvents[key](row, LastColumnValue != editText.String);

                if (MatrixCanAddEvents.ContainsKey(key) && row == matrixItem.RowCount)
                {
                    if (MatrixCanAddEvents[key](row))
                    {
                        matrixItem.AddRow();
                    }
                }
            }
        }

        public void EditFocus(string edit)
        {
            if (EditValidateEvents.ContainsKey(edit))
            {
                LastEditValue = ((EditText)SAPForm.Items.Item(edit).Specific).String;
            }
        }

        public void ComboSelect(string combo)
        {
            if (ComboSelectEvents.ContainsKey(combo))
            {
                ComboSelectEvents[combo](LastComboValue !=
                    (((ComboBox)SAPForm.Items.Item(combo).Specific).Selected == null ? string.Empty :
                    ((ComboBox)SAPForm.Items.Item(combo).Specific).Selected.Value));
            }
        }

        public void ColumnSelect(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColumnSelectEvents.ContainsKey(key))
            {
                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                ColumnSelectEvents[key](row, LastComboValue !=
                    (((ComboBox)matrixItem.Columns.Item(column).Cells.Item(row).Specific).Selected == null ? string.Empty :
                    ((ComboBox)matrixItem.Columns.Item(column).Cells.Item(row).Specific).Selected.Value));

                if (MatrixCanAddEvents.ContainsKey(key) && row == matrixItem.RowCount)
                {
                    if (MatrixCanAddEvents[key](row))
                    {
                        matrixItem.AddRow();
                    }
                }
            }
        }

        public void ComboFocus(string combo)
        {
            if (ComboSelectEvents.ContainsKey(combo))
            {
                LastComboValue = (((ComboBox)SAPForm.Items.Item(combo).Specific).Selected == null ? string.Empty :
                    ((ComboBox)SAPForm.Items.Item(combo).Specific).Selected.Value);
            }
        }

        public void ColumnFocus(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);
            if (ColumnValidateEvents.ContainsKey(key))
            {
                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                if (matrixItem.Columns.Item(column).Type == BoFormItemTypes.it_COMBO_BOX)
                {
                    LastColumnValue = (((ComboBox)matrixItem.Columns.Item(column).Cells.Item(row).Specific).Selected == null ? string.Empty :
                        ((ComboBox)matrixItem.Columns.Item(column).Cells.Item(row).Specific).Selected.Value);
                }
                else
                {
                    LastColumnValue = ((EditText)matrixItem.Columns.Item(column).Cells.Item(row).Specific).String;
                }
            }
        }

    }
    
}
