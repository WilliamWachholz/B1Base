using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using SAPbobsCOM;

namespace B1Base.DAO
{
    class AttachmentDAO
    {
        public void Insert(Model.AttachmentModel attachmentModel)
        {
            Attachments2 attachment = (Attachments2)AddOn.Instance.ConnectionController.Company.GetBusinessObject(BoObjectTypes.oAttachments2);
            try
            {
                if (attachment.GetByKey(attachmentModel.AbsEntry))
                {
                    attachment.Lines.SourcePath = Path.GetDirectoryName(attachmentModel.Path);
                    attachment.Lines.FileName = Path.GetFileNameWithoutExtension(attachmentModel.Path);
                    attachment.Lines.FileExtension = Path.GetExtension(attachmentModel.Path);

                    attachment.Update();

                    AddOn.Instance.ConnectionController.VerifyBussinesObjectSuccess();
                }
                else
                {
                    attachment.Lines.SourcePath = Path.GetDirectoryName(attachmentModel.Path);
                    attachment.Lines.FileName = Path.GetFileNameWithoutExtension(attachmentModel.Path);
                    attachment.Lines.FileExtension = Path.GetExtension(attachmentModel.Path).Replace(".", "");

                    attachment.Add();

                    AddOn.Instance.ConnectionController.VerifyBussinesObjectSuccess();

                    attachmentModel.AbsEntry = Convert.ToInt32(AddOn.Instance.ConnectionController.Company.GetNewObjectKey());
                }                
            }
            finally
            {
                Marshal.ReleaseComObject(attachment);
            }
        }

        public void Delete(Model.AttachmentModel attachmentModel)
        {
            Attachments2 attachment = (Attachments2)AddOn.Instance.ConnectionController.Company.GetBusinessObject(BoObjectTypes.oAttachments2);
            try
            {
                if (attachment.GetByKey(attachmentModel.AbsEntry))
                {
                    attachment.Lines.SetCurrentLine(attachmentModel.Line - 1);

                    //attachment.Lines.SourcePath = string.Empty;
                    attachment.Lines.FileName = string.Empty;
                    //attachment.Lines.FileExtension = string.Empty;

                    attachment.Update();

                    AddOn.Instance.ConnectionController.VerifyBussinesObjectSuccess();
                }               
            }
            finally
            {
                Marshal.ReleaseComObject(attachment);
            }
        }
    }
}
