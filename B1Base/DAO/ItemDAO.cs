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
            return new Model.ItemModel();
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

        }

        private void SetItemFields(SAPbobsCOM.Items item, Model.ItemModel itemModel)
        {
            item.ItemName = itemModel.ItemName;
            //item.AttachmentEntry = itemModel.AtcEntry;
            //item.PriceList. = itemModel.ListNum;
            //item.Valid = itemModel.ValidFor ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            //item.ValidFrom = itemModel.ValidFrom;
            //item.ValidTo = itemModel.ValidTo;
            //item.ValidRemarks = itemModel.ValidComm;
            //item.Frozen = itemModel.FrozenFor ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            //item.FrozenFrom = itemModel.FrozenFrom;
            //item.FrozenTo = itemModel.FrozenTo;
            //item.FrozenRemarks = itemModel.FrozenComm;
            //item.Manufacturer = itemModel.FirmCode;
            //item.UoMGroupEntry = itemModel.UgpEntry;
            //item.ShipType = itemModel.ShipType;
            //item.IssuePrimarilyBy = (IssuePrimarilyByEnum) itemModel.IssuePriBy;
            //item.SWW = itemModel.SWW;
            //item.MaterialType = (BoMaterialTypes)itemModel.MatType;
            if (itemModel.NCMCode > 0)
                item.NCMCode = itemModel.NCMCode;
            if (itemModel.DNFEntry > 0)
                item.DNFEntry = itemModel.DNFEntry;
            if (itemModel.ItmsGrpCod > 0)
                item.ItemsGroupCode = itemModel.ItmsGrpCod;
            
            //item.InventoryItem = itemModel.InvntItem ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            //item.SalesItem = itemModel.SellItem ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            //item.PurchaseItem = itemModel.PrchseItem ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            //item.NoDiscounts = itemModel.NoDiscount ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            //item.PreferredVendors = itemModel.CardCode;
            item.SupplierCatalogNo = itemModel.SuppCatNum;
            //item.UnitOfMeasurements. = itemModel.BuyUnitMsr;
            //item.GLMethod = itemModel.GLMethod == "" ? BoGLMethods.glm_WH : BoGLMethods.glm_ItemClass;
            //item.InventoryUOM = itemModel.InvntryUom;
            //item.InventoryWeight1 = itemModel.IWeight1;
            //item.CostAccountingMethod = itemModel.EvalSystem == "" ? BoInventorySystem.bis_FIFO : BoInventorySystem.bis_Standard;
            //item.AvgStdPrice = itemModel.AvgPrice;
            //item.ManageStockByWarehouse = itemModel.ByWh ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
            //item.DesiredInventory = itemModel.ReorderQty;
            //item.MinInventory = itemModel.MinLevel;
            //item.MaxInventory = itemModel.MaxLevel;
            //item.DefaultWarehouse = itemModel.DfltWH;
            //item.PlanningSystem = itemModel.PlaningSys == "" ? BoPlanningSystem.bop_MRP : BoPlanningSystem.bop_None;
            //item.ProcurementMethod = itemModel.PrcrmntMtd == "" ? BoProcurementMethod.bom_Buy : BoProcurementMethod.bom_Make;
            //item.OrderIntervals = itemModel.OrdrIntrvl.ToString();
            //item.OrderMultiple = itemModel.OrdrMulti;
            //item.MinOrderQuantity = itemModel.MinOrdrQty;
            //item.LeadTime = itemModel.LeadTime;
            //item.ToleranceDays = itemModel.ToleranDay;
            //item.IssueMethod = itemModel.IssueMthd == "" ? BoIssueMethod.im_Backflush : BoIssueMethod.im_Manual;
            //item.Picture = itemModel.PicturName;
            //item.User_Text = itemModel.UserText;

            //item.set_Properties(1, itemModel.QryGroup1 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(2, itemModel.QryGroup2 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(3, itemModel.QryGroup3 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(4, itemModel.QryGroup4 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(5, itemModel.QryGroup5 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(6, itemModel.QryGroup6 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(7, itemModel.QryGroup7 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(8, itemModel.QryGroup8 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(9, itemModel.QryGroup9 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(10, itemModel.QryGroup10 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(11, itemModel.QryGroup11 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(12, itemModel.QryGroup12 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(13, itemModel.QryGroup13 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(14, itemModel.QryGroup14 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(15, itemModel.QryGroup15 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(16, itemModel.QryGroup16 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(17, itemModel.QryGroup17 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(18, itemModel.QryGroup18 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(19, itemModel.QryGroup19 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(20, itemModel.QryGroup20 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(21, itemModel.QryGroup21 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(22, itemModel.QryGroup2 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(23, itemModel.QryGroup23 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(24, itemModel.QryGroup24 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(25, itemModel.QryGroup25 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(26, itemModel.QryGroup26 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(27, itemModel.QryGroup27 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(28, itemModel.QryGroup28 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(29, itemModel.QryGroup29 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(30, itemModel.QryGroup30 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(31, itemModel.QryGroup31 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(32, itemModel.QryGroup32 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(33, itemModel.QryGroup33 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(34, itemModel.QryGroup34 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(35, itemModel.QryGroup35 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(36, itemModel.QryGroup36 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(37, itemModel.QryGroup37 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(38, itemModel.QryGroup38 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(39, itemModel.QryGroup39 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(40, itemModel.QryGroup40 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(41, itemModel.QryGroup41 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(42, itemModel.QryGroup42 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(43, itemModel.QryGroup43 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(44, itemModel.QryGroup44 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(45, itemModel.QryGroup45 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(46, itemModel.QryGroup46 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(47, itemModel.QryGroup47 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(48, itemModel.QryGroup48 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(49, itemModel.QryGroup49 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(50, itemModel.QryGroup50 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(51, itemModel.QryGroup51 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(52, itemModel.QryGroup52 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(53, itemModel.QryGroup53 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(54, itemModel.QryGroup54 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(55, itemModel.QryGroup55 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(56, itemModel.QryGroup56 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(57, itemModel.QryGroup57 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(58, itemModel.QryGroup58 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(59, itemModel.QryGroup59 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(60, itemModel.QryGroup60 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(61, itemModel.QryGroup61 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(62, itemModel.QryGroup62 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(63, itemModel.QryGroup63 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            //item.set_Properties(64, itemModel.QryGroup64 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);

            //item.UserFields.Fields.Item("U_B1SYS_SPEDTipi").Value = itemModel.B1SYS_SPEDTipi;

            foreach (KeyValuePair<string, dynamic> userField in itemModel.UserFields)
            {

            }
        }
    }
}
