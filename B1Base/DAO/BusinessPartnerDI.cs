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

        public void InitializeObject(string cardCode, BoCardTypes cardType = BoCardTypes.cCustomer,  bool autoSeries = false)
        {
            _businessObject = Controller.ConnectionController.Instance.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);

            if (cardCode == null)
                cardCode = string.Empty;

            if (!_businessObject.GetByKey(cardCode))
            {
                _newObject = true;
                _businessObject.CardCode = cardCode;
                _businessObject.CardType = cardType;

                if (autoSeries)
                {
                    _businessObject.Series = B1Base.Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetAutoSeries", ((int) Model.EnumObjType.BusinessPartner).ToString(),
                        cardType == BoCardTypes.cCustomer ? "C" : "S");
                }
            }
        }

        public void FinalizeObject()
        {
            Marshal.ReleaseComObject(_businessObject);

            _businessObject = null;

            GC.Collect();
        }

        public void SetCardName(string value)
        {
            _businessObject.CardName = value == null ? "" : value;
        }

        public void SetCardFName(string value)
        {
            _businessObject.CardForeignName = value == null ? "" : value; ;
        }

        public void SetGroupCode(int value)
        {
            _businessObject.GroupCode = value;
        }

        public void SetAliasName(string value)
        {
            _businessObject.AliasName = value == null ? "" : value; ;
        }

        public void SetEMail(string value)
        {
            _businessObject.EmailAddress = value == null ? "" : value; ;
        }

        public void SetCurrency(string value)
        {
            _businessObject.Currency = value == null ? "##" : value;
        }

        public void SetListNum(int value)
        {
            _businessObject.PriceListNum = value;
        }

        public void SetIntrstRate(double value)
        {
            _businessObject.IntrestRatePercent = value;
        }

        public void SetPhone1(string value)
        {
            _businessObject.Phone1 = value == null ? "" : value; ;
        }

        public void SetPhone2(string value)
        {
            _businessObject.Phone2 = value == null ? "" : value; ;
        }

        public int SetAddressName(string value, int line = -1)
        {
            if (line == -1)
            {
                if (_businessObject.Addresses.AddressName != string.Empty)
                    _businessObject.Addresses.Add();

                line = _businessObject.Addresses.Count - 1;
            }

            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.AddressName = value == null ? "" : value;

            return line;
        }

        public void SetAddressAdresType(BoAddressType value, int line)
        {            
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.AddressType = value;
        }

        public void SetAddressAddrType(string value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.TypeOfAddress = value == null ? "" : value;
        }

        public void SetAddressStreet(string value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.Street = value == null ? "" : value;
        }

        public void SetAddressBuilding(string value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.BuildingFloorRoom = value.ToString();
        }

        public void SetAddressSreetNo(int value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.StreetNo = value.ToString();
        }

        public void SetAddressBlock(string value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.Block = value == null ? "" : value;
        }

        public void SetAddressCity(string value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.City = value == null ? "" : value;

            _businessObject.Addresses.County = Controller.ConnectionController.Instance.ExecuteSqlForDirectObject<string>("select coalesce(cast(\"AbsId\" as varchar), 'null') from OCNT where \"State\" = '{0}' and \"Name\" = '{1}'", _businessObject.Addresses.State, value);

        }

        public void SetAddressState(string value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.State = value == null ? "" : value;
        }

        public void SetAddressCountry(string value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.Country = value == null ? "" : value;
        }

        public void SetAddressZipCode(string value, int line)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.ZipCode = value == null ? "" : value;
        }

        public void SetAddressUserField(string userField, int line, dynamic value)
        {
            _businessObject.Addresses.SetCurrentLine(line);

            _businessObject.Addresses.UserFields.Fields.Item(userField).Value = value;
        }

        public void SetSlpCode(int value)
        {
            _businessObject.SalesPersonCode = value;
        }

        public void SetQryGroup3(bool value)
        {
            _businessObject.Properties[3] = value ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public void SetCpf(string value, string address = "", BoAddressType adresType = BoAddressType.bo_ShipTo)
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

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_BillTo && adresType == BoAddressType.bo_BillTo)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_ShipTo && adresType == BoAddressType.bo_ShipTo)
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

            _businessObject.FiscalTaxID.TaxId4 = value == null ? "" : value;
        }

        public void SetCnpj(string value, string address = "", BoAddressType adresType = BoAddressType.bo_ShipTo)
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

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_BillTo && adresType == BoAddressType.bo_BillTo)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_ShipTo && adresType == BoAddressType.bo_ShipTo)
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

            _businessObject.FiscalTaxID.TaxId0 = value == null ? "" : value;
        }

        public void SetInscricaoEstadual(string value, string address = "", BoAddressType adresType = BoAddressType.bo_ShipTo)
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

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_BillTo && adresType == BoAddressType.bo_BillTo)
                {
                    found = true;
                    break;
                }

                if (address == _businessObject.FiscalTaxID.Address && _businessObject.FiscalTaxID.AddrType == BoAddressType.bo_ShipTo && adresType == BoAddressType.bo_ShipTo)
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

            _businessObject.FiscalTaxID.TaxId1 = value == null ? "" : value;
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

            if (_businessObject.CardCode == string.Empty)
                _businessObject.CardCode = Controller.ConnectionController.Instance.LastObjectCode;
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
