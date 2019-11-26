using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace B1Base.Model
{
    public enum EnumObjType
    {
        [Description("")]
        None = -1,        
        ChartOfAccount = 1,
        [Description("NF")]
        Invoice = 13,        
        PurchaseRequest = 1470000113,
        PurchaseQuotation = 540000006,
        PurchaseOrder = 22,       
        BlanketAgreement=1250000025,
        SalesQuotation = 23,
        [Description("CR")]
        IncomingPayment = 24,
        SalesOrder = 17,
        SalesForecast = 198,
        InventoryTransferRequest = 1250000001,
        RecurringTransaction = 540000040,
        PurchaseTaxInvoice = 281,
        [Description("NE")]
        PurchaseInvoice = 18,
        [Description("DS")]
        InvoiceReturn = 14,
        [Description("LC")]
        JournalEntry = 30
    }
}
