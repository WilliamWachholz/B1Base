using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace B1Base.Controller
{
    public class AttachmentController
    {
        /// <summary>
        /// Salva anexos vinculados à um cadastro. Pode ser um cadastro customizado ou padrão do B1 que tenha funcionalidade de anexo, como, por exemplo, item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attachmentList"></param>
        /// <param name="model">Pode ser um BaseModel, ItemModel ou qualquer model com a propriedade AtcEntry</param>
        public void Save<T>(List<Model.AttachmentModel> attachmentList, T model)
        {
            DAO.AttachmentDAO attachmentDAO = new DAO.AttachmentDAO();

            int atcEntry = 0;

            foreach (Model.AttachmentModel attachmentModel in attachmentList)
            {
                if (attachmentModel.Insert)
                {
                    attachmentDAO.Insert(attachmentModel);

                    if (atcEntry == 0)
                    {
                        atcEntry = attachmentModel.AbsEntry;
                        attachmentList.Select(r => r.AbsEntry = atcEntry).ToList();
                    }
                }

                if (attachmentModel.Delete)
                {
                    attachmentDAO.Delete(attachmentModel);
                }
            }            

            Type type = typeof(T);

            var prop = type.GetProperty("AtcEntry");

            if (prop != null)
            {
                prop.SetValue(model, atcEntry);
            }
        }

        public void Open(int atcEntry, int line)
        {
            Model.AttachmentModel attachmentModel = new DAO.AttachmentDAO().Get(atcEntry, line);

            System.Diagnostics.Process.Start(attachmentModel.Path);
        }

        public string AttachmentFolder
        {
            get
            {
                return new DAO.AttachmentDAO().AttachmentFolder;
            }
        }

        public bool AttachmentFolderExists()
        {
            return Directory.Exists(AttachmentFolder);
        }

        public bool FileAlreadyExists(string fileName)
        {
            return File.Exists(Path.Combine(AttachmentFolder, fileName));
        }

        public static string MsgAttachmentFolder
        {
            get { return "A pasta de anexos não foi definida ou a pasta de anexos foi modificada ou eliminada."; }
        }

        public static string MSgConfirmFileOverride(string fileName)
        {
            return "Já existe um arquivo com este nome. substituir esse arquivo?";
        }
    }
}
