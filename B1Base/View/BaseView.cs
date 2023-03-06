using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using SAPbouiCOM;
using B1Base.Controller;
using System.Xml;
using System.IO;

namespace B1Base.View
{
    public abstract class BaseView
    {
        #region constants

        const string BUTTON_DOC_COPY = "10000330";

        const string USD_INITIALIZED = "USDINIT";

        const string EDIT_INITIALIZED = "EDTINIT";

        #endregion

        public string FormUID { get; private set; }
        public string FormType { get; private set; }

        #region variables

        bool m_copyFlag;
        bool m_addFlag;
        bool m_updateFlag;

        bool m_updateFailed;

        Dictionary<string, int> cachedDBDataSources = new Dictionary<string, int>();

        #endregion
        
        protected System.Globalization.NumberFormatInfo DefaultNumberFormat
        {
            get
            {
                System.Globalization.NumberFormatInfo result = new System.Globalization.NumberFormatInfo();
                result.NumberDecimalSeparator = ",";

                return result;
            }
        }

        protected System.Globalization.NumberFormatInfo DefaultSQLNumberFormat
        {
            get
            {
                System.Globalization.NumberFormatInfo result = new System.Globalization.NumberFormatInfo();
                result.NumberDecimalSeparator = ".";                     

                return result;
            }
        }
        
        protected int ConvertInt(string intValue)
        {
            if (intValue.Trim().Equals(""))
                return 0;
            else
                return Convert.ToInt32(intValue);
        }
       
        protected double ConvertDouble(string doubleValue)
        {
            if (doubleValue.Trim().Equals(""))
                return 0;
            else
               return double.Parse((doubleValue.Contains(",") ? doubleValue.Replace(".", "").Replace(",", ".") : doubleValue).Replace("R$", "").Replace("AU", "").Replace("%", ""), System.Globalization.CultureInfo.InvariantCulture);
        }

        public DateTime ConverteDate(string dateValue)
        {
            if (dateValue == "")
                return DateTime.MinValue;
            else
                return DateTime.ParseExact(dateValue, "dd/MM/yyyy", null);
        }

        public BaseView(string formUID, string formType)
        {
            if (!Inactive)
            {
                FormUID = formUID;
                FormType = formType;

                ParentView = AddOn.Instance.MainController.LastParent;

                AddOn.Instance.MainController.LastParent = null;
            }
        }

        public BaseView() { }

        public virtual BaseView CreateNew(string formUID, string formType) { return null;  }

        public delegate void ButtonClickEventHandler();
        public delegate void ButtonPressEventHandler();
        public delegate void ButtonOpenViewEventHandler(BaseView view);
        public delegate void ButtonComboClickEventHandler(bool changed);
        public delegate void FolderSelectEventHandler();
        public delegate void CustomMenuEventHandler();
        public delegate void KeyDownEventHandler();
        public delegate void ChooseFromEventHandler(params string[] values);
        public delegate void ColChooseFromEventHandler(int row, Dictionary<string, string> values);        
        public delegate void MatixRowEnterEventHandler(int row, string column, bool rowChanged, bool rowSelected);
        public delegate void MatixRowDoubleClickEventHandler(int row, string column, bool rowChanged);
        public delegate void GridRowClickEventHandler(int row, string column);
        public delegate void GridRowDoubleClickEventHandler(int row, string column);
        public delegate void GridTabPressedEventHandler(int row, string column);
        public delegate void GridLinkPressedEventHandler(int row, out bool suspend);
        public delegate void GridCustomMenuEventHandler(int row, string column);
        public delegate bool GridCheckAllEventHandler();
        public delegate void MatrixRowRemoveEventHandler(int row);
        public delegate void MatrixCustomMenuEventHandler(int row, string column);
        public delegate void MatrixColPasteForAllEventHandler(string column);
        public delegate void MatrixSortEventHandler(string column);
        public delegate bool MatrixCheckAllEventHandler();
        public delegate bool MatrixCanAddEventHandler(int row);
        public delegate void EditValidateEventHandler(bool changed);
        public delegate void EditFocusEventHandler();
        public delegate void ColumnValidateEventHandler(int row, bool changed);
        public delegate void ComboSelectEventHandler(bool changed);
        public delegate void ColumnSelectEventHandler(int row, bool changed);
        public delegate void CheckEventHandler();
        public delegate void OptionEventHandler();
        public delegate void RightClickMenuEventHandler();
        public delegate void ColumnCheckEventHandler(int row);
        public delegate void ColumnCheckProEventHandler(int[] rows, bool check);
        public delegate void LinkEventHandler(View.BaseView linkedView);
        public delegate void DocCopyEventHandler(int docEntry);
        public delegate void ColSuppressActionEventHandler(BoEventTypes action, int row, out bool supressed);
        public delegate void SuppressActionEventHandler(BoEventTypes action, out bool supressed);
        public delegate bool SupressMatrixDetailsEventHandler();
        public delegate bool ConfirmMessageBoxEventHandler();
        public delegate void MenuPasteEventHandler();
        
        public string LastEditValue { get; private set; }
        public string LastComboValue { get; private set; }
        public string LastColumnValue { get; private set; }
        public string LastButtonComboValue { get; protected set; }
        public int LastSortedColPos { get; private set; }
        public string LastButtonClicked { get; private set; }
        public Dictionary<string, string> LastCols { get; private set; }
        public Dictionary<string, int> LastRows { get; private set; }
        public Dictionary<string, int> LastBeforeRows { get; private set; }
        public Dictionary<string, object> LastParameters { get; private set; }
        public int LastRightClickRow { get; private set; }
        public string LastRightClickMatrix { get; private set; }
        public string LastRightClickCol { get; private set; }
        public BoFormMode LastFormMode { get; private set; }
        public int LastCopiedDocEntry { get; private set; }
        public int LastDocEntry { get; private set; }
        public int LastAbsEntry { get; protected set; }

        public int LastCode { get; protected set; }
        public int LastDocNum { get; protected set; }
        public int LastDraftEntry { get; protected set; }
        public bool SavedAsDraft { get; protected set; }

        public bool IsDraft { get; protected set; }
                
        public Model.EnumObjType LastCopiedObjType { get; private set; }
        public bool Frozen { get; private set; }
        public BoModifiersEnum LastModifier { get; private set; }

        protected virtual int DefaultPane { get { return 1; } }

        protected virtual int CreateControlsTime { get { return 1; } }

        private Timer m_timerFormClose = new Timer(1000);

        private DateTime m_StartTime = DateTime.Now;

        public View.BaseView ParentView { get; set; }

        private string m_BrowseTable = string.Empty;
        private string m_BrowseItem = string.Empty;
        
        private bool m_ReserveCode = false;

        private bool m_DefaultInsertMode = true;

        public virtual bool Invisible
        {
            get
            {
                return false;
            }
        }

        public virtual bool SecondaryView
        {
            get
            {
                return false;
            }
        }

        public virtual bool Inactive
        {
            get
            {
                return false;
            }
        }

        protected bool FormLoaded { get; private set; }

        public Form SAPForm
        {
            get
            {
                return Controller.ConnectionController.Instance.Application.Forms.Item(FormUID);
            }
        }

        public void Initialize()
        {
            if (!FormUID.Contains("F_") && !SecondaryView && SAPForm.BorderStyle != BoFormBorderStyle.fbs_Floating)
            {
                Form mainForm = Controller.ConnectionController.Instance.Application.Forms.GetForm("0", 1);

                SAPForm.Top = (System.Windows.Forms.SystemInformation.WorkingArea.Height - 115 - SAPForm.Height) / 2;
                SAPForm.Left = (mainForm.ClientWidth - SAPForm.Width) / 2;
            }

            try
            {
                LastEditValue = string.Empty;
                LastComboValue = string.Empty;
                LastButtonClicked = string.Empty;
                LastSortedColPos = 1;
                LastRows = new Dictionary<string, int>();
                LastCols = new Dictionary<string, string>();
                LastBeforeRows = new Dictionary<string, int>();
                LastRightClickRow = 1;
                LastRightClickMatrix = string.Empty;
                LastCopiedDocEntry = 0;
                LastCopiedObjType = Model.EnumObjType.None;
                LastDocEntry = 0;
                LastAbsEntry = 0;
                LastFormMode = SAPForm.Mode;
                LastModifier = BoModifiersEnum.mt_None;
                LastParameters = new Dictionary<string, object>();

                CreateControls();

                if (!SAPForm.IsSystem)
                    SAPForm.PaneLevel = DefaultPane;

                if (SAPForm.BusinessObject != null && SAPForm.BusinessObject.Key != string.Empty)
                    GotFormData();

                if (DocCopyEvents.Count > 0)
                {
                    m_copyFlag = true;

                    FormValidate();
                }

            }
            catch (Exception ex)
            {
                if (new Controller.ConfigController<Model.ConfigModel>("").GetConfig().ActivateLog)
                    B1Base.Controller.ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOn.Instance.MainController.AddOnID + "]" + ex.Message + ". FormType: " + this.GetType().Name);
            }
        }

        public void PostInitialize()
        {
            if (!FormLoaded && ( (SAPForm.UniqueID.StartsWith("RW") && SAPForm.Visible) || !SAPForm.UniqueID.StartsWith("RW") ))
            {
                FormLoaded = true;

                SAPForm.Select();                

                AfterCreateControls();
            }
        }

        private void CloseForm(object sender, ElapsedEventArgs e)
        {
            m_timerFormClose.Enabled = false;

            SAPForm.Close();                
        }

        protected void RequestClose()
        {
            m_timerFormClose.Interval = 200;
            m_timerFormClose.Elapsed += CloseForm;
            m_timerFormClose.Enabled = true;
        }

        protected virtual Dictionary<string, ButtonClickEventHandler> ButtonClickEvents { get { return new Dictionary<string, ButtonClickEventHandler>(); } }

        /// <summary>
        /// Não atribuir a esse evento o botão OK (uid=1). Para esses casos, usar as sobrecargas correspondentes (FindFormData, GotFormData, AddFormData, UpdateFormData e DeleteFormData)
        /// </summary>
        protected virtual Dictionary<string, ButtonClickEventHandler> ButtonBeforeClickEvents { get { return new Dictionary<string, ButtonClickEventHandler>(); } }

        protected virtual Dictionary<string, ButtonComboClickEventHandler> ButtonComboClickEvents { get { return new Dictionary<string, ButtonComboClickEventHandler>(); } }

        /// <summary>
        /// Não atribuir a esse evento o botão OK (uid=1). Para esses casos, usar as sobrecargas correspondentes (FindFormData, GotFormData, AddFormData, UpdateFormData e DeleteFormData)
        /// </summary>
        protected virtual Dictionary<string, ButtonPressEventHandler> ButtonPressEvents { get { return new Dictionary<string, ButtonPressEventHandler>(); } }

        protected virtual Dictionary<string, ButtonOpenViewEventHandler> ButtonOpenViewEvents { get { return new Dictionary<string, ButtonOpenViewEventHandler>(); } }

        protected virtual Dictionary<string, KeyDownEventHandler> KeyDownEvents { get { return new Dictionary<string, KeyDownEventHandler>(); } }

        protected virtual Dictionary<string, ChooseFromEventHandler> ChooseFromEvents { get { return new Dictionary<string, ChooseFromEventHandler>(); } }

        protected virtual Dictionary<string, ColChooseFromEventHandler> ColChooseFromEvents { get { return new Dictionary<string, ColChooseFromEventHandler>(); } }

        protected virtual Dictionary<string, GridRowDoubleClickEventHandler> GridRowDoubleClickEvents { get { return new Dictionary<string, GridRowDoubleClickEventHandler>(); } }

        protected virtual Dictionary<string, GridRowClickEventHandler> GridRowClickEvents { get { return new Dictionary<string, GridRowClickEventHandler>(); } }

        protected virtual Dictionary<string, GridLinkPressedEventHandler> GridLinkPressedEvents { get { return new Dictionary<string, GridLinkPressedEventHandler>(); } }

        protected virtual Dictionary<string, GridTabPressedEventHandler> GridTabPressedEvents { get { return new Dictionary<string, GridTabPressedEventHandler>(); } }

        protected virtual Dictionary<string, GridCheckAllEventHandler> GridCheckAllEvents { get { return new Dictionary<string, GridCheckAllEventHandler>(); } }

        protected virtual Dictionary<string, Tuple<string, GridCustomMenuEventHandler>> GridCustomMenuEvents { get { return new Dictionary<string, Tuple<string, GridCustomMenuEventHandler>>(); } }

        protected virtual Dictionary<string, MatixRowEnterEventHandler> MatrixRowEnterEvents { get { return new Dictionary<string, MatixRowEnterEventHandler>(); } }

        protected virtual Dictionary<string, MatixRowDoubleClickEventHandler> MatrixRowDoubleClickEvents { get { return new Dictionary<string, MatixRowDoubleClickEventHandler>(); } }

        protected virtual Dictionary<string, MatrixRowRemoveEventHandler> MatrixRowRemoveEvents { get { return new Dictionary<string, MatrixRowRemoveEventHandler>(); } }

        protected virtual Dictionary<string, Tuple<string, MatrixCustomMenuEventHandler>> MatrixCustomMenuEvents { get { return new Dictionary<string, Tuple<string, MatrixCustomMenuEventHandler>>(); } }

        protected virtual Dictionary<string, MatrixColPasteForAllEventHandler> MatrixColPasteForAllEvents { get { return new Dictionary<string, MatrixColPasteForAllEventHandler>(); } }

        protected virtual Dictionary<string, MatrixSortEventHandler> MatrixSortEvents { get { return new Dictionary<string, MatrixSortEventHandler>(); } }

        protected virtual Dictionary<string, MatrixCheckAllEventHandler> MatrixCheckAllEvents { get { return new Dictionary<string, MatrixCheckAllEventHandler>(); } }

        protected virtual Dictionary<string, MatrixCanAddEventHandler> MatrixCanAddEvents { get { return new Dictionary<string, MatrixCanAddEventHandler>(); } }

        protected virtual Dictionary<string, Tuple<string, CustomMenuEventHandler>> CustomMenuEvents { get { return new Dictionary<string, Tuple<string, CustomMenuEventHandler>>(); } }

        protected virtual Dictionary<string, FolderSelectEventHandler> FolderSelectEvents { get { return new Dictionary<string, FolderSelectEventHandler>(); } }

        protected virtual Dictionary<string, EditValidateEventHandler> EditValidateEvents { get { return new Dictionary<string, EditValidateEventHandler>(); } }

        protected virtual Dictionary<string, EditFocusEventHandler> EditFocusEvents { get { return new Dictionary<string, EditFocusEventHandler>(); } }

