using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class PaymentDI
    {
        Payments _businessObject = null;

        public void InitializeObject(string cardCode)
        {
            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oIncomingPayments);

            _businessObject.CardCode = cardCode;
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

        public void SetBPLId(int value)
        {
            _businessObject.BPLID = value;
        }

        public int SetInvoice(int docEntry, int instId, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Invoices.Count > 1 || _businessObject.Invoices.DocEntry > 0)
                    _businessObject.Invoices.Add();

                line = _businessObject.Invoices.Count - 1;
            }

            _businessObject.Invoices.SetCurrentLine(line);

            _businessObject.Invoices.DocEntry = docEntry;
            _businessObject.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
            _businessObject.Invoices.InstallmentId = instId;

            return line;
        }

        public int SetDownPayment(int docEntry, int instId, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Invoices.Count > 1 || _businessObject.Invoices.DocEntry > 0)
                    _businessObject.Invoices.Add();

                line = _businessObject.Invoices.Count - 1;
            }

            _businessObject.Invoices.SetCurrentLine(line);
            
            _businessObject.Invoices.DocEntry = docEntry;
            _businessObject.Invoices.InvoiceType = BoRcptInvTypes.it_DownPayment;
            _businessObject.Invoices.InstallmentId = instId;

            return line;
        }

        public void SetInvoiceSumApplied(double value, int line = -1)
        {
            _businessObject.Invoices.SetCurrentLine(line);

            _businessObject.Invoices.SumApplied = value;
        }

        public int SetCheck(int value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Checks.Count > 1 || _businessObject.Checks.CheckNumber > 0)
                    _businessObject.Checks.Add();

                line = _businessObject.Checks.Count - 1;
            }

            _businessObject.Checks.SetCurrentLine(line);

            _businessObject.Checks.CheckNumber = value;

            return line;
        }

        

        public void SetCheckDueDate(DateTime value, int line)
        {
            _businessObject.Checks.SetCurrentLine(line);

            _businessObject.Checks.DueDate = value;
        }

        public void SetCheckSum(double value, int line)
        {
            _businessObject.Checks.SetCurrentLine(line);

            _businessObject.Checks.CheckSum = value;
        }

        public void SetCheckCountryCod(string value, int line)
        {
            _businessObject.Checks.SetCurrentLine(line);

            _businessObject.Checks.CountryCode = value;
        }

        public void SetCheckBankCode(string value, int line)
        {
            _businessObject.Checks.SetCurrentLine(line);

            _businessObject.Checks.BankCode = value;
        }

        public void SetCheckBranch(string value, int line)
        {
            _businessObject.Checks.SetCurrentLine(line);

            _businessObject.Checks.Branch = value;
        }

        public void SetCheckAcct(string value, int line)
        {
            _businessObject.Checks.SetCurrentLine(line);

            _businessObject.Checks.AccounttNum = value;
        }

        public void SetCheckNum(int value, int line)
        {
            _businessObject.Checks.SetCurrentLine(line);

            _businessObject.Checks.CheckNumber = value;
        }

        public int SetCreditCard(string value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.CreditCards.Count > 1 || _businessObject.CreditCards.CreditCard > 0)
                    _businessObject.CreditCards.Add();

                line = _businessObject.CreditCards.Count - 1;
            }            

            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.CreditCard = B1Base.Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetCreditCard", value);

            _businessObject.CreditCards.CardValidUntil = new DateTime(DateTime.Now.Year + 1, 1, 1).AddDays(-1);

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

        public void SetCreditCardFirstDue(DateTime value, int line)
        {
            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.FirstPaymentDue = value;
        }

        public void SetCreditCardNumOfPmnts(int value, int line)
        {
            _businessObject.CreditCards.SetCurrentLine(line);

            _businessObject.CreditCards.NumOfPayments = value;
        }

        public void SetUserField(string key, dynamic value)
        {
            _businessObject.UserFields.Fields.Item(key).Value = value;
        }
        public Payments BusinessObject
        {
            get
            {
                return _businessObject;
            }
        }
    }
}
