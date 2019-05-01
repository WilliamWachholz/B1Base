using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    class JournalDAO
    {
        public Model.JournalModel Get(int transId)
        {
            Model.JournalModel journalModel = new Model.JournalModel();

            JournalEntries journal = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
            try
            {
                if (journal.GetByKey(transId))
                {
                    journalModel.TransId = transId;

                    journalModel.RefDate = journal.ReferenceDate;
                    journalModel.TaxDate = journal.TaxDate;
                    journalModel.DueDate = journal.DueDate;

                    for (int line = 0; line < journal.Lines.Count; line++)
                    {
                        journal.Lines.SetCurrentLine(line);

                        Model.JournalLineModel journalLineModel = new Model.JournalLineModel();
                        journalLineModel.Account = journal.Lines.AccountCode;
                        journalLineModel.Debit = journal.Lines.Debit;
                        journalLineModel.Credit = journal.Lines.Credit;

                        journalModel.JournalLineList.Add(journalLineModel);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(journal);
                GC.Collect();
            }

            return journalModel;
        }

        public void Save(Model.JournalModel journalModel)
        {
            JournalEntries journal = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
            try
            {
                if (journal.GetByKey(journalModel.TransId))
                {
                    SetFields(journal, journalModel);

                    journal.Update();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                }
                else
                {
                    SetFields(journal, journalModel);

                    journal.Add();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(journal);
                GC.Collect();
            }
        }

        public void Storno(int transId, DateTime stornoDate)
        {
            JournalEntries journal = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
            try
            {
                if (journal.GetByKey(transId))
                {
                    journal.UseAutoStorno = BoYesNoEnum.tYES;
                    journal.StornoDate = stornoDate;

                    journal.Update();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(journal);
                GC.Collect();
            }
        }

        private void SetFields(JournalEntries journal, Model.JournalModel journalModel)
        {
            journal.ReferenceDate = journalModel.RefDate;
            journal.TaxDate = journalModel.TaxDate;
            journal.DueDate = journalModel.DueDate;

            int line = 0;

            foreach (Model.JournalLineModel journalLineModel in journalModel.JournalLineList)
            {
                if (line > journal.Lines.Count - 1)
                    journal.Lines.Add();

                journal.Lines.SetCurrentLine(line);

                journal.Lines.AccountCode = journalLineModel.Account;
                journal.Lines.Credit = journalLineModel.Credit;
                journal.Lines.Debit = journalLineModel.Debit;
            }
        }
    }
}
