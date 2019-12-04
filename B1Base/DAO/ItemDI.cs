using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class ItemDI
    {
        Items _businessObject = null;

        bool _newObject = false;

        public void InitializeObject(string itemCode)
        {
            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItems);            

            if (!_businessObject.GetByKey(itemCode))
            {
                _newObject = true;            
                _businessObject.ItemCode = itemCode;
            }
        }

        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);
            GC.Collect();
        }

        public void SetCardCode(string value)
        {
            _businessObject.Mainsupplier = value;
        }

        public void SetSuppCatNum(string value)
        {
            _businessObject.SupplierCatalogNo = value;
        }

        public void SetBWeight1(double value)
        {
            _businessObject.PurchaseUnitWeight = value;
        }

        public void SetIWeight1(double value)
        {
            _businessObject.InventoryWeight = value;
        }

        public void SetSWeight1(double value)
        {
            _businessObject.SalesUnitWeight = value;
        }

        public void SetValidFor(bool value)
        {
            _businessObject.Valid = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetFrozenFor(bool value)
        {
            _businessObject.Frozen = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetUserField(string userField, dynamic value)
        {
            _businessObject.UserFields.Fields.Item(userField).Value = value;
        }

        public void Save()
        {
            if (_newObject)
            {
                _businessObject.Add();
            }
            else
            {
                _businessObject.Update();
            }

            Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
        }
    }
}
