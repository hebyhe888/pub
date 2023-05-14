/*命名空间: IPolarDBDataAccess
*
* 功 能： 定义一构建ORM对象(CURD)
* 类 名： 自定义通过实体构建新增&修改SQL!!!!
* Author   : HebyHe
* Created  : 16-04-2023 13:40:21
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
*  Copyright (c) 2022  All rights reserved.
*┌──────────────────────────────────┐
*│　此技术信息为本公司机密信息，未经本公司书面同意禁止向第三方披露．　│
*│　版权所有：XXXXX                                        │
*└──────────────────────────────────┘
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data;
using System.Reflection;
using Microsoft.Extensions.DiagnosticAdapter.Infrastructure;
using ServiceStack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;

namespace Utils
{
    public class SqlConstructor
    {
        public static IConfiguration _Iconfiguration { get; set; }
        public static string _hanadb { get; set; }
        static SqlConstructor()
        {
            string Path = "appsettings.json";
            _Iconfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Add(new JsonConfigurationSource
                {
                    Path = Path,
                    Optional = false,
                    ReloadOnChange = true
                })
                .Build();// 这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
            _hanadb = _Iconfiguration.GetSection("SAP_Configuration:HanaConnection:DataSource").Value;
        }
        /// <summary>
        /// Update构造器(单关键字更新)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        [Obsolete(message: "方法已过期,请使用新方法UpdatePlus.")]
        public static string Update<T>(T model) where T : class, new()
        {
            var primarykey = string.Empty;
            var primaryval = string.Empty;
            StringBuilder sqlbuilder = new StringBuilder();
            var tablename = model.GetType().Name;
            sqlbuilder.Append($"update {tablename} set ");
            System.Reflection.PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            propertyInfos.ToList().ForEach(pro =>
            {
                var fieldname = pro.Name;
                var fieldvalue = pro.GetValue(model, null);
                var customattributes = pro.CustomAttributes.ToList();
                if (customattributes.Count > 0)
                {
                    foreach (var customattribute in customattributes)
                    {
                        if (customattribute.AttributeType.Name == "ExplicitKeyAttribute")
                        {
                            primarykey = fieldname;
                            primaryval = (fieldvalue == null ? "" : fieldvalue.ToString());
                            break;
                        }
                    }
                }
                if (pro.PropertyType.FullName == typeof(string).FullName)
                {
                    if (fieldvalue != null)
                    {
                        if (!fieldname.ToUpper().Contains("_DATE"))
                        {
                            sqlbuilder.Append($"{fieldname}='{fieldvalue}',");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(fieldvalue.ToString()))
                            {
                                //如果长度为10位
                                if (fieldvalue != null && fieldvalue.ToString().Length == 10)
                                {
                                    sqlbuilder.Append($"{fieldname}=to_date('{fieldvalue}','yyyy-MM-dd'),");
                                }
                                else
                                {
                                    sqlbuilder.Append($"{fieldname}=to_date('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','yyyy-MM-dd HH24:mi:ss'),");
                                }
                            }
                            else
                            {
                                sqlbuilder.Append($"{fieldname}=null,");
                            }
                        }
                    }
                }
                else if (pro.PropertyType.FullName == typeof(DateTime).FullName)
                {
                    sqlbuilder.Append($"{fieldname}=to_date('{Convert.ToDateTime(fieldvalue).ToString("yyyy-MM-dd HH:mm:ss")}','yyyy-MM-dd HH24:mi:ss'),");
                }
                else if (pro.PropertyType.FullName == typeof(decimal).FullName)
                {
                    sqlbuilder.Append($"{fieldname}={fieldvalue},");
                }
            });
            if (string.IsNullOrEmpty(primarykey)) { throw new Exception($"table \"{tablename}\" No set Primay Key."); }
            int n = sqlbuilder.ToString().LastIndexOf(",");
            sqlbuilder.Remove(n, 1);
            sqlbuilder.Append($" where {primarykey}='{primaryval}'");
            return sqlbuilder.ToString();
        }

        /// <summary>
        /// Update构造器扩展(单关键字更新,支持多关键字更新)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="ConnType">1:PolarDB;5:HANA</param>
        /// <param name="fields">条件字段(为空则默认取带ExplicitKey属性字段,不为空则取当前可变参数字段为更新条件)</param>
        /// <returns></returns>
        public static string UpdatePlus<T>(T model, int ConnType = 1, params string[] fields) where T : class, new()
        {
            var primarykey = string.Empty;
            var primaryval = string.Empty;
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            StringBuilder sqlbuilder = new StringBuilder();
            var tablename = model.GetType().Name;
            if (ConnType != 5)
            {
                sqlbuilder.Append($"update {tablename} set ");
            }
            else
            {
                sqlbuilder.Append(@$"update ""{_hanadb}"".""@{tablename}"" set");
            }
            System.Reflection.PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            propertyInfos.ToList().ForEach(pro =>
            {
                bool isExists = false;
                var fieldname = ConnType != 5 ? pro.Name : (@$"""{pro.Name}""");
                var fieldvalue = pro.GetValue(model, null);
                var customattributes = pro.CustomAttributes.ToList();
                if (customattributes.Count > 0 && fields.ToList().Count == 0)
                {
                    foreach (var customattribute in customattributes)
                    {
                        if (customattribute.AttributeType.Name == "ExplicitKeyAttribute")
                        {
                            primarykey = fieldname;
                            primaryval = (fieldvalue == null ? "" : fieldvalue.ToString());
                            keyValues.Add(primarykey, primaryval);
                            break;
                        }
                    }
                }
                if (IsWriteProperty(pro))
                {
                    if (pro.PropertyType.FullName == typeof(string).FullName)
                    {
                        if (fieldvalue != null)
                        {
                            if (!fieldname.ToUpper().Contains("_DATE"))
                            {
                                isExists = AddConditions(fields, ref keyValues, fieldname, fieldvalue);
                                if (!isExists)
                                    sqlbuilder.Append($"{fieldname}='{(string.IsNullOrEmpty(fieldvalue.ToString()) ? fieldvalue : fieldvalue.ToString().Replace("'", "''"))}',");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(fieldvalue.ToString()))
                                {
                                    isExists = AddConditions(fields, ref keyValues, fieldname, fieldvalue);
                                    if (!isExists)
                                    {
                                        //如果长度为10位
                                        if (fieldvalue.ToString().Length == 10)
                                        {
                                            sqlbuilder.Append($"{fieldname}=to_date('{fieldvalue}','yyyy-MM-dd'),");
                                        }
                                        else
                                        {
                                            sqlbuilder.Append($"{fieldname}=to_date('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','yyyy-MM-dd HH24:mi:ss'),");
                                        }
                                    }
                                }
                                else
                                {
                                    sqlbuilder.Append($"{fieldname}=null,");
                                }
                            }
                        }
                    }
                    else if (pro.PropertyType.FullName == typeof(DateTime).FullName)
                    {
                        sqlbuilder.Append($"{fieldname}=to_date('{Convert.ToDateTime(fieldvalue).ToString("yyyy-MM-dd HH:mm:ss")}','yyyy-MM-dd HH24:mi:ss'),");
                    }
                    else if (pro.PropertyType.FullName == typeof(decimal).FullName
                    || pro.PropertyType.FullName == typeof(decimal?).FullName
                    || pro.PropertyType.FullName == typeof(Int32).FullName
                    || pro.PropertyType.FullName == typeof(Int32?).FullName
                    || pro.PropertyType.FullName == typeof(float).FullName
                    || pro.PropertyType.FullName == typeof(float?).FullName)
                    {
                        if (fieldvalue != null)
                        {
                            isExists = AddConditions(fields, ref keyValues, fieldname, fieldvalue);
                            if (!isExists)
                                sqlbuilder.Append($"{fieldname}={fieldvalue},");
                        }
                    }
                }
            });
            if (keyValues.Count == 0)
            {
                throw new Exception($"table \"{tablename}\" No set Primay Key.");
            }
            int n = sqlbuilder.ToString().LastIndexOf(",");
            sqlbuilder.Remove(n, 1);
            sqlbuilder.Append($" where 1=1 ");
            if (keyValues.Count > 0)
            {
                foreach (KeyValuePair<string, string> kv in keyValues)
                {
                    if (!kv.Key.ToUpper().Contains("_DATE"))
                        sqlbuilder.Append($" and {kv.Key}='{kv.Value}'");
                    else
                        sqlbuilder.Append($" and {kv.Key}=to_date('{kv.Value}','yyyy-MM-dd')");
                }
            }
            return sqlbuilder.ToString();
        }

        private static bool AddConditions(string[] fields, ref Dictionary<string, string> keyValues, string fieldname, object? fieldvalue)
        {
            bool flag = false;
            if (fields.ToList().Count != 0)
            {
                var tempfield = fields.ToList().Find(field => field.ToUpper() == fieldname.ToUpper());
                if (tempfield != null)
                {
                    flag = true;
                    keyValues.Add(tempfield, fieldvalue.ToString());
                }
            }
            return flag;
        }

        /// <summary>
        /// Insert构造器
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="model"></param>
        /// <param name="ConnType">1:PolarDB;5:HANA</param>
        /// <returns></returns>
        public static string Insert<T>(T model, int ConnType = 1) where T : class, new()
        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strSql1 = new StringBuilder();
            StringBuilder strSql2 = new StringBuilder();
            var tablename = model.GetType().Name;
            System.Reflection.PropertyInfo[] propertyInfos = model.GetType().GetProperties();
            propertyInfos.ToList().ForEach(pro =>
            {
                var fieldname = ConnType != 5 ? pro.Name : ($@"""{pro.Name}""");
                var fieldvalue = pro.GetValue(model, null);
                #region ******对其他实体类或者无关字段不作写入WriteAttribute(false)******
                bool flag = IsWriteProperty(pro);
                #endregion
                if (flag == true)
                {
                    if (pro.PropertyType.FullName == typeof(string).FullName)
                    {
                        if (fieldvalue != null)
                        {
                            if (!fieldname.ToUpper().Contains("_DATE"))
                            {
                                strSql1.Append($"{fieldname},");
                                strSql2.Append($"'{(string.IsNullOrEmpty(fieldvalue.ToString()) ? fieldvalue : fieldvalue.ToString().Replace("'", "''"))}',");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(fieldvalue.ToString()))
                                {
                                    if (fieldvalue.ToString().Length > 10)
                                    {
                                        strSql1.Append($"{fieldname},");
                                        strSql2.Append($"to_date('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','yyyy-MM-dd HH24:mi:ss'),");
                                    }
                                    else
                                    {
                                        strSql1.Append($"{fieldname},");
                                        strSql2.Append($"to_date('{fieldvalue}','yyyy-MM-dd'),");
                                    }
                                }
                                else
                                {
                                    strSql1.Append($"{fieldname},");
                                    strSql2.Append($"null,");
                                }
                            }
                        }
                    }
                    else if (pro.PropertyType.FullName == typeof(DateTime).FullName)
                    {
                        strSql1.Append($"{fieldname},");
                        strSql2.Append($"to_date('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','yyyy-MM-dd HH24:mi:ss'),");
                    }
                    else if (pro.PropertyType.FullName == typeof(Int32).FullName ||
                    pro.PropertyType.FullName == typeof(Int32?).FullName ||
                    pro.PropertyType.FullName == typeof(Int64?).FullName ||
                    pro.PropertyType.FullName == typeof(Int64).FullName ||
                    pro.PropertyType.FullName == typeof(decimal).FullName ||
                    pro.PropertyType.FullName == typeof(decimal?).FullName
                    || pro.PropertyType.FullName == typeof(float).FullName
                    || pro.PropertyType.FullName == typeof(float?).FullName)
                    {
                        if (fieldvalue != null)
                        {
                            strSql1.Append($"{fieldname},");
                            strSql2.Append($"{fieldvalue},");
                        }
                        else
                        {
                            strSql1.Append($"{fieldname},");
                            strSql2.Append($"null,");
                        }
                    }
                }
            });
            if (ConnType != 5)
            {
                strSql.Append($"insert into {tablename} (");
            }
            else
            {
                strSql.Append($@"insert into ""{_hanadb}"".""@{tablename}"" (");
            }
            strSql.Append(strSql1.ToString().Remove(strSql1.Length - 1));
            strSql.Append(")");
            strSql.Append(" values (");
            strSql.Append(strSql2.ToString().Remove(strSql2.Length - 1));
            strSql.Append(")");
            return strSql.ToString();
        }
        

        /// <summary>
        /// Delete构造器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="fieldnames">可选字段名数组</param>
        /// <returns></returns>
        public static string Delete<T>(T model, params string[] fieldnames) where T : class, new()
        {
            var primarykey = string.Empty;
            var primaryval = string.Empty;
            var strSql = new StringBuilder();
            var tablename = model.GetType().Name;
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            System.Reflection.PropertyInfo[] propertyInfos = model.GetType().GetProperties();
            propertyInfos.ToList().ForEach(pro =>
            {
                var fieldname = pro.Name;
                var fieldvalue = pro.GetValue(model, null);
                var customattributes = pro.CustomAttributes.ToList();
                if (customattributes.Count > 0)
                {
                    foreach (var customattribute in customattributes)
                    {
                        if (customattribute.AttributeType.Name == "ExplicitKeyAttribute")
                        {
                            primarykey = fieldname;
                            primaryval = fieldvalue == null ? "" : fieldvalue.ToString();
                            keyValues.Add(primarykey, primaryval);
                            break;
                        }
                    }
                }
            });
            if (keyValues.Count > 0)
            {
                strSql.Append($"delete from {tablename} where 1=1 ");
                keyValues.ToList().ForEach(kv =>
                {
                    if (fieldnames != null && fieldnames.ToList().Count > 0)
                    {
                        var isExists = fieldnames.ToList().Find(field => field.ToUpper() == kv.Key.ToUpper()) != null;
                        if (isExists)
                        {
                            strSql.Append($" and {kv.Key}='{kv.Value}'");
                        }
                    }
                    else
                        strSql.Append($" and {kv.Key}='{kv.Value}'");
                });
            }
            else
            {
                throw new Exception("No set primary key.");
            }
            return strSql.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="ConnType">1:PolarDB;5:Hana</param>
        /// <param name="fieldnames"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string Delete<T>(T model, int ConnType = 1, params string[] fieldnames) where T : class, new()
        {
            var primarykey = string.Empty;
            var primaryval = string.Empty;
            var strSql = new StringBuilder();
            var tablename = model.GetType().Name;
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            System.Reflection.PropertyInfo[] propertyInfos = model.GetType().GetProperties();
            propertyInfos.ToList().ForEach(pro =>
            {
                var fieldname = ConnType == 1 ? pro.Name : (@$"""{pro.Name}""");
                var fieldvalue = pro.GetValue(model, null);
                var customattributes = pro.CustomAttributes.ToList();
                if (customattributes.Count > 0)
                {
                    foreach (var customattribute in customattributes)
                    {
                        if (customattribute.AttributeType.Name == "ExplicitKeyAttribute")
                        {
                            primarykey = fieldname;
                            primaryval = fieldvalue == null ? "" : fieldvalue.ToString();
                            keyValues.Add(primarykey, primaryval);
                            break;
                        }
                    }
                }
            });
            if (keyValues.Count > 0)
            {
                if (ConnType == 1)
                    strSql.Append($"delete from {tablename} where 1=1 ");
                else
                    strSql.Append(@$"delete from ""{_hanadb}"".""@{tablename}"" where 1=1 ");
                keyValues.ToList().ForEach(kv =>
                {
                    if (fieldnames != null && fieldnames.ToList().Count > 0)
                    {
                        var isExists = fieldnames.ToList().Find(field => field == kv.Key) != null;
                        if (isExists)
                        {
                            strSql.Append($" and {kv.Key}='{kv.Value}'");
                        }
                    }
                    else
                        strSql.Append($" and {kv.Key}='{kv.Value}'");
                });
            }
            else
            {
                throw new Exception("No set primary key.");
            }
            return strSql.ToString();
        }
        /// <summary>
        /// 单表查询构造器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="objprefix">对象前缀(默认为空)</param>
        /// <returns></returns>
        public static StringBuilder BuilderCondition<T>(T model, string objprefix = "") where T : class
        {
            var condition = new StringBuilder();
            var prifix = !string.IsNullOrEmpty(objprefix) ? $"{objprefix}." : "";
            System.Reflection.PropertyInfo[] propertyInfos = model.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                var fieldname = propertyInfo.Name;
                var fieldvalue = propertyInfo.GetValue(model, null);
                if (propertyInfo.PropertyType.FullName == typeof(string).FullName)
                {
                    if (fieldvalue != null && !string.IsNullOrEmpty(fieldvalue.ToString()))
                    {
                        if (!fieldname.ToUpper().Contains("_DATE") && !fieldname.ToUpper().Contains("_TIME"))
                        {
                            condition.Append($" and {prifix}{fieldname} like '{fieldvalue}%'");
                        }
                        else
                        {
                            var daterr = fieldvalue.ToString().Replace(";", ",").Split(",");
                            var startdate = daterr[0];
                            var enddate = daterr[1];
                            condition.Append($" and trunc({prifix}{fieldname})>to_date('{startdate}','yyyy-MM-dd') and trunc({prifix}{fieldname})<to_date('{enddate}','yyyy-MM-dd')");
                        }
                    }
                }
                else if (propertyInfo.PropertyType.FullName == typeof(Int32).FullName ||
                    propertyInfo.PropertyType.FullName == typeof(decimal).FullName ||
                    ((propertyInfo.PropertyType.FullName == typeof(Int32?).FullName ||
                    propertyInfo.PropertyType.FullName == typeof(decimal?).FullName) && fieldvalue != null))
                {
                    condition.Append($" and {prifix}{fieldname}='{fieldvalue}'");
                }
            }
            return condition;
        }

        /// <summary>
        /// 是否是写入数据库属性
        /// </summary>
        /// <param name="pro"></param>
        /// <returns></returns>
        public static bool IsWriteProperty(PropertyInfo pro)
        {
            var customattributes = pro.CustomAttributes.ToList();
            var flag = true;
            if (customattributes.Count > 0)
            {
                foreach (var customattribute in customattributes)
                {
                    if (customattribute.AttributeType.Name == "WriteAttribute")
                    {
                        foreach (var param in customattribute.ConstructorArguments.ToList())
                        {
                            if (param.ArgumentType == typeof(Boolean))
                            {
                                var paramvalue = param.Value;
                                if (paramvalue != null)
                                {
                                    flag = (bool)paramvalue;
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return flag;
        }
    }
}
