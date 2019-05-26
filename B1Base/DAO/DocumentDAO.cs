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
        public void Save(Model.DocumentModel documentModel)
        {
            Documents document = GetDIObject(documentModel.ObjType);
            try
            {
                int line = 0;

                if (document.GetByKey(documentModel.DocEntry))
                {
                    for (line = document.Lines.Count - 1; line >= 0; line--)
                    {
                        document.Lines.SetCurrentLine(line);

                        document.Lines.Delete();
                    }

                    line = 0;

                    foreach (Model.DocumentItemModel documentItemModel in documentModel.DocumentItemList)
                    {
                        if (line > document.Lines.Count - 1)
                            document.Lines.Add();

                        document.Lines.SetCurrentLine(line);

                        document.Lines.ItemCode = documentItemModel.ItemCode;
                        document.Lines.Quantity = documentItemModel.Quantity;
                        document.Lines.Price = documentItemModel.Price;

                        foreach (KeyValuePair<string, dynamic> userField in documentItemModel.UserFields)
                        {
                            document.Lines.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                        }

                        line++;
                    }

                    foreach (KeyValuePair<string, dynamic> userField in documentModel.UserFields)
                    {
                        document.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                    }

                    document.Update();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                }
                else
                {
                    document.CardCode = documentModel.CardCode;
                    document.DocDate = documentModel.DocDate;

                    line = 0;

                    foreach (Model.DocumentItemModel documentItemModel in documentModel.DocumentItemList)
                    {
                        if (line > document.Lines.Count - 1)
                            document.Lines.Add();

                        document.Lines.SetCurrentLine(line);

                        document.Lines.ItemCode = documentItemModel.ItemCode;
                        document.Lines.Quantity = documentItemModel.Quantity;
                        document.Lines.Price = documentItemModel.Price;

                        foreach (KeyValuePair<string, dynamic> userField in documentItemModel.UserFields)
                        {
                            document.Lines.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                        }

                        line++;
                    }


                    foreach (KeyValuePair<string, dynamic> userField in documentModel.UserFields)
                    {
                        document.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                    }

                    document.Add();

                    documentModel.DocEntry = Controller.ConnectionController.Instance.LastObjectCode;

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(document);
                GC.Collect();
            }
        }

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

        public void SetUF(Model.DocumentModel documentModel)
        {
            Documents document = GetDIObject(documentModel.ObjType);
            try
            {
                if (document.GetByKey(documentModel.DocEntry))
                {
                    int line = 0;

                    foreach (Model.DocumentItemModel documentItemModel in documentModel.DocumentItemList)
                    {
                        document.Lines.SetCurrentLine(line);

                        foreach (KeyValuePair<string, dynamic> userField in documentItemModel.UserFields)
                        {
                            document.Lines.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                        }

                        line++;
                    }

                    foreach (KeyValuePair<string, dynamic> userField in documentModel.UserFields)
                    {
                        document.UserFields.Fields.Item(userField.Key).Value = userField.Value;
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

        public void SetUFItem(int docEntry, Model.EnumObjType objType, Dictionary<int, Dictionary<string, dynamic>> itemUserFields)
        {
            Documents document = GetDIObject(objType);
            try
            {
                if (document.GetByKey(docEntry))
                {
                    foreach (KeyValuePair<int, Dictionary<string, dynamic>> item in itemUserFields)
                    {
                        document.Lines.SetCurrentLine(item.Key);

                        foreach (KeyValuePair<string, dynamic> userField in item.Value)
                        {
                            document.Lines.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                        }
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

        public Model.DocumentModel Get(int docEntry, Model.EnumObjType objType)
        {
            Model.DocumentModel documentModel = new Model.DocumentModel();

            documentModel.DocEntry = docEntry;
            documentModel.ObjType = objType;

            Documents document = GetDIObject(documentModel.ObjType);
            try
            {                
                if (document.GetByKey(documentModel.DocEntry))
                {
                    documentModel.CardCode = document.CardCode;
                    documentModel.DocDate = document.DocDate;
                    documentModel.SlpCode = document.SalesPersonCode;

                    int line = 0;

                    for (line = document.Lines.Count - 1; line >= 0; line--)
                    {
                        document.Lines.SetCurrentLine(line);

                        Model.DocumentItemModel documentItemModel = new Model.DocumentItemModel();
                        documentItemModel.ItemCode = document.Lines.ItemCode;
                        documentItemModel.Price = document.Lines.Price;
                        documentItemModel.Quantity = document.Lines.Quantity;

                        for (int userField = 0; userField < document.Lines.UserFields.Fields.Count; userField++)
                        {
                            documentItemModel.UserFields.Add(document.Lines.UserFields.Fields.Item(userField).Name, document.Lines.UserFields.Fields.Item(userField).Value);
                        }

                        documentModel.DocumentItemList.Add(documentItemModel);
                    }

                    for (int userField = 0; userField < document.UserFields.Fields.Count; userField++)
                    {
                        documentModel.UserFields.Add(document.UserFields.Fields.Item(userField).Name, document.UserFields.Fields.Item(userField).Value);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(document);
                GC.Collect();
            }
            return documentModel;
        }

        private Documents GetDIObject(Model.EnumObjType objType)
        {
            switch (objType)
            {
                case Model.EnumObjType.Invoice:
                    return (Documents) Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oInvoices);
                case Model.EnumObjType.PurchaseOrder:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oPurchaseOrders);
                default:
                    return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oInvoices);                    
            }            
        }
    }
}
