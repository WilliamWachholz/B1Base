using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Data;
using SAPbobsCOM;

namespace B1Base.DAO
{
    public class BaseDAO<T> where T : Model.BaseModel
    {        
        public virtual string TableName
        {
            get
            {
                return this.GetType().Name.Replace("DAO", "").ToUpper();
            }
        }

        public T Get(int code)
        {
            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            T model = (T)Activator.CreateInstance(type);

            UserTable userTable = (UserTable)Controller.ConnectionController.Instance.Company.UserTables.Item(TableName);
            try
            {
                if (userTable.GetByKey(code.ToString()))
                {
                    foreach (var prop in props)
                    {
                        Model.BaseModel.NonDB nonDB = prop.GetCustomAttribute(typeof(Model.BaseModel.NonDB)) as Model.BaseModel.NonDB;

                        if (nonDB == null)
                        {
                            if (prop.PropertyType == typeof(Boolean))
                            {
                                prop.SetValue(model, userTable.UserFields.Fields.Item("U_" + prop.Name).Value.ToString().Equals("Y"), null);
                            }
                            else if (prop.PropertyType.IsEnum)
                            {
                                prop.SetValue(model, Convert.ChangeType(userTable.UserFields.Fields.Item("U_" + prop.Name).Value, Enum.GetUnderlyingType(prop.PropertyType)), null);
                            }
                            else
                            {
                                prop.SetValue(model, userTable.UserFields.Fields.Item("U_" + prop.Name).Value);
                            }
                        }
                    }

                    return model as T;
                }
                else
                {
                    foreach (var prop in props)
                    {
                        Model.BaseModel.NonDB nonDB = prop.GetCustomAttribute(typeof(Model.BaseModel.NonDB)) as Model.BaseModel.NonDB;

                        if (nonDB == null)
                        {
                            if (prop.PropertyType == typeof(Boolean))
                            {
                                prop.SetValue(model, userTable.UserFields.Fields.Item("U_" + prop.Name).Value.ToString().Equals("Y"), null);
                            }
                            else if (prop.PropertyType.IsEnum)
                            {
                                prop.SetValue(model, Convert.ChangeType(userTable.UserFields.Fields.Item("U_" + prop.Name).Value, Enum.GetUnderlyingType(prop.PropertyType)), null);
                            }
                            else
                            {
                                prop.SetValue(model, userTable.UserFields.Fields.Item("U_" + prop.Name).Value);
                            }
                        }
                    }

                    return model as T;
                }
            }
            finally
            {
                Marshal.ReleaseComObject(userTable);
            }
            
        }

        public void SaveViaSL(T model)
        {
            
        }
        
