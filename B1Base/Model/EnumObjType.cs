using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public enum EnumObjType
    {
        None = -1,
        Invoice = 13,
        PurchaseRequest = 1470000113,
        PurchaseQuotation = 540000006,
        PurchaseOrder = 22,       
        BlanketAgreement=1250000025,
        SalesQuotation = 23,
        SalesOrder = 17,
        SalesForecast = 198,
        InventoryTransferRequest = 1250000001,
        RecurringTransaction = 540000040,
        PurchaseTaxInvoice = 281
    }
}
