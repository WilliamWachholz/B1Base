using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class ItemDAO
    {
        public Model.ItemModel Get(string itemCode)
        {
            Model.ItemModel itemModel = new Model.ItemModel();

            Items item = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);
            try
            {
                if (item.GetByKey(itemCode))
                {
                    itemModel.ItemCode = item.ItemCode;
                    itemModel.ItemName = item.ItemName;
                    itemModel.SuppCatNum = item.SupplierCatalogNo;
                    itemModel.CardCode = item.Mainsupplier;
                    itemModel.BWeight1 = item.PurchaseUnitWeight;
                    itemModel.SWeight1 = item.SalesUnitWeight;
                    itemModel.IWeight1 = item.InventoryWeight;
                    itemModel.SWW = item.SWW;
                    itemModel.UserText = item.User_Text;

                    for (int userField = 0; userField < item.UserFields.Fields.Count; userField++)
                    {
                        itemModel.UserFields.Add(item.UserFields.Fields.Item(userField).Name, item.UserFields.Fields.Item(userField).Value);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(item);
                GC.Collect();
            }

            return itemModel;
        }

        public void Save(Model.ItemModel itemModel)
        {
            Items item = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);
            try
            {
                if (item.GetByKey(itemModel.ItemCode))
                {                    
                    SetItemFields(item, itemModel);

                    item.Update();
                }
                else
                {
                    item.ItemCode = itemModel.ItemCode;

                    SetItemFields(item, itemModel);

                    item.Add();
                }

                Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
            }
            finally
            {
                Marshal.ReleaseComObject(item);
                GC.Collect();
            }
        }

        public void Save(List<Model.ItemWarehouseModel> itemWarehouseList)
        {
            if (itemWarehouseList.Count() > 0)
            {
                string itemCode = itemWarehouseList.First().ItemCode;

                Items item = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);
                try
                {
                    if (item.GetByKey(itemCode))
                    {
                        int line = 0;
                        
                        for (line = item.WhsInfo.Count - 1; line >= 0; line--)
                        {
                            item.WhsInfo.SetCurrentLine(line);

                            item.WhsInfo.Delete();
                        }

                        line = 0;

                        foreach (Model.ItemWarehouseModel itemWarehouseModel in itemWarehouseList)
                        {
                            if (line > item.WhsInfo.Count - 1)
                                item.WhsInfo.Add();

                            item.WhsInfo.SetCurrentLine(line);
                            item.WhsInfo.WarehouseCode = itemWarehouseModel.WhsCode;
                            item.WhsInfo.Locked = itemWarehouseModel.Locked ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

                            line++;
                        }

                        item.Update();

                        Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(item);
                    GC.Collect();
                }

            }
        }

        public void Save(Model.ItemGroupModel itemGroupModel)
        {
            int groupCode = Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetItemGroupCode", itemGroupModel.GroupName);

            if (groupCode != 0)
            {
                itemGroupModel.GroupCode = groupCode;
                return;
            }

            ItemGroups itemGroup = (ItemGroups)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItemGroups);
            try
            {
                if (itemGroup.GetByKey(itemGroupModel.GroupCode))
                {
                    itemGroup.GroupName = itemGroupModel.GroupName;

                    foreach (KeyValuePair<string, dynamic> userField in itemGroupModel.UserFields)
                    {
                        itemGroup.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                    }

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();

                    itemGroup.Update();
                }
                else
                {
                    itemGroup.GroupName = itemGroupModel.GroupName;
                    itemGroup.InventoryAccount = "1.01.01.01.01";
                    itemGroup.CostAccount = "1.01.01.01.01";
                    itemGroup.TransfersAccount = "1.01.01.01.01";
                    itemGroup.VarianceAccount = "1.01.01.01.01";
                    itemGroup.PriceDifferencesAccount = "1.01.01.01.01";

                    foreach (KeyValuePair<string, dynamic> userField in itemGroupModel.UserFields)
                    {
                        itemGroup.UserFields.Fields.Item(userField.Key).Value = userField.Value;
                    }

                    itemGroup.Add();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();

                    itemGroupModel.GroupCode = Controller.ConnectionController.Instance.LastObjectCode;
                }
            }
            finally
            {
                GC.Collect();
                Marshal.ReleaseComObject(itemGroup);
            }

        }

        public void Save(string itemCode, string cardCode, string suppCatNum, bool inBPCatalog)
        {
            if (itemCode != string.Empty && cardCode != string.Empty && suppCatNum != string.Empty)
            {
                AlternateCatNum alternateCatNum = (AlternateCatNum)Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oAlternateCatNum);
                try
                {
                    if (!alternateCatNum.GetByKey(itemCode, cardCode, suppCatNum))
                    {
                        alternateCatNum.DisplayBPCatalogNumber = inBPCatalog ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        alternateCatNum.ItemCode = itemCode;
                        alternateCatNum.CardCode = cardCode;
                        alternateCatNum.Substitute = suppCatNum;

                        alternateCatNum.Add();

                        Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(alternateCatNum);
                    GC.Collect();
                }
            }
        }

        public void Save(string itemCode, int priceList, double price)
        {
            Items item = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);
            try
            {
                if (item.GetByKey(itemCode))
                {
                    for (int line = 0; line < item.PriceList.Count; line++)
                    {
                        item.PriceList.SetCurrentLine(line);

                        if (item.PriceList.PriceList == priceList)
                        {
                            item.PriceList.Price = price;
                            break;
                        }
                    }
                    item.Update();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(item);
                GC.Collect();
            }
        }

        public void Delete(Model.ItemModel itemModel)
        {
            Items item = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);
            try
            {
                if (item.GetByKey(itemModel.ItemCode))
                {
                    item.Remove();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(item);
                GC.Collect();
            }
        }

        private void SetItemFields(SAPbobsCOM.Items item, Model.ItemModel itemModel)
        {
            item.ItemName = itemModel.ItemName;

            item.SupplierCatalogNo = itemModel.SuppCatNum;

            if (itemModel.DfltWH != string.Empty)
                item.DefaultWarehouse = itemModel.DfltWH;

            if (itemModel.ItmsGrpCod > 0)
                item.ItemsGroupCode = itemModel.ItmsGrpCod;
            else
                item.ItemsGroupCode = 100;

            if (itemModel.FirmCode > 0)
                item.Manufacturer = itemModel.FirmCode;
            else
                item.Manufacturer = -1;

            if (itemModel.NCMCode > 0)
                item.NCMCode = itemModel.NCMCode;
            else item.NCMCode = -1;

            if (itemModel.DNFEntry > 0)
                item.DNFEntry = itemModel.DNFEntry;
            else
                item.DNFEntry = -1;

            if (itemModel.UgpEntry > 0)
                item.UoMGroupEntry = itemModel.UgpEntry;
            else
                item.UoMGroupEntry = -1;

            if (itemModel.ShipType > 0)
                item.ShipType = itemModel.ShipType;

            if (itemModel.MatGrp > 0)
                item.MaterialGroup = itemModel.MatGrp;
            else
                item.MaterialGroup = -1;

            item.SWW = itemModel.SWW;

            item.InventoryUOM = itemModel.InvntryUom;            
            item.InventoryWeight = itemModel.IWeight1;            
            
            item.AvgStdPrice = itemModel.AvgPrice;

            item.ProdStdCost = itemModel.PrdStdCst;            
            
            item.DesiredInventory = itemModel.ReorderQty;
            item.MinInventory = itemModel.MinLevel;
            item.MaxInventory = itemModel.MaxLevel;                  
            
            if (itemModel.OrdrIntrvl > 0)
                item.OrderIntervals = itemModel.OrdrIntrvl.ToString();
            
            item.OrderMultiple = itemModel.OrdrMulti;
            item.MinOrderQuantity = itemModel.MinOrdrQty;
            item.LeadTime = itemModel.LeadTime;
            item.ToleranceDays = itemModel.ToleranDay;

            if (itemModel.PicturName != string.Empty)
                item.Picture = itemModel.PicturName;

            item.User_Text = itemModel.UserText;

            if (itemModel.AtcEntry > 0)
                item.AttachmentEntry = itemModel.AtcEntry;

            if (itemModel.IssuePriBy > 0)
            {
                item.IssuePrimarilyBy = (IssuePrimarilyByEnum)itemModel.IssuePriBy - 1;
                item.ManageSerialNumbers = itemModel.IssuePriBy == 1 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                item.ManageBatchNumbers = itemModel.IssuePriBy == 2 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

                item.SRIAndBatchManageMethod = (BoManageMethod)itemModel.MngMethod;
            }

            item.MaterialType = (BoMaterialTypes)itemModel.MatType;

            item.NoDiscounts = itemModel.NoDiscount ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

            if (new CompanyDAO().MultiBranch == false)
                item.ManageStockByWarehouse = itemModel.ByWh ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

            item.InventoryItem = itemModel.InvntItem ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            item.SalesItem = itemModel.SellItem ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            item.PurchaseItem = itemModel.PrchseItem ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

            item.InCostRollup = itemModel.InCostRoll ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

            if (itemModel.CardCode != string.Empty)
                item.Mainsupplier = itemModel.CardCode;

            for (int line = 0; line < itemModel.CardCodes.Count; line++)
            {
                if (item.PreferredVendors.Count - 1 < line)
                    item.PreferredVendors.Add();

                item.PreferredVendors.SetCurrentLine(line);
                item.PreferredVendors.BPCode = itemModel.CardCodes[line];
            }
            
            item.Valid = (itemModel.ValidFor || itemModel.FrozenFor == false) ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            item.ValidFrom = itemModel.ValidFrom;
            item.ValidTo = itemModel.ValidTo;
            item.ValidRemarks = itemModel.ValidComm;
            item.Frozen = itemModel.FrozenFor ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            item.FrozenFrom = itemModel.FrozenFrom;
            item.FrozenTo = itemModel.FrozenTo;
            item.FrozenRemarks = itemModel.FrozenComm;            

            item.PurchaseUnit = itemModel.BuyUnitMsr;
            if (itemModel.NumInBuy > 0)
                item.PurchaseItemsPerUnit = itemModel.NumInBuy;
            else
                item.PurchaseItemsPerUnit = 1;

            item.PurchasePackagingUnit = itemModel.PurPackMsr;

            if (itemModel.PurPackUn > 0)
                item.PurchaseQtyPerPackUnit = itemModel.PurPackUn;
            else
                item.PurchaseQtyPerPackUnit = 1; 

            item.PurchaseUnitLength = itemModel.BLength1;
            item.PurchaseUnitWidth = itemModel.BWidth1;
            item.PurchaseUnitHeight = itemModel.BHeigth1;
            item.PurchaseUnitVolume = itemModel.BVolume1;

            if (itemModel.BVolUnit > 0)
                item.PurchaseVolumeUnit = itemModel.BVolUnit;
            else
                item.PurchaseVolumeUnit = 4;

            item.PurchaseUnitWeight = itemModel.BWeight1;

            if (itemModel.PurFactor1 > 0)
                item.PurchaseFactor1 = itemModel.PurFactor1;
            else
                item.PurchaseFactor1 = 1;

            if (itemModel.PurFactor2 > 0)
                item.PurchaseFactor2 = itemModel.PurFactor2;
            else
                item.PurchaseFactor2 = 1;

            if (itemModel.PurFactor3 > 0)
                item.PurchaseFactor3 = itemModel.PurFactor3;
            else
                item.PurchaseFactor3 = 1;

            if (itemModel.PurFactor4 > 0)
                item.PurchaseFactor4 = itemModel.PurFactor4;
            else
                item.PurchaseFactor4 = 1;

            item.SalesUnit = itemModel.SalUnitMsr;
            if (itemModel.NumInSale > 0)
                item.SalesItemsPerUnit = itemModel.NumInSale;
            else
                item.SalesItemsPerUnit = 1;

            item.SalesPackagingUnit = itemModel.PurPackMsr;

            if (itemModel.PurPackUn > 0)
                item.SalesQtyPerPackUnit = itemModel.PurPackUn;
            else
                item.SalesQtyPerPackUnit = 1;

            item.SalesUnitLength = itemModel.SLength1;
            item.SalesUnitWidth = itemModel.SWidth1;
            item.SalesUnitHeight = itemModel.SHeigth1;
            item.SalesUnitVolume = itemModel.SVolume1;

            if (itemModel.SVolUnit > 0)
                item.SalesVolumeUnit = itemModel.SVolUnit;
            else
                item.SalesVolumeUnit = 4;

            item.SalesUnitWeight = itemModel.SWeight1;

            if (itemModel.SalFactor1 > 0)
                item.SalesFactor1 = itemModel.SalFactor1;
            else
                item.SalesFactor1 = 1;

            if (itemModel.SalFactor2 > 0)
                item.SalesFactor2 = itemModel.SalFactor2;
            else
                item.SalesFactor2 = 1;

            if (itemModel.SalFactor3 > 0)
                item.SalesFactor3 = itemModel.SalFactor3;
            else
                item.SalesFactor3 = 1;

            if (itemModel.SalFactor4 > 0)
                item.SalesFactor4 = itemModel.SalFactor4;
            else
                item.SalesFactor4 = 1;


            switch (itemModel.GLMethod)
            {
                case "W":
                    item.GLMethod = BoGLMethods.glm_WH;
                    break;
                case "C":
                    item.GLMethod = BoGLMethods.glm_ItemClass;
                    break;
                case "L":
                    item.GLMethod = BoGLMethods.glm_ItemLevel;
                    break;
            }

            switch (itemModel.EvalSystem)
            {
                case "F":
                    item.CostAccountingMethod = BoInventorySystem.bis_FIFO;
                    break;
                case "A":
                    item.CostAccountingMethod = BoInventorySystem.bis_MovingAverage;
                    break;
                case "S":
                    item.CostAccountingMethod = BoInventorySystem.bis_Standard;
                    break;
            }

            switch (itemModel.PlaningSys)
            {
                case "N":
                    item.PlanningSystem = BoPlanningSystem.bop_None;
                    break;
                case "M":
                    item.PlanningSystem = BoPlanningSystem.bop_MRP;
                    break;
            }

            switch (itemModel.IssueMthd)
            {
                case "B":
                    item.IssueMethod = BoIssueMethod.im_Backflush;
                    break;
                case "M":
                    item.IssueMethod = BoIssueMethod.im_Manual;
                    break;
            }

            
            switch (itemModel.PrcrmntMtd)
            {
                case "B":
                    item.ProcurementMethod = BoProcurementMethod.bom_Buy;
                    break;
                case "M":
                    item.ProcurementMethod = BoProcurementMethod.bom_Make;
                    break;
            }

            if (itemModel.IsPhantom)
            {
                item.InventoryItem = BoYesNoEnum.tNO;
                item.IssueMethod = BoIssueMethod.im_Backflush;
                item.IsPhantom = BoYesNoEnum.tYES;
            }
            else
            {
                item.IsPhantom = BoYesNoEnum.tNO;
            }

            item.set_Properties(1, itemModel.QryGroup1 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(2, itemModel.QryGroup2 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(3, itemModel.QryGroup3 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(4, itemModel.QryGroup4 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(5, itemModel.QryGroup5 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(6, itemModel.QryGroup6 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(7, itemModel.QryGroup7 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(8, itemModel.QryGroup8 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(9, itemModel.QryGroup9 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(10, itemModel.QryGroup10 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(11, itemModel.QryGroup11 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(12, itemModel.QryGroup12 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(13, itemModel.QryGroup13 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(14, itemModel.QryGroup14 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(15, itemModel.QryGroup15 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(16, itemModel.QryGroup16 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(17, itemModel.QryGroup17 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(18, itemModel.QryGroup18 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(19, itemModel.QryGroup19 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(20, itemModel.QryGroup20 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(21, itemModel.QryGroup21 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(22, itemModel.QryGroup2 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(23, itemModel.QryGroup23 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(24, itemModel.QryGroup24 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(25, itemModel.QryGroup25 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(26, itemModel.QryGroup26 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(27, itemModel.QryGroup27 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(28, itemModel.QryGroup28 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(29, itemModel.QryGroup29 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(30, itemModel.QryGroup30 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(31, itemModel.QryGroup31 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(32, itemModel.QryGroup32 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(33, itemModel.QryGroup33 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(34, itemModel.QryGroup34 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(35, itemModel.QryGroup35 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(36, itemModel.QryGroup36 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(37, itemModel.QryGroup37 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(38, itemModel.QryGroup38 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(39, itemModel.QryGroup39 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(40, itemModel.QryGroup40 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(41, itemModel.QryGroup41 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(42, itemModel.QryGroup42 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(43, itemModel.QryGroup43 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(44, itemModel.QryGroup44 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(45, itemModel.QryGroup45 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(46, itemModel.QryGroup46 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(47, itemModel.QryGroup47 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(48, itemModel.QryGroup48 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(49, itemModel.QryGroup49 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(50, itemModel.QryGroup50 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(51, itemModel.QryGroup51 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(52, itemModel.QryGroup52 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(53, itemModel.QryGroup53 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(54, itemModel.QryGroup54 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(55, itemModel.QryGroup55 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(56, itemModel.QryGroup56 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(57, itemModel.QryGroup57 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(58, itemModel.QryGroup58 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(59, itemModel.QryGroup59 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(60, itemModel.QryGroup60 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(61, itemModel.QryGroup61 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(62, itemModel.QryGroup62 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(63, itemModel.QryGroup63 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            item.set_Properties(64, itemModel.QryGroup64 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);

            //if (itemModel.B1SYS_SPEDTipi != string.Empty)
            //    item.UserFields.Fields.Item("U_B1SYS_SPEDTipi").Value = itemModel.B1SYS_SPEDTipi;

            foreach (KeyValuePair<string, dynamic> userField in itemModel.UserFields)
            {
                item.UserFields.Fields.Item(userField.Key).Value = userField.Value;
            }
        }
    }
}
