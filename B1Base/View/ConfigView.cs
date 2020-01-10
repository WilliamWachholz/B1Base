using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbouiCOM;

namespace B1Base.View
{
    public class ConfigView : BaseView
    {
        #region constantes

        const string BUTTON_OK = "BTNOK";
        const string BUTTON_CREATE_METADATA = "BTNMETA";
        
        const string CHECK_AUTO_CREATE_METADATA = "CHK";
        const string CHECK_LOG = "5";

        const string MATRIX_SEQ = "6";
        const string DATA_SEQ = "DT_0";

        const string V_USER_TABLE = "V_1";
        const string V_NEXT_CODE = "V_0";

        #endregion

        public ConfigView(string formUID, string formType) : base(formUID, formType)        
        {
            
        }

        protected virtual void SetFields(Model.ConfigModel configModel)
        {            
            SetValue(CHECK_AUTO_CREATE_METADATA, configModel.AutoCreateMetadata);
            SetValue(CHECK_LOG, configModel.ActivateLog);
        }

        protected virtual void GetFields(Model.ConfigModel configModel)
        {
            configModel.Code = 1;
            configModel.AutoCreateMetadata = GetValue(CHECK_AUTO_CREATE_METADATA);
            configModel.ActivateLog = GetValue(CHECK_LOG);
        }

        protected virtual void SaveModel(Model.ConfigModel configModel)
        {
            new Controller.ConfigController<Model.ConfigModel>().SaveConfig(configModel);
        }

        protected virtual Model.ConfigModel GetModel()
        {
            return new Controller.ConfigController<Model.ConfigModel>().GetConfig();
        }

        protected override Dictionary<string, MatrixCanAddEventHandler> MatrixCanAddEvents
        {
            get
            {
                Dictionary<string, MatrixCanAddEventHandler> result = base.MatrixCanAddEvents;
                result.Add(MATRIX_SEQ + "." + V_USER_TABLE, MatrixSeqCanAdd);
                result.Add(MATRIX_SEQ + "." + V_NEXT_CODE, MatrixSeqCanAdd);

                return result;
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

        protected override void CreateControls()
        {            
            try
            {
                SAPForm.Title = string.Format(SAPForm.Title, AddOn.Instance.MainController.AddOnName);

                ControlMenus(false, false, false);

                Model.ConfigModel configModel = GetModel();

                SetFields(configModel);

                List<Model.ConfigSeqModel> configSeqList = new Controller.ConfigController<Model.ConfigModel>().GetListConfigSeq();
                
                SetValue<Model.ConfigSeqModel>(base.SAPForm.DataSources.DataTables.Item(DATA_SEQ), (Matrix)base.SAPForm.Items.Item(MATRIX_SEQ).Specific, configSeqList);
            }
            catch (Exception e)
            {
                Controller.ConnectionController.Instance.Application.StatusBar.SetText(e.Message);
            }
        }

        private new void ButtonOkClick()
        {            
            Model.ConfigModel configModel = GetModel();
            GetFields(configModel);
            
            SaveModel(configModel);

            new Controller.ConfigController<Model.ConfigModel>().SaveConfigSeq(GetValue<Model.ConfigSeqModel>(base.SAPForm.DataSources.DataTables.Item(DATA_SEQ), (Matrix)base.SAPForm.Items.Item(MATRIX_SEQ).Specific));

            base.SAPForm.Close();
        }

        private void ButtonCreateMetadataClick()
        {
            AddOn.Instance.MainController.CreateMetadata();

            AddOn.Instance.ConnectionController.Application.StatusBar.SetText("Metadados criados com sucesso", BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Success);
        }

        private bool MatrixSeqCanAdd(int row)
        {
            return GetValue(MATRIX_SEQ, V_USER_TABLE, row) != string.Empty &
                GetValue(MATRIX_SEQ, V_NEXT_CODE, row) > 0;
        }

        public override void AddFormData()
        {
            base.AddFormData();
        }
    }
}
