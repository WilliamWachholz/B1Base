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

                    if (businessPartnerModel.OwnerCode == 0)
                        businessPartner.OwnerCode = -1;
                    else
                        businessPartner.OwnerCode = businessPartnerModel.OwnerCode;                    

                    foreach (KeyValuePair<string, dynamic> userField in businessPartnerModel.UserFields)
                    {
                        businessPartner.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                    }

                    businessPartner.Update();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
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

                    for (int userField = 0; userField < businessPartner.UserFields.Fields.Count; userField++)
                    {
                        businessPartnerModel.UserFields.Add(businessPartner.UserFields.Fields.Item(userField).Name, businessPartner.UserFields.Fields.Item(userField).Value);
                    }
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
