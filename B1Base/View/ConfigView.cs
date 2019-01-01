using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbouiCOM;

namespace B1Base.View
{
    class ConfigView : BaseView
    {
        const string BUTTON_OK = "1";
        const string BUTTON_CREATE_METADATA = "BTNMETA";
        const string CHECK_AUTO_CREATE_METADATA = "CHK";
        const string CHECK_LOG = "5";
                

        public ConfigView(string formUID, string formType) : base(formUID, formType)        
        {
            
        }

        public override void AddFormData()
        {
            base.AddFormData();
        }

        protected override void CreateControls()
        {
            try
            {
                SAPForm.Title = string.Format(SAPForm.Title, AddOn.Instance.MainController.AddOnName);

                ControlMenus(false, false, false);

                Controller.ConfigController configController = new Controller.ConfigController();

                Model.ConfigModel configModel = configController.GetConfig(1);

                SetValue(CHECK_AUTO_CREATE_METADATA, configModel.AutoCreateMetadata);
                SetValue(CHECK_LOG, configModel.ActivateLog);
            }
            catch (Exception e)
            {
                Controller.ConnectionController.Instance.Application.StatusBar.SetText(e.Message);
            }
        }

        protected override Dictionary<string, BaseView.ButtonClickEventHandler> ButtonClickEvents
        {
            get
            {
                Dictionary<string, BaseView.ButtonClickEventHandler> result = base.ButtonClickEvents;
                result.Add(BUTTON_OK, HandleButtonOkClick);
                result.Add(BUTTON_CREATE_METADATA, HandleButtonCreateMetadataClick);

                return result;
            }
        }

        private void HandleButtonOkClick()
        {   
            Model.ConfigModel configModel = new Model.ConfigModel();
            configModel.Code = 1;
            configModel.AutoCreateMetadata = GetValue(CHECK_AUTO_CREATE_METADATA);
            configModel.ActivateLog = GetValue(CHECK_LOG);

            new Controller.ConfigController().SaveConfig(configModel);
        }

        private void HandleButtonCreateMetadataClick()
        {
            AddOn.Instance.MainController.CreateMetadata();
        }
    }
}
