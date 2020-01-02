using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class ItemDI
    {
        Items _businessObject = null;

        bool _newObject = false;

        public void InitializeObject(string itemCode)
        {
            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);            

            if (!_businessObject.GetByKey(itemCode))
            {
                _newObject = true;            
                _businessObject.ItemCode = itemCode;
            }
        }

        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);

            _businessObject = null;

            GC.Collect();
        }

        public void SetItemName(string value)
        {
            _businessObject.ItemName = value;
        }

        public void SetCardCode(string value)
        {
            _businessObject.Mainsupplier = value;
        }

        public void SetSuppCatNum(string value)
        {
            _businessObject.SupplierCatalogNo = value;
        }

        public void SetInvntItem(bool value)
        {
            _businessObject.InventoryItem = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetSellItem(bool value)
        {
            _businessObject.SalesItem = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetPrchseItem(bool value)
        {
            _businessObject.PurchaseItem = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetIsPhantom(bool value)
        {
            _businessObject.IsPhantom = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetNoDiscount(bool value)
        {
            _businessObject.NoDiscounts = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetByWh(bool value)
        {
            if (new CompanyDAO().MultiBranch == false)
                _businessObject.ManageStockByWarehouse = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetInCostRoll(bool value)
        {
            _businessObject.InCostRollup = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetReorderQty(double value)
        {
            _businessObject.DesiredInventory = value;
        }

        public void SetMinLevel(double value)
        {
            _businessObject.MinInventory = value;
        }

        public void SetMaxLevel(double value)
        {
            _businessObject.MaxInventory = value;
        }

        public void SetGLMethod(BoGLMethods value)
        {
            _businessObject.GLMethod = value;
        }

        public void SetIssuePriBy(int value, BoManageMethod manageMethod)
        {
            if (value > 0)
            {
                _businessObject.IssuePrimarilyBy = (IssuePrimarilyByEnum)value - 1;
                _businessObject.ManageSerialNumbers = value == 1 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                _businessObject.ManageBatchNumbers = value == 2 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

                _businessObject.SRIAndBatchManageMethod = manageMethod;
            }
        }

        public void SetMatType(BoMaterialTypes value)
        {
            _businessObject.MaterialType = value;
        }

        public void SetMatGrp(int value)
        {
            if (value > 0)
                _businessObject.MaterialGroup = value;
            else
                _businessObject.MaterialGroup = -1;
        }

        public void SetDNFEntry(int value)
        {
            if (value > 0)
                _businessObject.DNFEntry = value;
            else
                _businessObject.DNFEntry = -1;
        }

        public void SetShipType(int value)
        {
            if (value > 0)
                _businessObject.ShipType = value;
        }

        public void SetItmsGrpCod(int value)
        {
            if (value > 0)
                _businessObject.ItemsGroupCode = value;
            else
                _businessObject.ItemsGroupCode = 100;
        }

        public void SetAtcEntry(int value)
        {
            if (value > 0)
                _businessObject.AttachmentEntry = value;
        }

        public void SetFirmCode(int value)
        {
            if (value > 0)
                _businessObject.Manufacturer = value;
            else
                _businessObject.Manufacturer = -1;
        }

        public void SetInvntryUom(string value)
        {
            _businessObject.InventoryUOM = value;
        }

        public void SetEvalSytem(BoInventorySystem value)
        {
            _businessObject.CostAccountingMethod = value;
        }

        public void SetAvgPrice(double value)
        {
            _businessObject.AvgStdPrice = value;
        }

        public void SetDfltWH(string value)
        {
            if (value != string.Empty)
                _businessObject.DefaultWarehouse = value;
        }

        public void SetBWeight1(double value)
        {
            _businessObject.PurchaseUnitWeight = value;
        }

        public void SetIWeight1(double value)
        {
            _businessObject.InventoryWeight = value;
        }

        public void SetSWeight1(double value)
        {
            _businessObject.SalesUnitWeight = value;
        }

        public void SetBWidth1(double value)
        {
            _businessObject.PurchaseUnitWidth = value;
        }

        public void SetSWidth1(double value)
        {
            _businessObject.SalesUnitWidth = value;
        }

        public void SetBHeight1(double value)
        {
            _businessObject.PurchaseUnitHeight = value;
        }

        public void SetSHeight1(double value)
        {
            _businessObject.SalesUnitHeight = value;
        }

        public void SetBLength1(double value)
        {
            _businessObject.PurchaseUnitLength = value;
        }

        public void SetSLength1(double value)
        {
            _businessObject.SalesUnitLength = value;
        }

        public void SetUgpEntry(int value)
        {
            if (value > 0)
                _businessObject.UoMGroupEntry = value;
            else
                _businessObject.UoMGroupEntry = -1;
        }

        public void SetBuyUnitMsr(string value)
        {
            _businessObject.PurchaseUnit = value;
        }

        public void SetSalUnitMsr(string value)
        {
            _businessObject.SalesUnit = value;
        }



        public void SetToleranDay(int value)
        {
            _businessObject.ToleranceDays = value;
        }

        public void SetLeadTime(int value)
        {
            _businessObject.LeadTime = value;
        }

        public void SetMinOrdrQty(double value)
        {
            _businessObject.MinOrderQuantity = value;
        }

        public void SetOrdrMulti(double value)
        {
            _businessObject.OrderMultiple = value;
        }

        public void SetOrdrIntrvl(int value)
        {
            if (value > 0)
                _businessObject.OrderIntervals = value.ToString();
        }

        public void SetValidFor(bool value)
        {
            _businessObject.Valid = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetFrozenFor(bool value)
        {
            _businessObject.Frozen = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetPlaningSys(BoPlanningSystem value)
        {
            _businessObject.PlanningSystem = value;
        }

        public void SetIssueMthd(BoIssueMethod value)
        {
            _businessObject.IssueMethod = value;
        }

        public void SetPrdStdCst(double value)
        {
            _businessObject.ProdStdCost = value;
        }

        public void SetPicturName(string value)
        {
            if (value != string.Empty)
                _businessObject.Picture = value;
        }

        public void SetNCMCode(int value)
        {
            if (value > 0)
                _businessObject.NCMCode = value;
            else
                _businessObject.NCMCode = -1;
        }

        public void SetCodeBars(string value)
        {
            _businessObject.BarCode = value;
        }

        public void SetSWW(string value)
        {
            _businessObject.SWW = value;
        }

        public void SetUserText(string value)
        {
            _businessObject.User_Text = value;
        }


        public int SetVendorCode(string value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.PreferredVendors.BPCode != string.Empty)
                    _businessObject.PreferredVendors.Add();

                line = _businessObject.PreferredVendors.Count - 1;
            }

            _businessObject.PreferredVendors.SetCurrentLine(line);

            _businessObject.PreferredVendors.BPCode = value == null ? "" : value;

            return line;
        }


        public void SetUserField(string userField, dynamic value)
        {
            _businessObject.UserFields.Fields.Item(userField).Value = value;
        }

        public void Save()
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

        public Items BusinessObject
        {
            get

            {
                return _businessObject;
            }
        }
    }
}
