using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public virtual void CreateMetadata() { }
        protected abstract void ExitApp();
        protected virtual void CreateMenus() { }
        private bool LogIsActive { get; set; }

        protected const string MENU_SAP = "43520";
        protected string MENU_ADDON { get { return AddOnID;  } }
        protected string MENU_CONFIG { get { return AddOnID + "Cnf";  } }

        protected delegate void OpenMenuEventHandler();

        protected virtual Dictionary<string, OpenMenuEventHandler> OpenMenuEvents()
        {
            Dictionary<string, OpenMenuEventHandler> result = new Dictionary<string, OpenMenuEventHandler>();
            result.Add(MENU_CONFIG, HandleOpenMenuConfig);

            return result;
        }

        protected void CreateMenu(string menuFather, string menuID, string menuName, string imageFile, bool popup)
        {
            SAPbouiCOM.Menus oMenus = null;
            SAPbouiCOM.MenuItem oMenuItem = null;
            SAPbouiCOM.MenuCreationParams oCreationPackage = null;

            oMenuItem = ConnectionController.Instance.Application.Menus.Item(menuFather);
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
                }
                catch
                {
                }
            }
        }        

        public void Initialize() 
        {
            try
            {
                Controller.ConnectionController.Instance.Initialize();
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Falha ao conectar addOn {0}. {1}", AddOnName, e.Message.ToString()));
            }

            Controller.ConnectionController.Instance.Application.AppEvent += HandleAppEvent;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormLoad;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleGotFocus;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormClose;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleButtonClick;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFolderSelect;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleChooseFrom;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleButtonPress;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormValidate;            
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleFormResize;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleMatrixRowClick;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleMatrixSort;
            Controller.ConnectionController.Instance.Application.ItemEvent += HandleMatrixKeyDown;
            Controller.ConnectionController.Instance.Application.FormDataEvent += HandleFormDataLoad;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuInsert;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuSearch;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuDuplicate;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuAny;
            Controller.ConnectionController.Instance.Application.MenuEvent += HandleMenuRightClick;
            Controller.ConnectionController.Instance.Application.RightClickEvent += HandleRightClick;

            try
            {
                CreateMenu(MENU_SAP, MENU_ADDON, AddOnName, "addOnLogo.png", true);
                CreateMenu(MENU_ADDON, MENU_CONFIG, "Configuração", "", false);

                CreateMenus();
            }
            catch(Exception e)
            {
                ConnectionController.Instance.Application.StatusBar.SetText(string.Format("Falha ao criar menus: {0}", e.Message), BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
            }

            try
            {
                ConnectionController.Instance.CreateMetadata(AddOnID + "Cnf", "Code", FieldTypeEnum.Integer);
                ConnectionController.Instance.CreateMetadata(AddOnID + "Cnf", "AutoCreateMetadata", FieldTypeEnum.Alphanumeric, 1);
                ConnectionController.Instance.CreateMetadata(AddOnID + "Cnf", "ActivateLog", FieldTypeEnum.Alphanumeric, 1);

                Model.ConfigModel configModel = new Model.ConfigModel();
                configModel = new ConfigController().GetConfig(1);

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

        public View.BaseView OpenView(string formType)
        {
            return OpenView(false, formType);
        }

        public View.BaseView OpenView(bool unique, string formType)
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

                formUID = string.Format("RW{0}-{1}", count, new Random().Next(999));

                string srfPath = AddOn.Instance.CurrentDirectory + "\\SRF\\" + formType.Split('.')[2] + ".srf";

                if (File.Exists(srfPath) == false)
                {
                    Controller.ConnectionController.Instance.Application.StatusBar.SetText("Arquivo SRF não encontrado. Verifique a instalação do addOn.", BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
                    return null;
                }

                string xml = File.ReadAllText(srfPath);

                xml = xml.Replace("uid=\"RW0\"", string.Format("uid=\"{0}\"", formUID));

                if (Controller.ConnectionController.Instance.DBServerType == "SQLSERVER")
                    xml = xml.Replace("from dummy", "");

                Controller.ConnectionController.Instance.Application.LoadBatchActions(ref xml);
            }

            return m_Views.First(r => r.FormUID == formUID && r.FormType == formType);
        }       

        public void OpenMenu(string menu)
        {
            Controller.ConnectionController.Instance.Application.Menus.Item(menu).Activate();
        }

        private void HandleFormLoad(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_FORM_LOAD && pVal.BeforeAction == false)
            {
                try
                {
                    if (pVal.FormUID.StartsWith("RW"))
                    {
                        Assembly assembly = Assembly.LoadFile(AddOn.Instance.CurrentDirectory + "\\" + pVal.FormTypeEx.Split('.')[0] + ".dll");

                        Type type = assembly.GetType(pVal.FormTypeEx);

                        if (type == null)
                            return;

                        ConstructorInfo constructor = type.GetConstructor(new Type[] { formUID.GetType(), pVal.FormTypeEx.GetType() });
                        object formView = constructor.Invoke(new object[] { formUID, pVal.FormTypeEx });

                        m_Views.Add((View.BaseView)formView);
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
                                ConstructorInfo constructor = type.GetConstructor(new Type[] { formUID.GetType(), pVal.FormTypeEx.GetType() });
                                object formView = constructor.Invoke(new object[] { formUID, pVal.FormTypeEx });

                                m_Views.Add((View.BaseView)formView);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("114 - " + e.Message);
                        throw e;
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

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).GotFocus();

                        if (pVal.ItemUID != string.Empty)
                        {
                            if (pVal.ColUID != string.Empty)
                            {
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColumnFocus(pVal.ItemUID, pVal.Row, pVal.ColUID);
                                m_Views.First(r => r.FormUID == formUID && r.FormType == formType).MatrixRowEnter(pVal.ItemUID, pVal.Row, pVal.ColUID);
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
                        throw e;
                    }
                }
            }
        }

        private void HandleFormClose(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_FORM_CLOSE && pVal.BeforeAction == false)
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
                        throw e;
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
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ButtonClick(pVal.ItemUID);
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("187 - " + e.Message);
                        throw e;
                    }
                }
            }
        }

        private void HandleButtonPress(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && pVal.BeforeAction == false)
            {
                try
                {
                    string formType = pVal.FormTypeEx;

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ButtonPress(pVal.ItemUID);
                    }

                    if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formUID && r.FormType == formType).LinkPress(pVal.ItemUID, m_Views[m_Views.Count - 1]);
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("211 - " + e.Message);
                        throw e;
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
                            else
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
                            throw e;
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
                    //throw e;
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
                    throw e;
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
                        throw e;
                    }
                }
            }
        }

        private void HandleChooseFrom(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

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
                            if (chooseFromListEvent.SelectedObjects.Columns.Count > 9)
                            {
                                if (pVal.ColUID != string.Empty)
                                {
                                    Dictionary<string, string> values = new Dictionary<string, string>();

                                    for (int value = 0; value < 9; value++)
                                    {
                                        values.Add(chooseFromListEvent.SelectedObjects.Columns.Item(value).Name, chooseFromListEvent.SelectedObjects.GetValue(value, 0).ToString());
                                    }

                                    m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColChooseFrom(pVal.ItemUID, 
                                        pVal.Row, 
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
                                        values.Add(chooseFromListEvent.SelectedObjects.Columns.Item(value).Name, chooseFromListEvent.SelectedObjects.GetValue(value, 0).ToString());
                                    }

                                    m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ColChooseFrom(pVal.ItemUID, 
                                        pVal.Row, 
                                        pVal.ColUID,
                                        values);
                                }
                                else
                                {
                                    string[] values = new string[chooseFromListEvent.SelectedObjects.Columns.Count];

                                    for (int value = 0; value < values.Count() - 1; value++)
                                    {
                                        values[value] = chooseFromListEvent.SelectedObjects.GetValue(value, 0).ToString();
                                    }

                                    m_Views.First(r => r.FormUID == formUID && r.FormType == formType).ChooseFrom(pVal.ItemUID,
                                       values);
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
                        throw e;
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
                        throw e;
                    }
                }
            }
        }

        private void HandleMatrixRowClick(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.EventType == BoEventTypes.et_CLICK && pVal.BeforeAction == false)
            {
                try
                {
                    if (pVal.Row > 0)
                    {
                        string formType = pVal.FormTypeEx;

                        if (m_Views.Any(r => r.FormUID == formUID && r.FormType == formType))
                        {
                            m_Views.First(r => r.FormUID == formUID && r.FormType == formType).MatrixRowEnter(pVal.ItemUID, pVal.Row, pVal.ColUID);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("323 - " + e.Message);
                        throw e;
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
                        throw e;
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
                        throw e;
                    }
                }
            }
        }

        private void HandleFormDataLoad(ref BusinessObjectInfo objectInfo, out bool bubbleEvent)
        {
            bubbleEvent = true;
            if (objectInfo.ActionSuccess)
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
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("344");
                        throw e;
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

                    if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();
                    }

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuInsert();
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("384 - " + e.Message);
                        throw e;
                    }
                }
            }
        }

        private void HandleMenuSearch(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            if (pVal.MenuUID == "1281" && pVal.BeforeAction == false)
            {
                try
                {
                    string formId = ConnectionController.Instance.Application.Forms.ActiveForm.UniqueID;
                    string formType = ConnectionController.Instance.Application.Forms.ActiveForm.TypeEx;

                    if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();
                    }

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuSearch();
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("414 - " + e.Message);
                        throw e;
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

                    if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();
                    }

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuDuplicate();
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("444 - " + e.Message);
                        throw e;
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

                    if (formId.Contains("F_"))
                    {
                        formId = "F_" + (Convert.ToInt32(formId.Replace("F_", "")) - 1).ToString();
                    }

                    if (m_Views.Any(r => r.FormUID == formId && r.FormType == formType))
                    {
                        m_Views.First(r => r.FormUID == formId && r.FormType == formType).MenuRightClick(pVal.MenuUID);
                    }
                }
                catch (Exception e)
                {
                    if (LogIsActive)
                    {
                        ConnectionController.Instance.Application.StatusBar.SetText("444 - " + e.Message);
                        throw e;
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
                    ExitApp();
                    break;
            }
        }

        private void HandleOpenMenuConfig()
        {
            OpenView(true, "B1Base.View.ConfigView");
        }

    }
}
