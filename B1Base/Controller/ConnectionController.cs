using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using SAPbouiCOM;
using SAPbobsCOM;
using System.IO;

namespace B1Base.Controller
{
    public class ConnectionController
    {
        ConnectionController()
        {
            
        }

        static readonly ConnectionController m_Instance = new ConnectionController();

        public static ConnectionController Instance
        {
            get { return m_Instance; }
        }

        public SAPbobsCOM.Company Company { get; private set; }

        public SAPbouiCOM.Application Application { get; private set; }

        public string AddOnID { get; private set; }

        public string DBServerType { get; private set; }

        public bool Desenv { get; private set; }

        public int User
        {
            get
            {
                return Company.UserSignature;
            }
        }

        public bool SuperUser
        {
            get
            {
                return ExecuteSqlForObject<string>("GetUserIsSuper", User.ToString()) == "Y";
            }
        }

        public int LastObjectCode
        {
            get
            {
                string result = string.Empty;

                Company.GetNewObjectCode(out result);

                return Convert.ToInt32(result);
            }
        }

        public int LastLogInstance(Model.EnumObjType objType, int code)
        {
            return ExecuteSqlForObject<int>("GetLastLogInstance", ((int)objType).ToString(), code.ToString());
        }

        public void VerifyBussinesObjectSuccess()
        {
            int errorCode;
            string errorMessage;

            this.Company.GetLastError(out errorCode, out errorMessage);

            if (errorCode != 0)
                throw new Exception(string.Format("Código do erro: {0}. Mensagem: {1}.", errorCode, errorMessage));
        }

        public void Initialize(string addOnID, bool singleSign)
        {
            AddOnID = addOnID;
            try
            {
                SAPbouiCOM.SboGuiApi sboGuiApi = new SAPbouiCOM.SboGuiApi();

                string connectionString = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";

                Desenv = false;

                sboGuiApi.Connect(connectionString);
                this.Application = sboGuiApi.GetApplication(-1);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Erro ao conectar a aplicação: {0}.", e.Message));
            }

            try
            {
                Company = this.Application.Company.GetDICompany();

                DBServerType = Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? "HANA" : "SQLSERVER";
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Erro ao conectar ao SAP Business One: {0}.", e.Message));
            }
        }

        public void Initialize(string addOnID) 
        {
            AddOnID = addOnID;
            try
            {
                SAPbouiCOM.SboGuiApi sboGuiApi = new SAPbouiCOM.SboGuiApi();
                
                string connectionString = System.Convert.ToString(Environment.GetCommandLineArgs().GetValue(1));

                Desenv = Environment.GetCommandLineArgs().GetValue(1).ToString() == "1";

                if (Desenv)
                    connectionString = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";

                if (connectionString == string.Empty)
                {
                    throw new Exception("O aplicativo deve ser iniciado dentro do SAP Business One Client.");                    
                }

                sboGuiApi.Connect(connectionString);
                this.Application = sboGuiApi.GetApplication(-1);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Erro ao conectar a aplicação: {0}.", e.Message));
            }

            try
            {
                Company = this.Application.Company.GetDICompany();

                DBServerType = Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? "HANA" : "SQLSERVER";
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Erro ao conectar ao SAP Business One: {0}.", e.Message));
            }            
        }

