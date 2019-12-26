using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace B1Base.Model
{
    public class ItemModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int AtcEntry { get; set; }              
        public int ListNum { get; set; }
        public string CodeBars { get; set; }
        public bool ValidFor { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }        
        public string ValidComm { get; set; }
        public bool FrozenFor { get; set; }
        public DateTime FrozenFrom { get; set; }
        public DateTime FrozenTo { get; set; }        
        public string FrozenComm { get; set; }
        public int FirmCode { get; set; }
        public int UgpEntry { get; set; }
        public int ShipType { get; set; }
        public int IssuePriBy { get; set; }
        public int MngMethod { get; set; }
        public string SWW { get; set; }
        public int MatType { get; set; }
        public int MatGrp { get; set; }
        public int NCMCode { get; set; }
        public int DNFEntry { get; set; }
        public string ProductSrc { get; set; }
        public int ItmsGrpCod { get; set; }
        public string B1SYS_SPEDTipi { get; set; }
        public bool InvntItem { get; set; }
        public bool SellItem { get; set; }
        public bool PrchseItem { get; set; }
        public bool NoDiscount { get; set; }
        public string CardCode { get; set; }
        public List<string> CardCodes { get; set; }
        public string SuppCatNum { get; set; }
        public string BuyUnitMsr { get; set; }
        public double NumInBuy { get; set; }
        public string PurPackMsr { get; set; }
        public double PurPackUn { get; set; }
        public double BLength1 { get; set; }
        public double BWidth1 { get; set; }
        public double BHeigth1 { get; set; }
        public double BVolume1 { get; set; }
        public int BVolUnit { get; set; }
        public double BWeight1 { get; set; }
        public double PurFactor1 { get; set; }
        public double PurFactor2 { get; set; }
        public double PurFactor3 { get; set; }
        public double PurFactor4 { get; set; }
        public string SalUnitMsr { get; set; }
        public double NumInSale { get; set; }        
        public string SalPackMsr { get; set; }
        public double SalPackUn { get; set; }
        public double SLength1 { get; set; }
        public double SWidth1 { get; set; }
        public double SHeigth1 { get; set; }
        public double SVolume1 { get; set; }
        public int SVolUnit { get; set; }        
        public double SWeight1 { get; set; }        
        public double SalFactor1 { get; set; }        
        public double SalFactor2 { get; set; }        
        public double SalFactor3 { get; set; }        
        public double SalFactor4 { get; set; }        
        public string GLMethod { get; set; }        
        public string InvntryUom { get; set; }                
        public double IWeight1 { get; set; }        
        public string EvalSystem { get; set; }        
        public double AvgPrice { get; set; }
        public bool ByWh { get; set; }        
        public double ReorderQty { get; set; }        
        public double MinLevel { get; set; }        
        public double MaxLevel { get; set; }        
        public string DfltWH { get; set; }
        public string PlaningSys { get; set; }        
        public string PrcrmntMtd { get; set; }
        public int OrdrIntrvl { get; set; }        
        public double OrdrMulti { get; set; }        
        public double MinOrdrQty { get; set; }
        public int LeadTime { get; set; }
        public int ToleranDay { get; set; }
        public bool IsPhantom { get; set; }
        public string IssueMthd { get; set; }
        public double PrdStdCst { get; set; }
        public bool InCostRoll { get; set; }
        public string PicturName { get; set; }        
        public string UserText { get; set; }
        public bool QryGroup1 { get; set; }
        public bool QryGroup2 { get; set; }
        public bool QryGroup3 { get; set; }
        public bool QryGroup4 { get; set; }
        public bool QryGroup5 { get; set; }
        public bool QryGroup6 { get; set; }
        public bool QryGroup7 { get; set; }
        public bool QryGroup8 { get; set; }
        public bool QryGroup9 { get; set; }
        public bool QryGroup10 { get; set; }
        public bool QryGroup11 { get; set; }
        public bool QryGroup12 { get; set; }
        public bool QryGroup13 { get; set; }
        public bool QryGroup14 { get; set; }
        public bool QryGroup15 { get; set; }
        public bool QryGroup16 { get; set; }
        public bool QryGroup17 { get; set; }
        public bool QryGroup18 { get; set; }
        public bool QryGroup19 { get; set; }
        public bool QryGroup20 { get; set; }
        public bool QryGroup21 { get; set; }
        public bool QryGroup22 { get; set; }
        public bool QryGroup23 { get; set; }
        public bool QryGroup24 { get; set; }
        public bool QryGroup25 { get; set; }
        public bool QryGroup26 { get; set; }
        public bool QryGroup27 { get; set; }
        public bool QryGroup28 { get; set; }
        public bool QryGroup29 { get; set; }
        public bool QryGroup30 { get; set; }
        public bool QryGroup31 { get; set; }
        public bool QryGroup32 { get; set; }
        public bool QryGroup33 { get; set; }
        public bool QryGroup34 { get; set; }
        public bool QryGroup35 { get; set; }
        public bool QryGroup36 { get; set; }
        public bool QryGroup37 { get; set; }
        public bool QryGroup38 { get; set; }
        public bool QryGroup39 { get; set; }
        public bool QryGroup40 { get; set; }
        public bool QryGroup41 { get; set; }
        public bool QryGroup42 { get; set; }
        public bool QryGroup43 { get; set; }
        public bool QryGroup44 { get; set; }
        public bool QryGroup45 { get; set; }
        public bool QryGroup46 { get; set; }
        public bool QryGroup47 { get; set; }
        public bool QryGroup48 { get; set; }
        public bool QryGroup49 { get; set; }
        public bool QryGroup50 { get; set; }
        public bool QryGroup51 { get; set; }
        public bool QryGroup52 { get; set; }
        public bool QryGroup53 { get; set; }
        public bool QryGroup54 { get; set; }
        public bool QryGroup55 { get; set; }
        public bool QryGroup56 { get; set; }
        public bool QryGroup57 { get; set; }
        public bool QryGroup58 { get; set; }
        public bool QryGroup59 { get; set; }
        public bool QryGroup60 { get; set; }
        public bool QryGroup61 { get; set; }
        public bool QryGroup62 { get; set; }
        public bool QryGroup63 { get; set; }
        public bool QryGroup64 { get; set; }

        public Dictionary<string, dynamic> UserFields { get; set; }

        public ItemModel()
        {
            CardCodes = new List<string>();
            UserFields = new Dictionary<string, dynamic>();            
        }
    }
}
