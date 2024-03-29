﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class JournalDAO
    {
        public Model.JournalModel Get(int transId)
        {
            Model.JournalModel journalModel = new Model.JournalModel();

            JournalEntries journal = (JournalEntries) Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
            try
            {
                if (journal.GetByKey(transId))
                {
                    journalModel.TransId = transId;

                    journalModel.Ref1 = journal.Reference;
                    journalModel.Ref2 = journal.Reference2;
                    journalModel.Memo = journal.Memo;
                    journalModel.RefDate = journal.ReferenceDate;
                    journalModel.TaxDate = journal.TaxDate;
                    journalModel.DueDate = journal.DueDate;

                    for (int line = 0; line < journal.Lines.Count; line++)
                    {
                        journal.Lines.SetCurrentLine(line);

                        Model.JournalLineModel journalLineModel = new Model.JournalLineModel();
                        journalLineModel.BplId = journal.Lines.BPLID;
                        journalLineModel.Account = journal.Lines.AccountCode;
                        journalLineModel.ShortName = journal.Lines.ShortName;
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
            JournalEntries journal = (JournalEntries) Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
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
            JournalEntries journal = (JournalEntries) Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
            try
            {
                if (journal.GetByKey(transId))
                {
                    journal.Cancel();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
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
            journal.Reference = journalModel.Ref1;
            journal.Reference2 = journalModel.Ref2;
            journal.Memo = journalModel.Memo;


            int line = 0;

            foreach (Model.JournalLineModel journalLineModel in journalModel.JournalLineList)
            {
                if (line > journal.Lines.Count - 1)
                    journal.Lines.Add();

                journal.Lines.SetCurrentLine(line);

                if (journalLineModel.BplId > 0)
                    journal.Lines.BPLID = journalLineModel.BplId;

                if (journalLineModel.Account != string.Empty)
                    journal.Lines.AccountCode = journalLineModel.Account;
                if (journalLineModel.ShortName != string.Empty)
                    journal.Lines.ShortName = journalLineModel.ShortName;
                journal.Lines.Credit = journalLineModel.Credit;
                journal.Lines.Debit = journalLineModel.Debit;

                line++;
            }

            foreach (KeyValuePair<string, dynamic> userField in journalModel.UserFields)
            {
                journal.UserFields.Fields.Item(userField.Key).Value = userField.Value;
            }
        }
    }
}
