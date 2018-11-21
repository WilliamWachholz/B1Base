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
        
        Controller.ConfigController _configController;
        UserDataSource _usdAutoCreateMetadata;

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

                _configController = new Controller.ConfigController();
                
                _usdAutoCreateMetadata = SAPForm.DataSources.UserDataSources.Item("USDAUTOCM");

                Model.ConfigModel configModel = _configController.GetConfig(1);
                _usdAutoCreateMetadata.Value = configModel.AutoCreateMetadata ? "Y" : "N";
            }
            catch (Exception e)
            {
                AddOn.Instance.ConnectionController.Application.StatusBar.SetText(e.Message);
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
            configModel.AutoCreateMetadata = _usdAutoCreateMetadata.Value == "Y";

            _configController.SaveConfig(configModel);
        }

        private void HandleButtonCreateMetadataClick()
        {
            AddOn.Instance.MainController.CreateMetadata();
        }
    }
}
