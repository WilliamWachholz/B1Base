using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class ItemDAO
    {
        public Model.ItemModel Get(string itemCode)
        {
            return new Model.ItemModel();
        }

        public void Save(Model.ItemModel itemModel)
        {
            Items item = AddOn.Instance.ConnectionController.Company.GetBusinessObject(BoObjectTypes.oItems);
            try
            {
                item.set_Properties(1, itemModel.QryGroup1 ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);
            }
            finally
            {
                Marshal.ReleaseComObject(item);
                GC.Collect();
            }
        }
    }
}
