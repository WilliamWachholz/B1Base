using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Reflection;
using System.IO;
using SAPbouiCOM;

namespace B1Base.Controller
{
    public abstract class MainController
    {
        List<View.BaseView> m_Views = new List<View.BaseView>();

        public abstract string AddOnID { get; }

        public abstract string AddOnName { get; }
        
        protected abstract void ExitApp();
        
        public virtual void CreateMetadata() { }        

        protected virtual void CreateMenus() { }

        private Dictionary<string, string> FormTypeViews { get; set; }

        public View.BaseView LastParent { get; set; }

        public View.BaseView LastView
        {
            get
            {
                if (m_Views.Count > 1)
                    return m_Views[m_Views.Count - 2];
                else
                    return null;
            }
        }

        public View.BaseView LastActiveView { get; protected set; }

        public List<View.BaseView> GetViews(string formType)
        {
            List<View.BaseView> result = new List<View.BaseView>();

            foreach (View.BaseView view in m_Views)
            {
                if (view.FormType == formType)
                {
                    result.Add(view);
                }
            }

            return result;
        }

        private List<string> Menus { get; set; }

        public bool LogIsActive { get; set; }        

        private string LastStatusBarMsg { get; set; }

        private bool SuppressChoose { get; set; }

        private bool SupressPicker { get; set; }

        private bool SupressDetails { get; set; }

        protected virtual bool LockUserFieldsWidth { get { return false; } }

        Timer m_timerFinalize = new Timer(60000);

        string m_NextMessage = string.Empty;

        protected const string MENU_SAP = "43520";
        protected const string MENU_CONFIG_SAP = "43525";
        
        protected string MENU_ADDON { get { return AddOnID;  } }
        protected string MENU_ADDON_CONFIG { get { return AddOnID + "43525"; } }
        protected string MENU_CONFIG { get { return AddOnID + "Cnf";  } }

        protected delegate void OpenMenuEventHandler();

        protected virtual Dictionary<string, OpenMenuEventHandler> OpenMenuEvents()
        {
            Dictionary<string, OpenMenuEventHandler> result = new Dictionary<string, OpenMenuEventHandler>();
            result.Add(MENU_CONFIG, MenuConfigOpen);

            return result;
        }        

