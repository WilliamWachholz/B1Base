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

        private List<string> Menus { get; set; }

        private bool LogIsActive { get; set; }        
        private string LastStatusBarMsg { get; set; }
        private bool SuppressChoose { get; set; }
        private bool SupressPicker { get; set; }

        Timer m_timerFinalize = new Timer(60000);

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

        protected void CreateMenu(string menuFather, string menuID, string menuName, string imageFile, bool popup)
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
                    oCreationPackage = ConnectionController.Instance.Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams);

                    oCreationPackage.Type = popup ? SAPbouiCOM.BoMenuType.mt_POPUP : SAPbouiCOM.BoMenuType.mt_STRING;
                    oCreationPackage.UniqueID = menuID;
                    oCreationPackage.String = menuName;
                    if (imageFile != string.Empty)
                        oCreationPackage.Image = AddOn.Instance.CurrentDirectory + @"\img\" + imageFile;
                    oCreationPackage.Position = 12;
                   
                    try
                    {
                        oMenuItem = oMenus.AddEx(oCreationPackage);

                        Menus.Add(menuID);
                    }
                    catch
                    {
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
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleMatrixKeyDown;            
            Controller.ConnectionController.Instance.Application.FormDataEvent += HandleFormData;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuInsert;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuSearch;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuDuplicate;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuAny;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuRightClick;
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

                ConnectionController.Instance.CreateMetadata(true, "Code", FieldTypeEnum.Integer);
                ConnectionController.Instance.CreateMetadata(true, "AutoCreateMetadata", FieldTypeEnum.Alphanumeric, 1);
                ConnectionController.Instance.CreateMetadata(true, "ActivateLog", FieldTypeEnum.Alphanumeric, 1);

                ConnectionController.Instance.CreateMetadata(AddOnID + "Seq", "Code", FieldTypeEnum.Integer);
                ConnectionController.Instance.CreateMetadata(AddOnID + "Seq", "UserTable", FieldTypeEnum.Alphanumeric, 40);
                ConnectionController.Instance.CreateMetadata(AddOnID + "Seq", "NextCode", FieldTypeEnum.Integer);

                Model.ConfigModel configModel = new Model.ConfigModel();
                configModel = new ConfigController<Model.ConfigModel>().GetConfig();

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
            return OpenView(false, formType, parentView, wait);
        }

        public View.BaseView OpenView(bool unique, string formType, View.BaseView parentView, bool wait = false)
        {
            try
            {
                bool notExists = false;

                try
                {
                    Controller.ConnectionController.Instance.Application.Forms.GetForm(formType, 1);
                }
                catch
                {
                    notExists = true;
                }

                string formUID = "RW1";

                string newFormType = formType;

                if ((unique == false) || (unique && notExists))
                {
                    int count = 0;

                    bool next = true;

                    while (next)
                    {
                        count++;

                        try
                        {
                            Controller.ConnectionController.Instance.Application.Forms.GetForm(formType, count);
                        }
                        catch
                        {
                            next = false;
                        }
                    }

                    newFormType = formType.Split('.')[2].Length > 20 ? formType.Split('.')[2].Substring(0, 20) : formType.Split('.')[2];

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

                    if (Controller.ConnectionController.Instance.DBServerType == "SQLSERVER")
                        xml = xml.Replace("from dummy", "");

                    if (!FormTypeViews.ContainsKey(newFormType))
                        FormTypeViews.Add(newFormType, formType);

                    Controller.ConnectionController.Instance.Application.LoadBatchActions(ref xml);
                }

                if (wait)
                    System.Threading.Thread.Sleep(1000);

                m_Views.First(r => r.FormUID == formUID && r.FormType == newFormType).ParentView = parentView;

                return m_Views.First(r => r.FormUID == formUID && r.FormType == newFormType);
            }
            catch(Exception ex)
            {
                if (LogIsActive)
                {
                    ConnectionController.Instance.Application.StatusBar.SetText(ex.Message);
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

        private void HandleFormLoad(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            string formType = pVal.FormTypeEx;

            if (pVal.EventType == BoEventTypes.et_FORM_LOAD && pVal.BeforeAction == false)
            {
                bubbleEvent = true;

                try
                {
                    if (SuppressChoose)
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
                        if (FormTypeViews.ContainsKey(pVal.FormTypeEx))
                        {
                            Assembly assembly = Assembly.LoadFile(AddOn.Instance.CurrentDirectory + "\\" + FormTypeViews[pVal.FormTypeEx].Split('.')[0] + ".dll");

                            Type type = assembly.GetType(FormTypeViews[pVal.FormTypeEx]);

                            if (type == null)
                                return;

                            if (m_Views.Where(r => r.FormUID == formUID).Count() == 0)
                            {
                                ConstructorInfo constructor = type.GetConstructor(new Type[] { formUID.GetType(), pVal.FormTypeEx.GetType() });
                                object formView = constructor.Invoke(new object[] { formUID, pVal.FormTypeEx });

                                m_Views.Add((View.BaseView)formView);
                            }
                        }
                        else
                        {
                            string[] dlls = Directory.GetFiles(AddOn.Instance.CurrentDirectory, "*.dll");

                            foreach (string dll in dlls)
                            {
                                Assembly assembly = Assembly.LoadFile(dll);

                                Type type = assembly.GetType(assembly.GetName().Name + ".View." + pVal.FormTypeEx.Split('.')[pVal.FormTypeEx.Split('.').Count() - 1]);

                                if (type != null)
                                {
                                    if (m_Views.Where(r => r.FormUID == formUID).Count() == 0)
                                    {
                                        ConstructorInfo constructor = type.GetConstructor(new Type[] { formUID.GetType(), pVal.FormTypeEx.GetType() });
                                        object formView = constructor.Invoke(new object[] { formUID, pVal.FormTypeEx });

                                        m_Views.Add((View.BaseView)formView);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string[] dlls = Directory.GetFiles(AddOn.Instance.CurrentDirectory, "*.dll");

                        foreach (string dll in dlls)
                        {
                            Assembly assembly = Assembly.LoadFile(dll);

                            Type type = assembly.GetType(assembly.GetName().Name + ".View.Form" + pVal.FormTypeEx + "View");

                            if (type != null)
                            {
                                if (m_Views.Where(r => r.FormUID == formUID).Count() == 0)
                                {
                                    ConstructorInfo constructor = type.GetConstructor(new Type[] { formUID.GetType(), pVal.FormTypeEx.GetType() });
                                    object formView = constructor.Invoke(new object[] { formUID, pVal.FormTypeEx });

                                    m_Views.Add((View.BaseView)formView);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {                        
                        ConnectionController.Instance.Application.StatusBar.SetText("114 - " + e.Message);
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

        private void HandleGotFocus(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_GOT_FOCUS && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).GotFocus();

                        if (pVal.ItemUID != string.Empty)
                        {
                            if (pVal.ColUID != string.Empty)
                            {
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColumnFocus(pVal.ItemUID, pVal.Row, pVal.ColUID);
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).MatrixRowEnter(pVal.ItemUID, pVal.Row, pVal.ColUID, pVal.Modifiers);
                            }
                            else
                            {
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).EditFocus(pVal.ItemUID);
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ComboFocus(pVal.ItemUID);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("138 - " + e.Message);
                    }
                }
            }
        }

        private void HandleFormClose(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_FORM_UNLOAD && pVal.BeforeAction == true)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).Close();
                        m_Views.Remove(m_Views.First(r => r.FormUID == formUID && r.FormType == formType));
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("163 - " + e.Message);
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

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).LostFocus();
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("259 - " + e.Message);
                    }
                }
            }
        }

        private void HandlePickerClick(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_PICKER_CLICKED && pVal.BeforeAction == true)
            {
                string formType = pVal.FormTypeEx;

                if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                {
                    if (pVal.ColUID != string.Empty)
                    {
                        
                    }
                    else
                    {
                        SupressPicker = m_Views.First(r => r.FormUID == formUID && r.FormType == formType).SupressPickerClick(pVal.ItemUID);
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
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ButtonOkClick();
                        }
                    }
                    else
                    {

                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ButtonClick(pVal.ItemUID);
                        }
                    }                   
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("187 - " + e.Message);
                    }
                }
            }            
        }

        private void HandleButtonPress(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_MATRIX_LINK_PRESSED && pVal.BeforeAction == true)
            {
                string formType = pVal.FormTypeEx;
                string formId = pVal.FormUID;

                if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                {
                    m_Views.First(r => r.FormUID == formUID && r.FormType == formType).MatrixRowEnter(pVal.ItemUID, pVal.Row, pVal.ColUID, pVal.Modifiers);
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
                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ButtonOkPress();
                        }
                    }
                    else
                    {
                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ButtonPress(pVal.ItemUID);

                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ButtonOpenView(pVal.ItemUID, m_Views[m_Views.Count - 1]);

                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).LinkPress(pVal.ItemUID, m_Views[m_Views.Count - 1]);
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
                        ConnectionController.Instance.Application.StatusBar.SetText("211 - " + e.Message);
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
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColumnChecked(pVal.ItemUID, pVal.Row, pVal.ColUID);
                        }
                        else
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).Checked(pVal.ItemUID);

                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).OptionSelect(pVal.ItemUID);
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
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColumnValidate(pVal.ItemUID, pVal.Row, pVal.ColUID);
                            }
                            else if (pVal.ItemUID != string.Empty)
                            {
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).EditValidate(pVal.ItemUID);
                            }

                            bubbleEvent = false;
                        }
                    }
                    catch (Exception e)
                    {
                        if (LogIsActive)
                        {
                            ConnectionController.Instance.Application.StatusBar.SetText("235 - " + e.Message);
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
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColumnSelect(pVal.ItemUID, pVal.Row, pVal.ColUID);
                        }
                        else
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ComboSelect(pVal.ItemUID);
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

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).FormValidate();
                        
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
                    ConnectionController.Instance.Application.StatusBar.SetText("235 - " + e.Message);
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

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).Resize(); ;
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("259 - " + e.Message);
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
                            SuppressChoose = m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColSupressChooseFrom(pVal.ItemUID, pVal.Row, pVal.ColUID);
                        }
                        else
                        {
                            SuppressChoose = m_Views.First(r => r.FormUID == formUID && r.FormType == formType).SupressChooseFrom(pVal.ItemUID);
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

                                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColChooseFrom(pVal.ItemUID,
                                            pVal.Row + cfRow,
                                            pVal.ColUID,
                                            values);
                                    }
                                    else
                                    {
                                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ChooseFrom(pVal.ItemUID,
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

                                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColChooseFrom(pVal.ItemUID,
                                            pVal.Row + cfRow,
                                            pVal.ColUID,
                                            values);
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

                                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ChooseFrom(pVal.ItemUID,
                                           values);
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
                        ConnectionController.Instance.Application.StatusBar.SetText("299 - " + e.Message);
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

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).FolderSelect(pVal.ItemUID);
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("323 - " + e.Message);                        
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

                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).GridRowDoubleClick(pVal.ItemUID, pVal.Row);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("323 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMatrixRowClick(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;
            
            if (pVal.EventType == BoEventTypes.et_DOUBLE_CLICK && pVal.BeforeAction == false)
            {
                try
                {
                    if (pVal.Row > 0)
                    {
                        string formType = pVal.FormTypeEx;

                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).MatrixRowDoubleClick(pVal.ItemUID, pVal.Row, pVal.ColUID);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("323 - " + e.Message);
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

                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).MatrixRowEnter(pVal.ItemUID, pVal.Row, pVal.ColUID, pVal.Modifiers);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("323 - " + e.Message);
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

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).MatrixSort(pVal.ItemUID, pVal.ColUID);
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("323 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMatrixKeyDown(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_KEY_DOWN && pVal.BeforeAction == false)
            {
                try
                {
                    if (pVal.Row > 0)
                    {
                        string formType = pVal.FormTypeEx;

                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            if (pVal.CharPressed == 9)
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).MatrixTabPressed(pVal.ItemUID, pVal.Row, pVal.ColUID);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("323 - " + e.Message);
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
                            if (!m_Views.First(r => r.FormUID == formId && r.FormType == formType).ValidateFormData(out msg, true))
                            {
                                AddOn.Instance.ConnectionController.Application.StatusBar.SetText(msg, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);

                                bubbleEvent = false;
                            }
                        }
                        else if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD || objectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE)
                        {
                            if (!m_Views.First(r => r.FormUID == formId && r.FormType == formType).ValidateFormData(out msg, false))
                            {                                
                                AddOn.Instance.ConnectionController.Application.StatusBar.SetText(msg, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);

                                bubbleEvent = false;
                            }
                        }                        
                    }
                }                 
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("344 - " + e.Message);
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
                            m_Views.First(r => r.FormUID == formId && r.FormType == formType).GotFormData();
                        }
                        if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD)
                        {                            
                            m_Views.First(r => r.FormUID == formId && r.FormType == formType).AddFormData();
                        }
                        if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE)
                        {
                            m_Views.First(r => r.FormUID == formId && r.FormType == formType).UpdateFormData();
                        }
                        if (objectInfo.EventType == BoEventTypes.et_FORM_DATA_DELETE)
                        {
                            m_Views.First(r => r.FormUID == formId && r.FormType == formType).DeleteFormData();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("344 - " + e.Message);
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
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;
                    
                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuInsert();
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuInsert();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("384 - " + e.Message);                        
                    }
                }
            }
        }

        private void HandleMenuSearch(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;
            
            if (pVal.MenuUID == "1281")
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
                    try
                    {
                        if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuSearch();
                        }
                        else if (formId.Contains("F_"))
                        {
                            formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                            if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                            {
                                m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuSearch();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (LogIsActive)
                        {
                            ConnectionController.Instance.Application.StatusBar.SetText("414 - " + e.Message);
                        }
                    }
                }
            }
        }

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
                        m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuDuplicate();
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuDuplicate();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("444 - " + e.Message);
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
                        m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuRightClick(pVal.MenuUID);
                    }
                    else if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();

                        if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuRightClick(pVal.MenuUID);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("444 - " + e.Message);
                    }
                }
            }
        }

        private void HandleMenuAny(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.BeforeAction == false)
            {
                if (OpenMenuEvents().ContainsKey(pVal.MenuUID))
                {
                    OpenMenuEvents()[pVal.MenuUID]();
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
