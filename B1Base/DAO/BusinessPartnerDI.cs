using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class BusinessPartnerDI
    {
        BusinessPartners _businessObject = null;

        bool _newObject = false;

        public void InitializeObject(string cardCode)
        {
            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);

            if (!_businessObject.GetByKey(cardCode))
            {
                _newObject = true;
                _businessObject.CardCode = cardCode;
            }
        }

        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);
            GC.Collect();
        }

        public void SetCardName(string value)
        {
            _businessObject.CardName = value;
        }

        public void SetCardFName(string value)
        {
            _businessObject.CardForeignName = value;
        }

        public void SetEMail(string value)
        {
            _businessObject.EmailAddress = value;
        }

        public void SetPhone1(string value)
        {
            _businessObject.Phone1 = value;
        }

        public void SetPhone2(string value)
        {
            _businessObject.Phone2 = value;
        }

        public void SetAddressName(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.AddressName = value;            
        }

        public void SetAddressAdresType(Model.EnumAdresType value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.AddressType = value == Model.EnumAdresType.Bill ? BoAddressType.bo_BillTo : BoAddressType.bo_ShipTo;
        }

        public void SetAddressAddrType(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.TypeOfAddress = value;
        }

        public void SetAddressStreet(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.Street = value.ToString();
        }

        public void SetAddressBuilding(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);            

            _businessObject.Addresses.BuildingFloorRoom = value.ToString();
        }

        public void SetAddressSreetNo(int value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.StreetNo = value.ToString();
        }

        public void SetAddressBlock(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.Block = value.ToString();
        }

        public void SetAddressCity(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.City = value.ToString();
        }

        public void SetAddressState(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.State = value.ToString();
        }

        public void SetAddressCountry(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.Country = value.ToString();
        }

        public void SetAddressZipCode(string value, int line)
        {
            while (_businessObject.Address.Count() >= line)
                _businessObject.Addresses.Add();

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.ZipCode = value.ToString();
        }

        public void SetCpf(string value, string address = "", Model.EnumAdresType adresType = Model.EnumAdresType.Ship)
        {
            bool found = false;

            for (int i = 0; i < _businessObject.FiscalTaxID.Count; i++)
            {
                _businessObject.FiscalTaxID.SetCurrentLine(i);

                if (address == string.Empty && _businessObject.FiscalTaxID.Address == string.Empty)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_BillTo && adresType == Model.EnumAdresType.Bill)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_ShipTo && adresType == Model.EnumAdresType.Ship)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                _businessObject.FiscalTaxID.Add();
                _businessObject.FiscalTaxID.SetCurrentLine(_businessObject.FiscalTaxID.Count + 1);
            }

            _businessObject.FiscalTaxID.TaxId4 = value;
        }

        public void SetCnpj(string value, string address = "", Model.EnumAdresType adresType = Model.EnumAdresType.Ship)
        {
            bool found = false;

            for (int i = 0; i < _businessObject.FiscalTaxID.Count; i++)
            {
                _businessObject.FiscalTaxID.SetCurrentLine(i);

                if (address == string.Empty && _businessObject.FiscalTaxID.Address == string.Empty)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_BillTo && adresType == Model.EnumAdresType.Bill)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_ShipTo && adresType == Model.EnumAdresType.Ship)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                _businessObject.FiscalTaxID.Add();
                _businessObject.FiscalTaxID.SetCurrentLine(_businessObject.FiscalTaxID.Count + 1);
            }

            _businessObject.FiscalTaxID.TaxId0 = value;
        }

        public void SetInscricaoEstadual(string value, string address = "", Model.EnumAdresType adresType = Model.EnumAdresType.Ship)
        {
            bool found = false;

            for (int i = 0; i < _businessObject.FiscalTaxID.Count; i++)
            {
                _businessObject.FiscalTaxID.SetCurrentLine(i);

                if (address == string.Empty && _businessObject.FiscalTaxID.Address == string.Empty)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_BillTo && adresType == Model.EnumAdresType.Bill)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_ShipTo && adresType == Model.EnumAdresType.Ship)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                _businessObject.FiscalTaxID.Add();
                _businessObject.FiscalTaxID.SetCurrentLine(_businessObject.FiscalTaxID.Count + 1);
            }

            _businessObject.FiscalTaxID.TaxId1 = value;
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

        public BusinessPartners BusinessObject
        {
            get
            {
                return _businessObject;
            }
        }
    }
}
