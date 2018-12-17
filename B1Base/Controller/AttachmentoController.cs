using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Controller
{
    public class AttachmentoController
    {
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

                if (attachmentModel.Remove)
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

    }
}
