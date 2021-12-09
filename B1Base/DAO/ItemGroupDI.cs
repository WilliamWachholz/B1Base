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


        bool m_ItemClassSet = false;
        ItemClassEnum m_ItemClass;

        bool m_MaterialTypeSet = false;
        BoMaterialTypes m_MaterialType;

        bool m_MaterialGroupSet = false;
        int m_MaterialGroup;

        bool m_NCMCodeSet = false;
        int m_NCMCode = 0;

        bool m_ProductSourceSet = false;
        int m_ProductSource = 0;


        public void InitializeObject(int groupCode)
        {
            _businessObject = (ItemGroups) Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oItemGroups);

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

        public void SetUgpCode(int value)
        {
            _businessObject.DefaultUoMGroup = value;
        }

        public void SetPlaningSys(BoPlanningSystem value)
        {
            _businessObject.PlanningSystem = value;
        }

        public void SetPrcmntMtd(BoProcurementMethod value)
        {
            _businessObject.ProcurementMethod = value;
        }

        public void SetOrdrIntrvl(int value)
        {
            _businessObject.OrderInterval = value;
        }

        public void MinOrdrQty(double value)
        {
            _businessObject.MinimumOrderQuantity = value;
        }

        public void LeadTime(int value)
        {
            _businessObject.LeadTime = value;
        }
        public void ToleranDay(int value)
        {
            _businessObject.ToleranceDays = value;
        }

        public void SetInvntSys(BoInventorySystem value)
        {
            _businessObject.InventorySystem = value;
        }

        public void SetItemClass(ItemClassEnum value)
        {
            m_ItemClass = value;
            m_ItemClassSet = true;
        }

        public void SetMatType(BoMaterialTypes value)
        {
            m_MaterialType = value;
            m_MaterialTypeSet = true;
        }

        public void SetMatGrp(int value)
        {
            if (value > 0)
                m_MaterialGroup = value;
            else
                m_MaterialGroup = -1;

            m_MaterialGroupSet = true;
        }

        public void SetNCMCode(int value)
        {
            if (value > 0)
                m_NCMCode = value;
            else
                m_NCMCode = -1;

            m_NCMCodeSet = true;
        }

        public void SetProductSrc(string value)
        {
            if (value != string.Empty)
            {
                m_ProductSource = Convert.ToInt32(value);

                m_ProductSourceSet = true;
            }
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

            if (m_ItemClassSet)
            {
                Controller.ConnectionController.Instance.ExecuteStatementDirect(@"UPDATE OITB SET ""ItemClass"" = '{0}' WHERE ""ItmsGrpCod"" = {1}", ((int)ItemClassEnum.itcMaterial).ToString(), _businessObject.Number.ToString());

                m_ItemClassSet = false;
            }

            if (m_MaterialTypeSet)
            {
                Controller.ConnectionController.Instance.ExecuteStatementDirect(@"UPDATE OITB SET ""MatType"" = '{0}' WHERE ""ItmsGrpCod"" = {1}", ((int) m_MaterialType).ToString(), _businessObject.Number.ToString());

                m_MaterialTypeSet = false;
            }

            if (m_MaterialGroupSet)
            {
                Controller.ConnectionController.Instance.ExecuteStatementDirect(@"UPDATE OITB SET ""MatGrp"" = '{0}' WHERE ""ItmsGrpCod"" = {1}", m_MaterialGroup.ToString(), _businessObject.Number.ToString());

                m_MaterialGroupSet = false;
            }

            if (m_NCMCodeSet)
            {
                Controller.ConnectionController.Instance.ExecuteStatementDirect(@"UPDATE OITB SET ""NCMCode"" = '{0}' WHERE ""ItmsGrpCod"" = {1}", m_NCMCodeSet.ToString(), _businessObject.Number.ToString());

                m_NCMCodeSet = false;
            }

            if (m_ProductSourceSet)
            {
                Controller.ConnectionController.Instance.ExecuteStatementDirect(@"UPDATE OITB SET ""ProductSrc"" = '{0}' WHERE ""ItmsGrpCod"" = {1}", m_ProductSource.ToString(), _businessObject.Number.ToString());

                m_ProductSourceSet = false;
            }
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
