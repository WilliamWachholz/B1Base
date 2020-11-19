using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    class PaymentDI
    {
        Payments _businessObject = null;

        public void InitializeObject(string cardCode)
        {
            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oIncomingPayments);
        }

        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);

            _businessObject = null;

            GC.Collect();
        }

        public void Save()
        {
            _businessObject.Add();


            Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
        }


        public int SetInvoice(int docEntry, int instId, int line = -1)
        {
            if (line == -1)
            {
                _businessObject.Invoices.Add();

                line = _businessObject.Invoices.Count - 1;
            }

            _businessObject.Invoices.SetCurrentLine(line);

            _businessObject.Invoices.DocEntry = docEntry;
            _businessObject.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
            _businessObject.Invoices.InstallmentId = instId;

            return line;
        }

        public void SetInvoiceSumApplied(double value, int line = -1)
        {
            _businessObject.Invoices.SetCurrentLine(line);

            _businessObject.Invoices.SumApplied = value;
        }

        public int SetCreditCard(string value, int line = -1)
        {
            if (line == -1)
            {
                _businessObject.CreditCards.Add();

                line = _businessObject.CreditCards.Count - 1;
            }

            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.CreditCard = B1Base.Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetCreditCard", value);

            return line;
        }

        public void SetCreditCardAcct(string value, int line)
        {
            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.CreditAcct = value;
        }

        public void SetCreditCardNum(string value, int line)
        {
            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.CreditCardNumber = value;
        }

        public void SetCreditVoucherNum(string value, int line)
        {
            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.VoucherNum = value;
        }

        public void SetCreditCardSum(double value, int line)
        {
            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.CreditSum = value;
        }

        public void SetCreditCardNumOfPmnts(int value, int line)
        {
            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.NumOfPayments = value;
        }
    }
}
