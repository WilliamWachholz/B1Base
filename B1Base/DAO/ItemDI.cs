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

        ItemsBean _bean = null;

        bool _newObject = false;

        public void InitializeObject(string itemCode)
        {
            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);

            _bean = new ItemsBean();

            if (!_businessObject.GetByKey(itemCode))
            {
                _newObject = true;            
                _businessObject.ItemCode = itemCode;
            }
        }

        public void InitializeObject(string itemCode, string baseItemCode)
        {
            string xml = string.Empty;

            B1Base.DAO.ItemDI itemBaseDI = new B1Base.DAO.ItemDI();
            itemBaseDI.InitializeObject(baseItemCode);
            try
            {
                xml = itemBaseDI.GetXmlForDI();
            }
            finally
            {
                itemBaseDI.FinalizeObject();
            }

            xml = xml.Replace(baseItemCode, itemCode);

            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);

            _bean = new ItemsBean();

            if (!_businessObject.GetByKey(itemCode))
            {
                _newObject = true;
                _businessObject.ItemCode = itemCode;
            }

            _businessObject.UpdateFromXML(xml);

            _businessObject.Series = 3;
        }

        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);

            _businessObject = null;

            GC.Collect();
        }

        public void CopyFrom(string baseItemCode)
        {
            string xml = string.Empty;

            B1Base.DAO.ItemDI itemBaseDI = new B1Base.DAO.ItemDI();
            itemBaseDI.InitializeObject(baseItemCode);
            try
            {
                xml = itemBaseDI.GetXmlForDI();
            }
            finally
            {
                itemBaseDI.FinalizeObject();
            }

            xml = xml.Replace(baseItemCode, _businessObject.ItemCode);            

            _businessObject.UpdateFromXML(xml);

            _businessObject.Series = 3;

        }

        public void SetItemName(string value)
        {
            _businessObject.ItemName = value;
        }

        public void SetFrgnName(string value)
        {
            _businessObject.ForeignName = value;
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
               
                _businessObject.InventoryItem = BoYesNoEnum.tYES;
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

        bool _updateBWeight1 = false;

        public void SetBWeight1(double value)
        {
            _businessObject.PurchaseUnitWeight = value;

            _updateBWeight1 = true;
        }

        public void SetIWeight1(double value)
        {
            _businessObject.InventoryWeight = value;
        }

        bool _updateSWeight1 = false;

        public void SetSWeight1(double value)
        {
            _businessObject.SalesUnitWeight = value;

            _updateSWeight1 = true;
        }

        bool _updateBWidth1 = false;

        public void SetBWidth1(double value)
        {
            _businessObject.PurchaseUnitWidth = value;

            _updateBWidth1 = true;
        }

        bool _updateSWidth1 = false;

        public void SetSWidth1(double value)
        {
            _businessObject.SalesUnitWidth = value;

            _updateSWidth1 = true;
        }

        bool _updateBHeight1 = false;

        public void SetBHeight1(double value)
        {
            _businessObject.PurchaseUnitHeight = value;

            _updateBHeight1 = true;
        }

        bool _updateSHeight1 = false;

        public void SetSHeight1(double value)
        {
            _businessObject.SalesUnitHeight = value;

            _updateSHeight1 = true;
        }

        bool _updateBLength1 = false;

        public void SetBLength1(double value)
        {            
            _businessObject.PurchaseUnitLength = value;

            _updateBLength1 = true;
        }

        bool _updateSLength1 = false;

        public void SetSLength1(double value)
        {
            _businessObject.SalesUnitLength = value;

            _updateSLength1 = true;
        }

        public void SetBVolume(double value)
        {
            _businessObject.PurchaseUnitVolume = value;
        }

        public void SetBVolUnit(int value)
        {
            _businessObject.PurchaseVolumeUnit = value == 0 ? 1 : value;
        }

        public void SetSVolume(double value)
        {
            _businessObject.SalesUnitVolume = value;
        }

        public void SetSVolUnit(int value)
        {
            _businessObject.SalesVolumeUnit = value == 0 ? 1 : value;
        }

        public void SetUgpEntry(int value)
        {
            if (value > 0)
                _businessObject.UoMGroupEntry = value;
            else
                _businessObject.UoMGroupEntry = -1;
        }

        public void SetCntUnitMsr(string value)
        {
            if (_businessObject.UoMGroupEntry != -1)
            {
                _businessObject.DefaultCountingUoMEntry = B1Base.Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetUoMEntry", value);
            }
        }

        public void SetCntUnitMsr(int value)
        {
            _businessObject.DefaultCountingUoMEntry = value;            
        }

        public void SetBuyUnitMsr(int value)
        {
            _businessObject.DefaultPurchasingUoMEntry = value;
        }

        public void SetBuyUnitMsr(string value)
        {            
            _businessObject.PurchaseUnit = value;

            if (_businessObject.UoMGroupEntry != -1)
            {
                _businessObject.DefaultPurchasingUoMEntry = B1Base.Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetUoMEntry", value);
            }
        }

        public void SetSalUnitMsr(int value)
        {
            _businessObject.DefaultSalesUoMEntry = value;
        }

        public void SetSalUnitMsr(string value)
        {
            _businessObject.SalesUnit = value;

            if (_businessObject.UoMGroupEntry != -1)
            {
                _businessObject.DefaultSalesUoMEntry = B1Base.Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetUoMEntry", value);
            }
        }

        public void SetPurPackMsr(string value)
        {
            _businessObject.PurchasePackagingUnit = value;
        }

        public void SetSalPackMsr(string value)
        {
            _businessObject.SalesPackagingUnit = value;
        }

        public void SetNumInBuy(double value)
        {
            _businessObject.PurchaseItemsPerUnit = value == 0 ? 1 : value;
        }

        public void SetPurPackUn(double value)
        {
            _businessObject.PurchaseQtyPerPackUnit = value == 0 ? 1 : value;
        }

        public void SetNumInSale(double value)
        {
            _businessObject.SalesItemsPerUnit = value == 0 ? 1 : value;
        }

        public void SetSalPackUn(double value)
        {
            _businessObject.SalesQtyPerPackUnit = value == 0 ? 1 : value;
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

            //bool found = false;

            //if (value != string.Empty)
            //{
            //    for (int i = 0; i < _businessObject.BarCodes.Count; i++)
            //    {
            //        _businessObject.BarCodes.SetCurrentLine(i);

            //        if (_businessObject.BarCodes.BarCode == string.Empty)
            //        {
            //            found = true;
            //            break;
            //        }
            //    }

            //    if (found == false)
            //    {
            //        _businessObject.BarCodes.Add();
            //        _businessObject.BarCodes.SetCurrentLine(_businessObject.BarCodes.Count - 1);
            //    }

            //    _businessObject.BarCodes.BarCode = value;
            //    _businessObject.BarCodes.UoMEntry = 1;
            //}            
        }

        public void SetSWW(string value)
        {
            _businessObject.SWW = value;
        }

        public void SetOSvcCode(string value)
        {
            int code = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForBasicObject<int>("AbsEntry", "OSCD", "ServiceCD", "'" + value + "'", "0");

            if (code > 0)
                _businessObject.OutgoingServiceCode = code;
        }

        public void SetOSvcCode(int value)
        {
            if (value > 0)
                _businessObject.OutgoingServiceCode = value;
        }

        public void SetItemClass(ItemClassEnum value)
        {
            _businessObject.ItemClass = value;
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

        public void SetProductSrc(string value)
        {
            if (value != string.Empty)
            {
                _businessObject.ProductSource = Convert.ToInt32(value);
            }
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

            if (_updateBWeight1 || _updateSWeight1 || _updateBWidth1 || _updateSWidth1 || _updateBHeight1 || _updateSHeight1 || _updateBLength1 || _updateSLength1)
            {
                for (int row = 0; row < _businessObject.UnitOfMeasurements.Count; row++)
                {
                    _businessObject.UnitOfMeasurements.SetCurrentLine(row);

                    if (_businessObject.UnitOfMeasurements.UoMType == ItemUoMTypeEnum.iutPurchasing && _businessObject.UnitOfMeasurements.UoMEntry == _businessObject.DefaultPurchasingUoMEntry)
                    {
                        if (_updateBWeight1)
                        {
                            _businessObject.UnitOfMeasurements.Weight1 = _businessObject.PurchaseUnitWeight;
                            _businessObject.UnitOfMeasurements.Weight1Unit = 3;
                        }

                        if (_updateBWidth1)
                        {
                            _businessObject.UnitOfMeasurements.Width1 = _businessObject.PurchaseUnitWidth;
                            _businessObject.UnitOfMeasurements.Width1Unit = 4;
                        }

                        if (_updateBHeight1)
                        {
                            _businessObject.UnitOfMeasurements.Height1 = _businessObject.PurchaseUnitHeight;
                            _businessObject.UnitOfMeasurements.Height1Unit = 4;
                        }

                        if (_updateBLength1)
                        {
                            _businessObject.UnitOfMeasurements.Length1 = _businessObject.PurchaseUnitLength;
                            _businessObject.UnitOfMeasurements.Length1Unit = 4;
                        }
                    }

                    if (_businessObject.UnitOfMeasurements.UoMType == ItemUoMTypeEnum.iutSales && _businessObject.UnitOfMeasurements.UoMEntry == _businessObject.DefaultSalesUoMEntry)
                    {
                        if (_updateSWeight1)
                        {
                            _businessObject.UnitOfMeasurements.Weight1 = _businessObject.SalesUnitWeight;
                            _businessObject.UnitOfMeasurements.Weight1Unit = 3;
                        }

                        if (_updateSWidth1)
                        {
                            _businessObject.UnitOfMeasurements.Width1 = _businessObject.SalesUnitWidth;
                            _businessObject.UnitOfMeasurements.Width1Unit = 4;
                        }

                        if (_updateSHeight1)
                        {
                            _businessObject.UnitOfMeasurements.Height1 = _businessObject.SalesUnitHeight;
                            _businessObject.UnitOfMeasurements.Height1Unit = 4;
                        }

                        if (_updateSLength1)
                        {
                            _businessObject.UnitOfMeasurements.Length1 = _businessObject.SalesUnitLength;
                            _businessObject.UnitOfMeasurements.Length1Unit = 4;
                        }
                    }
                }

                _businessObject.Update();
            }
        }

        public void Delete()
        {
            if (!_newObject)
            {
                _businessObject.Remove();

                Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
            }
        }

        public void LoadBean()
        {
            _bean.ItemCode = _businessObject.ItemCode;
            _bean.SuppCatNum = _businessObject.SupplierCatalogNo;
            _bean.BWeight1 = _businessObject.PurchaseUnitWeight;
            _bean.IWeight1 = _businessObject.InventoryWeight;
            _bean.SWeight1 = _businessObject.SalesUnitWeight;
            _bean.BWidth1 = _businessObject.PurchaseUnitWidth;
            _bean.SWidth1 = _businessObject.SalesUnitWidth;
            _bean.BHeight1 = _businessObject.PurchaseUnitHeight;
            _bean.SHeight1 = _businessObject.SalesUnitHeight;
            _bean.BLength1 = _businessObject.PurchaseUnitLength;
            _bean.SLength1 = _businessObject.SalesUnitLength;
            _bean.UgpEntry = _businessObject.UoMGroupEntry;
            _bean.BuyUnitMsr = _businessObject.PurchaseUnit;
            _bean.SalUnitMsr = _businessObject.SalesUnit;
            _bean.PlaningSys = _businessObject.PlanningSystem;
            _bean.OrdrIntrvl = _businessObject.OrderIntervals == "" ? 0 : Convert.ToInt32(_businessObject.OrderIntervals);
            _bean.OrdrMulti = _businessObject.OrderMultiple;
            _bean.MinOrdrQty = _businessObject.MinOrderQuantity;
            _bean.LeadTime = _businessObject.LeadTime;
            _bean.ToleranDay = _businessObject.ToleranceDays;
            _bean.NCMCode = _businessObject.NCMCode;
            _bean.CodeBars = _businessObject.BarCode;
            _bean.ValidFor = _businessObject.Valid == BoYesNoEnum.tYES;
            _bean.FrozenFor = _businessObject.Frozen == BoYesNoEnum.tYES;
            _bean.SWW = _businessObject.SWW;
            _bean.CardCode = _businessObject.Mainsupplier;
            _bean.OSvcCode = _businessObject.OutgoingServiceCode;
            _bean.ItemClass = _businessObject.ItemClass;
            _bean.MatType = _businessObject.MaterialType;
            _bean.ProductSrc = _businessObject.ProductSource;
            _bean.IssuePriBy = (int) _businessObject.IssuePrimarilyBy;
            _bean.MngMethod = _businessObject.SRIAndBatchManageMethod;
            _bean.PreferredVendors = _businessObject.PreferredVendors;

            for (int userField = 0; userField < _businessObject.UserFields.Fields.Count; userField++)
            {
                _bean.UserFields.Add(_businessObject.UserFields.Fields.Item(userField).Name, _businessObject.UserFields.Fields.Item(userField).Value);
            }

            _bean.Xml = _businessObject.GetAsXML();
        }

        public string GetXmlForDI()
        {
            B1Base.Controller.ConnectionController.Instance.Company.XmlExportType = SAPbobsCOM.BoXmlExportTypes.xet_ExportImportMode;

            return _businessObject.GetAsXML();
        }

        public Items BusinessObject
        {
            get
            {
                return _businessObject;
            }
        }

        public ItemsBean Bean
        {
            get
            {
                return _bean;
            }
        }

        public bool NewObject
        {
            get
            {
                return _newObject;
            }
        }

        public class ItemsBean
        {
            public string ItemCode { get; set; }

            public string SuppCatNum { get; set; }

            public double BWeight1 { get; set; }

            public double IWeight1 { get; set; }

            public double SWeight1 { get; set; }

            public double BWidth1 { get; set; }

            public double SWidth1 { get; set; }

            public double BHeight1 { get; set; }

            public double SHeight1 { get; set; }

            public double BLength1 { get; set; }

            public double SLength1 { get; set; }

            public int UgpEntry { get; set; }

            public string BuyUnitMsr { get; set; }

            public string SalUnitMsr { get; set; }

            public BoPlanningSystem PlaningSys { get; set; }

            public int OrdrIntrvl { get; set; }

            public double OrdrMulti { get; set; }

            public double MinOrdrQty { get; set; }

            public int LeadTime { get; set; }

            public int ToleranDay { get; set; }

            public int NCMCode { get; set; }

            public string CodeBars { get; set; }

            public bool ValidFor { get; set; }

            public bool FrozenFor { get; set; }

            public string SWW { get; set; }

            public string CardCode { get; set; }

            public int OSvcCode { get; set; }

            public ItemClassEnum ItemClass { get; set; }

            public BoMaterialTypes MatType { get; set; }

            public int ProductSrc { get; set; }

            public int IssuePriBy { get; set; }

            public BoManageMethod MngMethod { get; set; }

            public Dictionary<string, dynamic> UserFields { get; set; }

            public string Xml { get; set; }

            public Items_PreferredVendors PreferredVendors { get; set; }

            public ItemsBean()
            {
                UserFields = new Dictionary<string, dynamic>();
            }
        }
    }
}
