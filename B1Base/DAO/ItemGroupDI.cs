using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class ItemGroupDI
    {
        ItemGroups _businessObject = null;

        bool _newObject = false;

        public void InitializeObject(int groupCode)
        {
            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItemGroups);

            if (!_businessObject.GetByKey(groupCode))
            {
                _newObject = true;
            }
        }

        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);

            _businessObject = null;

            GC.Collect();
        }

        public void SetGroupName(string value)
        {
            _businessObject.GroupName = value;
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

            _businessObject.Update();
        }

        public void Delete()
        {
            if (!_newObject)
            {
                _businessObject.Remove();
            }
        }

        public bool NewObject
        {
            get
            {
                return _newObject;
            }
        }
    }
}
