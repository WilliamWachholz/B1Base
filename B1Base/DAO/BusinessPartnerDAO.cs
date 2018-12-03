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
            BusinessPartners businessPartner = AddOn.Instance.ConnectionController.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            try
            {
                if (businessPartner.GetByKey(businessPartnerModel.CardCode))
                {
                    businessPartner.SalesPersonCode = businessPartnerModel.SlpCode;
                    businessPartner.AgentCode = businessPartnerModel.AgentCode;

                    businessPartner.Update();

                    AddOn.Instance.ConnectionController.VerifyBussinesObjectSuccess();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(businessPartner);
                GC.Collect();
            }
        }
    }
}
