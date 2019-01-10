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
        const string BUTTON_OK = "BTNOK";
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
                result.Add(BUTTON_OK, ButtonOkClick);
                result.Add(BUTTON_CREATE_METADATA, ButtonCreateMetadataClick);

                return result;
            }
        }

        private new void ButtonOkClick()
        {   
            Model.ConfigModel configModel = new Model.ConfigModel();
            configModel.Code = 1;
            configModel.AutoCreateMetadata = GetValue(CHECK_AUTO_CREATE_METADATA);
            configModel.ActivateLog = GetValue(CHECK_LOG);

            new Controller.ConfigController().SaveConfig(configModel);

            base.SAPForm.Close();
        }

        private void ButtonCreateMetadataClick()
        {
            AddOn.Instance.MainController.CreateMetadata();
        }
    }
}
