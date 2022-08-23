using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class DocumentDI
    {
        Documents _businessObject = null;
        Documents _businessObjectDraft = null;

        bool _newObject = false;

        bool _draft = false;

        public void InitializeObject(int docEntry, Model.EnumObjType objType, bool draft = false)
        {
            _draft = draft;

            _businessObject = GetDIObject(objType);

            _businessObjectDraft = GetDIObject(Model.EnumObjType.Draft);
            _businessObjectDraft.DocObjectCode = GetObjType(objType);
            _businessObjectDraft.GetByKey(docEntry);

            if (!draft)
            {
                if (!_businessObject.GetByKey(docEntry))
                {
                    _newObject = true;
                }
            }
        }

        public void InitializeObject(int docEntry, Model.EnumObjType objType, int baseDocEntry, Model.EnumObjType baseObjType)
        {
            string xml = string.Empty;

            DocumentDI documentBaseDI = new DocumentDI();
            documentBaseDI.InitializeObject(baseDocEntry, baseObjType);
            try
            {
                xml = documentBaseDI.GetXmlForDI();
            }
            finally
            {
                documentBaseDI.FinalizeObject();
            }

            for (int lineNum = 0; lineNum < 100; lineNum++)
            {
                string replaceString = string.Format(@"<LineNum>{0}</LineNum>", lineNum);

                if (xml.IndexOf(replaceString) == -1)
                    break;
                else
                {
                    string newString = string.Format(@"<LineNum>{0}</LineNum>				    
				    <BaseType>{1}</BaseType>
				    <BaseEntry>{2}</BaseEntry>
				    <BaseLine>{3}</BaseLine>", lineNum, (int)objType, baseDocEntry, lineNum);

                    xml = xml.Replace(replaceString, newString);
                }
            }

            xml = xml.Replace("<Object>" + ((int) baseObjType).ToString() + "</Object>", "<Object>" + ((int)objType).ToString() + "</Object>");

            xml = xml.Replace("<DocEntry>" + baseDocEntry.ToString() + "</DocEntry>", "");
            
            _businessObject = GetDIObject(objType);

            if (!_businessObject.GetByKey(docEntry))
            {
                _newObject = true;
            }

            _businessObject.UpdateFromXML(xml);

            if (_newObject)
                _businessObject.DocNum = 0;

            _businessObjectDraft = GetDIObject(Model.EnumObjType.Draft);
            _businessObjectDraft.DocObjectCode = GetObjType(objType);
        }

        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);

            _businessObject = null;

            GC.Collect();

            Marshal.ReleaseComObject(_businessObjectDraft);

            _businessObjectDraft = null;

            GC.Collect();
        }

        public void CopyFrom(int baseDocEntry, Model.EnumObjType objType)
        {
            string xml = string.Empty;

            DocumentDI documentBaseDI = new DocumentDI();
            documentBaseDI.InitializeObject(baseDocEntry, objType);
            try
            {
                xml = documentBaseDI.GetXmlForDI();
            }
            finally
            {
                documentBaseDI.FinalizeObject();
            }

            for (int lineNum = 0; lineNum < 100; lineNum++)
            {
                string replaceString = string.Format(@"<LineNum>{0}</LineNum>", lineNum);

                if (xml.IndexOf(replaceString) == -1)
                    break;
                else
                {
                    string newString = string.Format(@"<LineNum>{0}</LineNum>				    
				    <BaseType>{1}</BaseType>
				    <BaseEntry>{2}</BaseEntry>
				    <BaseLine>{3}</BaseLine>", lineNum, (int)objType, baseDocEntry, lineNum);

                    xml = xml.Replace(replaceString, newString);
                }
            }

            _businessObject.UpdateFromXML(xml);

            _businessObjectDraft = GetDIObject(Model.EnumObjType.Draft);
            _businessObjectDraft.DocObjectCode = GetObjType(objType);
        }

        public void SetShipToCode(string value)
        {
            _businessObject.ShipToCode = value == null ? "" : value;

            _businessObjectDraft.ShipToCode = value == null ? "" : value;
        }

        public void SetPayToCode(string value)
        {
            _businessObject.PayToCode = value == null ? "" : value;

            _businessObjectDraft.PayToCode = value == null ? "" : value;
        }

        public void SetBplId(int value)
        {
            _businessObject.BPL_IDAssignedToInvoice = value;
            _businessObjectDraft.BPL_IDAssignedToInvoice = value;
        }

        public void SetCardCode(string value)
        {
            _businessObject.CardCode = value == null ? "" : value;

            _businessObjectDraft.CardCode = value == null ? "" : value;
        }

        public void SetDocDate(DateTime value)
        {
            _businessObject.DocDate = value;

            _businessObjectDraft.DocDate = value;
        }

        public void SetTaxDate(DateTime value)
        {
            _businessObject.TaxDate = value;

            _businessObjectDraft.TaxDate = value;
        }

        public void SetDocDueDate(DateTime value)
        {
            _businessObject.DocDueDate = value;

            _businessObjectDraft.DocDueDate = value;
        }

        public void SetDocCur(string value)
        {
            _businessObject.DocCurrency = value == null ? "" : value;

            _businessObjectDraft.DocCurrency = value == null ? "" : value;
        }

        public void SetDocRate(double value)
        {
            _businessObject.DocRate = value;

            _businessObjectDraft.DocRate = value;
        }

        public void SetTrnspCode(int value)
        {
            _businessObject.TransportationCode = value;

            _businessObjectDraft.TransportationCode = value;
        }

        public void SetTrackNo(string value)
        {
            _businessObject.TrackingNumber = value == null ? "" : value;

            _businessObjectDraft.TrackingNumber = value == null ? "" : value;
        }

        public void SetOwnerCode(int value)
        {
            _businessObject.DocumentsOwner = value;

            _businessObjectDraft.DocumentsOwner = value;
        }

        public void SetGroupNum(int value)
        {
            _businessObject.GroupNumber = value;

            _businessObjectDraft.GroupNumber = value;
        }

        public void SetPeyMethod(string value)
        {
            _businessObject.PaymentMethod = value == null ? "" : value;

            _businessObjectDraft.PaymentMethod = value == null ? "" : value;
        }

        public void SetComments(string value)
        {
            _businessObject.Comments = value == null ? "" : value;

            _businessObjectDraft.Comments = value == null ? "" : value;
        }

        public void SetUserField(string key, dynamic value)
        {
            _businessObject.UserFields.Fields.Item(key).Value = value;

            _businessObjectDraft.UserFields.Fields.Item(key).Value = value;
        }

        public int SetItemCode(string value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Lines.ItemCode != string.Empty)
                {
                    _businessObject.Lines.Add();
                    _businessObjectDraft.Lines.Add();
                }

                line = _businessObject.Lines.Count - 1;
            }
            else if (_businessObject.Lines.Count <= line)
            {
                _businessObject.Lines.Add();
                _businessObjectDraft.Lines.Add();
            }

            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.ItemCode = value == null ? "" : value;

            _businessObjectDraft.Lines.SetCurrentLine(line);

            _businessObjectDraft.Lines.ItemCode = value == null ? "" : value;

            return line;
        }

        public void SetItemQuantity(double value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.Quantity = value;

            _businessObjectDraft.Lines.SetCurrentLine(line);

            _businessObjectDraft.Lines.Quantity = value;
        }

        public void SetItemPrice(double value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.UnitPrice = value;

            _businessObjectDraft.Lines.SetCurrentLine(line);

            _businessObjectDraft.Lines.UnitPrice = value;
        }

        public void SetItemDiscPrcnt(double value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.DiscountPercent = value;

            _businessObjectDraft.Lines.SetCurrentLine(line);

            _businessObjectDraft.Lines.DiscountPercent = value;
        }

        public void SetItemUsage(string value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.Usage = value == null ? "" : value;

            _businessObjectDraft.Lines.SetCurrentLine(line);

            _businessObjectDraft.Lines.Usage = value == null ? "" : value;
        }

        public void SetItemWarehouse(string value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.WarehouseCode = value == null ? "" : value;

            _businessObjectDraft.Lines.SetCurrentLine(line);

            _businessObjectDraft.Lines.WarehouseCode = value == null ? "" : value;
        }

        public void SetItemTaxCode(string value, int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            _businessObject.Lines.TaxCode = value == null ? "" : value;

            _businessObjectDraft.Lines.SetCurrentLine(line);

            _businessObjectDraft.Lines.TaxCode = value == null ? "" : value;
        }

        public void SetItemBatchNumber(int line, string batchNumber, double quantity)
        {
            _businessObject.Lines.SetCurrentLine(line);

            if (_businessObject.Lines.BatchNumbers.Count > 1 || _businessObject.Lines.BatchNumbers.BatchNumber != string.Empty)
            {
                _businessObject.Lines.BatchNumbers.Add();
                _businessObjectDraft.Lines.BatchNumbers.Add();
            }

            _businessObject.Lines.BatchNumbers.BatchNumber = batchNumber;
            _businessObject.Lines.BatchNumbers.Quantity = quantity;

            _businessObjectDraft.Lines.BatchNumbers.BatchNumber = batchNumber;
            _businessObjectDraft.Lines.BatchNumbers.Quantity = quantity;
        }

        public int SetExpenseCode(int value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Expenses.ExpenseCode != 0)
                {
                    _businessObject.Expenses.Add();
                    _businessObjectDraft.Expenses.Add();
                }

                line = _businessObject.Expenses.Count - 1;
            }

            _businessObject.Expenses.SetCurrentLine(line);

            _businessObject.Expenses.ExpenseCode = value;

            _businessObjectDraft.Expenses.SetCurrentLine(line);

            _businessObjectDraft.Expenses.ExpenseCode = value;

            return line;
        }

        public void SetExpenseTotal(double value, int line)
        {            
            _businessObject.Expenses.SetCurrentLine(line);

            _businessObject.Expenses.LineTotal = value;

            _businessObjectDraft.Expenses.SetCurrentLine(line);

            _businessObjectDraft.Expenses.LineTotal = value;
        }

        public void SetExpenseDistribuitionMethod(BoAdEpnsDistribMethods value, int line)
        {
            _businessObject.Expenses.SetCurrentLine(line);

            _businessObject.Expenses.DistributionMethod = value;

            _businessObjectDraft.Expenses.SetCurrentLine(line);

            _businessObjectDraft.Expenses.DistributionMethod = value;
        }

        public int SetInstallmentDate(DateTime value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Installments.Count > 1)
                {
                    _businessObject.Installments.Add();
                    _businessObjectDraft.Installments.Add();
                }

                line = _businessObject.Installments.Count - 1;
            }

            _businessObject.Installments.SetCurrentLine(line);

            _businessObject.Installments.DueDate = value;

            _businessObjectDraft.Installments.SetCurrentLine(line);

            _businessObjectDraft.Installments.DueDate = value;

            return line;
        }

        public void SetInstallmentTotal(double value, int line)
        {
            _businessObject.Installments.SetCurrentLine(line);

            _businessObject.Installments.Total = value;

            _businessObjectDraft.Installments.SetCurrentLine(line);

            _businessObjectDraft.Installments.Total = value;
        }

        public void SetInstallmentPercentage(double value, int line)
        {
            _businessObject.Installments.SetCurrentLine(line);

            _businessObject.Installments.Percentage = value;

            _businessObjectDraft.Installments.SetCurrentLine(line);

            _businessObjectDraft.Installments.Percentage = value;
        }

        public void SetIncoterms(string value)
        {
            _businessObject.TaxExtension.Incoterms = value;
            _businessObjectDraft.TaxExtension.Incoterms = value;
        }

        public void SetCarrier(string value)
        {
            _businessObject.TaxExtension.Carrier = value;
            _businessObjectDraft.TaxExtension.Carrier = value;
        }

        public void AutoSelectItemBatchSerial(int line)
        {
            _businessObject.Lines.SetCurrentLine(line);

            bool manageSerialNumber = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForObject<bool>("GetItemManageSerialNumber", _businessObject.Lines.ItemCode);
            bool manageBatchNumber = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForObject<bool>("GetItemManageBatchNumber", _businessObject.Lines.ItemCode);

            if (manageSerialNumber)
            {

            }
            else if (manageBatchNumber)
            {
                List<Model.BatchModel> batchList = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForList<Model.BatchModel>("GetListAvailableBatch", _businessObject.Lines.ItemCode, _businessObject.Lines.WarehouseCode);

                foreach (Model.BatchModel batchModel in batchList)
                {
                    if (batchModel.Quantity > _businessObject.Lines.InventoryQuantity)
                    {

                    }
                }
            }
        }

        public void Save()
        {
            _businessObject.UserFields.Fields.Item("U_DIUpdate").Value = "Y";

            if (_draft)
            {
                SaveAsDraft();
                return;
            }

            try
            {
                if (_newObject)
                {
                    _businessObject.Add();
                }
                else
                {
                    _businessObject.Update();
                }

                Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
            }
            catch (Exception ex)
            {
                throw ex;
                //guarda em tabela de banco. Criar uma tabela no banco para cada addon para guardar erro select (sql) e objetos DI (xml)
            }

            if (_newObject)
                _businessObject.GetByKey(Controller.ConnectionController.Instance.LastObjectCode);
        }

        public void SaveAsDraft()
        {
            try
            {
                if (_businessObjectDraft.DocEntry > 0)
                {
                    _businessObjectDraft.Update();
                }
                else
                {
                    _businessObjectDraft.Add();
                }

                Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
            }
            catch (Exception ex)
            {
                throw ex;
                //guarda em tabela de banco. Criar uma tabela no banco para cada addon para guardar erro select (sql) e objetos DI (xml)
            }
        }

        public string GetXmlForDI()
        {
            B1Base.Controller.ConnectionController.Instance.Company.XmlExportType = SAPbobsCOM.BoXmlExportTypes.xet_ExportImportMode;

            return _businessObject.GetAsXML();
        }

        private Documents GetDIObject(Model.EnumObjType objType)
        {
            return (Documents)Controller.ConnectionController.Instance.Company.GetBusinessObject(GetObjType(objType));
        }

        private BoObjectTypes GetObjType(Model.EnumObjType objType)
        {
            switch (objType)
            {
                case Model.EnumObjType.Draft:
                    return BoObjectTypes.oDrafts;
                case Model.EnumObjType.Invoice:
                    return BoObjectTypes.oInvoices;
                case Model.EnumObjType.SalesOrder:
                    return BoObjectTypes.oOrders;
                case Model.EnumObjType.InvoiceReturn:
                    return BoObjectTypes.oCreditNotes;
                case Model.EnumObjType.PurchaseOrder:
                    return BoObjectTypes.oPurchaseOrders;
                case Model.EnumObjType.PurchaseInvoice:
                    return BoObjectTypes.oPurchaseInvoices;
                default:
                    return BoObjectTypes.oInvoices;
            }
        }

        public Documents BusinessObject
        {
            get
            {
                return _businessObject;
            }
        }
    }
}