        public void Initialize(string addOnID, string server, string companyDB, string userName, string password, string licenseServer, string dbUserName, string dbPassword, string dbServerType)
        {
            AddOnID = addOnID;

            this.Company = null;
            this.Company = new SAPbobsCOM.Company();

            try
            {
                this.Company.Server = server;
                this.Company.CompanyDB = companyDB;
                this.Company.UserName = userName;
                this.Company.Password = password;
                this.Company.LicenseServer = licenseServer;
                this.Company.DbUserName = dbUserName;
                this.Company.DbPassword = dbPassword;
                this.Company.UseTrusted = false;                

                switch (dbServerType)
                {
                    case "MSSQL2005":
                        this.Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                        break;
                    case "MSSQL2008":
                        this.Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                        break;
                    case "MSSQL2012":
                        this.Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                        break;
                    case "MSSQL2014":
                        this.Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                        break;
                    case "HANA":
                        this.Company.DbServerType = BoDataServerTypes.dst_HANADB;
                        break;
                }
                try
                {
                    this.Company.Connect();
                    int iErro;
                    string sErro;
                    this.Company.GetLastError(out iErro, out sErro);
                    if (iErro != 0)
                    {
                        throw new Exception(iErro.ToString() + " - " + sErro);
                    }

                    DBServerType = Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? "HANA" : "SQLSERVER";
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao conectar ao SAP Business ONE: " + ex.Message);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Finalize()
        {
            this.Company.Disconnect();

            GC.Collect();

            GC.WaitForPendingFinalizers();

            GC.Collect();
        }

        public void CreateMetadata<T>() where T : Model.BaseModel
        {
            List<FieldMetadata> listFieldMetadata = PrepareObject<T>();

            foreach (FieldMetadata fieldMetadata in listFieldMetadata)
            {
                CreateMetadata(fieldMetadata);
            }
        }        

        public void CreateMetadata(FieldMetadata fieldMetadata)
        {
            CreateMetadata(fieldMetadata.Table, fieldMetadata.Field, fieldMetadata.FieldType, fieldMetadata.Size);
        }

        public void CreateMetadata(string table, string field, FieldTypeEnum fieldType, string fieldTitle)
        {
            CreateMetadata(table, field, fieldType, 10, null, "", fieldTitle);
        }

        public void CreateMetadata(bool configTable, string field, FieldTypeEnum fieldType, string fieldTitle)
        {
            CreateMetadata(configTable, field, fieldType, 10, null, "", fieldTitle);
        }

        public void CreateMetadata(string table, string field, FieldTypeEnum fieldType, int size = 10, Dictionary<string, string> validValues = null, string defaultValue = "", string fieldTitle = "")
        {
            int tableExists = ExecuteSqlForObject<int>("GetTableExists", table);

            if (tableExists == 0)
            {
                Application.StatusBar.SetText("Criando tabela " + table, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Warning);

                UserTablesMD userTable = Company.GetBusinessObject(BoObjectTypes.oUserTables);
                try
                {
                    userTable.TableName = table;
                    userTable.TableDescription = table;
                    userTable.TableType = BoUTBTableType.bott_NoObject;
                    
                    userTable.Add();

                    VerifyBussinesObjectSuccess();
                }
                catch (Exception e)
                {
                    Application.StatusBar.SetText("Erro ao criar tabela: " + e.Message, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);
                }
                finally
                {
                    Marshal.ReleaseComObject(userTable);
                    GC.Collect();
                }
            }

            bool sapTable = tableExists == 2;

            bool fieldExists = ExecuteSqlForObject<bool>("GetFieldExists", table, field);

            if (!fieldExists)
            {
                Application.StatusBar.SetText("Criando campo " + table + "." + field, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Warning);

                UserFieldsMD userField = Company.GetBusinessObject(BoObjectTypes.oUserFields);
                try
                {
                    userField.TableName = (sapTable ? "" : "@") + table;
                    userField.Name = field;
                    userField.Description = fieldTitle == string.Empty ? field : fieldTitle;

                    switch (fieldType)
                    {
                        case FieldTypeEnum.Address:
                            userField.Type = BoFieldTypes.db_Alpha;
                            userField.SubType = BoFldSubTypes.st_Address;
                            break;
                        case FieldTypeEnum.Alphanumeric:
                            userField.Type = BoFieldTypes.db_Alpha;
                            userField.EditSize = size;

                            if (validValues != null)
                            {
                                foreach (KeyValuePair<string, string> validValue in validValues)
                                {
                                    userField.ValidValues.Add();
                                    userField.ValidValues.SetCurrentLine(userField.ValidValues.Count - 1);

                                    userField.ValidValues.Value = validValue.Key;
                                    userField.ValidValues.Description = validValue.Value;
                                }
                            }
                            break;
                        case FieldTypeEnum.Date:
                            userField.Type = BoFieldTypes.db_Date;
                            break;
                        case FieldTypeEnum.Image:
                            userField.Type = BoFieldTypes.db_Alpha;
                            userField.SubType = BoFldSubTypes.st_Image;
                            break;
                        case FieldTypeEnum.Integer:
                            userField.Type = BoFieldTypes.db_Numeric;
                            userField.EditSize = 11;

                            if (validValues != null)
                            {
                                foreach (KeyValuePair<string, string> validValue in validValues)
                                {
                                    userField.ValidValues.Add();
                                    userField.ValidValues.SetCurrentLine(userField.ValidValues.Count - 1);

                                    userField.ValidValues.Value = validValue.Key;
                                    userField.ValidValues.Description = validValue.Value;
                                }
                            }
                            break;
                        case FieldTypeEnum.Measurement:
                            userField.Type = BoFieldTypes.db_Float;
                            userField.SubType = BoFldSubTypes.st_Measurement;
                            break;
                        case FieldTypeEnum.Memo:
                            userField.Type = BoFieldTypes.db_Memo;
                            break;
                        case FieldTypeEnum.Percentage:
                            userField.Type = BoFieldTypes.db_Float;
                            userField.SubType = BoFldSubTypes.st_Percentage;
                            break;
                        case FieldTypeEnum.Phone:
                            userField.Type = BoFieldTypes.db_Alpha;
                            userField.SubType = BoFldSubTypes.st_Phone;
                            break;
                        case FieldTypeEnum.Price:
                            userField.Type = BoFieldTypes.db_Float;
                            userField.SubType = BoFldSubTypes.st_Price;
                            break;
                        case FieldTypeEnum.Quantity:
                            userField.Type = BoFieldTypes.db_Float;
                            userField.SubType = BoFldSubTypes.st_Quantity;
                            break;
                        case FieldTypeEnum.Rate:
                            userField.Type = BoFieldTypes.db_Float;
                            userField.SubType = BoFldSubTypes.st_Rate;
                            break;
                        case FieldTypeEnum.Sum:
                            userField.Type = BoFieldTypes.db_Float;
                            userField.SubType = BoFldSubTypes.st_Sum;
                            break;
                        case FieldTypeEnum.Time:
                            userField.Type = BoFieldTypes.db_Date;
                            userField.SubType = BoFldSubTypes.st_Time;
                            break;
                    }

                    
                    if (defaultValue != string.Empty)
                        userField.DefaultValue = defaultValue;

                    userField.Add();

                    VerifyBussinesObjectSuccess();
                }
                catch (Exception e)
                {
                    Application.StatusBar.SetText("Erro ao criar campo: " + e.Message, BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error);
                }
                finally
                {
                    Marshal.ReleaseComObject(userField);
                    GC.Collect();
                }
            }
        }

        public void CreateMetadata(bool configTable, string field, FieldTypeEnum fieldType, int size = 10, Dictionary<string, string> validValues = null, string defaultValue = "", string fieldTitle = "")
        {
            CreateMetadata(AddOnID + "Cnf", field, fieldType, size, validValues, defaultValue, fieldTitle);
        }

        public void ExecuteStatement(string sqlScript, params string[] variables)
        {
            string sql = GetSQL(sqlScript, variables);
   
            Recordset recordSet = null;
            try
            {
                recordSet = (Recordset)Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                recordSet.DoQuery(sql);
            }
            finally
            {
                Marshal.ReleaseComObject(recordSet);
                GC.Collect();
            }
        }

        public T ExecuteSqlForObject<T>(string sqlScript, params string[] variables)
        {
            string sql = GetSQL(sqlScript, variables);
   
            Type type = typeof(T);
            Recordset recordSet = null;
            try
            {
                recordSet = (Recordset)Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                recordSet.DoQuery(sql);
                if (!recordSet.EoF)
                {
                    if (!isNotCoreType(type))
                    {
                        object obj = recordSet.Fields.Item(0).Value;

                        if (type == typeof(bool))
                        {
                            if (obj.GetType() != typeof(Int32))
                            {
                                String errMsg = String.Format("Object of type {0}, needs to be integer for SQL object of type {1}", obj.GetType(), type);
                                throw new ArgumentException(errMsg);
                            }

                            return (T)((Convert.ToInt32(obj) != 0) as object);
                        }
                        else
                        {
                            if (obj.GetType() != type)
                            {
                                String errMsg = String.Format("Object of type {0}. SQL object type is {1}", obj.GetType(), type);
                                throw new ArgumentException(errMsg);
                            }
                            return (T)obj;
                        }
                    }
                    else
                    {
                        var ret = PrepareObject<T>(recordSet);
                        return ret;
                    }
                }
                return default(T);
            }
            catch (Exception e)
            {
                throw new Exception(sqlScript + " - " + e.Message);
            }
            finally
            {
                Marshal.ReleaseComObject(recordSet);
                GC.Collect();
            }
        }

        public List<T> ExecuteSqlForList<T>(string sqlScript, params string[] variables)
        {
            string sql = GetSQL(sqlScript, variables);
   
            var lst = new List<T>();
            Type type = typeof(T);
            Recordset recordSet = (Recordset)Company.GetBusinessObject(BoObjectTypes.BoRecordset);

            try
            {
                recordSet.DoQuery(sql);
                while (!recordSet.EoF)
                {
                    T obj;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        obj = (T)Activator.CreateInstance(type, new[] {recordSet.Fields.Item(0).Value, recordSet.Fields.Item(1).Value.ToString() });                        
                    }
                    else if (isNotCoreType(type))
                        obj = PrepareObject<T>(recordSet);
                    else
                    {
                        if (recordSet.Fields.Item(0).Value.GetType() != type)
                        {
                            String errMsg = String.Format("Object of type {0}. SQL object type is {1}", recordSet.Fields.Item(0).Value.GetType(), type);
                            throw new Exception(errMsg);
                        }
                        obj = (T)recordSet.Fields.Item(0).Value;
                    }
                    lst.Add(obj);
                    recordSet.MoveNext();
                }
                return lst;
            }
            catch (Exception e)
            {
                throw new Exception(sqlScript + " - " + e.Message);
            }
            finally
            {
                Marshal.ReleaseComObject(recordSet);
                GC.Collect();
            }
        }

        public void ExecuteSQLForMatrix(string sqlScript, Matrix matrix, DataTable dataTable, params string[] variables)
        {
            ExecuteSQLForMatrix(sqlScript, matrix, dataTable, false, variables);
        }

        public void ExecuteSQLForMatrix(string sqlScript, Matrix matrix, DataTable dataTable, bool addLastLine, params string[] variables)
        {
            string sql = GetSQL(sqlScript, variables);

            //Verifficar locais onde é usado o AddOn.algumacoisa, pois nem sempre estará instanciado. Mover o CurrentDirectory. Colocar manifest em todos os addons para que rodem como adm
            //File.WriteAllText(AddOn.Instance.CurrentDirectory + "//SQLLog.txt", sql);

            try
            {
                dataTable.ExecuteQuery(sql);

                matrix.LoadFromDataSource();

                if (addLastLine)
                {
                    matrix.AddRow();
                    matrix.ClearRowData(matrix.RowCount);

                    if (matrix.Columns.Item(0).Description == "Pos")
                    {
                        ((EditText)matrix.Columns.Item(0).Cells.Item(matrix.RowCount).Specific).String = matrix.RowCount.ToString();
                    }

                    matrix.AutoResizeColumns();
                }
            }
            catch (Exception e)
            {
                throw new Exception(sqlScript + " - " + e.Message);
            }
        }

        public void ExecuteSQLForGrid(string sqlScript, Grid grid, params string[] variables)
        {
            string sql = GetSQL(sqlScript, variables);
            try
            {
                grid.DataTable.ExecuteQuery(sql);
            }
            catch (Exception e)
            {
                throw new Exception(sqlScript + " - " + e.Message);
            }
        }

        public void ExecuteSQLForForm(string sqlScript, DataTable dataTable, params string[] variables)
        {
            string sql = GetSQL(sqlScript, variables);
            try
            {
                dataTable.ExecuteQuery(sql);

                if (dataTable.Rows.Count == 0)
                    dataTable.Rows.Add();
            }
            catch (Exception e)
            {
                throw new Exception(sqlScript + " - " + e.Message);
            }
        }

        private string GetSQL(string sqlScript, params string[] variables)
        {            
            using (var stream = new MemoryStream(File.ReadAllBytes(AddOn.Instance.CurrentDirectory + "//SQL//" + DBServerType + "//" + sqlScript + ".sql")))
            {
                if (stream != null)
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        string sql = string.Format(streamReader.ReadToEnd(), variables);

                        return sql;
                    }
                }
            }
            return string.Empty;
        }