        public void SaveViaInsert(T model)
        {
            Type type = typeof(T);

            if (model.Code > 0)
            {
                string update = @"update ""@" + TableName + @""" set ";

                var props = type.GetProperties().Where(r => r.Name != "Changed" && r.Name != "Code");

                foreach (var prop in props)
                {
                    if (prop.PropertyType == typeof(Boolean))
                    {
                        update += @"""U_" + prop.Name + @""" = " + ( (bool)prop.GetValue(model) ? "'Y'" : "'N'" ) + ", ";
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        update += @"""U_" + prop.Name + @""" = " + ((int)prop.GetValue(model)).ToString() + ", ";
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(prop.GetValue(model)) == DateTime.MinValue || Convert.ToDateTime(prop.GetValue(model)) == new DateTime(1899, 12, 30))
                        {
                            if (Controller.ConnectionController.Instance.DBServerType == "HANA")
                                update += @"""U_" + prop.Name + @""" = " + "to_date(null)" + ", ";
                            else
                                update += @"""U_" + prop.Name + @""" = " + "cast(null as date)" + ", ";
                        }
                        else
                        {
                            if (prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) != null)
                            {
                                if ((prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType).Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Time)
                                {
                                    update += @"""U_" + prop.Name + @""" = " + Convert.ToDateTime(prop.GetValue(model)).ToString("HHmm") + ", ";
                                }
                            }
                            else
                            {
                                update += @"""U_" + prop.Name + @""" = " + "cast ('" + Convert.ToDateTime(prop.GetValue(model)).ToString("yyyy-MM-dd") + "' as date)" + ", ";
                            }
                        }
                    }
                    else if (prop.PropertyType == typeof(Int32))
                    {
                        update += @"""U_" + prop.Name + @""" = " + prop.GetValue(model).ToString() + ", ";
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        if (Convert.ToDouble(prop.GetValue(model)) == 0)
                        {
                            update += @"""U_" + prop.Name + @""" = 0.0, ";
                        }
                        else
                        {
                            Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                            int decimalDigits = 2;

                            if (specificType != null)
                            {
                                decimalDigits = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForObject<int>("GetDisplayDecimalDigits", ((int)specificType.Value).ToString());
                            }

                            update += @"""U_" + prop.Name + @""" = " + string.Format("cast({0} as decimal(15,{1}))", Convert.ToDouble(prop.GetValue(model)).ToString(new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." }), decimalDigits.ToString()) + ", ";
                        }
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        if (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) != null)
                            update += @"""U_" + prop.Name + @""" = " + "cast('" + prop.GetValue(model).ToString().Replace("'", "''") + "' as varchar(" + (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) as Model.BaseModel.Size).Value.ToString() + "))" + ", ";
                        else
                            update += @"""U_" + prop.Name + @""" = " + "cast('" + prop.GetValue(model).ToString().Replace("'", "''") + "' as nvarchar)" + ", ";
                    }
                }

                update = update.Substring(0, update.Length - 2);

                update += @" where ""U_Code"" = " + model.Code.ToString();

                Controller.ConnectionController.Instance.ExecuteStatementDirect(update);
            }
            else
            {
                var props = type.GetProperties().Where(r => r.Name != "Changed" && r.Name != "Code");

                string insert = @"insert into ""@" + TableName + @"""(";

                insert += @" ""Code"", ""Name"", ""U_Code"", ";

                foreach (var prop in props)
                {
                    if (!prop.PropertyType.IsGenericType)
                        insert += @"""U_" + prop.Name + @""", ";
                }

                insert = insert.Substring(0, insert.Length - 2) + ")";

                insert += " values (";                               

                //Type seqDAOType = Type.GetType(type.AssemblyQualifiedName.Replace("Model", "DAO").Replace(type.Name.Replace("Model", "DAO"), "ConfigSeqDAO"));

                //if (seqDAOType == null)
                //{
                //    model.Code = Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetLastCode", TableName, new ConfigSeqDAO().TableName);
                //}
                //else
                //{
                //    var dao = (DAO.ConfigSeqDAO)Activator.CreateInstance(seqDAOType);

                //    model.Code = Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetLastCode", TableName, dao.TableName);
                //}                

                //insert += string.Format(" {0}, ", model.Code);
                //insert += string.Format(" {0}, ", model.Code);
                //insert += string.Format(" {0}, ", model.Code);

                insert += @"coalesce((select max(""U_Code"") from ""@" + TableName + @"""), 0) + 1, ";

                string guid = Guid.NewGuid().ToString();

                insert += "'" + guid + "', ";  
                                    
                insert += @"coalesce((select max(""U_Code"") from ""@" + TableName + @"""), 0) + 1, ";

                foreach (var prop in props)
                {
                    if (prop.PropertyType == typeof(Boolean))
                    {
                        insert += ((bool)prop.GetValue(model) ? "'Y'" : "'N'") + ", ";
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        insert += ((int)prop.GetValue(model)).ToString() + ", ";
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(prop.GetValue(model)) == DateTime.MinValue || Convert.ToDateTime(prop.GetValue(model)) == new DateTime(1899, 12, 30))
                        {
                            if (Controller.ConnectionController.Instance.DBServerType == "HANA")                            
                                insert += "to_date(null)" + ", ";
                            else
                                insert += "cast(null as date)" + ", ";
                        }
                        else
                        {
                            if (prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) != null)
                            {
                                if ((prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType).Value == Model.BaseModel.SpecificType.SpecificTypeEnum.Time)
                                {
                                    insert += Convert.ToDateTime(prop.GetValue(model)).ToString("HHmm");
                                }
                            }
                            else
                            {
                                insert += "cast ('" + Convert.ToDateTime(prop.GetValue(model)).ToString("yyyy-MM-dd") + "' as date)" + ",";
                            }
                        }
                    }
                    else if (prop.PropertyType == typeof(Int32))
                    {
                        insert += prop.GetValue(model).ToString() + ", ";
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        if (Convert.ToDouble(prop.GetValue(model)) == 0)
                        {
                            insert += "0.0, ";
                        }
                        else
                        {
                            Model.BaseModel.SpecificType specificType = prop.GetCustomAttribute(typeof(Model.BaseModel.SpecificType)) as Model.BaseModel.SpecificType;

                            int decimalDigits = 2;

                            if (specificType != null)
                            {
                                decimalDigits = B1Base.AddOn.Instance.ConnectionController.ExecuteSqlForObject<int>("GetDisplayDecimalDigits", ((int)specificType.Value).ToString());
                            }

                            insert += string.Format("cast({0} as decimal(15,{1}))", Convert.ToDouble(prop.GetValue(model)).ToString(new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." }), decimalDigits.ToString()) + ", ";
                        }
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        if (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) != null)
                            insert += "cast('" + prop.GetValue(model).ToString().Replace("'", "''") + "' as varchar(" + (prop.GetCustomAttribute(typeof(Model.BaseModel.Size)) as Model.BaseModel.Size).Value.ToString() + "))" + ", ";
                        else
                            insert += "cast('" + prop.GetValue(model).ToString().Replace("'", "''") + "' as nvarchar)" + ", ";
                    }
                }

                insert = insert.Substring(0, insert.Length - 2) + ")";

                Controller.ConnectionController.Instance.ExecuteStatementDirect(insert);

                int code = Controller.ConnectionController.Instance.ExecuteSqlForDirectObject<int>(@"select ""Code"" from " + @"""@" + TableName + @""" where ""Name"" = '" + guid + "'");

                model.Code = code;

                Controller.ConnectionController.Instance.ExecuteStatementDirect(@"update ""@" + TableName + @""" set ""Name"" = ""Code"" where ""Name"" = '" + guid + "'");
            }
        }

        public void Save(T model, bool retry = false)
        {
            if (Controller.ConnectionController.Instance.ODBCConnection)
            {
                SaveViaInsert(model);
            }
            else
            {
                Type type = typeof(T);

                var props = type.GetProperties().Where(r => r.Name != "Changed" && r.Name != "Code");

                UserTable userTable = (UserTable)Controller.ConnectionController.Instance.Company.UserTables.Item(TableName);
                try
                {
                    if (userTable.GetByKey(model.Code.ToString()))
                    {
                        foreach (var prop in props)
                        {
                            Model.BaseModel.NonDB nonDB = prop.GetCustomAttribute(typeof(Model.BaseModel.NonDB)) as Model.BaseModel.NonDB;

                            if (nonDB == null)
                            {
                                if (prop.PropertyType == typeof(Boolean))
                                {
                                    userTable.UserFields.Fields.Item("U_" + prop.Name).Value = (bool)prop.GetValue(model) ? "Y" : "N";
                                }
                                else if (prop.PropertyType.IsEnum)
                                {
                                    userTable.UserFields.Fields.Item("U_" + prop.Name).Value = (int)prop.GetValue(model);
                                }
                                else
                                {
                                    userTable.UserFields.Fields.Item("U_" + prop.Name).Value = prop.GetValue(model);
                                }
                            }
                        }

                        userTable.Update();

                        Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                    }
                    else
                    {
                        foreach (var prop in props)
                        {
                            Model.BaseModel.NonDB nonDB = prop.GetCustomAttribute(typeof(Model.BaseModel.NonDB)) as Model.BaseModel.NonDB;

                            if (nonDB == null)
                            {
                                if (prop.PropertyType == typeof(Boolean))
                                {
                                    userTable.UserFields.Fields.Item("U_" + prop.Name).Value = (bool)prop.GetValue(model) ? "Y" : "N";
                                }
                                else if (prop.PropertyType.IsEnum)
                                {
                                    userTable.UserFields.Fields.Item("U_" + prop.Name).Value = (int)prop.GetValue(model);
                                }
                                else
                                {
                                    userTable.UserFields.Fields.Item("U_" + prop.Name).Value = prop.GetValue(model);
                                }
                            }
                        }

                        if (model.Code == 0)
                        {
                            Type seqDAOType = Type.GetType(type.AssemblyQualifiedName.Replace("Model", "DAO").Replace(type.Name.Replace("Model", "DAO"), "ConfigSeqDAO"));

                            if (seqDAOType == null)
                                model.Code = Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetLastCode", TableName, new ConfigSeqDAO().TableName);
                            else
                            {
                                var dao = (DAO.ConfigSeqDAO)Activator.CreateInstance(seqDAOType);

                                model.Code = Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetLastCode", TableName, dao.TableName);
                            }

                        }

                        userTable.UserFields.Fields.Item("U_Code").Value = model.Code;
                        userTable.Code = model.Code.ToString();
                        userTable.Name = model.Code.ToString();

                        userTable.Add();

                        try
                        {
                            Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                        }
                        catch
                        {
                            if (retry)
                            {
                                userTable.Add();

                                Type seqDAOType = Type.GetType(type.AssemblyQualifiedName.Replace("Model", "DAO").Replace(type.Name.Replace("Model", "DAO"), "ConfigSeqDAO"));

                                if (seqDAOType == null)
                                    model.Code = Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetLastCode", TableName, new ConfigSeqDAO().TableName);
                                else
                                {
                                    var dao = (DAO.ConfigSeqDAO)Activator.CreateInstance(seqDAOType);

                                    model.Code = Controller.ConnectionController.Instance.ExecuteSqlForObject<int>("GetLastCode", TableName, dao.TableName);
                                }

                                Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                            }
                        }
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(userTable);
                    GC.Collect();
                }
            }
        }

        public void Delete(T model)
        {
            if (Controller.ConnectionController.Instance.ODBCConnection)
            {
                Controller.ConnectionController.Instance.ExecuteStatementDirect(@"DELETE FROM ""@" + TableName + @""" WHERE ""U_Code"" = " + model.Code.ToString());
            }
            else
            {
                Type type = typeof(T);

                UserTable userTable = (UserTable)Controller.ConnectionController.Instance.Company.UserTables.Item(TableName);
                try
                {
                    if (userTable.GetByKey(model.Code.ToString()))
                    {
                        userTable.Remove();
                    }

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                }
                finally
                {
                    Marshal.ReleaseComObject(userTable);
                    GC.Collect();
                }
            }
        }

        public void UpdateField(int code, string field, object value)
        {
            UserTable userTable = (UserTable)Controller.ConnectionController.Instance.Company.UserTables.Item(TableName);
            try
            {
                if (userTable.GetByKey(code.ToString()))
                {
                    userTable.UserFields.Fields.Item("U_" + field).Value = value;

                    userTable.Update();

                    Controller.ConnectionController.Instance.VerifyBussinesObjectSuccess();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(userTable);
                GC.Collect();
            }            
        }
    }
}