        protected virtual Dictionary<string, ColumnValidateEventHandler> ColumnValidateEvents { get { return new Dictionary<string, ColumnValidateEventHandler>(); } }

        protected virtual Dictionary<string, ComboSelectEventHandler> ComboSelectEvents { get { return new Dictionary<string, ComboSelectEventHandler>(); } }

        protected virtual Dictionary<string, ColumnSelectEventHandler> ColumnSelectEvents { get { return new Dictionary<string, ColumnSelectEventHandler>(); } }

        protected virtual Dictionary<string, CheckEventHandler> CheckEvents { get { return new Dictionary<string, CheckEventHandler>(); } }

        protected virtual Dictionary<string, ColumnCheckEventHandler> ColumnCheckEvents { get { return new Dictionary<string, ColumnCheckEventHandler>(); } }

        protected virtual Dictionary<string, ColumnCheckProEventHandler> ColumnCheckProEvents { get { return new Dictionary<string, ColumnCheckProEventHandler>(); } }

        protected virtual Dictionary<string, OptionEventHandler> OptionEvents { get { return new Dictionary<string, OptionEventHandler>(); } }

        protected virtual Dictionary<string, RightClickMenuEventHandler> RightClickMenuEventEvents { get { return new Dictionary<string, RightClickMenuEventHandler>(); } }

        protected virtual Dictionary<string, LinkEventHandler> LinkEvents { get { return new Dictionary<string, LinkEventHandler>(); } }

        protected virtual Dictionary<Model.EnumObjType, DocCopyEventHandler> DocCopyEvents { get { return new Dictionary<Model.EnumObjType, DocCopyEventHandler>(); } }

        protected virtual Dictionary<string, SuppressActionEventHandler> SupressActionEvents { get { return new Dictionary<string, SuppressActionEventHandler>(); } }

        protected virtual Dictionary<string, ColSuppressActionEventHandler> ColSupressActionEvents { get { return new Dictionary<string, ColSuppressActionEventHandler>(); } }

        protected virtual Dictionary<string, SupressMatrixDetailsEventHandler> SupressMatrixDetailsEvents { get { return new Dictionary<string, SupressMatrixDetailsEventHandler>(); } }

        protected virtual Dictionary<string, ConfirmMessageBoxEventHandler> ConfirmMessageBoxEvents { get { return new Dictionary<string, ConfirmMessageBoxEventHandler>(); } }

        protected virtual Dictionary<string, MenuPasteEventHandler> MenuPasteEvents { get { return new Dictionary<string, MenuPasteEventHandler>(); } }

        protected virtual void CreateControls() { }

        protected virtual void AfterCreateControls() { }

        /// <summary>
        /// Controla os menus da barra superior do SAP
        /// </summary>
        /// <param name="enableInsert">Caso true, informar browseTable e browseItem</param>
        /// <param name="enableSearch"></param>
        /// <param name="enableNavigation"></param>
        /// <param name="browseTable">tabela de usuário, sem @</param>
        /// <param name="browseItem">nome do item do Code da tabela</param>        
        protected void ControlMenus(bool enableInsert, bool enableSearch, bool enableNavigation, string browseTable = "", string browseItem = "", bool defaultInsertMode = true, bool reserveCode = false)
        {
            SAPForm.Select();

            SAPForm.EnableMenu("1282", enableInsert);
            SAPForm.EnableMenu("1281", enableSearch);
            SAPForm.EnableMenu("1283", enableNavigation);
            SAPForm.EnableMenu("1284", enableNavigation);
            SAPForm.EnableMenu("1285", enableNavigation);
            SAPForm.EnableMenu("1286", enableNavigation);

            if (enableInsert)
            {
                if (browseItem != string.Empty && browseTable != string.Empty)
                {

                    SAPForm.DataSources.DBDataSources.Add(string.Format("@{0}", browseTable));

                    SAPForm.Items.Add("BACKCODE", BoFormItemTypes.it_EDIT).Left = 9999;

                    SAPForm.Items.Add("DUMMY", BoFormItemTypes.it_EDIT).Left = 9999;

                    ((EditText)SAPForm.Items.Item("BACKCODE").Specific).DataBind.SetBound(true, string.Format("@{0}", browseTable), "U_Code");

                    SAPForm.DataBrowser.BrowseBy = "BACKCODE";

                    m_BrowseItem = browseItem;
                    m_BrowseTable = browseTable;

                    m_DefaultInsertMode = defaultInsertMode;

                    m_ReserveCode = reserveCode;

                    if (m_DefaultInsertMode)
                    {
                        Controller.ConnectionController.Instance.Application.ActivateMenuItem("1282");
                    }
                    else
                    {
                        SAPForm.Mode = BoFormMode.fm_FIND_MODE;

                        MenuSearch();
                    }
                }                          
            }
        }

        /// <summary>
        /// Chama o SAPForm.Freeze(true) e não passa por nenhum evento de Validate dos compontens
        /// </summary>
        protected void Freeze()
        {
            SAPForm.Freeze(true);
            Frozen = true;
        }

        protected void Unfreeze()
        {
            SAPForm.Freeze(false);
            Frozen = false;
        }

        /// <summary>
        /// Usar no lugar do Form.ActiveItem pois controla o LastValue do evento Validate(changed)
        /// </summary>
        /// <param name="item"></param>
        protected void FocusItem(string item)
        {
            if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EDIT)
            {
                LastEditValue = ((EditText)SAPForm.Items.Item(item).Specific).String;                
            }

