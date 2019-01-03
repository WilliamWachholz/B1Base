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

        public List<Model.AttachmentModel> Get(int atcEntry)
        {
            return new DAO.AttachmentDAO().Get(atcEntry);
        }

        public int Duplicate(int atcEntry)
        {
            int absEntry = 0;

            DAO.AttachmentDAO attachmentDAO = new DAO.AttachmentDAO();

            List<Model.AttachmentModel> attachmentList = Get(atcEntry);

            attachmentList.Select(r => r.AbsEntry = 0).ToList();

            for (int line = 0; line < attachmentList.Count; line++)
            {
                Model.AttachmentModel attachmentModel = attachmentList[line];
                attachmentModel.AbsEntry = absEntry;

                attachmentDAO.Insert(attachmentModel);

                if (line == 0)
                    absEntry = attachmentModel.AbsEntry;
            }

            return absEntry;
        }

        public void Open(int atcEntry, int line)
        {
            Model.AttachmentModel attachmentModel = new DAO.AttachmentDAO().Get(atcEntry, line);

            System.Diagnostics.Process.Start(attachmentModel.Path);
        }

        public void SaveImage(string file)
        {
            File.Copy(file, Path.Combine(ImageFolder, Path.GetFileName(file)), true);
        }

        public string ImageFolder
        {
            get
            {
                return new DAO.AttachmentDAO().GetImageFolder();
            }
        }

        public string AttachmentFolder
        {
            get
            {
                return new DAO.AttachmentDAO().GetAttachmentFolder();
            }
        }

        public bool AttachmentFolderExists()
        {
            return Directory.Exists(AttachmentFolder);
        }

        public bool ImageFolderExists()
        {
            return Directory.Exists(ImageFolder);
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
