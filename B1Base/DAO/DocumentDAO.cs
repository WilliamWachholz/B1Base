using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class DocumentDAO
    {
        public void Save(List<Model.DocumentInstallmentModel> documentInstallmentList)
        {
            if (documentInstallmentList.Count() > 0)
            {
                int docEntry = documentInstallmentList.First().DocEntry;
                Model.EnumObjType objType = documentInstallmentList.First().ObjType;

                Documents document = GetDIObject(objType);
                try
                {
                    if (document.GetByKey(docEntry))
                    {
                        int line = 0;

                        for (line = document.Installments.Count - 1; line >= 0; line--)
                        {
                            document.Installments.SetCurrentLine(line);

                            document.Installments.Delete();
                        }

                        line = 0;

                        foreach (Model.DocumentInstallmentModel documentInstallmentModel in documentInstallmentList)
                        {
                            if (line > document.Installments.Count - 1)
                                document.Installments.Add();

                            document.Installments.SetCurrentLine(line);
                            document.Installments.DueDate = documentInstallmentModel.DueDate;
                            document.Installments.Percentage = documentInstallmentModel.InstPrcnt;
                            document.Installments.Total = documentInstallmentModel.InsTotal;

                            foreach (KeyValuePair<string, dynamic> userField in documentInstallmentModel.UserFields)
                            {
                                document.Installments.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                            }

                            line++;
                        }

                        document.Update();

                        Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(document);
                    GC.Collect();
                }
            }
        }

        private Documents GetDIObject(Model.EnumObjType objType)
        {
            switch (objType)
            {
                case Model.EnumObjType.Invoice:
                    return (Documents) Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oInvoices);
                default:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oInvoices);                    
            }            
        }
    }
}