        protected void CreateMenu(string menuFather, string menuID, string menuName, string imageFile, bool popup, int position = 99)
        {
            if (menuFather == MENU_ADDON)
                CreateMenu(MENU_SAP, MENU_ADDON, AddOnName, "", true);

            SAPbouiCOM.Menus oMenus = null;
            SAPbouiCOM.MenuItem oMenuItem = null;
            SAPbouiCOM.MenuCreationParams oCreationPackage = null;

            oMenuItem = ConnectionController.Instance.Application.Menus.Item(menuFather);

            if (ConnectionController.Instance.ExecuteSqlForObject<string>("GetMenuIsEnabled", menuFather, ConnectionController.Instance.User.ToString()) == "Y")
            {
                oMenus = oMenuItem.SubMenus;

                if (!oMenus.Exists(menuID))
                {
                    oCreationPackage = (MenuCreationParams) ConnectionController.Instance.Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams);

                    oCreationPackage.Type = popup ? SAPbouiCOM.BoMenuType.mt_POPUP : SAPbouiCOM.BoMenuType.mt_STRING;
                    oCreationPackage.UniqueID = menuID;
                    oCreationPackage.String = menuName;
                    if (imageFile != string.Empty)
                        oCreationPackage.Image = AddOn.Instance.CurrentDirectory + @"\img\" + imageFile;
                    oCreationPackage.Position = position;
                    oCreationPackage.Enabled = true;                             
                   
                    try
                    {
                        oMenuItem = oMenus.AddEx(oCreationPackage);

                        Menus.Add(menuID);
                    }
                    catch (Exception e)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("Erro ao criar menu: " + e.Message);
                    }
                }
            }
        }        

        public void Initialize() 
        {
            try
            {
                Controller.ConnectionController.Instance.Initialize(AddOnID);

                FormTypeViews = new Dictionary<string, string>();

                Menus = new List<string>();
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Falha ao conectar addOn {0}. {1}", AddOnName, e.Message.ToString()));
            }

            Controller.ConnectionController.Instance.Application.AppEvent += HandleAppEvent;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormLoad;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleGotFocus;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleLostFocus;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormDeactivate;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandlePickerClick;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleButtonClick;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFolderSelect;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleChooseFrom;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleButtonPress;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormValidate;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormResize;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleGridRowClick;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleMatrixRowClick;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleMatrixSort;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleKeyDown;
            Controller.ConnectionController.Instance.Application.FormDataEvent += HandleFormData;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormClose;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuInsert;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuSaveAsDraft;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuSearch;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuDuplicate;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuCancel;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuAny;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuRightClick;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuPaste;
            Controller.ConnectionController.Instance.Application.StatusBarEvent += HandleStatusBarMessage;
            Controller.ConnectionController.Instance.Application.RightClickEvent += HandleRightClick;

            try
            {
                CreateMenu(MENU_CONFIG_SAP, MENU_ADDON_CONFIG, AddOnName, "", true);
                CreateMenu(MENU_ADDON_CONFIG, MENU_CONFIG, "Configuração", "", false);

                CreateMenus();
            }
            catch(Exception e)
            {
                ConnectionController.Instance.Application.StatusBar.SetText(string.Format("Falha ao criar menus: {0}", e.Message), BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
            }

            try
            {
                Dictionary<string, string> yesNoValidValues = new Dictionary<string, string>();
                yesNoValidValues.Add("Y", "Sim");
                yesNoValidValues.Add("N", "Não");

                ConnectionController.Instance.CreateMetadata("OINV", "DIUpdate", FieldTypeEnum.Alphanumeric, 1, yesNoValidValues);

                new ConfigController<Model.ConfigModel>("").CreateMetadata("Code", FieldTypeEnum.Integer);
                new ConfigController<Model.ConfigModel>("").CreateMetadata("AutoCreateMetadata", FieldTypeEnum.Alphanumeric, 1);
                new ConfigController<Model.ConfigModel>("").CreateMetadata("ActivateLog", FieldTypeEnum.Alphanumeric, 1);

                ConnectionController.Instance.CreateMetadata(new DAO.ConfigSeqDAO().TableName, "Code", FieldTypeEnum.Integer);
                ConnectionController.Instance.CreateMetadata(new DAO.ConfigSeqDAO().TableName, "UserTable", FieldTypeEnum.Alphanumeric, 40);
                ConnectionController.Instance.CreateMetadata(new DAO.ConfigSeqDAO().TableName, "NextCode", FieldTypeEnum.Integer);

                Model.ConfigModel configModel = new Model.ConfigModel();
                configModel = new ConfigController<Model.ConfigModel>("").GetConfig();

                LogIsActive = configModel.ActivateLog;

                if (configModel.AutoCreateMetadata)
                    CreateMetadata();
            }
            catch (Exception e)
            {
                ConnectionController.Instance.Application.StatusBar.SetText(string.Format("Falha ao criar campos de usuário: {0}", e.Message), BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
            }

            ConnectionController.Instance.Application.StatusBar.SetText(string.Format("AddOn {0} conectado.", AddOnName), SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Success); 
        }

        private void Finalize(object sender, ElapsedEventArgs e)
        {
            m_timerFinalize.Enabled = false;
            ExitApp();
        }

        public virtual void MenuConfigOpen()
        {
            OpenView(true, "B1Base.View.ConfigView");
        }

        public View.BaseView OpenView(string formType)
        {
            return OpenView(false, formType, null);
        }

        public View.BaseView OpenView(bool unique, string formType)
        {
            return OpenView(unique, formType, null);
        }

        public View.BaseView OpenView(string formType, View.BaseView parentView, bool wait = false)
        {
            return OpenView(false, formType, parentView, 0, 0, wait);
        }

        public View.BaseView OpenView(string formType, View.BaseView parentView, int top, int left, bool wait = false)
        {
            return OpenView(false, formType, parentView, top, left, wait);
        }

        public View.BaseView OpenView(bool unique, string formType, View.BaseView parentView, int top = 0, int left = 0, bool wait = false)
        {
            try
            {
                bool notExists = false;

                string newFormType = formType;

                if (formType.Split('.').Count() > 2)
                    newFormType = formType.Split('.')[2].Length > 40 ? formType.Split('.')[2].Substring(0, 40) : formType.Split('.')[2];

                try
                {
                    Controller.ConnectionController.Instance.Application.Forms.GetForm(newFormType, 1);
                }
                catch
                {
                    notExists = true;
                }

                string formUID = "RW1";

                LastParent = parentView;

                if ((unique == false) || (unique && notExists))
                {
                    int count = 0;

                    bool next = true;

                    while (next)
                    {
                        count++;

                        try
                        {
                            Controller.ConnectionController.Instance.Application.Forms.GetForm(newFormType, count);
                        }
                        catch
                        {
                            next = false;
                        }
                    }
                    
                    formUID = string.Format("RW{0}-{1}", count, new Random().Next(999));

                    string srfPath = AddOn.Instance.CurrentDirectory + "\\SRF\\" + formType.Split('.')[2] + ".srf";

                    if (File.Exists(srfPath) == false)
                    {
                        Controller.ConnectionController.Instance.Application.StatusBar.SetText("Arquivo SRF não encontrado. Verifique a instalação do addOn.", BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
                        return null;
                    }

                    string xml = File.ReadAllText(srfPath);

                    xml = xml.Replace("uid=\"RW0\"", string.Format("uid=\"{0}\"", formUID));
                    xml = xml.Replace(string.Format("appformnumber=\"{0}\"", formType), string.Format("appformnumber=\"{0}\"", newFormType));
                    xml = xml.Replace(string.Format("FormType=\"{0}\"", formType), string.Format("FormType=\"{0}\"", newFormType));

                    if (top > 0)
                        xml = xml.Replace("top=\"99\"", string.Format("top=\"{0}\"", top));

                    if (left > 0)
                        xml = xml.Replace("left=\"999\"", string.Format("left=\"{0}\"", left));

                    if (Controller.ConnectionController.Instance.DBServerType == "SQLSERVER")
                        xml = xml.Replace("from dummy", "");

                    if (!FormTypeViews.ContainsKey(newFormType + AddOnID))
                        FormTypeViews.Add(newFormType + AddOnID, formType);

                    Controller.ConnectionController.Instance.Application.LoadBatchActions(ref xml);
                }

                if (wait)
                    System.Threading.Thread.Sleep(1000);
                
                return m_Views.First(r => r.FormUID == formUID && r.FormType == newFormType);
            }
            catch(Exception ex)
            {
                if (LogIsActive)
                {
                    ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + ex.Message);
                }
                return null;
            }
        }

        public void OpenMenu(string menu)
        {
            Controller.ConnectionController.Instance.Application.Menus.Item(menu).Activate();
        }

        public void OpenMenuInsert()
        {
            Controller.ConnectionController.Instance.Application.Menus.Item("1282").Activate();
        }

        public void OpenMenuSearch()
        {
            Controller.ConnectionController.Instance.Application.Menus.Item("1281").Activate();
        }

        public void OpenMenuRefresh()
        {
            Controller.ConnectionController.Instance.Application.Menus.Item("1304").Activate();
        }

		public void OpenMenuLastRecord()
		{
			Controller.ConnectionController.Instance.Application.Menus.Item("1291").Activate();
		}

        public void ConfirmNextMessage(string message)
        {
            m_NextMessage = message;
        }

        private void HandleFormLoad(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            string formType = pVal.FormTypeEx;

            if (pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                bubbleEvent = true;

                try
                {

                    if (SupressDetails && pVal.FormType == 1)
                    {
                        ConnectionController.Instance.Application.Forms.Item(pVal.FormUID).Close();
                        SupressDetails = false;
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 114 - " + e.Message);
                    }
                }
            }

            if ((pVal.EventType == BoEventTypes.et_FORM_LOAD || (pVal.EventType == BoEventTypes.et_FORM_ACTIVATE && pVal.FormType == 198) ) && !pVal.BeforeAction)
            {
                bubbleEvent = true;

                try
                {
                    if (SuppressChoose && pVal.FormType == 10003)
                    {
                        ConnectionController.Instance.Application.Forms.Item(pVal.FormUID).Close();
                        SuppressChoose = false;
                    }
                    else if (SupressPicker)
                    {
                        ConnectionController.Instance.Application.Forms.Item(pVal.FormUID).Close();
                        SupressPicker = false;
                    }
                    else if (pVal.FormUID.StartsWith("RW"))
                    {
                        if (FormTypeViews.ContainsKey(pVal.FormTypeEx + AddOnID))
                        {
                            Assembly assembly = Assembly.LoadFile(AddOn.Instance.CurrentDirectory + "\\" + FormTypeViews[pVal.FormTypeEx + AddOnID].Split('.')[0] + ".dll");

                            Type type = assembly.GetType(FormTypeViews[pVal.FormTypeEx + AddOnID]);

                            if (type == null)
                                return;

                            if (m_Views.Where(r => r.FormUID == formUID).Count() == 0)
                            {
                                ConstructorInfo constructor = type.GetConstructor(new Type[] { formUID.GetType(), pVal.FormTypeEx.GetType() });
                                object formView = constructor.Invoke(new object[] { formUID, pVal.FormTypeEx });

                                m_Views.Add((View.BaseView)formView);
                            }
                        }
                    }
                    else if (pVal.FormType == 0)
                    {
                        try
                        {
                            Form form = ConnectionController.Instance.Application.Forms.Item(pVal.FormUID);

                            string text1 =
                               ((StaticText)form.Items.Item("7").Specific).Caption;

                            string text2 = string.Empty;

                            try
                            {
                                text2 = ((StaticText)form.Items.Item("1000001").Specific).Caption;
                            }
                            catch { }

                            foreach (View.BaseView view in m_Views)
                            {
                                if (view.ConfirmationBoxLoad(text1 + " " + text2))
                                {
                                    form.Items.Item("1").Click(BoCellClickType.ct_Regular);
                                }
                            }

                            if (m_NextMessage.Trim() == (text1 + " " + text2).Trim())
                            {
                                form.Items.Item("1").Click(BoCellClickType.ct_Regular);
                            }

                            m_NextMessage = string.Empty;
                        }
                        catch { }

                        if (m_GridOnFilter)
                        {
                            Form form = ConnectionController.Instance.Application.Forms.Item(pVal.FormUID);
                            form.Items.Item("2").Click(BoCellClickType.ct_Regular);
                        }
                    }
                    else if (pVal.FormType == 60268)
                    {
                        if (m_GridOnFilter)
                        {
                            m_GridOnFilter = false;
                            m_FilteredView.SAPForm.Items.Item(m_FilteredView.SAPForm.Settings.MatrixUID).Enabled = true;
                        }
                    }                    
                    else if (!pVal.FormTypeEx.Contains("-"))
                    {
                        string[] dlls = Directory.GetFiles(AddOn.Instance.CurrentDirectory, "*.dll");

                        foreach (string dll in dlls)
                        {
                            try
                            {
                                Assembly assembly = Assembly.LoadFile(dll);

                                if (assembly.GetName().Name.Contains("Code"))
                                {
                                    Type type = assembly.GetType(assembly.GetName().Name + ".View.Form" + pVal.FormTypeEx + "View");                                                                       

                                    if (type != null)
                                    {
                                        ConstructorInfo constructor = type.GetConstructor(new Type[] { formUID.GetType(), pVal.FormTypeEx.GetType() });
                                        object formView = constructor.Invoke(new object[] { formUID, pVal.FormTypeEx });                                        

                                        if (!((View.BaseView)formView).Inactive)
                                            m_Views.Add((View.BaseView)formView);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //ConnectionController.Instance.Application.StatusBar.SetText(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        if (LockUserFieldsWidth)
                        {
                            if (formType.StartsWith("-"))
                            {
                                int mainType = 0;

                                try
                                {
                                    mainType = Convert.ToInt32(formType.Replace("-", ""));

                                    if (ConnectionController.Instance.Application.Forms.GetFormByTypeAndCount(mainType, 0).UniqueID != string.Empty)
                                    {
                                        ConnectionController.Instance.Application.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount).Width = 350;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 114 - " + e.Message);
                    }
                }
            }            

            try
            {
                if (m_Views.Where(r => r.FormUID == formUID).Count() > 0)
                {
                    if (m_Views.First(r => r.FormUID == formUID).Invisible && (pVal.EventType == BoEventTypes.et_FORM_LOAD || pVal.EventType == BoEventTypes.et_FORM_ACTIVATE || pVal.EventType == BoEventTypes.et_FORM_DRAW || pVal.EventType == BoEventTypes.et_FORM_RESIZE))
                    {
                        ConnectionController.Instance.Application.Forms.Item(formUID).Visible = false;
                        ConnectionController.Instance.Application.Forms.Item(formUID).VisibleEx = false;
                    }
                }
            }
            catch { }
        }

        private void HandleLostFocus(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_LOST_FOCUS && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;
                    
                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        view.FormLostFocus();
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 259 - " + e.Message);
                    }
                }
            }
        }

        private void HandleGotFocus(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_GOT_FOCUS && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                    {
                        view.GotFocus();

                        if (pVal.ItemUID != string.Empty)
                        {
                            if (pVal.ColUID != string.Empty)
                            {
                                view.ColumnFocus(pVal.ItemUID, pVal.Row, pVal.ColUID);
                                view.MatrixRowEnter(pVal.ItemUID, pVal.Row, pVal.ColUID, pVal.Modifiers);
                            }
                            else
                            {
                                view.EditFocus(pVal.ItemUID);
                                view.ComboFocus(pVal.ItemUID);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 138 - " + e.Message);
                    }
                }
            }
        }

        private void HandleFormClose(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_FORM_CLOSE && pVal.BeforeAction == true)
            {
                string formType = pVal.FormTypeEx;

                foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                { 
                    if (bubbleEvent)
                        bubbleEvent = view.CanClose();
                }
            }

            if (pVal.EventType == BoEventTypes.et_FORM_UNLOAD && pVal.BeforeAction == true)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        view.Close();
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 163 - " + e.Message);
                    }
                }

                if (pVal.FormType == 0 && pVal.FormUID == "F_1")
                {
                    m_timerFinalize.Elapsed += Finalize;
                    m_timerFinalize.Enabled = true;
                }
            }
        }

        private void HandleFormDeactivate(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_FORM_DEACTIVATE && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    LastActiveView = m_Views.Last(r => r.FormUID == formUID && r.FormType == formType);

                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        view.LostFocus();

                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 259 - " + e.Message);
                    }
                }
            }
        }

        private void HandlePickerClick(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_PICKER_CLICKED && pVal.BeforeAction == true)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    if (pVal.ColUID == string.Empty)
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        {
                            SupressPicker = view.SupressPickerClick(pVal.ItemUID);

                            if (SupressPicker)
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 259 - " + e.Message);
                    }
                }
        }
        }

        private void HandleButtonClick(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_CLICK && pVal.BeforeAction == false)
            {                
                try
                {
                    string formId = pVal.FormUID;
                    string formType = pVal.FormTypeEx;

                    if (pVal.ItemUID == "1")
                    {
                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            {                                
                                view.ButtonOkClick();
                            }
                        }

                        if (formType == "50106")
                        {
                            LastActiveView.AfterSaveAsDraft();
                        }
                    }
                    else
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            view.ButtonClick(pVal.ItemUID);                        
                    }                   
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 187 - " + e.Message);
                    }
                }
            }            
        }

        private void HandleButtonPress(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_MATRIX_LINK_PRESSED && pVal.BeforeAction == true)
            {
                try
                {
                    string formType = pVal.FormTypeEx;
                    string formId = pVal.FormUID;

                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        view.MatrixRowEnter(pVal.ItemUID, pVal.Row, pVal.ColUID, pVal.Modifiers);
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 211 - " + e.Message);
                    }
                }
            }

            if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;
                    string formId = pVal.FormUID;

                    if (pVal.ItemUID == "1")
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            view.ButtonOkPress();                       
                    }
                    else
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        {
                            view.ButtonPress(pVal.ItemUID);

                            view.ButtonOpenView(pVal.ItemUID, m_Views[m_Views.Count - 1]);

                            view.LinkPress(pVal.ItemUID, m_Views[m_Views.Count - 1]);
                        }
                    }

                    if (formType == "60059" && pVal.ItemUID == "11")
                    {
                        if (((EditText)((Matrix)Controller.ConnectionController.Instance.Application.Forms.GetForm(formType, 1).Items.Item("3").Specific).Columns.Item("7").Cells.Item(pVal.Row)).String == AddOnID)
                        {
                            foreach (string menu in Menus)
                            {
                                try
                                {
                                    ConnectionController.Instance.Application.Menus.RemoveEx(menu);
                                }
                                catch { }
                            }
                            ExitApp();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 211 - " + e.Message);
                    }
                }
            }

            if (pVal.EventType == BoEventTypes.et_COMBO_SELECT && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;
                    string formId = pVal.FormUID;

                    if (pVal.ItemUID == "1")
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            view.ButtonOkPress();                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 211 - " + e.Message);
                    }
                }
            }
        }

        private void HandleFormValidate(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        if (pVal.ColUID != string.Empty)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                view.ColumnChecked(pVal.ItemUID, pVal.Row, pVal.ColUID, pVal.Modifiers);
                        }
                        else
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            {
                                view.Checked(pVal.ItemUID);

                                view.OptionSelect(pVal.ItemUID);
                            }
                        }

                        bubbleEvent = false;
                    }
                }
                catch (Exception e)
                {
                    //ConnectionController.Instance.Application.StatusBar.SetText("235 - " + e.Message);
                    //throw e;
                }
            }

            if (pVal.EventType == BoEventTypes.et_KEY_DOWN && pVal.CharPressed == 32 && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        if (pVal.ColUID != string.Empty)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                view.ColumnChecked(pVal.ItemUID, pVal.Row, pVal.ColUID, pVal.Modifiers);
                        }
                        else
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            {
                                view.Checked(pVal.ItemUID);

                                view.OptionSelect(pVal.ItemUID);
                            }
                        }

                        bubbleEvent = false;
                    }
                }
                catch (Exception e)
                {
                    //ConnectionController.Instance.Application.StatusBar.SetText("235 - " + e.Message);
                    //throw e;
                }
            }

            if (bubbleEvent)
            {
                if (pVal.EventType == BoEventTypes.et_VALIDATE && pVal.BeforeAction == false)
                {
                    try
                    {
                        string formType = pVal.FormTypeEx;

                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            if (pVal.ColUID != string.Empty)
                            {
                                foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                    view.ColumnValidate(pVal.ItemUID, pVal.Row, pVal.ColUID);
                            }
                            else if (pVal.ItemUID != string.Empty)
                            {
                                foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                    view.EditValidate(pVal.ItemUID);
                            }

                            bubbleEvent = false;
                        }
                    }
                    catch (Exception e)
                    {
                        if (LogIsActive)
                        {
                            ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 235 - " + e.Message);
                        }
                    }
                }
            }

            if (pVal.EventType == BoEventTypes.et_COMBO_SELECT && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        if (pVal.ColUID != string.Empty)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                view.ColumnSelect(pVal.ItemUID, pVal.Row, pVal.ColUID);
                        }
                        else
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                view.ComboSelect(pVal.ItemUID);
                        }

                        bubbleEvent = false;
                    }
                }
                catch (Exception e)
                {
                    //ConnectionController.Instance.Application.StatusBar.SetText("235 - " + e.Message);
                }
            }

            if (pVal.EventType == BoEventTypes.et_FORM_ACTIVATE && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                    {
                        view.FormValidate();

                        bubbleEvent = false;
                    }
                }
                catch (Exception e)
                {
                    //ConnectionController.Instance.Application.StatusBar.SetText("235 - " + e.Message);
                }
            }
        }

        private void HandleRightClick(ref ContextMenuInfo eventInfo, out bool bubbleEvent)
        {
            bubbleEvent = true;

            try
            {
                string formUID = eventInfo.FormUID;                

                foreach (View.BaseView baseView in m_Views.Where(r => r.FormUID == formUID))
                {
                    baseView.RightClick(eventInfo.ItemUID, eventInfo.Row, eventInfo.ColUID);
                }
            }
            catch (Exception e)
            {
                if (LogIsActive)
                {
                    ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 235 - " + e.Message);
                }
            }
        }

        private void HandleFormResize(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_FORM_RESIZE && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        view.Resize();                       
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 259 - " + e.Message);
                    }
                }
            }
        }

        private void HandleChooseFrom(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;
            
            if (pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST && pVal.BeforeAction == true)
            {
                if (!SuppressChoose)
                {
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        if (pVal.ColUID != string.Empty)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            {
                                SuppressChoose = view.ColSupressChooseFrom(pVal.ItemUID, pVal.Row, pVal.ColUID);

                                if (SuppressChoose)
                                    break;
                            }
                        }
                        else
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            {
                                SuppressChoose = view.SupressChooseFrom(pVal.ItemUID);

                                if (SuppressChoose)
                                    break;
                            }
                        }
                    }
                }
            }

            if (pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    SAPbouiCOM.IChooseFromListEvent chooseFromListEvent = ((SAPbouiCOM.IChooseFromListEvent)(pVal));

                    if ((chooseFromListEvent.BeforeAction == false) && (chooseFromListEvent.SelectedObjects != null))
                    {
                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            for (int cfRow = 0; cfRow < chooseFromListEvent.SelectedObjects.Rows.Count; cfRow++)
                            {
                                if (chooseFromListEvent.SelectedObjects.Columns.Count > 33)
                                {
                                    if (pVal.ColUID != string.Empty)
                                    {
                                        Dictionary<string, string> values = new Dictionary<string, string>();

                                        for (int value = 0; value < 33; value++)
                                        {
                                            try
                                            {
                                                values.Add(chooseFromListEvent.SelectedObjects.Columns.Item(value).Name, chooseFromListEvent.SelectedObjects.GetValue(value, cfRow).ToString());
                                            }
                                            catch { }
                                        }

                                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                            view.ColChooseFrom(pVal.ItemUID, pVal.Row + cfRow, pVal.ColUID, values);
                                    }
                                    else
                                    {
                                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                            view.ChooseFrom(pVal.ItemUID,
                                                            chooseFromListEvent.SelectedObjects.GetValue(0, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(1, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(2, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(3, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(4, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(5, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(6, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(7, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(8, 0).ToString(),
                                                            chooseFromListEvent.SelectedObjects.GetValue(9, 0).ToString());
                                    }
                                }
                                else
                                {
                                    if (pVal.ColUID != string.Empty)
                                    {
                                        Dictionary<string, string> values = new Dictionary<string, string>();

                                        for (int value = 0; value < chooseFromListEvent.SelectedObjects.Columns.Count - 1; value++)
                                        {
                                            try
                                            {
                                                values.Add(chooseFromListEvent.SelectedObjects.Columns.Item(value).Name, chooseFromListEvent.SelectedObjects.GetValue(value, cfRow).ToString());
                                            }
                                            catch { }
                                        }

                                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                            view.ColChooseFrom(pVal.ItemUID, pVal.Row + cfRow, pVal.ColUID, values);
                                    }
                                    else
                                    {
                                        string[] values = new string[chooseFromListEvent.SelectedObjects.Columns.Count];

                                        for (int value = 0; value < values.Count() - 1; value++)
                                        {
                                            try
                                            {
                                                values[value] = chooseFromListEvent.SelectedObjects.GetValue(value, 0).ToString();
                                            }
                                            catch { }
                                        }

                                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                            view.ChooseFrom(pVal.ItemUID, values);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 299 - " + e.Message);
                    }
                }
            }
        }

        private void HandleFolderSelect(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_CLICK && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        view.FolderSelect(pVal.ItemUID);                    
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 323 - " + e.Message);                        
                    }
                }
            }
        }

        private void HandleGridRowClick(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_DOUBLE_CLICK && pVal.BeforeAction == false)
            {
                try
                {
                    if (pVal.Row >= 0)
                    {
                        string formType = pVal.FormTypeEx;

                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            view.GridRowDoubleClick(pVal.ItemUID, pVal.Row, pVal.ColUID);
                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 323 - " + e.Message);
                    }
                }
            }

            if (pVal.EventType == BoEventTypes.et_CLICK && pVal.BeforeAction == false)
            {
                try
                {
                    if (pVal.Row >= 0)
                    {
                        string formType = pVal.FormTypeEx;

                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            view.GridRowClick(pVal.ItemUID, pVal.Row, pVal.ColUID);                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 323 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMatrixRowClick(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_DOUBLE_CLICK)
            {
                try
                {
                    if (pVal.BeforeAction)
                    {
                        if (!SupressDetails)
                        {
                            string formType = pVal.FormTypeEx;

                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            {
                                SupressDetails = view.SupressMatrixDetails(pVal.ItemUID);

                                if (SupressDetails)
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (pVal.Row > 0)
                        {
                            string formType = pVal.FormTypeEx;

                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                view.MatrixRowDoubleClick(pVal.ItemUID, pVal.Row, pVal.ColUID);                            
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 323 - " + e.Message);
                    }
                }
            }

            if (pVal.EventType == BoEventTypes.et_CLICK && pVal.BeforeAction == false)
            {
                try
                {
                    if (pVal.Row > 0)
                    {
                        string formType = pVal.FormTypeEx;

                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            view.MatrixRowEnter(pVal.ItemUID, pVal.Row, pVal.ColUID, pVal.Modifiers);                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 323 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMatrixSort(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_GRID_SORT && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                        view.MatrixSort(pVal.ItemUID, pVal.ColUID);                    
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 323 - " + e.Message);
                    }
                }
            }
        }

        private void HandleKeyDown(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_KEY_DOWN && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        if (pVal.CharPressed == 9)
                        {
                            if (pVal.Row > 0)
                            {
                                foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                    view.MatrixTabPressed(pVal.ItemUID, pVal.Row, pVal.ColUID);
                            }

                            if (pVal.Row >= 0)
                            {
                                foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                                    view.GridTabPressed(pVal.ItemUID, pVal.Row, pVal.ColUID);
                            }
                        }
                        else
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formUID && r.FormType == formType).ToList())
                            {
                                view.KeyDown(pVal.ItemUID);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 323 - " + e.Message);
                    }
                }
            }
        }

        private void HandleFormData(ref BusinessObjectInfo objectInfo, out bool bubbleEvent)
        {
            bubbleEvent = true;
            if (objectInfo.BeforeAction == true)
            {
                try
                {
                    string formId = objectInfo.FormUID;
                    string formType = objectInfo.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        string msg;

                        if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_DELETE)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            {
                                if (!view.ValidateFormData(out msg, true))
                                {
                                    AddOn.Instance.ConnectionController.Application.StatusBar.SetText(msg, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);

                                    bubbleEvent = false;

                                    break;
                                }
                            }
                        }
                        else if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD || objectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            {
                                if (!view.ValidateFormData(out msg, false))
                                {
                                    AddOn.Instance.ConnectionController.Application.StatusBar.SetText(msg, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);

                                    bubbleEvent = false;

                                    break;
                                }
                            }                       
                        }                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 344 - " + e.Message);
                    }
                }
            }
            else if (objectInfo.ActionSuccess)
            {
                try
                {
                    string formId = objectInfo.FormUID;
                    string formType = objectInfo.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_LOAD)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                                view.GotFormData();
                        }
                        if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            {
                                view.AddFormData();
                            }
                        }
                        if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                                view.UpdateFormData();
                        }
                        if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_DELETE)
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                                view.DeleteFormData();
                        }
                    }                    
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 344 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMenuInsert(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.MenuUID == "1282" && pVal.BeforeAction == false)
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx.Replace("-", "");

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuInsert();
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                                view.MenuInsert();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 384 - " + e.Message);                        
                    }
                }
            }
        }

        private void HandleMenuSaveAsDraft(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.MenuUID == "5907" && pVal.BeforeAction == true)
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx.Replace("-", "");

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuSaveAsDraft();
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                                view.MenuSaveAsDraft();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 384 - " + e.Message);
                    }
                }
            }

            if (pVal.MenuUID == "5907" && pVal.BeforeAction == false)
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx.Replace("-", "");

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.AfterSaveAsDraft();
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                                view.AfterSaveAsDraft();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 384 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMenuSearch(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.MenuUID == "1281")
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;

                    if (pVal.BeforeAction)
                    {
                        if (!formId.Contains("F_"))
                        {
                            if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                            {
                                if (Controller.ConnectionController.Instance.Application.MessageBox("Dados não gravados serão perdidos. Continuar?", 1, "Sim", "Não") != 1)
                                {
                                    bubbleEvent = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                                view.MenuSearch();
                        }
                        else if (formId.Contains("F_"))
                        {
                            formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                            if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                            {
                                foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                                    view.MenuSearch();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 414 - " + e.Message);
                    }
                }
            }
            else if (pVal.MenuUID == "4870")
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;

                    if (pVal.BeforeAction)
                    {
                        if (!formId.Contains("F_"))
                        {
                            foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            {
                                m_GridOnFilter = true;

                                m_FilteredView = view;

                                if (m_FilteredView.SAPForm.ActiveItem == m_FilteredView.SAPForm.Settings.MatrixUID)
                                {
                                    for (int i = 0; i <= m_FilteredView.SAPForm.Items.Count; i++)
                                    {
                                        if (m_FilteredView.SAPForm.Items.Item(i).Type == BoFormItemTypes.it_EDIT && m_FilteredView.SAPForm.Items.Item(i).Enabled)
                                        {
                                            m_FilteredView.SAPForm.Items.Item(i).Click();

                                            break;
                                        }
                                    }
                                }

                                m_FilteredView.SAPForm.Items.Item(m_FilteredView.SAPForm.Settings.MatrixUID).Enabled = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 414 - " + e.Message);
                    }
                }
            }
        }

        bool m_GridOnFilter = false;
        View.BaseView m_FilteredView = null;

        private void HandleMenuDuplicate(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.MenuUID == "1287" && pVal.BeforeAction == false)
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuDuplicate();
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuDuplicate();                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 444 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMenuCancel(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.MenuUID == "1284" && pVal.BeforeAction == false)
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuCancel();
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuCancel();                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 444 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMenuRightClick(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.BeforeAction == false)
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuRightClick(pVal.MenuUID);
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuRightClick(pVal.MenuUID);                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 444 - " + e.Message);
                    }
                }
            }
            else
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.RightMenuClicked(pVal.MenuUID);
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.RightMenuClicked(pVal.MenuUID);                        
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 444 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMenuPaste(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.MenuUID == "773")
            {
                if (pVal.BeforeAction)
                {
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleFormLoad;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleGotFocus;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleLostFocus;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleFormClose;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleFormDeactivate;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandlePickerClick;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleButtonClick;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleFolderSelect;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleChooseFrom;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleButtonPress;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleFormValidate;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleFormResize;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleGridRowClick;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleMatrixRowClick;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleMatrixSort;
                    Controller.ConnectionController.Instance.Application.ItemEvent -= HandleKeyDown;
                    Controller.ConnectionController.Instance.Application.FormDataEvent -= HandleFormData;
                    Controller.ConnectionController.Instance.Application.MenuEvent -= HandleMenuInsert;
                    Controller.ConnectionController.Instance.Application.MenuEvent -= HandleMenuSaveAsDraft;
                    Controller.ConnectionController.Instance.Application.MenuEvent -= HandleMenuSearch;
                    Controller.ConnectionController.Instance.Application.MenuEvent -= HandleMenuDuplicate;
                    Controller.ConnectionController.Instance.Application.MenuEvent -= HandleMenuCancel;
                    Controller.ConnectionController.Instance.Application.MenuEvent -= HandleMenuAny;
                    Controller.ConnectionController.Instance.Application.MenuEvent -= HandleMenuRightClick;
                    Controller.ConnectionController.Instance.Application.StatusBarEvent -= HandleStatusBarMessage;
                    Controller.ConnectionController.Instance.Application.RightClickEvent -= HandleRightClick;
                }
                else
                {
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormLoad;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleGotFocus;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleLostFocus;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormClose;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormDeactivate;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandlePickerClick;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleButtonClick;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleFolderSelect;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleChooseFrom;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleButtonPress;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormValidate;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormResize;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleGridRowClick;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleMatrixRowClick;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleMatrixSort;
                    Controller.ConnectionController.Instance.Application.ItemEvent += HandleKeyDown;
                    Controller.ConnectionController.Instance.Application.FormDataEvent += HandleFormData;
                    Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuInsert;
                    Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuSaveAsDraft;
                    Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuSearch;
                    Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuDuplicate;
                    Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuCancel;
                    Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuAny;
                    Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuRightClick;
                    Controller.ConnectionController.Instance.Application.StatusBarEvent += HandleStatusBarMessage;
                    Controller.ConnectionController.Instance.Application.RightClickEvent += HandleRightClick;

                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuPaste();
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        foreach (View.BaseView view in m_Views.Where(r => r.FormUID == formId && r.FormType == formType).ToList())
                            view.MenuPaste();
                    }
                }
            }
        }

        private void HandleMenuAny(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.BeforeAction == false)
            {
                try
                {
                    if (OpenMenuEvents().ContainsKey(pVal.MenuUID))
                    {
                        OpenMenuEvents()[pVal.MenuUID]();
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("[" + AddOnID + "]" + " 323 - " + e.Message);
                    }
                }
            }
        }        

        private void HandleAppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:                        
                    {                        
                        foreach (string menu in Menus)
                        {
                            try
                            {
                                ConnectionController.Instance.Application.Menus.RemoveEx(menu);
                            }
                            catch { }
                        }                        
                        ExitApp();                                                
                    }
                    break;                    
            }
        }

        private void HandleStatusBarMessage(string text, BoStatusBarMessageType messageType)
        {
            if (text.Contains("Ação suspendida pelo add-on"))
            {
                ConnectionController.Instance.Application.StatusBar.SetText(LastStatusBarMsg, BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
            }
            else
            {
                LastStatusBarMsg = text;
            }
        }

    }
}