            SAPForm.ActiveItem = item;
        }

        public void ForceSave()
        {
            if (SAPForm.Mode == BoFormMode.fm_OK_MODE)
                SAPForm.Mode = BoFormMode.fm_UPDATE_MODE;

            SAPForm.Items.Item("1").Click();
        }

        protected Item CreateItem(string item, BoFormItemTypes itemType, int top, int left, int fromPane = 0, int toPane = 0, bool enabled = true, bool visible = true)
        {

            int templateId = 0;

            try
            {
                DBDataSource dbTemplate = AddOn.Instance.ConnectionController.Application.Forms.GetFormByTypeAndCount(234000021, 1).DataSources.DBDataSources.Item(0);

                templateId = Convert.ToInt32(dbTemplate.GetValue(0, dbTemplate.Offset));
            }
            catch { }

            Item _item = SAPForm.Items.Add(item, itemType);
            
            Model.CustomizedTemplateModel customizedTemplateModel = AddOn.Instance.ConnectionController.ExecuteSqlForObject<Model.CustomizedTemplateModel>("GetCustomizedTemplate", AddOn.Instance.ConnectionController.User.ToString(), FormType, item, templateId.ToString());

            _item.FromPane = fromPane;
            _item.ToPane = toPane;
            if (customizedTemplateModel == null)
            {
                _item.Top = top;
                _item.Left = left;                

                if (!visible)
                    _item.Visible = false;
                else
                    _item.Enabled = enabled;
            }
            else
            {
                _item.Top = customizedTemplateModel.Top;
                _item.Left = customizedTemplateModel.Left;

                if (!customizedTemplateModel.Visible)                
                    _item.Visible = false;                
                else
                    _item.Enabled = customizedTemplateModel.Editable;
            }
            _item.Visible = true;

            return _item;
        }

        protected string GeneratePartialXML(string xmlScript, params string[] variables)
        {
            string xml = "";

            using (var stream = new MemoryStream(File.ReadAllBytes(AddOn.Instance.CurrentDirectory + "//XML//" + xmlScript + ".xml")))
            {
                if (stream != null)
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        xml = string.Format(streamReader.ReadToEnd(), variables);
                    }
                }
            }

            return xml;
        }

        protected void UpdateFormByXML(string xmlScript, params string[] variables)
        {
            try
            {
                string xml = "";

                using (var stream = new MemoryStream(File.ReadAllBytes(AddOn.Instance.CurrentDirectory + "//XML//" + xmlScript + ".xml")))
                {
                    if (stream != null)
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            xml = string.Format(streamReader.ReadToEnd(), variables);
                        }
                    }
                }
                
                string sFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");

                File.WriteAllText(sFileName, xml);

                string sXPath = "Application//forms//action//form//@uid";
                
                XmlDocument xDoc = new XmlDocument();

                xDoc.Load(sFileName);

                XmlNode xNode = xDoc.SelectSingleNode(sXPath);
                xNode.InnerText = SAPForm.UniqueID;

                int templateId = 0;

                try
                {
                    DBDataSource dbTemplate = AddOn.Instance.ConnectionController.Application.Forms.GetFormByTypeAndCount(234000021, 1).DataSources.DBDataSources.Item(0);

                    templateId = Convert.ToInt32(dbTemplate.GetValue(0, dbTemplate.Offset));
                }
                catch { }

                string sXPathItems = "Application//forms//action//form//items//action";

                XmlNodeList xItemNodeList = xDoc.SelectSingleNode(sXPathItems).ChildNodes;

                foreach (XmlNode xItemNode in xItemNodeList)
                {
                    string item = xItemNode.SelectSingleNode("@uid").InnerText;

                    Model.CustomizedTemplateModel customizedTemplateModel = AddOn.Instance.ConnectionController.ExecuteSqlForObject<Model.CustomizedTemplateModel>("GetCustomizedTemplate",
                        AddOn.Instance.ConnectionController.User.ToString(), FormType, item, templateId.ToString());

                    if (customizedTemplateModel != null)
                    {
                        xItemNode.SelectSingleNode("@top").InnerText = customizedTemplateModel.Top.ToString();
                        xItemNode.SelectSingleNode("@left").InnerText = customizedTemplateModel.Left.ToString();

                        if (!customizedTemplateModel.Visible)
                            xItemNode.SelectSingleNode("@visible").InnerText = "0";
                        else
                            xItemNode.SelectSingleNode("@enabled").InnerText = customizedTemplateModel.Editable ? "1" : "0";                        
                    }
                }

                string sXML = xDoc.InnerXml.ToString();

                Controller.ConnectionController.Instance.Application.LoadBatchActions(ref sXML);
            }
            catch (Exception ex)
            {
                if (ex.Message != "XML - Unexpected tag  [66000-42]")
                    Controller.ConnectionController.Instance.Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
            }
        }

        protected string LoadComboXml<T>()
        {
            string result = "";

            var type = typeof(T);

            var enumValues = Enum.GetValues(type);

            foreach (var enumValue in enumValues)
            {
                result += string.Format(@"<ValidValue value=""{0}"" description=""{1}""/>", ((int)enumValue).ToString(), Model.EnumOperation.GetEnumDescription(enumValue));
            }

            return result;
        }

        protected string LoadComboXml(string sqlScript, params string[] variables)
        {
            string result = "";

            List<KeyValuePair<dynamic, string>> validValues = Controller.ConnectionController.Instance.ExecuteSqlForList<KeyValuePair<dynamic, string>>(sqlScript, variables);

            foreach (KeyValuePair<dynamic, string> validValue in validValues)
            {
                result += string.Format(@"<ValidValue value=""{0}"" description=""{1}""/>", validValue.Key, validValue.Value);
            }

            return result;
        }

        protected string LoadComboXml(string table, string codeField, string nameField)
        {
            return LoadComboXml("GetComboValues", table, codeField, nameField);
        }

        protected void LoadCombo(Matrix matrix, string column, string sqlScript, params string[] variables)
        {
            bool noRow = matrix.RowCount == 0;

            if (noRow)
                matrix.AddRow();

            ComboBox combo = (ComboBox)matrix.Columns.Item(column).Cells.Item(1).Specific;
            
            for (int value = combo.ValidValues.Count - 1; value >= 0; value--)
            {
                combo.ValidValues.Remove(value, BoSearchKey.psk_Index);
            }

            List<KeyValuePair<dynamic, string>> validValues = Controller.ConnectionController.Instance.ExecuteSqlForList<KeyValuePair<dynamic, string>>(sqlScript, variables);

            foreach (KeyValuePair<dynamic, string> validValue in validValues)
            {
                combo.ValidValues.Add(validValue.Key.ToString(), validValue.Value);
            }

            if (noRow)
                matrix.DeleteRow(1);
        }

        protected void LoadCombo(ComboBox combo, string sqlScript, params string[] variables)
        {
            for (int value = combo.ValidValues.Count - 1; value >= 0; value--)
            {
                combo.ValidValues.Remove(value, BoSearchKey.psk_Index);
            }

            List<KeyValuePair<dynamic, string>> validValues = Controller.ConnectionController.Instance.ExecuteSqlForList<KeyValuePair<dynamic, string>>(sqlScript, variables);

            foreach (KeyValuePair<dynamic, string> validValue in validValues)
            {
                combo.ValidValues.Add(validValue.Key.ToString(), validValue.Value);
            }
        }

        protected void LoadCombo(string table, string codeField, string nameField, ComboBox combo)
        {
            LoadCombo(combo, "GetComboValues", table, codeField, nameField);
        }

        protected void LoadCombo(string table, string codeField, string nameField, Matrix matrix, string column)
        {
            LoadCombo(matrix, column, "GetComboValues", table, codeField, nameField);
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

        protected void LoadCombo<T>(Matrix matrix, string column)
        {
            bool noRow = matrix.RowCount == 0;

            if (noRow)
                matrix.AddRow();

            ComboBox combo = (ComboBox)matrix.Columns.Item(column).Cells.Item(1).Specific;

            for (int value = combo.ValidValues.Count - 1; value >= 0; value--)
            {
                combo.ValidValues.Remove(value, BoSearchKey.psk_Index);
            }

            var type = typeof(T);

            var enumValues = Enum.GetValues(type);

            foreach (var enumValue in enumValues)
            {
                combo.ValidValues.Add(((int)enumValue).ToString(), Model.EnumOperation.GetEnumDescription(enumValue));
            }

            if (noRow)
                matrix.DeleteRow(1);
        }

        protected void LoadSystemCombo(ComboBox combo, string sqlScript, params string[] variables)
        {
            string tempValue = combo.Selected == null ? "" : combo.Selected.Value;
            
            combo.ValidValues.LoadSeries("3", BoSeriesMode.sf_Add);

            List<KeyValuePair<dynamic, string>> validValues = Controller.ConnectionController.Instance.ExecuteSqlForList<KeyValuePair<dynamic, string>>(sqlScript, variables);

            foreach (KeyValuePair<dynamic, string> validValue in validValues)
            {
                try
                {
                    combo.ValidValues.Add(validValue.Key.ToString(), validValue.Value);
                }
                catch { }
            }

            if (validValues.Exists(r => r.Key.ToString() == tempValue))
            {
                try
                {
                    combo.Select(tempValue, BoSearchKey.psk_ByValue);
                }
                catch { }
            }
        }

        protected void LoadButtonCombo<T>(ButtonCombo buttonCombo)
        {
            var type = typeof(T);

            var enumValues = Enum.GetValues(type);

            foreach (var enumValue in enumValues)
            {
                buttonCombo.ValidValues.Add(((int)enumValue).ToString(), Model.EnumOperation.GetEnumDescription(enumValue));
            }
        }

        protected void FilterChoose(EditText edit, string field, BoConditionOperation operation, string value)
        {
            if (edit.ChooseFromListUID != string.Empty)
            {
                ChooseFromList choose = (ChooseFromList)SAPForm.ChooseFromLists.Item(edit.ChooseFromListUID);

                Conditions conditions = new Conditions();
                
                Condition condition = conditions.Add();

                condition.Alias = field;
                condition.Operation = operation;
                condition.CondVal = value;

                choose.SetConditions(conditions);
            }
        }

        protected void FilterChoose(Matrix matrix, string column, string field, BoConditionOperation operation, string value)
        {
            Column edit = matrix.Columns.Item(column);

            if (edit.ChooseFromListUID != string.Empty)
            {
                ChooseFromList choose = (ChooseFromList)SAPForm.ChooseFromLists.Item(edit.ChooseFromListUID);

                Conditions conditions = new Conditions();

                Condition condition = conditions.Add();

                condition.Alias = field;
                condition.Operation = operation;
                condition.CondVal = value;

                choose.SetConditions(conditions);
            }
        }

        protected void FilterChoose(Grid grid, string column, string field, BoConditionOperation operation, string value)
        {
            EditTextColumn edit = (EditTextColumn) grid.Columns.Item(column);

            if (edit.ChooseFromListUID != string.Empty)
            {
                ChooseFromList choose = (ChooseFromList)SAPForm.ChooseFromLists.Item(edit.ChooseFromListUID);

                Conditions conditions = new Conditions();

                Condition condition = conditions.Add();

                condition.Alias = field;
                condition.Operation = operation;
                condition.CondVal = value;

                choose.SetConditions(conditions);
            }
        }

        protected void ClearFilterChoose(EditText edit)
        {
            ChooseFromList choose = (ChooseFromList)SAPForm.ChooseFromLists.Item(edit.ChooseFromListUID);

            Conditions conditions = new Conditions();

            choose.SetConditions(conditions);
        }

        public void ClearValue(string item)
        {
            if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_COMBO_BOX)
            {
                ComboBox combo = (ComboBox)SAPForm.Items.Item(item).Specific;

                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(combo.DataBind.Alias);

                userDataSource.Value = "";
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_CHECK_BOX)
            {
                CheckBox check = (CheckBox)SAPForm.Items.Item(item).Specific;

                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(check.DataBind.Alias);

                userDataSource.Value = "N";
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EDIT)
            {
                EditText editText = (EditText)SAPForm.Items.Item(item).Specific;

                if (editText.ChooseFromListUID != string.Empty)
                {
                    try
                    {
                        ChooseFromList chooseFromList = SAPForm.ChooseFromLists.Item(editText.ChooseFromListUID);

                        UserDataSource codeDataSource = SAPForm.DataSources.UserDataSources.Item("_" + editText.DataBind.Alias);

                        UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                        userDataSource.Value = "";
                        codeDataSource.Value = "";
                    }
                    catch
                    {
                        UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                        userDataSource.Value = "";
                    }
                }
                else
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((EditText)SAPForm.Items.Item(item).Specific).DataBind.Alias);

                    if (userDataSource.DataType == BoDataType.dt_SHORT_NUMBER || userDataSource.DataType == BoDataType.dt_LONG_NUMBER ||
                        userDataSource.DataType == BoDataType.dt_MEASURE || userDataSource.DataType == BoDataType.dt_PERCENT ||
                        userDataSource.DataType == BoDataType.dt_PRICE || userDataSource.DataType == BoDataType.dt_QUANTITY ||
                        userDataSource.DataType == BoDataType.dt_RATE || userDataSource.DataType == BoDataType.dt_SUM)
                    {
                        userDataSource.Value = "";
                    }
                    else
                    {
                        userDataSource.Value = "";
                    }                    
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EXTEDIT)
            {
                EditText extEdit = (EditText)SAPForm.Items.Item(item).Specific;
                extEdit.String = string.Empty;

                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(extEdit.DataBind.Alias);

                userDataSource.Value = "";
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_MATRIX)
            {
                Matrix matrix = (Matrix)SAPForm.Items.Item(item).Specific;
                matrix.Clear();

                LastBeforeRows.Remove(item);
                LastRows.Remove(item);
                LastCols.Remove(item);
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_GRID)
            {
                Grid grid = (Grid)SAPForm.Items.Item(item).Specific;                
                grid.DataTable.Rows.Clear();
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_PICTURE)
            {
                PictureBox picture = (PictureBox)SAPForm.Items.Item(item).Specific;
                picture.Picture = string.Empty;
            }
        }

        public dynamic GetValue(string item, bool fromDataSource)
        {
            return GetValue(item, "", 0, fromDataSource);
        }

        /// <summary>
        /// Busca valor de um componente na tela ou direto de um dataSource
        /// </summary>
        /// <param name="item">ID do componente ou do dataSource. Para o caso do dataSource ser um DBDataSources, separar o nome da tabela e o nome da coluna com ponto (".")</param>
        /// <param name="fromDataSource">Caso o valor deve ser buscado direto do dataSource associado</param>
        /// <returns></returns>
        public dynamic GetValue(string item, string column = "", int row = 0, bool fromDataSource = false) 
        {
            if (SAPForm.Mode == BoFormMode.fm_FIND_MODE)
            {
                if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_COMBO_BOX)
                {
                    ComboBox combo = ((ComboBox)SAPForm.Items.Item(item).Specific);

                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(combo.DataBind.Alias);

                    if (userDataSource.DataType == BoDataType.dt_SHORT_NUMBER || userDataSource.DataType == BoDataType.dt_LONG_NUMBER)
                    {
                        if (combo.Selected == null)
                            return 0;
                        else return Convert.ToInt32(combo.Selected.Value);
                    }
                    else
                    {
                        if (combo.Selected == null)
                            return string.Empty;
                        else return combo.Selected.Value;
                    }
                }
                else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EDIT)
                {
                    EditText editText = (EditText)SAPForm.Items.Item(item).Specific;
                   
                    return editText.String;
                }
                else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_CHECK_BOX)
                {
                    CheckBox check = (CheckBox)SAPForm.Items.Item(item).Specific;

                    return check.Checked;
                }
                else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_MATRIX)
                {
                    Matrix matrix = (Matrix)SAPForm.Items.Item(item).Specific;

                    if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_EDIT)
                    {
                        EditText editText = (EditText)matrix.Columns.Item(column).Cells.Item(row).Specific;

                        return editText.String;
                    }
                    else if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_COMBO_BOX)
                    {
                        ComboBox combo = (ComboBox)matrix.Columns.Item(column).Cells.Item(row).Specific;

                        try
                        {
                            if (combo.Selected == null)
                                return 0;
                            else
                                return Convert.ToInt32(combo.Selected.Value);
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    }
                    else if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_PICTURE)
                    {
                        PictureBox pictureBox = (PictureBox)matrix.Columns.Item(column).Cells.Item(row).Specific;

                        return pictureBox.Picture;
                    }
                    else return string.Empty;
                }
                else return string.Empty;
            }
            else if (fromDataSource)
            {
                if (item.Contains("."))
                {
                    string tableName = item.Split('.')[0];
                    string tableCol = item.Split('.')[1];

                    for (int index = 0; index < SAPForm.DataSources.DBDataSources.Count; index++)
                    {
                        if (cachedDBDataSources.ContainsKey(tableName))
                            index = cachedDBDataSources[tableName];

                        if (SAPForm.DataSources.DBDataSources.Item(index).TableName == tableName)
                        {
                            if (!cachedDBDataSources.ContainsKey(tableName))
                                cachedDBDataSources.Add(tableName, index);

                            if (SAPForm.DataSources.DBDataSources.Item(index).Fields.Item(tableCol).Type == BoFieldsType.ft_Date)
                            {
                                if (SAPForm.DataSources.DBDataSources.Item(index).GetValue(tableCol, SAPForm.DataSources.DBDataSources.Item(index).Offset) == string.Empty)
                                {
                                    return new DateTime(1990, 1, 1);
                                }
                                else
                                {
                                    return
                                        DateTime.ParseExact(SAPForm.DataSources.DBDataSources.Item(index).GetValue(tableCol, SAPForm.DataSources.DBDataSources.Item(index).Offset),
                                        "yyyyMMdd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None);
                                }
                            }
                            else if (SAPForm.DataSources.DBDataSources.Item(index).Fields.Item(tableCol).Type == BoFieldsType.ft_Integer)
                            {
                                if (SAPForm.DataSources.DBDataSources.Item(index).GetValue(tableCol, SAPForm.DataSources.DBDataSources.Item(index).Offset) == string.Empty)
                                {
                                    return 0;
                                }
                                else
                                {
                                    return Convert.ToInt32(SAPForm.DataSources.DBDataSources.Item(index).GetValue(tableCol, SAPForm.DataSources.DBDataSources.Item(index).Offset));
                                }
                            }
                            else
                            {
                                return SAPForm.DataSources.DBDataSources.Item(index).GetValue(tableCol, SAPForm.DataSources.DBDataSources.Item(index).Offset);
                            }
                        }
                    }
                }
                else
                {
                    return SAPForm.DataSources.UserDataSources.Item(item).Value;
                }

                return string.Empty;
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_MATRIX)
            {
                Matrix matrix = (Matrix)SAPForm.Items.Item(item).Specific;

                if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_EDIT || matrix.Columns.Item(column).Type == BoFormItemTypes.it_LINKED_BUTTON)
                {
                    EditText editText = (EditText)matrix.Columns.Item(column).Cells.Item(row).Specific;

                    try
                    {
                        DataTable dataTable = SAPForm.DataSources.DataTables.Item(editText.DataBind.TableName);

                        BoFieldsType fieldType = BoFieldsType.ft_AlphaNumeric;
                        
                        try
                        {
                            fieldType = dataTable.Columns.Item(editText.DataBind.Alias).Type;
                        }
                        catch
                        {
                            int unboundCols = 0;
                            for (int col = 0; col < matrix.Columns.Count; col++)
                            {
                                if (matrix.Columns.Item(col).DataBind.TableName == null)
                                    unboundCols++;
                                if (matrix.Columns.Item(column).UniqueID == matrix.Columns.Item(col).UniqueID)
                                {
                                    
                                    fieldType = dataTable.Columns.Item(col - unboundCols).Type;                                    
                                    break;
                                }
                            }
                        }

                        if (fieldType == BoFieldsType.ft_Integer)
                        {
                            if (editText.String == string.Empty)
                                return 0;
                            else return Convert.ToInt32(editText.String);
                        }
                        else if (fieldType == BoFieldsType.ft_Float || fieldType == BoFieldsType.ft_Measure ||
                            fieldType == BoFieldsType.ft_Percent || fieldType == BoFieldsType.ft_Price ||
                            fieldType == BoFieldsType.ft_Quantity || fieldType == BoFieldsType.ft_Rate || fieldType == BoFieldsType.ft_Sum)
                        {
                            if (editText.String == string.Empty)
                                return 0;
                            else return ConvertDouble(editText.String);
                        }
                        else
                        {
                            return editText.String;
                        }
                    }
                    catch
                    {
                        return editText.String;
                    }
                }
                else if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_CHECK_BOX)
                {
                    return ((CheckBox)matrix.Columns.Item(column).Cells.Item(row).Specific).Checked;
                }
                else if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_COMBO_BOX)
                {
                    ComboBox combo = (ComboBox)matrix.Columns.Item(column).Cells.Item(row).Specific;

                    DataTable dataTable = SAPForm.DataSources.DataTables.Item(combo.DataBind.TableName);

                    try
                    {
                        BoFieldsType fieldType = dataTable.Columns.Item(combo.DataBind.Alias).Type;

                        if (fieldType == BoFieldsType.ft_Integer)
                        {
                            if (combo.Selected == null)
                                return 0;
                            else return Convert.ToInt32(combo.Selected.Value);
                        }
                        else
                        {
                            if (combo.Selected == null)
                                return string.Empty;
                            else return combo.Selected.Value;
                        }
                    }
                    catch
                    {
                        return combo.Selected.Value;
                    }
                }
                else if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_PICTURE)
                {
                    PictureBox pictureBox = (PictureBox)matrix.Columns.Item(column).Cells.Item(row).Specific;

                    return pictureBox.Picture;
                }
                else return string.Empty;
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_GRID)
            {
                Grid grid = (Grid)SAPForm.Items.Item(item).Specific;

                return grid.DataTable.GetValue(column, row);                
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_COMBO_BOX)
            {
                ComboBox combo = (ComboBox)SAPForm.Items.Item(item).Specific;

                if (combo.DataBind.TableName == null)
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(combo.DataBind.Alias);

                    if (userDataSource.DataType == BoDataType.dt_SHORT_NUMBER || userDataSource.DataType == BoDataType.dt_LONG_NUMBER)
                    {
                        if (userDataSource.Value == string.Empty)
                            return 0;
                        else return Convert.ToInt32(userDataSource.Value);
                    }
                    else return userDataSource.Value;
                }
                else
                {
                    DataTable dataTable = SAPForm.DataSources.DataTables.Item(combo.DataBind.TableName);

                    string alias = combo.Item.Description;

                    try
                    {
                        alias = combo.DataBind.Alias;
                    }
                    catch { }

                    return dataTable.GetValue(alias, 0);
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EDIT)
            {
                EditText editText = (EditText)SAPForm.Items.Item(item).Specific;

                if (editText.DataBind.TableName == null)
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(editText.DataBind.Alias);

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
                        else return ConvertDouble(userDataSource.Value);
                    }
                    else if (userDataSource.DataType == BoDataType.dt_DATE)
                    {
                        if (userDataSource.Value == string.Empty)
                            return new DateTime(1990, 1, 1);
                        else return ConverteDate(userDataSource.Value);
                    }
                    else return userDataSource.Value;
                }
                else
                {
                    if (editText.DataBind.TableName.StartsWith("@"))
                    {
                        DBDataSource dbDataSource = SAPForm.DataSources.DBDataSources.Item(editText.DataBind.TableName);

                        return dbDataSource.GetValue(editText.DataBind.Alias, 0);
                    }
                    else
                    {
                        DataTable dataTable = SAPForm.DataSources.DataTables.Item(editText.DataBind.TableName);

                        string alias = editText.Item.Description;

                        try
                        {
                            alias = editText.DataBind.Alias;
                        }
                        catch { }

                        if (editText.ChooseFromListUID != string.Empty)
                        {
                            try
                            {
                                return dataTable.GetValue("_" + alias, 0);
                            }
                            catch
                            {
                                return dataTable.GetValue(alias, 0);
                            }
                        }
                        else
                        {
                            return dataTable.GetValue(alias, 0);
                        }
                    }
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_CHECK_BOX)
            {
                CheckBox checkBox = (CheckBox)SAPForm.Items.Item(item).Specific;

                if (checkBox.DataBind.TableName == null)
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((CheckBox)SAPForm.Items.Item(item).Specific).DataBind.Alias);

                    return userDataSource.Value == "Y";
                }
                else
                {
                    DataTable dataTable = SAPForm.DataSources.DataTables.Item(checkBox.DataBind.TableName);

                    string alias = checkBox.Item.Description;

                    try
                    {
                        alias = checkBox.DataBind.Alias;
                    }
                    catch { }

                    return dataTable.GetValue(alias, 0).ToString() == "Y";
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_PICTURE)
            {
                PictureBox pictureBox = (PictureBox)SAPForm.Items.Item(item).Specific;

                if (pictureBox.DataBind.TableName == null)
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(pictureBox.DataBind.Alias);

                    if (userDataSource.Value == string.Empty)
                        return string.Empty;
                    else return System.IO.Path.Combine(new Controller.AttachmentController().ImageFolder, userDataSource.Value);
                }
                else
                {
                    DataTable dataTable = SAPForm.DataSources.DataTables.Item(pictureBox.DataBind.TableName);

                    if (dataTable.GetValue(pictureBox.Item.Description, 0) == string.Empty)
                        return string.Empty;
                    else return System.IO.Path.Combine(new Controller.AttachmentController().ImageFolder, dataTable.GetValue(pictureBox.Item.Description, 0).ToString());
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EXTEDIT)
            {
                EditText extEdit = (EditText)SAPForm.Items.Item(item).Specific;

                if (extEdit.DataBind.TableName == null)
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(extEdit.DataBind.Alias);

                    return userDataSource.Value;
                }
                else
                {
                    DataTable dataTable = SAPForm.DataSources.DataTables.Item(extEdit.DataBind.TableName);

                    string alias = extEdit.Item.Description;

                    try
                    {
                        alias = extEdit.DataBind.Alias;
                    }
                    catch { }

                    return dataTable.GetValue(alias, 0);
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_OPTION_BUTTON)
            {
                OptionBtn option = (OptionBtn)SAPForm.Items.Item(item).Specific;

                if (option.DataBind.TableName == null)
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(option.DataBind.Alias);

                    return userDataSource.Value == ((OptionBtn)SAPForm.Items.Item(item).Specific).ValOn;
                }
                else
                {
                    DataTable dataTable = SAPForm.DataSources.DataTables.Item(option.DataBind.TableName);

                    string s = ((OptionBtn)SAPForm.Items.Item(item).Specific).ValOn;

                    return dataTable.GetValue(option.Item.Description, 0).ToString() == ((OptionBtn)SAPForm.Items.Item(item).Specific).ValOn;
                }
            }
            else return string.Empty;
        }

        protected List<T> GetValue<T>(DataTable dataTable, Matrix matrix, bool haveLastLine = true) where T : Model.BaseModel
        {
            List<T> result = new List<T>();

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            matrix.FlushToDataSource();

            for (int row = 0; row < dataTable.Rows.Count - (haveLastLine ? 1 : 0); row++)
            {
                T model = (T)Activator.CreateInstance(type);

                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    if (props.Where(r => r.Name == dataTable.Columns.Item(col).Name).Count() > 0)
                    {
                        var prop = props.First(r => r.Name == dataTable.Columns.Item(col).Name);

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
                            Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                            if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Time)
                            {
                                if (dataTable.GetValue(col, row).ToString() != "")
                                {
                                    int time = Convert.ToInt32(dataTable.GetValue(col, row).ToString().Replace(":", ""));

                                    int hours = time / 100;
                                    int minutes = time % 100;

                                    DateTime date = DateTime.Today.AddHours(hours).AddMinutes(minutes);

                                    prop.SetValue(model, date, null);
                                }                                
                            }
                            else
                            {
                                prop.SetValue(model, dataTable.GetValue(col, row));
                            }
                        }
                    }
                }

                result.Add(model);
            }

            return result;
        }

        public void SetValue(string item, dynamic value, string column = "", int row = 0, bool toDataSource = false)
        {
            if (item.Contains("."))
            {
                string tableName = item.Split('.')[0];
                string tableCol = item.Split('.')[1];

                for (int index = 0; index < SAPForm.DataSources.DBDataSources.Count; index++)
                {
                    if (SAPForm.DataSources.DBDataSources.Item(index).TableName == tableName)
                    {
                        //if (SAPForm.DataSources.DBDataSources.Item(index).Fields.Item(tableCol).Type == BoFieldsType.ft_Float)
                        //{
                            SAPForm.DataSources.DBDataSources.Item(index).SetValue(tableCol, row, value);                            
                        //}
                        //else if (SAPForm.DataSources.DBDataSources.Item(index).Fields.Item(tableCol).Type == BoFieldsType.ft_Date)
                        //{
                        //    SAPForm.DataSources.DBDataSources.Item(index).SetValue(tableCol, row, value);
                        //}
                        //else if (SAPForm.DataSources.DBDataSources.Item(index).Fields.Item(tableCol).Type == BoFieldsType.ft_Integer)
                        //{
                        //    SAPForm.DataSources.DBDataSources.Item(index).SetValue(tableCol, row, value);
                        //}
                    }
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_MATRIX)
            {
                Matrix matrix = (Matrix)SAPForm.Items.Item(item).Specific;

                if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_EDIT || matrix.Columns.Item(column).Type == BoFormItemTypes.it_LINKED_BUTTON)
                {
                    EditText edit = (EditText)matrix.Columns.Item(column).Cells.Item(row).Specific;

                    if (value.GetType() == typeof(DateTime))
                    {
                        edit.String = value.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        edit.String = value.ToString();
                    }

                    matrix.Item.Refresh();
                    SAPForm.Refresh();
                    SAPForm.Update();
                }
                else if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_COMBO_BOX)
                {
                    ComboBox combo = (ComboBox)matrix.Columns.Item(column).Cells.Item(row).Specific;

                    combo.Select(value.ToString(), BoSearchKey.psk_ByValue);
                }
                else if (matrix.Columns.Item(column).Type == BoFormItemTypes.it_CHECK_BOX)
                {
                    CheckBox check = (CheckBox)matrix.Columns.Item(column).Cells.Item(row).Specific;

                    check.Checked = value;
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_COMBO_BOX)
            {
                ComboBox combo = (ComboBox)SAPForm.Items.Item(item).Specific;

                Type type = value.GetType();

                if (combo.DataBind.TableName == null)
                {
                    UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(combo.DataBind.Alias);

                    if (type.IsEnum)
                    {
                        userDataSource.Value = (Convert.ToInt32(value)).ToString();
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
                else
                {
                    DataTable dataTable = SAPForm.DataSources.DataTables.Item(combo.DataBind.TableName);

                    if (type.IsEnum)
                    {
                        dataTable.SetValue(combo.Item.Description, 0, ((int)value));
                    }
                    else 
                    {
                        dataTable.SetValue(combo.Item.Description, 0, value);
                    }
                }
            }
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_EDIT)
            {
                EditText editText = (EditText)SAPForm.Items.Item(item).Specific;

                if (editText.DataBind.TableName == null)
                {
                    try
                    {
                        string chooseFrom = string.Empty;

                        try
                        {
                            chooseFrom = editText.ChooseFromListUID;
                        }
                        catch { }

                        if (chooseFrom != string.Empty)
                        {
                            try
                            {
                                ChooseFromList chooseFromList = SAPForm.ChooseFromLists.Item(chooseFrom);

                                string descValue = Controller.ConnectionController.Instance.ExecuteSqlForObject<string>("GetChooseValue", chooseFromList.ObjectType, value.ToString());

                                UserDataSource codeDataSource = SAPForm.DataSources.UserDataSources.Item("_" + editText.DataBind.Alias);

                                if (codeDataSource.DataType == BoDataType.dt_SHORT_NUMBER || codeDataSource.DataType == BoDataType.dt_LONG_NUMBER)
                                {
                                    if (value != 0)
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

                            if (userDataSource.DataType == BoDataType.dt_SHORT_NUMBER || userDataSource.DataType == BoDataType.dt_LONG_NUMBER ||
                                userDataSource.DataType == BoDataType.dt_MEASURE || userDataSource.DataType == BoDataType.dt_PERCENT ||
                                userDataSource.DataType == BoDataType.dt_PRICE || userDataSource.DataType == BoDataType.dt_QUANTITY ||
                                userDataSource.DataType == BoDataType.dt_RATE || userDataSource.DataType == BoDataType.dt_SUM)
                            {
                                if (value != 0)
                                    userDataSource.Value = value.ToString(DefaultNumberFormat);
                                else
                                    userDataSource.Value = string.Empty;
                            }
                            else if (userDataSource.DataType == BoDataType.dt_DATE)
                            {
                                try
                                {
                                    if (value != DateTime.MinValue && value > new DateTime(1990, 1, 1))
                                        userDataSource.Value = value.ToString("dd/MM/yyyy");
                                    else
                                        userDataSource.Value = string.Empty;
                                }
                                catch
                                {
                                    userDataSource.Value = DateTime.Now.ToString("dd/MM/yyyy");
                                }
                            }
                            else
                            {
                                userDataSource.Value = value.ToString();
                            }
                        }
                    }
                    catch
                    {
                        editText.String = value;
                    }
                }
                else
                {
                    try
                    {
                        DataTable dataTable = SAPForm.DataSources.DataTables.Item(editText.DataBind.TableName);

                        string alias = editText.Item.Description;

                        try
                        {
                            alias = editText.DataBind.Alias;
                        }
                        catch { }

                        if (editText.ChooseFromListUID != string.Empty)
                        {
                            ChooseFromList chooseFromList = SAPForm.ChooseFromLists.Item(editText.ChooseFromListUID);

                            string descValue = Controller.ConnectionController.Instance.ExecuteSqlForObject<string>("GetChooseValue", chooseFromList.ObjectType, value.ToString());

                            try
                            {
                                if (dataTable.Columns.Item("_" + editText.Item.Description).Type == BoFieldsType.ft_AlphaNumeric)
                                    dataTable.SetValue("_" + alias, 0, value);
                                else
                                    dataTable.SetValue("_" + alias, 0, Convert.ToInt32(value));

                                dataTable.SetValue(alias, 0, descValue);
                            }
                            catch
                            {
                                if (dataTable.Columns.Item(alias).Type == BoFieldsType.ft_AlphaNumeric)
                                    dataTable.SetValue(alias, 0, value);
                                else
                                    dataTable.SetValue(alias, 0, Convert.ToInt32(value));
                            }
                        }
                        else
                        {
                            dataTable.SetValue(alias, 0, value);
                        }
                    }
                    catch
                    {
                        editText.String = value;
                    }
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
            else if (SAPForm.Items.Item(item).Type == BoFormItemTypes.it_OPTION_BUTTON)
            {
                UserDataSource userDataSource = SAPForm.DataSources.UserDataSources.Item(((OptionBtn)SAPForm.Items.Item(item).Specific).DataBind.Alias);
                userDataSource.Value = value ? ((OptionBtn)SAPForm.Items.Item(item).Specific).ValOn : ((OptionBtn)SAPForm.Items.Item(item).Specific).ValOn;
            }
        }

        public void SetValue<T>(DataTable dataTable, Matrix matrix, List<T> list, bool addLastLine = true, bool clearRows = true) where T : Model.BaseModel
        {
            if (clearRows)
                dataTable.Rows.Clear();

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            foreach (T model in list)
            {
                dataTable.Rows.Add();

                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    if (props.Where(r => r.Name == dataTable.Columns.Item(col).Name).Count() > 0)
                    {
                        var prop = props.First(r => r.Name == dataTable.Columns.Item(col).Name);

                        if (prop.PropertyType == typeof(Boolean))
                        {
                            dataTable.SetValue(col, dataTable.Rows.Count - 1, (bool)prop.GetValue(model) ? "Y" : "N");
                        }
                        else if (prop.PropertyType.IsEnum)
                        {
                            dataTable.SetValue(col, dataTable.Rows.Count - 1, (int)prop.GetValue(model)); ;
                        }
                        else
                        {
                            Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                            if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Time)
                            {
                                DateTime date = Convert.ToDateTime(prop.GetValue(model));

                                string time = date.Hour.ToString("00") + ":" + date.Minute.ToString("00");

                                dataTable.SetValue(col, dataTable.Rows.Count - 1, time);
                            }
                            else
                            {
                                dataTable.SetValue(col, dataTable.Rows.Count - 1, prop.GetValue(model));
                            }
                        }
                    }
                }
            }

            if (addLastLine)
                dataTable.Rows.Add();

            matrix.LoadFromDataSource();

            if (addLastLine)
            {
                if (matrix.RowCount == 0)
                {
                    matrix.AddRow();
                }

                try
                {
                    if (matrix.Columns.Item(0).Description == "Pos" || matrix.Columns.Item(0).DataBind.Alias == "Pos")
                        ((EditText)matrix.Columns.Item(0).Cells.Item(matrix.RowCount).Specific).String = matrix.RowCount.ToString();
                }
                catch { }
            }
        }

        public void SetValuePro<T>(DataTable dataTable, Matrix matrix, List<T> list, bool addLastLine = false)
        {
            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed" && r.Name != "Code");

            List<string> selects = new List<string>();

            foreach (T model in list)
            {
                List<string> values = new List<string>();

                foreach (var prop in props)
                {
                    if (prop.PropertyType == typeof(Boolean))
                    {
                        values.Add((bool)prop.GetValue(model) ? "'Y'" : "'N'" + @" AS """ + prop.Name + @"""");
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        values.Add(((int)prop.GetValue(model)).ToString() + @" AS """ + prop.Name + @"""");
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(prop.GetValue(model)) == DateTime.MinValue || Convert.ToDateTime(prop.GetValue(model)) == new DateTime(1899, 12, 30))
                        {
                            if (ConnectionController.Instance.DBServerType == "HANA")
                                values.Add("to_date(null) " + @" AS """ + prop.Name + @"""");
                            else
                                values.Add("cast(null as date)");
                        }
                        else
                        {
                            values.Add("cast ('" + Convert.ToDateTime(prop.GetValue(model)).ToString("yyyy-MM-dd") + "' as date)" + @" AS """ + prop.Name + @"""");
                        }
                    }
                    else if (prop.PropertyType == typeof(Int32))
                    {
                        values.Add(prop.GetValue(model).ToString() + @" AS """ + prop.Name + @"""");
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        if (Convert.ToDouble(prop.GetValue(model)) == 0)
                        {
                            values.Add("0.0" + @" AS """ + prop.Name + @"""");
                        }
                        else
                        {
                            Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                            int decimalDigits = 2;

                            if (specificType != null)
                            {
                                decimalDigits = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForObject<int>("GetDisplayDecimalDigits", ((int)specificType.Value).ToString());
                            }

                            values.Add(string.Format("cast({0} as decimal(15,{1}))", Convert.ToDouble(prop.GetValue(model)).ToString(DefaultSQLNumberFormat), decimalDigits.ToString()) + @" AS """ + prop.Name + @"""");
                        }
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        if (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) != null)
                            values.Add("cast('" + prop.GetValue(model).ToString().Replace("'", "''") + "' as varchar(" + (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) as Model.BaseModel.Size).Value.ToString() + "))" + @" AS """ + prop.Name + @"""");
                        else
                            values.Add("cast('" + prop.GetValue(model).ToString().Replace("'", "''") + "' as nvarchar)" + @" AS """ + prop.Name + @"""");
                    }
                }

                selects.Add(" select " + (type.BaseType == typeof(Model.BaseModel) ? type.GetProperty("Code").GetValue(model) + @" AS ""Code"" " + "," : "") + string.Join(",", values.ToArray()) + (Controller.ConnectionController.Instance.DBServerType == "HANA" ? " from dummy " : " "));
            }

            if (list.Count == 0)
                dataTable.Rows.Clear();
            else
            {
                try
                {
                    dataTable.ExecuteQuery(string.Join("union all", selects.ToArray()));
                }
                catch (Exception ex)
                {
                    System.IO.File.WriteAllText("sql.sql", string.Join("union all", selects.ToArray()));
                }
            }

            matrix.LoadFromDataSource();

            if (addLastLine)
            {
                matrix.AddRow();
                matrix.ClearRowData(matrix.RowCount);

            }
        }
        
        public void SetValuePro<T>(Grid grid, List<T> list)
        {
            DataTable dataTable = grid.DataTable;

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed" && r.Name != "Code");

            List<string> selects = new List<string>();

            foreach (T model in list)
            {
                List<string> values = new List<string>();

                foreach (var prop in props)
                {
                    if (prop.PropertyType == typeof(Boolean))
                    {
                        values.Add((bool)prop.GetValue(model) ? "'Y'" : "'N'" + " as " + prop.Name);
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        values.Add(((int)prop.GetValue(model)).ToString() + " as " + prop.Name);
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        values.Add("cast ('" + Convert.ToDateTime(prop.GetValue(model)).ToString("yyyy-MM-dd") + "' as date)" + " as " + prop.Name);
                    }
                    else if (prop.PropertyType == typeof(Int32))
                    {
                        values.Add(prop.GetValue(model).ToString() + " as " + prop.Name);
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        if (Convert.ToDouble(prop.GetValue(model)) == 0)
                        {
                            values.Add("0.0" + " as " + prop.Name);
                        }
                        else
                        {
                            Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                            int decimalDigits = 2;

                            if (specificType != null)
                            {
                                decimalDigits = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForObject<int>("GetDisplayDecimalDigits", ((int)specificType.Value).ToString());
                            }

                            values.Add(string.Format("cast({0} as decimal(15,{1}))" + " as " + prop.Name, Convert.ToDouble(prop.GetValue(model)).ToString(DefaultSQLNumberFormat), decimalDigits.ToString()));
                        }
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        if (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) != null)
                            values.Add("cast('" + prop.GetValue(model) + "' as varchar(" + (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) as Model.BaseModel.Size).Value.ToString() + "))" + " as " + prop.Name);
                        else
                            values.Add("cast('" + prop.GetValue(model).ToString() + "' as nvarchar)" + " as " + prop.Name);
                    }
                }

                selects.Add(" select " + (type.BaseType == typeof(Model.BaseModel) ? type.GetProperty("Code").GetValue(model) + " as Code," : "") + string.Join(",", values.ToArray()) + (Controller.ConnectionController.Instance.DBServerType == "HANA" ? " from dummy " : " "));
            }

            if (list.Count == 0)
                dataTable.Rows.Clear();
            else
            {
                try
                {
                    dataTable.ExecuteQuery(string.Join("union all", selects.ToArray()));
                }
                catch (Exception ex)
                {
                    System.IO.File.WriteAllText("C:\\RNV Soluções\\SQL.txt", string.Join("union all", selects.ToArray()));
                }
            }            
        }

        public void SetValue<T>(DataTable dataTable, T model) where T : Model.BaseModel
        {
            dataTable.Rows.Clear();
            dataTable.Rows.Add();

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            for (int col = 0; col < dataTable.Columns.Count; col++)
            {
                try
                {
                    var prop = props.First(r => r.Name == dataTable.Columns.Item(col).Name);

                    if (prop.PropertyType == typeof(Boolean))
                    {
                        dataTable.SetValue(col, dataTable.Rows.Count - 1, (bool)prop.GetValue(model) ? "Y" : "N");
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        dataTable.SetValue(col, dataTable.Rows.Count - 1, (int)prop.GetValue(model)); ;
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(prop.GetValue(model)) != DateTime.MinValue && Convert.ToDateTime(prop.GetValue(model)) != new DateTime(1899, 12, 30))
                        {
                            dataTable.SetValue(col, dataTable.Rows.Count - 1, prop.GetValue(model));
                        }
                    }
                    else
                    {
                        dataTable.SetValue(col, dataTable.Rows.Count - 1, prop.GetValue(model));
                    }
                }
                catch { }
            }
        }
        
        public void SetValuePro<T>(DataTable dataTable, T model, string sqlScript, params string[] variables) 
        {
            string select = Controller.ConnectionController.Instance.GetSQL(sqlScript, variables);

            dataTable.Rows.Clear();
            dataTable.Rows.Add();

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            foreach (var prop in props)
            {
                string value = string.Empty;

                if (prop.PropertyType == typeof(Boolean))
                {
                    value = (bool)prop.GetValue(model) ? "'Y'" : "'N'";
                }
                else if (prop.PropertyType.IsEnum)
                {
                    value = ((int)prop.GetValue(model)).ToString();
                }
                else if (prop.PropertyType == typeof(DateTime))
                {
                    value = "cast ('" + Convert.ToDateTime(prop.GetValue(model)).ToString("yyyy-MM-dd") + "' as date)";
                }
                else if (prop.PropertyType == typeof(Int32))
                {
                    value = prop.GetValue(model).ToString();
                }
                else if (prop.PropertyType == typeof(double))
                {
                    if (Convert.ToDouble(prop.GetValue(model)) == 0)
                    {
                        value = "0.0";
                    }
                    else
                    {
                        Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                        int decimalDigits = 2;

                        if (specificType != null)
                        {
                            decimalDigits = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForObject<int>("GetDisplayDecimalDigits", ((int)specificType.Value).ToString());
                        }

                        value = string.Format("cast({0} as decimal(15,{1}))", Convert.ToDouble(prop.GetValue(model)).ToString(DefaultSQLNumberFormat), decimalDigits.ToString());
                    }
                }
                else if (prop.PropertyType == typeof(string))
                {
                    if (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) != null)
                        value = "cast('" + prop.GetValue(model) + "' as varchar(" + (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) as Model.BaseModel.Size).Value.ToString() + "))";
                    else
                    {
                        Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                        if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Memo)
                        {
                            value = "'" + prop.GetValue(model).ToString() + "'";
                        }
                        else
                            value = prop.GetValue(model).ToString();
                    }
                }

                select = select.Replace("\"U_" + prop.Name + "\"", value);
            }

            try
            {
                dataTable.ExecuteQuery(select);
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("C:\\RNV Soluções\\SQL.txt", select);
            }
        }

        public void SetValuePro<T>(DataTable dataTable, T model)
        {
            dataTable.Rows.Clear();            

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            string select = string.Empty;

            List<string> values = new List<string>();

            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(Boolean))
                {
                    values.Add((bool)prop.GetValue(model) ? "'Y'" : "'N'");
                }
                else if (prop.PropertyType.IsEnum)
                {
                    values.Add(((int)prop.GetValue(model)).ToString());
                }
                else if (prop.PropertyType == typeof(DateTime))
                {
                    if (Convert.ToDateTime(prop.GetValue(model)) == DateTime.MinValue || Convert.ToDateTime(prop.GetValue(model)) == new DateTime(1899, 12, 30))
                    {
                        if (ConnectionController.Instance.DBServerType == "HANA")
                            values.Add("to_date(null)");
                        else
                            values.Add("cast(null as date)");
                    }
                    else
                    {
                        values.Add("cast ('" + Convert.ToDateTime(prop.GetValue(model)).ToString("yyyy-MM-dd") + "' as date)");
                    }
                }
                else if (prop.PropertyType == typeof(Int32))
                {
                    values.Add(prop.GetValue(model).ToString() + " as " + prop.Name);
                }
                else if (prop.PropertyType == typeof(double))
                {
                    if (Convert.ToDouble(prop.GetValue(model)) == 0)
                    {
                        values.Add("0.0");
                    }
                    else
                    {
                        Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                        int decimalDigits = 2;

                        if (specificType != null)
                        {
                            decimalDigits = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForObject<int>("GetDisplayDecimalDigits", ((int)specificType.Value).ToString());
                        }

                        values.Add(string.Format("cast({0} as decimal(15,{1}))", Convert.ToDouble(prop.GetValue(model)).ToString(DefaultSQLNumberFormat), decimalDigits.ToString()));
                    }
                }
                else if (prop.PropertyType == typeof(string))
                {
                    if (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) != null)
                        values.Add("cast('" + prop.GetValue(model) + "' as varchar(" + (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) as Model.BaseModel.Size).Value.ToString() + "))");
                    else
                    {
                        Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                        if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Memo)
                        {
                            values.Add("'" + prop.GetValue(model).ToString() + "'");
                        }
                        else
                            values.Add(prop.GetValue(model).ToString());
                    }
                }
            }

            select = " select " + type.GetProperty("Code").GetValue(model) + "," + string.Join(",", values.ToArray()) + (Controller.ConnectionController.Instance.DBServerType == "HANA" ? " from dummy " : " ");

            try
            {
                dataTable.ExecuteQuery(select);
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("C:\\RNV Soluções\\SQL.txt", select);
            }
        }

        public virtual int FindFormData()
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(((EditText)SAPForm.Items.Item(m_BrowseItem).Specific).String, @"^\d+$"))
            {
                if (Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetModelExists", m_BrowseTable, ((EditText)SAPForm.Items.Item(m_BrowseItem).Specific).String) > 0)
                    return Convert.ToInt32(((EditText)SAPForm.Items.Item(m_BrowseItem).Specific).String);
                else
                    return -1;
            }
            else
                ((EditText)SAPForm.Items.Item(m_BrowseItem).Specific).ClickPicker();

            return 0;
        }

        /// <summary>
        /// Chamar método base para realizar as operações necessárias
        /// </summary>
        public virtual void GotFormData()
        {
            IsDraft = false;

            if (!FormUID.Contains("F_") && !SecondaryView)
            {
                if (m_BrowseItem != string.Empty)
                {
                    int code = GetValue(string.Format("@{0}.U_Code", m_BrowseTable), "", 0, true);

                    if (m_BrowseItem == "BACKCODE")
                        SetValue(m_BrowseItem, code.ToString());
                    else
                        SetValue(m_BrowseItem, code);

                    SAPForm.ActiveItem = "DUMMY";

                    SAPForm.Items.Item(m_BrowseItem).Enabled = false;

                    SAPForm.EnableMenu("1282", true);
                    SAPForm.EnableMenu("1281", true);
                }
            }
            else
            {
                //try
                //{
                //    int docNum = ((EditText)SAPForm.Items.Item("8").Specific).String;

                //    string table = ((EditText)SAPForm.Items.Item("8").Specific).DataBind.TableName;

                //    int objType = Convert.ToInt32(SAPForm.DataSources.DBDataSources.Item(table).GetValue("ObjType", 0));

                //    if (new DAO.DocumentDAO().GetDocEntry(docNum, (Model.EnumObjType)objType) == 0)
                //    {
                //        IsDraft = true;
                //    }
                //}
                //catch { }
                IsDraft = SAPForm.Title.Contains("Esboço");
            }
        }

        /// <summary>
        /// Chamar método base para realizar as operações necessárias
        /// </summary>
        public virtual void AddFormData() 
        {
            m_addFlag = true;

            if (FormUID.Contains("F_") && !SecondaryView)
            {
                try
                {
                    string xml = SAPForm.BusinessObject.Key;

                    LastDocEntry = Convert.ToInt32(xml.Substring(xml.IndexOf("<DocEntry>") + 10, xml.IndexOf("</DocEntry>") - (xml.IndexOf("<DocEntry>") + 10)));

                }
                catch
                {
                    LastDocEntry = 0;
                }

                try
                {
                    string xml = SAPForm.BusinessObject.Key;

                    LastAbsEntry = Convert.ToInt32(xml.Substring(xml.IndexOf("<AbsEntry>") + 10, xml.IndexOf("</AbsEntry>") - (xml.IndexOf("<AbsEntry>") + 10)));

                }
                catch
                {
                    LastAbsEntry = 0;
                }

                try
                {
                    string xml = SAPForm.BusinessObject.Key;

                    LastCode = Convert.ToInt32(xml.Substring(xml.IndexOf("<Code>") + 6, xml.IndexOf("</Code>") - (xml.IndexOf("<Code>") + 6)));

                }
                catch
                {
                    LastCode = 0;
                }
            }
        }

        /// <summary>
        /// Chamar método base para realizar as operações necessárias
        /// </summary>
        public virtual void UpdateFormData()
        {
            m_updateFlag = true;
        }

        public virtual void DeleteFormData() { }

        public virtual void BeforeAddFormData() { }

        public virtual void BeforeUpdateFormData() { }

        public virtual void BeforeDeleteFormData() { }

        private void AfterAddFormDataInternal()
        {
            //try
            //{
            //    int docNum = LastDocNum;

            //    string table = ((EditText)SAPForm.Items.Item("8").Specific).DataBind.TableName;

            //    int objType = Convert.ToInt32(SAPForm.DataSources.DBDataSources.Item(table).GetValue("ObjType", 0));

            //    if (new DAO.DocumentDAO().GetDocEntry(docNum, (Model.EnumObjType)objType) == 0)
            //    {
            //        SavedAsDraft = true;

            //        for (int i = 0; i <= 3; i++)
            //        {
            //            System.Threading.Thread.Sleep(1000);

            //            LastDraftEntry = ConnectionController.Instance.ExecuteSqlForObject<int>("GetLastDraftEntry", docNum.ToString(), objType.ToString());

            //            if (LastDraftEntry > 0)
            //                break;
            //        }                    
            //    }
            //}
            //catch
            //{
            //    SavedAsDraft = false;
            //    LastDraftEntry = 0;
            //}

            AfterAddFormData();

            SavedAsDraft = false;
        }

        public virtual void AfterAddFormData() { }

        public virtual void AfterUpdateFormData() { }

        public virtual void AfterDeleteFormData() { }

        public virtual bool ValidateFormData(out string msg, bool delete) { msg = string.Empty; return true; }

        /// <summary>
        /// Chamar método base para realizar as operações necessárias
        /// </summary>               
        public virtual void MenuInsert() 
        {
            if (!FormUID.Contains("F_") && !SecondaryView)
            {
                if (m_BrowseItem != string.Empty)
                {
                    int code = Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetLastCode", m_BrowseTable, new DAO.ConfigSeqDAO().TableName);

                    if (m_BrowseItem == "BACKCODE")
                        SetValue(m_BrowseItem, code.ToString());
                    else
                        SetValue(m_BrowseItem, code);

                    if (m_ReserveCode)
                        Controller.ConnectionController.Instance.ExecuteStatement("UpdateLastCode", m_BrowseTable, new DAO.ConfigSeqDAO().TableName, code.ToString());

                    SAPForm.ActiveItem = "DUMMY";

                    SAPForm.Items.Item(m_BrowseItem).Enabled = false;

                    SAPForm.EnableMenu("1282", false);
                    SAPForm.EnableMenu("1281", true);
                }
            }
        }

        /// <summary>
        /// Chamar método base para realizar as operações necessárias
        /// </summary>
        public virtual void MenuSearch() 
        {
            if (!FormUID.Contains("F_") && !SecondaryView)
            {
                if (m_BrowseItem != string.Empty)
                {
                    SAPForm.Items.Item(m_BrowseItem).Enabled = true;

                    SAPForm.ActiveItem = m_BrowseItem;

                    ((EditText)SAPForm.Items.Item(m_BrowseItem).Specific).String = "";

                    SAPForm.EnableMenu("1281", false);
                    SAPForm.EnableMenu("1282", true);
                }
            }
        }

        public virtual void MenuDuplicate() { }

        public virtual void MenuSaveAsDraft() { }

        public virtual void AfterSaveAsDraft()
        {
        }

        public virtual void MenuCancel() { }

        public virtual void Resize() { }

        public virtual void GotFocus() { }

        public virtual void LostFocus() { }

        public virtual bool CanClose()
        {
            return true;
        }

        public virtual void Close() { }

        public virtual void ContextMenuShow() { }

        public void FormLostFocus()
        {
            UnloadMenus();
        }

        public void UnloadMenus()
        {
            foreach (KeyValuePair<string, MatrixColPasteForAllEventHandler> menuEvent in MatrixColPasteForAllEvents)
            {
                string menuID = string.Format("MNUPFA{0}{1}", SAPForm.TypeEx, SAPForm.UniqueID);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);
            }

            foreach (KeyValuePair<string, MatrixRowRemoveEventHandler> menuEvent in MatrixRowRemoveEvents)
            {
                string menuID = string.Format("MNUREM{0}{1}", SAPForm.TypeEx, SAPForm.UniqueID);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);
            }

            foreach (KeyValuePair<string, Tuple<string, MatrixCustomMenuEventHandler>> matrixCustomEvent in MatrixCustomMenuEvents)
            {
                string menuID = string.Format("{0}{1}{2}", matrixCustomEvent.Key, SAPForm.TypeEx, SAPForm.UniqueID);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);
            }


            if (Controller.ConnectionController.Instance.Application.Menus.Exists("HZLINEMTX"))
                Controller.ConnectionController.Instance.Application.Menus.RemoveEx("HZLINEMTX");

            foreach (KeyValuePair<string, Tuple<string, CustomMenuEventHandler>> customEvent in CustomMenuEvents)
            {
                string menuID = string.Format("{0}{1}{2}", customEvent.Key, SAPForm.TypeEx, SAPForm.UniqueID);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);
            }
        }

        public virtual void ButtonOkClick()
        {
            LastFormMode = SAPForm.Mode;

            if (!FormUID.Contains("F_") && !SecondaryView)
            {
                if (SAPForm.Mode == BoFormMode.fm_FIND_MODE)
                {
                    int code = FindFormData();

                    if (code > 0)
                    {
                        ((EditText)SAPForm.Items.Item("BACKCODE").Specific).String = code.ToString();

                        SAPForm.ActiveItem = "DUMMY";

                        SAPForm.Items.Item(m_BrowseItem).Enabled = false;

                        SAPForm.EnableMenu("1282", true);
                        SAPForm.EnableMenu("1281", true);
                    }
                    else if (code == -1)
                    {
                        Controller.ConnectionController.Instance.Application.StatusBar.SetText("Nenhum registro concordante encontrado");
                    }
                }
                else if (SAPForm.Mode == BoFormMode.fm_ADD_MODE)
                {
                    BeforeAddFormData();
                }
                else if (SAPForm.Mode == BoFormMode.fm_UPDATE_MODE)
                {
                    BeforeUpdateFormData();

                    m_updateFailed = false;

                    string msg;

                    if (ValidateFormData(out msg, false))
                    {
                        try
                        {
                            UpdateFormData();
                        }
                        catch(Exception ex)
                        {
                            AddOn.Instance.ConnectionController.Application.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);

                            m_updateFailed = true;

                            return;
                        }
                    }
                    else
                    {
                        AddOn.Instance.ConnectionController.Application.StatusBar.SetText(msg, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);

                        m_updateFailed = true;                        

                        return;
                    }
                }
            }
            else
            {
                try
                {
                    string value = ((EditText)SAPForm.Items.Item("8").Specific).String;

                    LastDocNum = Convert.ToInt32(value);
                }
                catch
                {
                    LastDocNum = 0;
                }

                if (SAPForm.Mode == BoFormMode.fm_ADD_MODE)
                {
                    BeforeAddFormData();
                }
                else if (SAPForm.Mode == BoFormMode.fm_UPDATE_MODE)
                {
                    BeforeUpdateFormData();
                }
            }
        }

        public virtual void ButtonOkPress()
        {
            if (!FormUID.Contains("F_") && !SecondaryView)
            {
                if (SAPForm.Mode == BoFormMode.fm_ADD_MODE)
                {
                    string msg;

                    if (ValidateFormData(out msg, false))
                    {
                        AddFormData();

                        SAPForm.Mode = BoFormMode.fm_OK_MODE;

                        if (m_DefaultInsertMode)
                        {
                            SAPForm.EnableMenu("1282", true);

                            Controller.ConnectionController.Instance.Application.ActivateMenuItem("1282");
                        }
                        else
                        {
                            SAPForm.Mode = BoFormMode.fm_FIND_MODE;

                            MenuSearch();
                        }                        
                    }
                    else
                    {
                        AddOn.Instance.ConnectionController.Application.StatusBar.SetText(msg, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);                        

                        return;
                    }
                }

                if (m_updateFailed)
                {
                    SAPForm.Mode = BoFormMode.fm_UPDATE_MODE;

                    AddOn.Instance.ConnectionController.Application.StatusBar.SetText("Operação completada com erro. Verifique o log de mensagens e tente novamente.", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);
                }
            }

            if (m_addFlag)
            {
                m_addFlag = false;

                AfterAddFormDataInternal();
            }
            else if (m_updateFlag)
            {
                m_updateFlag = false;

                AfterUpdateFormData();
            }
        }

        public void ButtonBeforeClick(string button)
        {
            if (ButtonBeforeClickEvents.ContainsKey(button))
            {
                ButtonBeforeClickEvents[button]();
            }
        }

        public void ButtonClick(string button)
        {
            if (button == BUTTON_DOC_COPY)
            {
                if (DocCopyEvents.Count > 0)
                {
                    m_copyFlag = true;
                }
            }

            if (ButtonClickEvents.ContainsKey(button))
            {
                LastButtonClicked = button;

                ButtonClickEvents[button]();
            }

            if (ButtonOpenViewEvents.ContainsKey(button))
            {
                LastButtonClicked = button;
            }

            if (ButtonComboClickEvents.ContainsKey(button) && !Frozen)
            {
                ButtonComboClickEvents[button](LastButtonComboValue !=
                    (((ButtonCombo)SAPForm.Items.Item(button).Specific).Selected == null ? string.Empty :
                    ((ButtonCombo)SAPForm.Items.Item(button).Specific).Selected.Value));
            }
        }

        public void ButtonPress(string button)
        {
            if (ButtonPressEvents.ContainsKey(button))
            {
                ButtonPressEvents[button]();
            }
        }

        public bool SupressPickerClick(string edit)
        {
            if (SupressActionEvents.ContainsKey(edit) && !Frozen)
            {
                bool supressed = false;

                SupressActionEvents[edit](BoEventTypes.et_PICKER_CLICKED, out supressed);

                return supressed;
            }

            return false;
        }

        public bool SupressChooseFrom(string edit)
        {
            if (SupressActionEvents.ContainsKey(edit) && !Frozen)
            {
                bool supressed = false;

                SupressActionEvents[edit](BoEventTypes.et_CHOOSE_FROM_LIST, out supressed);

                return supressed;
            }

            return false;
        }

        public bool ColSupressChooseFrom(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColSupressActionEvents.ContainsKey(key) && !Frozen)
            {
                bool supressed = false;

                ColSupressActionEvents[key](BoEventTypes.et_CHOOSE_FROM_LIST, row, out supressed);

                return supressed;
            }

            return false;
        }

        public bool SupressMatrixDetails(string matrix)
        {
            if (SupressMatrixDetailsEvents.ContainsKey(matrix))
            {
                return SupressMatrixDetailsEvents[matrix]() && !Frozen;
            }
            else
                return false;
        }

        public void KeyDown(string edit)
        {
            if (KeyDownEvents.ContainsKey(edit))
            {
                KeyDownEvents[edit]();
            }
        }

        public void ChooseFrom(string edit, params string[] values)
        {
            if (ChooseFromEvents.ContainsKey(edit))
            {
                EditText editText = (EditText)SAPForm.Items.Item(edit).Specific;

                if (editText.DataBind.TableName == null)
                {
                    try
                    {
                        UserDataSource codeDataSource = SAPForm.DataSources.UserDataSources.Item("_" + editText.DataBind.Alias);
                        codeDataSource.Value = values[0];

                        UserDataSource valueDataSource = SAPForm.DataSources.UserDataSources.Item(editText.DataBind.Alias);
                        valueDataSource.Value = values[1];
                    }
                    catch
                    {
                        SAPForm.DataSources.UserDataSources.Item(editText.DataBind.Alias).Value = values[0];
                    }
                }
                else
                {
                    try
                    {
                        DataTable dataTable = SAPForm.DataSources.DataTables.Item(editText.DataBind.TableName);

                        string alias = editText.Item.Description;

                        try
                        {
                            alias = editText.DataBind.Alias;
                        }
                        catch { }

                        try
                        {
                            if (dataTable.Columns.Item("_" + alias).Type == BoFieldsType.ft_AlphaNumeric)
                                dataTable.SetValue("_" + alias, 0, values[0]);
                            else
                                dataTable.SetValue("_" + alias, 0, Convert.ToInt32(values[0]));
                            if (dataTable.Columns.Item(alias).Type == BoFieldsType.ft_AlphaNumeric)
                                dataTable.SetValue(alias, 0, values[1]);
                            else
                                dataTable.SetValue(alias, 0, Convert.ToInt32(values[1]));
                        }
                        catch
                        {
                            if (dataTable.Columns.Item(alias).Type == BoFieldsType.ft_AlphaNumeric)
                                dataTable.SetValue(alias, 0, values[0]);
                            else
                                dataTable.SetValue(alias, 0, Convert.ToInt32(values[0]));
                        }
                    }
                    catch (Exception e) { string s = e.Message; }
                }

                ChooseFromEvents[edit](values);
            }
        }

        public void ColChooseFrom(string matrix, int row, string column, Dictionary<string, string> values)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColChooseFromEvents.ContainsKey(key))
            {
                if (SAPForm.Items.Item(matrix).Type == BoFormItemTypes.it_GRID)
                {
                    EditTextColumn col = ((EditTextColumn)((Grid)SAPForm.Items.Item(matrix).Specific).Columns.Item(column));

                    col.SetText(row, values[col.ChooseFromListAlias]);
                }
                else
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

                            if (values.ContainsKey(alias) && matrixItem.Columns.Item(col).ChooseFromListUID == matrixItem.Columns.Item(column).ChooseFromListUID)
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

                    if (MatrixCanAddEvents.ContainsKey(key) && !Frozen)
                    {
                        if (row == matrixItem.RowCount)
                        {
                            if (MatrixCanAddEvents[key](row))
                            {
                                matrixItem.AddRow();
                                matrixItem.ClearRowData(matrixItem.RowCount);

                                try
                                {
                                    if (matrixItem.Columns.Item(0).Description == "Pos" || matrixItem.Columns.Item(0).DataBind.Alias == "Pos")
                                        ((EditText)matrixItem.Columns.Item(0).Cells.Item(matrixItem.RowCount).Specific).String = matrixItem.RowCount.ToString();
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        public void FolderSelect(string folder)
        {
            if (FolderSelectEvents.ContainsKey(folder))
            {
                FolderSelectEvents[folder]();
            }
        }

        public void GridRowDoubleClick(string grid, int row, string column)
        {
            
            if (row == -1)
            {
                string key = string.Format("{0}.{1}", grid, column);

                if (GridCheckAllEvents.ContainsKey(key))
                {
                    if (GridCheckAllEvents[key]())
                    {
                        bool check = false;

                        Grid gridItem = (Grid)SAPForm.Items.Item(grid).Specific;

                        if (gridItem.Rows.Count > 0)
                            check = !((CheckBoxColumn)gridItem.Columns.Item(column)).IsChecked(0);

                        List<int> rows = new List<int>();

                        for (int aux = 0; aux < gridItem.Rows.Count; aux++)
                        {
                            ((CheckBoxColumn)gridItem.Columns.Item(column)).Check(aux, check);

                            rows.Add(aux);
                        }

                        if (ColumnCheckProEvents.ContainsKey(key))
                            ColumnCheckProEvents[key](rows.ToArray(), check);
                    }
                }
            }
            else if (GridRowDoubleClickEvents.ContainsKey(grid) && !Frozen)
            {
                if (LastRows.ContainsKey(grid))
                {
                    LastBeforeRows[grid] = LastRows[grid];
                    LastRows[grid] = row;
                    LastCols[grid] = column;
                }
                else
                {
                    LastBeforeRows.Add(grid, 1);
                    LastRows.Add(grid, row);
                    LastCols.Add(grid, column);
                }

                GridRowDoubleClickEvents[grid](row, column);
            }
        }

        public void GridRowClick(string grid, int row, string column)
        {
            if (GridRowClickEvents.ContainsKey(grid) && !Frozen)
            {
                if (LastRows.ContainsKey(grid))
                {
                    LastBeforeRows[grid] = LastRows[grid];
                    LastRows[grid] = row;
                    LastCols[grid] = column;
                }
                else
                {
                    LastBeforeRows.Add(grid, 1);
                    LastRows.Add(grid, row);
                    LastCols.Add(grid, column);
                }

                GridRowClickEvents[grid](row, column);
            }
        }

        public void GridTabPressed(string grid, int row, string column)
        {
            if (GridTabPressedEvents.ContainsKey(grid))
            {
                GridTabPressedEvents[grid](row, column);
            }
        }

        public void GridLinkPressed(string grid, int row, string column, ref bool suspend)
        {
            if (GridLinkPressedEvents.ContainsKey(grid + "." + column) && !Frozen)
            {
                if (LastRows.ContainsKey(grid))
                {
                    LastBeforeRows[grid] = LastRows[grid];
                    LastRows[grid] = row;
                    LastCols[grid] = column;
                }
                else
                {
                    LastBeforeRows.Add(grid, 1);
                    LastRows.Add(grid, row);
                    LastCols.Add(grid, column);
                }

                GridLinkPressedEvents[grid + "." + column](row, out suspend);
            }
        }

        public void MatrixRowEnter(string matrix, int row, string column, BoModifiersEnum modifier)
        {
            if (MatrixRowEnterEvents.ContainsKey(matrix) && !Frozen)
            {
                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                bool rowChanged = LastRows.ContainsKey(matrix) ? row != LastRows[matrix] : true;

                if (LastRows.ContainsKey(matrix))
                {
                    LastBeforeRows[matrix] = LastRows[matrix];
                    LastRows[matrix] = row;
                    LastCols[matrix] = column;
                }
                else
                {
                    LastBeforeRows.Add(matrix, 1);
                    LastRows.Add(matrix, row);
                    LastCols.Add(matrix, column);
                }

                LastModifier = modifier;

                int selectedRow = 0;

                try
                {
                    selectedRow = matrixItem.GetNextSelectedRow();
                }
                catch { }

                MatrixRowEnterEvents[matrix](row, column, rowChanged, selectedRow == row);
            }
                        
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColumnValidateEvents.ContainsKey(key) && !Frozen)
            {
                if (SAPForm.Items.Item(matrix).Type == BoFormItemTypes.it_MATRIX)
                {
                    Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                    EditText editText = (EditText)matrixItem.Columns.Item(column).Cells.Item(row).Specific;

                    LastColumnValue = editText.String;
                }
                else if (SAPForm.Items.Item(matrix).Type == BoFormItemTypes.it_GRID)
                {
                    Grid gridItem = (Grid)SAPForm.Items.Item(matrix).Specific;

                    LastColumnValue = gridItem.DataTable.GetValue(column, row).ToString();
                }
            }

        }

        public void MatrixRowDoubleClick(string matrix, int row, string column)
        {
            if (MatrixRowDoubleClickEvents.ContainsKey(matrix) && !Frozen)
            {
                bool rowChanged = LastRows.ContainsKey(matrix) ? row != LastRows[matrix] : true;

                if (LastRows.ContainsKey(matrix))
                {
                    LastBeforeRows[matrix] = LastRows[matrix];
                    LastRows[matrix] = row;
                    LastCols[matrix] = column;
                }
                else
                {
                    LastBeforeRows.Add(matrix, 1);
                    LastRows.Add(matrix, row);
                    LastCols.Add(matrix, column);
                }

                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                MatrixRowDoubleClickEvents[matrix](row, column, rowChanged);
            }
        }

        public void MatrixTabPressed(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (MatrixCanAddEvents.ContainsKey(key) && !Frozen)
            {
                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                if (row == matrixItem.RowCount)
                {
                    if (MatrixCanAddEvents[key](row))
                    {                                                    
                        matrixItem.AddRow();
                        matrixItem.ClearRowData(matrixItem.RowCount);

                        try
                        {
                            if (matrixItem.Columns.Item(0).Description == "Pos" || matrixItem.Columns.Item(0).DataBind.Alias == "Pos")
                                ((EditText)matrixItem.Columns.Item(0).Cells.Item(matrixItem.RowCount).Specific).String = matrixItem.RowCount.ToString();
                        }
                        catch { }

                        if (matrixItem.Columns.Item(column).Width >= matrixItem.Item.Width - 60)
                            matrixItem.SetCellFocus(row + 1, matrixItem.GetCellFocus().ColumnIndex);
                        else 
                        {
                            for (int col = matrixItem.Columns.Count - 1; col >= 0; col--)
                            {
                                if (matrixItem.Columns.Item(col).UniqueID == column)
                                {
                                    matrixItem.SetCellFocus(row + 1, matrixItem.GetCellFocus().ColumnIndex);
                                    break;
                                }
                                else if (matrixItem.Columns.Item(col).Visible)
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void RightMenuClicked(string menu)
        {
            if (RightClickMenuEventEvents.ContainsKey(menu))
            {
                RightClickMenuEventEvents[menu]();
            }
        }

        public void MenuRightClick(string menu)
        {
            if (menu == "1283" && !SAPForm.UniqueID.Contains("F_"))
            {
                string msg;

                if (ValidateFormData(out msg, true))
                {
                    if (B1Base.AddOn.Instance.ConnectionController.Application.MessageBox("Deseja realmente remover esse registro?", 2, "Sim", "Não") == 1)
                    {
                        BeforeDeleteFormData();

                        DeleteFormData();

                        SAPForm.Mode = BoFormMode.fm_OK_MODE;

                        AfterDeleteFormData();

                        SAPForm.EnableMenu("1282", true);

                        AddOn.Instance.MainController.OpenMenuInsert();

                        B1Base.AddOn.Instance.ConnectionController.Application.StatusBar.SetText("Operação completada com êxito.", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Success);
                    }
                }
                else
                {
                    AddOn.Instance.ConnectionController.Application.StatusBar.SetText(msg, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);
                }
            }

            if (LastRightClickMatrix != null && MatrixRowRemoveEvents.ContainsKey(LastRightClickMatrix) && menu.StartsWith("MNUREM"))
            {
                MatrixRowRemoveEvents[LastRightClickMatrix](LastRightClickRow);
            }

            if (LastRightClickMatrix != null && MatrixColPasteForAllEvents.ContainsKey(LastRightClickMatrix) && menu.StartsWith("MNUPFA"))
            {
                System.Windows.Forms.IDataObject idat = null;
                Exception threadEx = null;
                String text = "";
                System.Threading.Thread staThread = new System.Threading.Thread(
                    delegate()
                    {
                        try
                        {
                            idat = System.Windows.Forms.Clipboard.GetDataObject();
                            text = idat.GetData(System.Windows.Forms.DataFormats.Text).ToString();
                        }

                        catch (Exception ex)
                        {
                            threadEx = ex;
                        }
                    });
                staThread.SetApartmentState(System.Threading.ApartmentState.STA);
                staThread.Start();
                staThread.Join();

                Matrix matrix = (Matrix)SAPForm.Items.Item(LastRightClickMatrix).Specific;
                int row = matrix.GetNextSelectedRow();

                while (row > 0 && row <= matrix.RowCount)
                {
                    //SetValue(matrix.Item.UniqueID, text, LastRightClickCol, row);

                    ((EditText)matrix.Columns.Item(LastRightClickCol).Cells.Item(row).Specific).String = text;

                    row = matrix.GetNextSelectedRow(row);
                }

                MatrixColPasteForAllEvents[LastRightClickMatrix](LastRightClickCol);
            }

            foreach (KeyValuePair<string, Tuple<string, MatrixCustomMenuEventHandler>> matrixCustomEvent in MatrixCustomMenuEvents)
            {
                if (menu.Contains(matrixCustomEvent.Key))
                {
                    matrixCustomEvent.Value.Item2(LastRightClickRow, LastRightClickCol);
                }
            }

            foreach (KeyValuePair<string, Tuple<string, GridCustomMenuEventHandler>> gridCustomEvent in GridCustomMenuEvents)
            {
                if (menu.Contains(gridCustomEvent.Key))
                {
                    gridCustomEvent.Value.Item2(LastRightClickRow, LastRightClickCol);
                }
            }

            foreach (KeyValuePair<string, Tuple<string, CustomMenuEventHandler>> customEvent in CustomMenuEvents)
            {
                string menuID = string.Format("{0}{1}{2}", customEvent.Key, SAPForm.TypeEx, SAPForm.UniqueID);

                //if (menu.Contains(customEvent.Key))
                if (menu.Equals(menuID))
                {
                    customEvent.Value.Item2();
                }
            }
        }        

        public void MatrixSort(string matrix, string column)
        {
            if (MatrixSortEvents.ContainsKey(matrix))
            {
                if (((Matrix)SAPForm.Items.Item(matrix).Specific).Columns.Item(column).Type != BoFormItemTypes.it_CHECK_BOX)
                {
                    Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;
                    for (int colPos = 1; colPos < matrixItem.Columns.Count; colPos++)
                    {
                        if (matrixItem.Columns.Item(colPos).UniqueID == column)
                        {
                            LastSortedColPos = colPos;
                            break;
                        }
                    }
                }

                MatrixSortEvents[matrix](column);
            }
        }

        public void RightClick(string item, int row, string col)
        {
            foreach (KeyValuePair<string, MatrixColPasteForAllEventHandler> menuEvent in MatrixColPasteForAllEvents)
            {
                string menuID = string.Format("MNUPFA{0}{1}", SAPForm.TypeEx, SAPForm.UniqueID);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);
            }

            foreach (KeyValuePair<string, MatrixRowRemoveEventHandler> menuEvent in MatrixRowRemoveEvents)
            {
                string menuID = string.Format("MNUREM{0}{1}", SAPForm.TypeEx, SAPForm.UniqueID);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);
            }

            int pos = 1;

            if (MatrixColPasteForAllEvents.ContainsKey(item))
            {
                string menuID = string.Format("MNUPFA{0}{1}", SAPForm.TypeEx, SAPForm.UniqueID);

                SAPbouiCOM.Matrix matrix = (Matrix)SAPForm.Items.Item(item).Specific;

                string colTitle = matrix.Columns.Item(col).Title;
                string firstCol = matrix.Columns.Item(0).UniqueID;

                if (row > 0 && row < matrix.RowCount && (col != firstCol || colTitle != "#" || colTitle != ""))
                {
                    LastRightClickMatrix = item;
                    LastRightClickRow = row;
                    LastRightClickCol = col;

                    MenuItem menuItem = null;
                    Menus menu = null;
                    MenuCreationParams creationPackage = null;

                    creationPackage = ((MenuCreationParams)(Controller.ConnectionController.Instance.Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams)));

                    creationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                    creationPackage.UniqueID = menuID;
                    creationPackage.String = "Colar (linhas selecionadas)";
                    creationPackage.Enabled = true;
                    creationPackage.Position = pos;

                    pos++;

                    menuItem = Controller.ConnectionController.Instance.Application.Menus.Item("1280");
                    menu = menuItem.SubMenus;
                    menu.AddEx(creationPackage);
                }
            }

            if (MatrixRowRemoveEvents.ContainsKey(item))
            {
                string menuID = string.Format("MNUREM{0}{1}", SAPForm.TypeEx, SAPForm.UniqueID);

                SAPbouiCOM.Matrix matrix = (Matrix)SAPForm.Items.Item(item).Specific;

                string colTitle = matrix.Columns.Item(col).Title;
                string firstCol = matrix.Columns.Item(0).UniqueID;

                if (row > 0 && row <= matrix.RowCount && (col != firstCol || colTitle != "#" || colTitle != ""))
                {
                    LastRightClickMatrix = item;
                    LastRightClickRow = row;
                    LastRightClickCol = col;

                    MenuItem menuItem = null;
                    Menus menu = null;
                    MenuCreationParams creationPackage = null;

                    creationPackage = ((MenuCreationParams)(Controller.ConnectionController.Instance.Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams)));

                    creationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                    creationPackage.UniqueID = menuID;
                    creationPackage.String = "Eliminar linha";
                    creationPackage.Enabled = true;
                    creationPackage.Position = pos;

                    pos++;

                    menuItem = Controller.ConnectionController.Instance.Application.Menus.Item("1280");
                    menu = menuItem.SubMenus;
                    menu.AddEx(creationPackage);
                }
            }

            foreach (KeyValuePair<string, Tuple<string, MatrixCustomMenuEventHandler>> matrixCustomEvent in MatrixCustomMenuEvents)
            {
                string menuID = string.Format("{0}{1}", matrixCustomEvent.Key, SAPForm.TypeEx);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);

                if (item != "" && matrixCustomEvent.Key.StartsWith(item))
                {                   
                    SAPbouiCOM.Matrix matrix = (Matrix)SAPForm.Items.Item(item).Specific;

                    string colTitle = matrix.Columns.Item(col).Title;
                    string firstCol = matrix.Columns.Item(0).UniqueID;

                    if (row > 0 && row <= matrix.RowCount && (col != firstCol || colTitle != "#" || colTitle != ""))
                    {
                        LastRightClickMatrix = item;
                        LastRightClickRow = row;
                        LastRightClickCol = col;

                        MenuItem menuItem = null;
                        Menus menu = null;
                        MenuCreationParams creationPackage = null;

                        creationPackage = ((MenuCreationParams)(Controller.ConnectionController.Instance.Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams)));

                        creationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                        creationPackage.UniqueID = menuID;
                        creationPackage.String = matrixCustomEvent.Value.Item1;
                        creationPackage.Enabled = true;
                        creationPackage.Position = pos;

                        pos++;

                        menuItem = Controller.ConnectionController.Instance.Application.Menus.Item("1280");
                        menu = menuItem.SubMenus;
                        menu.AddEx(creationPackage);
                    }
                }
            }

            foreach (KeyValuePair<string, Tuple<string, GridCustomMenuEventHandler>> gridCustomEvent in GridCustomMenuEvents)
            {
                string menuID = string.Format("{0}{1}{2}", gridCustomEvent.Key, SAPForm.TypeEx, SAPForm.UniqueID);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);

                if (item != "" && gridCustomEvent.Key.StartsWith(item))
                {
                    SAPbouiCOM.Grid grid = (Grid)SAPForm.Items.Item(item).Specific;

                    string colTitle = grid.Columns.Item(col).TitleObject.Caption;
                    string firstCol = grid.Columns.Item(0).UniqueID;

                    if (row >= 0 && row < grid.Rows.Count && (col != firstCol || colTitle != "#" || colTitle != ""))
                    {
                        LastRightClickMatrix = item;
                        LastRightClickRow = row;
                        LastRightClickCol = col;

                        MenuItem menuItem = null;
                        Menus menu = null;
                        MenuCreationParams creationPackage = null;

                        creationPackage = ((MenuCreationParams)(Controller.ConnectionController.Instance.Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams)));

                        creationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                        creationPackage.UniqueID = menuID;
                        creationPackage.String = gridCustomEvent.Value.Item1;
                        creationPackage.Enabled = true;
                        creationPackage.Position = pos;

                        pos++;

                        menuItem = Controller.ConnectionController.Instance.Application.Menus.Item("1280");
                        menu = menuItem.SubMenus;
                        menu.AddEx(creationPackage);
                    }
                }
            }


            if (Controller.ConnectionController.Instance.Application.Menus.Exists("HZLINEMTX"))
                Controller.ConnectionController.Instance.Application.Menus.RemoveEx("HZLINEMTX");


            if (pos > 1)
            {
                MenuItem menuItem = null;
                Menus menu = null;
                MenuCreationParams creationPackage = null;

                creationPackage = ((MenuCreationParams)(Controller.ConnectionController.Instance.Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams)));

                creationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                creationPackage.UniqueID = "HZLINEMTX";
                creationPackage.String = "_________________";
                creationPackage.Enabled = true;
                creationPackage.Position = pos;

                pos++;

                menuItem = Controller.ConnectionController.Instance.Application.Menus.Item("1280");
                menu = menuItem.SubMenus;
                menu.AddEx(creationPackage);
            }

            foreach (KeyValuePair<string, Tuple<string, CustomMenuEventHandler>> customEvent in CustomMenuEvents)
            {
                string menuID = string.Format("{0}{1}{2}", customEvent.Key, SAPForm.TypeEx, SAPForm.UniqueID);

                if (Controller.ConnectionController.Instance.Application.Menus.Exists(menuID))
                    Controller.ConnectionController.Instance.Application.Menus.RemoveEx(menuID);

                MenuItem menuItem = null;
                Menus menu = null;
                MenuCreationParams creationPackage = null;

                creationPackage = ((MenuCreationParams)(Controller.ConnectionController.Instance.Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams)));

                creationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                creationPackage.UniqueID = menuID;
                creationPackage.String = customEvent.Value.Item1;
                creationPackage.Enabled = true;
                creationPackage.Position = pos;

                pos++;
                
                menuItem = Controller.ConnectionController.Instance.Application.Menus.Item("1280");
                menu = menuItem.SubMenus;
                menu.AddEx(creationPackage);
            }

            ContextMenuShow();
        }

        public void Checked(string check)
        {
            if (CheckEvents.ContainsKey(check) && !Frozen)
            {
                CheckEvents[check]();
            }
        }

        public void ColumnChecked(string matrix, int row, string column, SAPbouiCOM.BoModifiersEnum modifiers = SAPbouiCOM.BoModifiersEnum.mt_None)
        {            
            string key = string.Format("{0}.{1}", matrix, column);

            if (row == 0 && SAPForm.Items.Item(matrix).Type == BoFormItemTypes.it_MATRIX)
            {
                if (MatrixCheckAllEvents.ContainsKey(key))
                {
                    if (MatrixCheckAllEvents[key]())
                    {
                        bool check = false;

                        Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                        if (matrixItem.RowCount > 0)
                            check = !((CheckBox)matrixItem.Columns.Item(column).Cells.Item(1).Specific).Checked;

                        for (int aux = 1; aux <= matrixItem.RowCount; aux++)
                        {                            
                            ((CheckBox)matrixItem.Columns.Item(column).Cells.Item(aux).Specific).Checked = check;

                            if (ColumnCheckEvents.ContainsKey(key))
                                ColumnCheckEvents[key](aux);
                        }
                    }
                }
            }            
            else if (ColumnCheckEvents.ContainsKey(key) && !Frozen)
            {
                if (SAPForm.Items.Item(matrix).Type == BoFormItemTypes.it_GRID)
                {
                    if (LastCols.ContainsKey(matrix))
                        LastCols[matrix] = column;
                    else
                        LastCols.Add(matrix, column);
                }

                ColumnCheckEvents[key](row);
            }
            else if (ColumnCheckProEvents.ContainsKey(key) && !Frozen)
            {
                List<int> rows = new List<int>();

                bool check = false;

                if (SAPForm.Items.Item(matrix).Type == BoFormItemTypes.it_GRID)
                {
                    //check = ((CheckBoxColumn)((Grid)SAPForm.Items.Item(matrix).Specific).Columns.Item(column)).IsChecked(row);

                    check = ((Grid)SAPForm.Items.Item(matrix).Specific).DataTable.GetValue(column, row).ToString() != "N";
                }
                else
                {
                    check = ((CheckBox)((Matrix)SAPForm.Items.Item(matrix).Specific).Columns.Item(column).Cells.Item(row).Specific).Checked;
                }

                if (LastRows.ContainsKey(matrix))
                {
                    LastBeforeRows[matrix] = LastRows[matrix];
                    LastRows[matrix] = row;
                    LastCols[matrix] = column;
                }
                else
                {
                    LastBeforeRows.Add(matrix, 1);
                    LastRows.Add(matrix, row);
                    LastCols.Add(matrix, column);
                }

                if (modifiers == BoModifiersEnum.mt_SHIFT)
                {
                    int lastRow = LastBeforeRows[matrix];

                    if (lastRow > row)
                    {
                        for (int curr = row; curr <= lastRow; curr++)
                        {
                            rows.Add(curr);
                        }
                    }
                    else
                    {
                        for (int curr = lastRow; curr <= row; curr++)
                        {
                            rows.Add(curr);
                        }
                    }
                }
                else
                {
                    rows.Add(row);
                }

                ColumnCheckProEvents[key](rows.ToArray(), check);
            }
        }

        public void EditValidate(string edit)
        {
            if (EditValidateEvents.ContainsKey(edit) && !Frozen)
            {
                EditText editText = ((EditText)SAPForm.Items.Item(edit).Specific);

                try
                {
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
                }
                catch { }
                
                EditValidateEvents[edit](LastEditValue != editText.String);
            }
        }

        public void ColumnValidate(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColumnValidateEvents.ContainsKey(key) && !Frozen)
            {
                if (SAPForm.Items.Item(matrix).Type == BoFormItemTypes.it_MATRIX)
                {
                    Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                    EditText editText = (EditText)matrixItem.Columns.Item(column).Cells.Item(row).Specific;

                    try
                    {
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
                    }
                    catch { }

                    bool changed = LastColumnValue != editText.String;

                    ColumnValidateEvents[key](row, changed);

                    if (MatrixCanAddEvents.ContainsKey(key) && row == matrixItem.RowCount)
                    {
                        if (MatrixCanAddEvents[key](row))
                        {
                            matrixItem.AddRow();
                            matrixItem.ClearRowData(matrixItem.RowCount);

                            try
                            {
                                if (matrixItem.Columns.Item(0).Description == "Pos" || matrixItem.Columns.Item(0).DataBind.Alias == "Pos")
                                    ((EditText)matrixItem.Columns.Item(0).Cells.Item(matrixItem.RowCount).Specific).String = matrixItem.RowCount.ToString();
                            }
                            catch { }
                        }
                    }
                }
                else if (SAPForm.Items.Item(matrix).Type == BoFormItemTypes.it_GRID)
                {
                    Grid gridItem = (Grid)SAPForm.Items.Item(matrix).Specific;

                    string value = gridItem.DataTable.GetValue(column, row).ToString();

                    bool changed = LastColumnValue != value;

                    if (LastCols.ContainsKey(matrix))
                        LastCols[matrix] = column;
                    else
                        LastCols.Add(matrix, column);

                    ColumnValidateEvents[key](row, changed);
                }
            }            
        }

        public void EditFocus(string edit)
        {
            if (EditValidateEvents.ContainsKey(edit) && !Frozen)
            {
                LastEditValue = ((EditText)SAPForm.Items.Item(edit).Specific).String;
            }

            if (EditFocusEvents.ContainsKey(edit) && !Frozen)
            {
                EditFocusEvents[edit]();
            }
        }

        public void ComboSelect(string combo)
        {
            if (ComboSelectEvents.ContainsKey(combo) && !Frozen)
            {
                ComboSelectEvents[combo](LastComboValue !=
                    (((ComboBox)SAPForm.Items.Item(combo).Specific).Selected == null ? string.Empty :
                    ((ComboBox)SAPForm.Items.Item(combo).Specific).Selected.Value));
            }
        }

        public void ColumnSelect(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);

            if (ColumnSelectEvents.ContainsKey(key) && !Frozen)
            {
                Matrix matrixItem = (Matrix)SAPForm.Items.Item(matrix).Specific;

                bool rowChanged = LastRows.ContainsKey(matrix) ? row != LastRows[matrix] : true;

                if (LastRows.ContainsKey(matrix))
                {
                    LastBeforeRows[matrix] = LastRows[matrix];
                    LastRows[matrix] = row;
                    LastCols[matrix] = column;
                }
                else
                {
                    LastBeforeRows.Add(matrix, 1);
                    LastRows.Add(matrix, row);
                    LastCols.Add(matrix, column);
                }

                ColumnSelectEvents[key](row, LastComboValue !=
                    (((ComboBox)matrixItem.Columns.Item(column).Cells.Item(row).Specific).Selected == null ? string.Empty :
                    ((ComboBox)matrixItem.Columns.Item(column).Cells.Item(row).Specific).Selected.Value));

                if (MatrixCanAddEvents.ContainsKey(key) && row == matrixItem.RowCount)
                {
                    if (MatrixCanAddEvents[key](row))
                    {
                        matrixItem.AddRow();
                        matrixItem.ClearRowData(matrixItem.RowCount);

                        try
                        {
                            if (matrixItem.Columns.Item(0).Description == "Pos" || matrixItem.Columns.Item(0).DataBind.Alias == "Pos")
                                ((EditText)matrixItem.Columns.Item(0).Cells.Item(matrixItem.RowCount).Specific).String = matrixItem.RowCount.ToString();
                        }
                        catch { }
                    }
                }
            }
        }

        public void ComboFocus(string combo)
        {
            if (ComboSelectEvents.ContainsKey(combo) && !Frozen)
            {
                LastComboValue = (((ComboBox)SAPForm.Items.Item(combo).Specific).Selected == null ? string.Empty :
                    ((ComboBox)SAPForm.Items.Item(combo).Specific).Selected.Value);
            }
        }

        public void ColumnFocus(string matrix, int row, string column)
        {
            string key = string.Format("{0}.{1}", matrix, column);
            if (ColumnValidateEvents.ContainsKey(key) && !Frozen)
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

        public void OptionSelect(string option)
        {
            if (OptionEvents.ContainsKey(option) && !Frozen)
            {
                OptionEvents[option]();
            }
        }

        public void LinkPress(string link, View.BaseView linkedView)
        {
            if (LinkEvents.ContainsKey(link) && !Frozen)
            {
                LinkEvents[link](linkedView);
            }
        }

        public void FormValidate()
        {
            if (m_copyFlag)
            {
                m_copyFlag = false;
                Model.EnumObjType objType = Model.EnumObjType.None;
                int docEntry = 0;

                switch (FormType)
                {
                    case "133":
                        docEntry = GetValue("INV1.BaseEntry", true);
                        objType = (Model.EnumObjType)GetValue("INV1.BaseType", true);

                        if (docEntry == 0)
                        {
                            Matrix matrixItem = (Matrix)SAPForm.Items.Item("38").Specific;

                            objType = Model.EnumObjType.SalesOrder;
                            docEntry = Convert.ToInt32(((EditText)matrixItem.Columns.Item("45").Cells.Item(1).Specific).String);
                        }

                        if (docEntry > 0)
                        {
                            if (DocCopyEvents.ContainsKey(objType))
                            {
                                LastCopiedDocEntry = docEntry;
                                LastCopiedObjType = objType;

                                DocCopyEvents[objType](docEntry);
                            }
                        }
                        break;
                    case "179":
                        docEntry = GetValue("RIN1.BaseEntry", true);
                        objType = (Model.EnumObjType)GetValue("RIN1.BaseType", true);

                        if (docEntry > 0)
                        {
                            if (DocCopyEvents.ContainsKey(objType))
                            {
                                LastCopiedDocEntry = docEntry;
                                LastCopiedObjType = objType;

                                DocCopyEvents[objType](docEntry);
                            }
                        }
                        break;
                    case "141":
                        docEntry = GetValue("PCH1.BaseEntry", true);
                        objType = (Model.EnumObjType)GetValue("PCH1.BaseType", true);

                        if (docEntry > 0)
                        {
                            if (DocCopyEvents.ContainsKey(objType))
                            {
                                LastCopiedDocEntry = docEntry;
                                LastCopiedObjType = objType;

                                DocCopyEvents[objType](docEntry);
                            }
                        }
                        break;
                    case "143":
                        docEntry = GetValue("PDN1.BaseEntry", true);
                        objType = (Model.EnumObjType)GetValue("PDN1.BaseType", true);

                        if (docEntry == 0)
                        {
                            Matrix matrixItem = (Matrix)SAPForm.Items.Item("38").Specific;

                            objType = Model.EnumObjType.PurchaseOrder;
                            docEntry = Convert.ToInt32(((EditText)matrixItem.Columns.Item("45").Cells.Item(1).Specific).String);
                        }

                        if (docEntry > 0)
                        {
                            if (DocCopyEvents.ContainsKey(objType))
                            {
                                LastCopiedDocEntry = docEntry;
                                LastCopiedObjType = objType;

                                DocCopyEvents[objType](docEntry);
                            }
                        }
                        break;
                }
            }
        }

        public void ButtonOpenView(string button, BaseView view)
        {
            if (ButtonOpenViewEvents.ContainsKey(button))
            {
                view.ParentView = this;

                ButtonOpenViewEvents[button](view);
            }
        }

        public bool ConfirmationBoxLoad(string message)
        {
            bool result = false;

            if (ConfirmMessageBoxEvents.ContainsKey(message))
                result = ConfirmMessageBoxEvents[message]();

            return result;
        }

        public void MenuPaste()
        {
            string item = SAPForm.ActiveItem;

            if (MenuPasteEvents.ContainsKey(item))
            {
                MenuPasteEvents[item]();
            }
        }
    }    
}