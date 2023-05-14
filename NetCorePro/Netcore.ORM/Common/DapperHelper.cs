/*命名空间: IPolarDBDataAccess
*
* 功 能： 自定义构建通用Dapper帮助类
* 类 名： DapperHelper
* Author   : HebyHe
* Created  : 21 - 09 - 2022 13:40:21
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
*  Copyright (c) 2022 All rights reserved.
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
using System.Data;
using Dapper;
using Dapper.Contrib;
using DBUtility.Dapper.Common;
using Dapper.Contrib.Extensions;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;
using ServiceStack;
using System.IO.Enumeration;
using Microsoft.AspNetCore.Connections;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System.Data.Common;

namespace DBUtility.ORM.Dapper
{
    public class DapperHelper
    {
        public static IConfiguration _Iconfiguration { get; set; }
        static DapperHelper()
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
        }

        /// <summary>
        /// 获取单个对象(多个关键字)
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="model">实体对象</param>
        /// <returns></returns>
        public static T Get<T>(T model, ConnType connType = ConnType.MYSQL) where T : class
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                var primarykey = string.Empty;
                var primaryval = string.Empty;
                var condition = new StringBuilder();
                Dictionary<string, string> keyValues = new Dictionary<string, string>();
                var tablename = typeof(T).Name;
                PropertyInfo[] propertyInfos = typeof(T).GetProperties();
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
                                keyValues.Add(primarykey, primaryval);
                                break;
                            }
                        }
                    }
                });
                if (keyValues.AsList().Count > 0)
                {
                    keyValues.AsList().ForEach(param =>
                    {
                        condition.Append($" and {param.Key}='{param.Value}'");
                    });
                }
                return connection.QueryFirstOrDefault<T>($"select *from {tablename} where 1=1 {condition.ToString()}");
            }
        }
        /// <summary>
        /// 获取单个对象列表(单个对象)
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="model">实体对象</param>
        /// <param name="likefieldnames">可模糊查询字段</param>
        /// <returns></returns>
        public static List<T> GetSingleList<T>(T model, params string[] likefieldnames) where T : class
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                var conditon = new StringBuilder();
                var TableName = typeof(T).Name;
                PropertyInfo[] propertyInfos = typeof(T).GetProperties();
                propertyInfos.ToList().ForEach(pro =>
                {
                    var fieldname = pro.Name;
                    var fieldvalue = pro.GetValue(model, null);
                    if (pro.PropertyType.Name == typeof(string).Name || pro.PropertyType.Name.Equals(typeof(int).Name))
                    {
                        if (Utils.SqlConstructor.IsWriteProperty(pro))
                        {
                            if (fieldname.ToUpper().Contains("_DATE") || fieldname.ToUpper().Contains("_TIME"))
                            {
                                if (fieldvalue != null && !string.IsNullOrWhiteSpace(fieldvalue.ToString()))
                                {
                                    if (fieldvalue.ToString().Replace(":", ",").Replace(";", ",").Contains(","))
                                    {
                                        var daterange = fieldvalue.ToString().Split(",");
                                        conditon.Append($" and {fieldname}>=to_date('{daterange[0]}','yyyy-MM-dd')")
                                                .Append($" and {fieldname}<=to_date('{daterange[1]}','yyyy-MM-dd')");
                                    }
                                }
                            }
                            else
                            {
                                if (fieldvalue != null)
                                {
                                    string? tempfield = likefieldnames.ToList().Find(field => field.ToUpper() == fieldname.ToUpper());
                                    if (tempfield != null)
                                        conditon.Append($" and {fieldname} like '%{fieldvalue}%'");
                                    else
                                        conditon.Append($" and {fieldname}='{fieldvalue}'");
                                }
                            }
                        }
                    }
                });
                return connection.Query<T>($"select *from {TableName} where 1=1 {conditon.ToString()}").ToList();
            }
        }
        /// <summary>
        /// 获取对象列表通过关键字(支持动态关键字查找)
        /// </summary>
        /// <typeparam name="T">返回实体类型</typeparam>
        /// <param name="model">对象实体</param>
        /// <param name="keyfilednames">[ExplicitKey]属性所属关键字</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<T> GetList<T>(T model, params string[] keyfilednames) where T : class
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            var condition = new StringBuilder();
            var tablename = typeof(T).Name;
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                PropertyInfo[] propertyInfos = typeof(T).GetProperties();
                propertyInfos.ToList().ForEach(pro =>
                {
                    var fieldname = pro.Name;
                    var fieldvalue = pro.GetValue(model, null);
                    var customattributes = pro.CustomAttributes.ToList();
                    if (customattributes.Count > 0)
                    {
                        customattributes.ForEach(customattribute =>
                        {
                            if (customattribute.AttributeType.Name == "ExplicitKeyAttribute")
                            {
                                keyValues.Add(fieldname, (fieldvalue == null ? "" : fieldvalue.ToString()));
                            }
                        });
                    }
                });
                if (keyfilednames != null && keyfilednames.AsList().Count > 0)
                {
                    keyfilednames.AsList().ForEach(key =>
                    {
                        keyValues.AsList().ForEach(kv =>
                        {
                            if (kv.Key.ToUpper() == key.ToUpper())
                            {
                                condition.Append($" and {kv.Key}='{kv.Value}'");
                            }
                        });
                    });
                }
                if (string.IsNullOrEmpty(condition.ToString())) { throw new Exception("Please set the correct query field."); }
                return connection.Query<T>($"select *from {tablename} where 1=1 {condition.ToString()}").AsList();
            }
        }
        /// <summary>
        /// 获取单个对象(返回Object实体)
        /// </summary>
        /// <param name="sql">查询SQL语句</param>
        /// <returns></returns>
        public static T GetDefaultObject<T>(string sql, ConnType connType = ConnType.MYSQL)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return connection.QueryFirstOrDefault<T>(sql);
            }
        }
        /// <summary>
        /// 获取默认对象列表
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static List<T> GetDefaultObjectList<T>(string sql, ConnType connType = ConnType.MYSQL) where T : class
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return connection.Query<T>(sql).AsList();
            }
        }
        /// <summary>
        /// 获取对象列表
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <returns></returns>
        public static List<T> GetAll<T>(ConnType connType = ConnType.MYSQL) where T : class
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return connection.GetAll<T>().AsList();
            }
        }
        /// <summary>
        /// 获取对象列表Async
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> GetAllAsync<T>(ConnType connType = ConnType.MYSQL) where T : class
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return await connection.GetAllAsync<T>();
            }
        }

        /// <summary>
        /// 获取单个对象值(整型，字符串等...)
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">查询的SQL语句</param>
        /// <returns></returns>
        public static T GetExecuteScalar<T>(string sql, ConnType connType = ConnType.MYSQL)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return connection.ExecuteScalar<T>(sql);
            }
        }
        /// <summary>
        /// 获取单个对象值(整型，字符串等...)
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">查询的SQL语句</param>
        /// <returns></returns>
        public static async Task<T> GetExecuteScalarAsync<T>(string sql, ConnType connType = ConnType.MYSQL) where T : class, new()
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return await connection.ExecuteScalarAsync<T>(sql);
            }
        }
        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">执行的SQL语句</param>
        /// <returns></returns>
        public static bool Execute(string sql, ConnType connType = ConnType.MYSQL)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return connection.Execute(sql) > 0;
            }
        }

        /// <summary>
        /// 删除行通过传入实体
        /// </summary>
        /// <typeparam name="T">实体对象</typeparam>
        /// <typeparam name="S">关键字类型</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Delete<T, S>(S id, IDbConnection conn = null, ConnType connType = ConnType.MYSQL) where T : class, new()
        {
            IDbConnection connection = (conn == null ? ConnectionFactory.GetSqlConnection(connType) : conn);
            var primarykey = string.Empty;
            var tablename = typeof(T).Name;
            System.Reflection.PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            foreach (var pro in propertyInfos)
            {
                if (pro.CustomAttributes.ToList().Count > 0)
                {
                    primarykey = pro.Name;
                    break;
                }
            }
            if (string.IsNullOrEmpty(primarykey)) { throw new Exception("Table model has no key set."); }
            var status = connection.Execute($"delete from {tablename} where {primarykey}='{id}'") > 0;
            if (conn == null) { connection.Close(); connection.Dispose(); }
            return status;
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="T">实体对象</typeparam>
        /// <typeparam name="S">关键字类型</typeparam>
        /// <param name="ids">关键字列表</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool DeleteList<T, S>(List<S> ids, ConnType connType = ConnType.MYSQL) where T : class, new()
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                using (IDbTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        var primarykey = string.Empty;
                        var tablename = typeof(T).Name;
                        System.Reflection.PropertyInfo[] propertyInfos = typeof(T).GetProperties();
                        foreach (var pro in propertyInfos)
                        {
                            if (pro.CustomAttributes.ToList().Count > 0)
                            {
                                primarykey = pro.Name;
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(primarykey)) { throw new Exception("Table model has no key set."); }
                        foreach (var p in ids)
                        {
                            connection.Execute($"delete from {tablename} where {primarykey}='{p}'");
                        }
                        trans.Commit();
                        return true;
                    }
                    catch (Exception error)
                    {
                        trans.Rollback();
                        throw new Exception(error.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 新增一笔记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="model">实体对象</param>
        /// <returns></returns>
        public static bool Insert<T>(T model) where T : class, new()
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                var effectrows= connection.Insert<T>(model);
                return effectrows >= 0;
            }
        }
        /// <summary>
        /// 更新一笔记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="model">实体对象</param>
        /// <returns></returns>
        public static bool Update<T>(T model) where T : class, new()
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                return connection.Update<T>(model);
            }
        }
        /// <summary>
        /// 删除一笔记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="model">实体对象</param>
        /// <returns></returns>
        public static bool delete<T>(T model) where T : class, new()
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                return connection.Delete<T>(model);
            }
        }

        /// <summary>
        /// 获取单个对象(一个关键字)
        /// </summary>
        /// <typeparam name="T">返回实体类型</typeparam>
        /// <typeparam name="S">输入参数类型</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Get<T, S>(S id, ConnType connType = ConnType.MYSQL) where T : class
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return connection.Get<T>(id);
            }
        }
        /// <summary>
        /// 获取两个表关联映射数据
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id", CommandType commandType = CommandType.Text)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                return connection.Query(sql, map, param, null, true, splitOn, null, commandType);
            }
        }
        /// <summary>
        /// 获取三个表关联映射数据
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThree"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThree, TReturn>(string sql, Func<TFirst, TSecond, TThree, TReturn> map, object param = null, string splitOn = "Id", CommandType commandType = CommandType.Text)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                return connection.Query(sql, map, param, null, true, splitOn, null, commandType);
            }
        }
        /// <summary>
        /// 获取四个表关联映射数据
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThree"></typeparam>
        /// <typeparam name="TFour"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThree, TFour, TReturn>(string sql, Func<TFirst, TSecond, TThree, TFour, TReturn> map, object param = null, string splitOn = "Id", CommandType commandType = CommandType.Text)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                return connection.Query(sql, map, param, null, true, splitOn, null, commandType);
            }
        }
        /// <summary>
        /// 获取五个表关联映射数据
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThree"></typeparam>
        /// <typeparam name="TFour"></typeparam>
        /// <typeparam name="TFive"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThree, TFour, TFive, TReturn>(string sql, Func<TFirst, TSecond, TThree, TFour, TFive, TReturn> map, object param = null, string splitOn = "Id", CommandType commandType = CommandType.Text)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                return connection.Query(sql, map, param, null, true, splitOn, null, commandType);
            }
        }
        /// <summary>
        /// 获取六个表关联映射数据
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <typeparam name="TThree"></typeparam>
        /// <typeparam name="TFour"></typeparam>
        /// <typeparam name="TFive"></typeparam>
        /// <typeparam name="FSix"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="splitOn"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThree, TFour, TFive, FSix, TReturn>(string sql, Func<TFirst, TSecond, TThree, TFour, TFive, FSix, TReturn> map, object param = null, string splitOn = "Id", CommandType commandType = CommandType.Text)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                return connection.Query(sql, map, param, null, true, splitOn, null, commandType);
            }
        }
        /// <summary>
        /// 获取单表数据分页
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="model">查询参数对象</param>
        /// <param name="sortfield">排序字段</param>
        /// <param name="pageindex">当前页索引</param>
        /// <param name="pagesize">页码</param>
        /// <param name="totalrecords">返回查询总记录数</param>
        /// <returns></returns>
        public static List<T> GetListByPage<T>(T model, string sortfield, int pageindex, int pagesize, out int totalrecords) where T : class, new()
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                totalrecords = 0;
                if (pagesize == 0) { pagesize = 10; }
                if (pagesize == 0) { pageindex = 1; }
                var startindex = pagesize * (pageindex - 1) + 1;
                var endindex = pagesize * pageindex;
                var condition = new StringBuilder();
                System.Reflection.PropertyInfo[] propertyInfos = typeof(T).GetProperties();
                propertyInfos.ToList().ForEach(info =>
                {
                    var fieldname = info.Name;
                    var fieldval = info.GetValue(model, null);
                    if (info.PropertyType.FullName == typeof(string).FullName)
                    {
                        if (fieldval != null && !string.IsNullOrEmpty(fieldval.ToString()))
                        {
                            if (fieldname.ToUpper().Contains("_DATE"))
                            {
                                if (fieldval.ToString().Contains(","))
                                {
                                    var splitdate = fieldval.ToString().Split(",");
                                    condition.Append($" and {fieldname}>=DATE_FORMAT('{splitdate[0]} 00:00:00','%Y-%m-%d %H-%i-%s') " +
                                        $" and {fieldname}<=DATE_FORMAT('{splitdate[1]} 23:59:59','%Y-%m-%d %H-%i-%s')");
                                }
                            }
                            else
                            {
                                condition.Append($" and {fieldname}='{fieldval}'");
                            }
                        }
                    }
                    else if(info.PropertyType.FullName==typeof(DateTime).FullName)
                    {
                        //模型日期类型不做处理
                    }
                });
                var TableName = typeof(T).Name;
                var rdscount = $"select count(1) from {TableName} where 1=1 {condition.ToString()}";
                var records = $"select *from (select row_number() over (order by {sortfield}) as seqid,a.* from {TableName} a where 1=1 {condition.ToString()}) temp where seqid between {startindex} and {endindex}";
                var querystring = $"{rdscount};{records}";
                using (var mutired = connection.QueryMultiple(querystring))
                {
                    totalrecords = mutired.ReadFirst<int>();
                    return mutired.Read<T>().ToList();
                }
            }
        }
        /// <summary>
        /// 获取单个对象Async(一个关键字)
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <typeparam name="S">输入参数类型</typeparam>
        /// <param name="id">输入参数值</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T, S>(S id, ConnType connType = ConnType.MYSQL) where T : class where S : class
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                return await connection.GetAsync<T>(id);
            }
        }
        /// <summary>
        /// 事务处理
        /// </summary>
        /// <param name="sqls">执行SQL列表集合</param>
        /// <returns></returns>
        public static bool ExecuteTransaction(List<string> sqls, ConnType connType = ConnType.MYSQL)
        {
            var flag = true;
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection(connType))
            {
                using (IDbTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var sql in sqls)
                        {
                            connection.Execute(sql);
                        }
                        trans.Commit();
                    }
                    catch (Exception error)
                    {
                        flag = false;
                        trans.Rollback();
                        throw new Exception(error.Message);
                    }
                }
                return flag;
            }
        }
    }
}
