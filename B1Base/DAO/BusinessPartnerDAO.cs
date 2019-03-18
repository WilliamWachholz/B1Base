using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class BusinessPartnerDAO
    {
        public void Save(Model.BusinessPartnerModel businessPartnerModel)
        {
            BusinessPartners businessPartner = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            try
            {
                if (businessPartner.GetByKey(businessPartnerModel.CardCode))
                {
                    if (businessPartnerModel.SlpCode == 0)
                        businessPartner.SalesPersonCode = -1;
                    else
                        businessPartner.SalesPersonCode = businessPartnerModel.SlpCode;                    

                    businessPartner.Update();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();

                    Controller.ConnectionController.Instance.ExecuteStatement("UpdateBusinessPartnerAgentCode", businessPartnerModel.AgentCode, businessPartnerModel.CardCode);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(businessPartner);
                GC.Collect();
            }
        }

        public Model.BusinessPartnerModel Get(string cardCode)
        {
            Model.BusinessPartnerModel businessPartnerModel = new Model.BusinessPartnerModel();

            BusinessPartners businessPartner = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            try
            {
                if (businessPartner.GetByKey(cardCode))
                {
                    businessPartnerModel.CardCode = businessPartner.CardCode;
                    businessPartnerModel.SlpCode = businessPartner.SalesPersonCode;
                    businessPartnerModel.AgentCode = Controller.ConnectionController.Instance.ExecuteSqlForObject<string>("GetBusinessPartnerAgentCode", cardCode);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(businessPartner);
                GC.Collect();
            }

            return businessPartnerModel;
        }
    }
}