        #region private methods

        private T PrepareObject<T>(Recordset oRs)
        {
            Type type = typeof(T);

            T obj = (T)Activator.CreateInstance(type);
            for (int i = 0; i < oRs.Fields.Count; i++)
            {
                string name = oRs.Fields.Item(i).Name;
                object value = oRs.Fields.Item(i).Value;
                var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null)
                {
                    String errMsg = String.Format("Object {0} does not have property {1}", type, name);
                    throw new Exception(errMsg);
                }
                if (prop.PropertyType != value.GetType() && !prop.PropertyType.IsEnum && prop.PropertyType != typeof(Boolean))
                {
                    String errMsg = String.Format("Object {0} has property {1} of type {2}. Statement object type is {3}.", type, name, prop.PropertyType, value.GetType());
                    throw new Exception(errMsg);
                }

                if (prop.PropertyType == typeof(Boolean))
                {
                    if (value.GetType() == typeof(string))
                        prop.SetValue(obj, value.ToString().Equals("Y") ? true : false, null);
                    else
                        prop.SetValue(obj, Convert.ToBoolean(value), null);
                }
                else if (prop.PropertyType.IsEnum)
                    prop.SetValue(obj, Convert.ChangeType(value, Enum.GetUnderlyingType(prop.PropertyType)), null);
                else
                    prop.SetValue(obj, value, null);
            }
            return obj;
        }

        private List<FieldMetadata> PrepareObject<T>() where T : Model.BaseModel
        {
            List<FieldMetadata> result = new List<FieldMetadata>();

            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            string table = type.Name.Replace("Model", "");

            foreach (var prop in props)
            {
                Model.BaseModel.NonDB nonDB = prop.GetCustomAttribute(typeof(Model.BaseModel.NonDB)) as Model.BaseModel.NonDB;

                if (nonDB == null)
                {
                    FieldMetadata fieldMetadata = new FieldMetadata();
                    fieldMetadata.Table = table;
                    fieldMetadata.Field = prop.Name;

                    if (prop.PropertyType == typeof(Boolean))
                    {
                        fieldMetadata.FieldType = FieldTypeEnum.Alphanumeric;
                        fieldMetadata.Size = 1;
                        fieldMetadata.ValidValues.Add("Y", "Sim");
                        fieldMetadata.ValidValues.Add("N", "Não");
                    }
                    else if (prop.PropertyType == typeof(String))
                    {
                        Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                        if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Phone)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Phone;
                        }
                        else if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Memo)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Memo;
                        }
                        else if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Image)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Image;
                        }
                        else
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Alphanumeric;

                            Model.BaseModel.Size size = prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) as Model.BaseModel.Size;

                            if (size != null)
                                fieldMetadata.Size = size.Value;
                            else
                                fieldMetadata.Size = 1;
                        }
                    }
                    else if (prop.PropertyType == typeof(Int32))
                    {
                        fieldMetadata.FieldType = FieldTypeEnum.Integer;
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        fieldMetadata.FieldType = FieldTypeEnum.Integer;

                        var enumValues = Enum.GetValues(prop.PropertyType);

                        foreach (var enumValue in enumValues)
                        {
                            fieldMetadata.ValidValues.Add(enumValue.ToString(), Model.EnumOperation.GetEnumDescription(enumValue));
                        }
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                        if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Time)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Time;
                        }
                        else
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Date;
                        }
                    }
                    else if (prop.PropertyType == typeof(Double))
                    {
                        Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                        if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Quantity)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Quantity;
                        }
                        else if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Percent)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Percentage;
                        }
                        else if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Price)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Price;
                        }
                        else if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Rate)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Rate;
                        }
                        else if (specificType != null && specificType.Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Measurement)
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Measurement;
                        }
                        else
                        {
                            fieldMetadata.FieldType = FieldTypeEnum.Quantity;
                        }
                    }

                    result.Add(fieldMetadata);
                }
            }

            return result;
        }

        private bool isNotCoreType(Type type)
        {
            return (type != typeof(object) && Type.GetTypeCode(type) == TypeCode.Object);
        }

        #endregion
    }

    public class FieldMetadata
    {
        public string Table { get; set; }
        public string Field { get; set; }
        public FieldTypeEnum FieldType { get; set; }
        public int Size { get; set; }
        public Dictionary<string, string> ValidValues { get; set; }
        public string DefaultValue { get; set; }

        public FieldMetadata()
        {
            Table = string.Empty;
            Field = string.Empty;
            FieldType = FieldTypeEnum.Alphanumeric;
            Size = 1;
            ValidValues = new Dictionary<string, string>();
            DefaultValue = string.Empty;
        }
    }

    public enum FieldTypeEnum
    {
        //[Alphanumeric + st_Nome]
        Alphanumeric = 1,
        //[Alphanumeric + st_Phone]
        Phone = 2,
        //[Alphanumeric + st_Address]        
        Address = 3,
        //[Alphanumeric + st_Image]        
        Image = 4,
        //[Date + st_None]
        Date = 5,
        //[Date + st_Time]
        Time = 6,
        //[Float + st_Measurement]
        Measurement = 7,
        //[Float + st_Percentage]
        Percentage = 8,
        //[Float + st_Price]
	    Price = 9,
        //[Float + st_Quantity]
	    Quantity = 10,
        //[Float + st_Rate]
	    Rate = 11,
        //[Float + st_Sum]
	    Sum = 12,
        //[Memo + st_None]
        Memo = 13,
        //[Memo + st_Link]
        Link = 14,
        //[Numeric + SubSize 7~11]
        Integer = 15,
        //[Numeric + SubSize 1~6]
        Smallint = 16,
    }
}
