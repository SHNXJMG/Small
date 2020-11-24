using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Data;
using Crawler.Base.KdService;


namespace Crawler
{
    public class ToolCoreDb
    {
        private static log4net.ILog _logger;
        /// <summary>
        /// 日志记录对象
        /// </summary>
        public static log4net.ILog Logger
        {
            get
            {
                if (_logger == null)
                    _logger = log4net.LogManager.GetLogger(typeof(ToolCoreDb));
                return _logger;
            }
        }

        /// <summary>
        /// 从App.config配置文件中读取的抓取WebService字符串，配置项为：configuration-->appSettings-->CrawServiceUrl
        /// </summary>
        public static string CrawServiceUrl
        {
            get { return ConfigurationManager.AppSettings["CrawServiceUrl"]; }
        }
        private static CrawlerService NewService()
        {
            try
            {
                CrawlerService service = new CrawlerService();
                service.Url = CrawServiceUrl;
                service.Timeout = 1000 * 300;
                return service;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Logger.Error("连接服务器失败");
                throw new Exception("连接服务器失败。");
            }
        }

        /// <summary>
        /// 获取或设置是否记录重复数据比较的SQL语句
        /// </summary>
        public static bool LogExistCompareSQL { get; set; }

        /// <summary>
        /// 从App.config配置文件中读取的数据库连接字符串，配置项为：configuration-->appSettings-->DbConnString
        /// </summary>
        public static string DbConnString
        {
            get { return ConfigurationManager.AppSettings["DbConnString"]; }
        }

        /// <summary>
        /// 从App.config配置文件中读取的附件存取地址字符串，配置项为：configuration-->appSettings-->DbServerPath
        /// </summary>
        public static string DbServerPath
        {
            get { return ConfigurationManager.AppSettings["DbServerPath"]; }
        }


        /// <summary>
        /// 批量保存数据，在保存之前会根据isExistFields参数值判断该条数据是否重复
        /// </summary>
        /// <param name="entityList"></param>
        /// <param name="isExistFields">如不需要判断是否存在数据，则该参数值为null或空字符串</param>
        /// <returns></returns>
        public static int SaveDatas(IList entityList, string isExistFields)
        {
            if (entityList == null || entityList.Count < 1) return 0;
            int success = 0;
            object[] successList;
            success = ToolCoreDb.SaveDatas(entityList, isExistFields, new List<BaseAttach>(), out successList, true, false, false);
            return success;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="entity">保存的实体</param>
        /// <param name="isExistFields">判断重复数据字段，则该参数值为null或空字符串</param>
        /// <param name="isUpdate">当数据为重复时，是否需要更新</param> 
        /// <param name="existFields">更新重复数据时，其它加入其它条件的字段</param> 
        ///  <param name="isUpdateCtx">当数据有重复时，是否更新CtxHtml与Ctx</param>
        /// <returns>true:成功，false:失败</returns>
        public static bool SaveEntity(object entity, string isExistFields, bool isUpdate = false, bool isUpdateCtx = false, string existFields = null)
        {
            CrawlerService service = NewService();
            bool isSave = service.SaveEntity(entity, isExistFields, isUpdate, isUpdateCtx, existFields);
            return isSave;
        }

        /// <summary>
        /// 批量保存数据，在保存之前会根据isExistFields参数值判断该条数据是否重复
        /// </summary>
        /// <param name="entityList"></param>
        /// <param name="isExistFields">如不需要判断是否存在数据，则该参数值为null或空字符串</param>
        ///  <param name="successList"> 返回成功插入表的数据</param>
        /// <returns></returns>
        public static int SaveDatas(IList entityList, string isExistFields, List<BaseAttach> listBaseAttach, out object[] successList, bool isUpdate, bool isUpdateHtlCtx, bool isUpdateAttach)
        {
            if (listBaseAttach == null) listBaseAttach = new List<BaseAttach>();

            CrawlerService service = NewService();
            List<object> list = new List<object>();
            foreach (object obj in entityList)
            {
                list.Add(obj);
            }
            int result = service.AddItem(list.ToArray(), isExistFields, listBaseAttach.ToArray(), isUpdate, isUpdateHtlCtx, null, isUpdateAttach, out successList);
            return result;
        }



        /// <summary>
        /// 获取实体属性值
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="piName">实体名称</param>
        /// <returns></returns>
        public static string GetEntityPropertyInfoValue(object entity, string piName)
        {
            if (entity != null)
            {
                Type type = entity.GetType();
                PropertyInfo[] pis = type.GetProperties();
                if (pis != null)
                {
                    foreach (PropertyInfo pi in pis)
                    {
                        if (pi.Name == piName)
                        {
                            object value = pi.GetValue(entity, null);
                            return Convert.ToString(value);
                        }
                    }
                }
            }
            return string.Empty;
        }



        /// <summary>
        /// 得到guid
        /// </summary>
        /// <returns></returns>
        public static string NewGuid
        {
            get
            {
                return Guid.NewGuid().ToString("N");
            }
        }

        public static string EmptyGuid
        {
            get
            {
                return Guid.Empty.ToString("N");
            }
        }

        /// <summary>
        /// 生成附件信息实体
        /// </summary>
        /// <param name="attachName">附件名称</param>
        /// <param name="SourceID">外键id</param>
        /// <param name="url">附件访问url</param>
        /// <returns></returns>
        public static BaseAttach GenBaseAttach(string attachName, string SourceID, string url)
        {
            BaseAttach attach = new BaseAttach();

            attach.AttachName = string.IsNullOrEmpty(attachName) ? null : attachName;
            attach.AttachServerPath = url;
            attach.AttachSize = 0;
            attach.AttachType = "url";
            attach.CompanyId = string.Empty;
            attach.SourceID = SourceID;
            attach.Id = NewGuid;
            attach.Creator = EmptyGuid;
            attach.CreateTime = DateTime.Now;

            return attach;
        }

        /// <summary>
        /// 执行存储过程，默认为处理企业库中重复数据及错误格式
        /// </summary>
        /// <param name="proName"></param>
        /// <returns></returns>
        public static void ExecuteProcedure(string proName = "UP_ClearCorpRepeat")
        {
            CrawlerService service = NewService();
            service.ExecuteProcedure(proName);
        }
        /// <summary>
        /// 查询第一行第一列数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql)
        {
            CrawlerService service = NewService();
            return service.ExecuteScalar("kdxx" + sql);
        }
        /// <summary>
        /// 执行一条SQL语句，返回受影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteSql(string sql)
        {
            CrawlerService service = NewService();
            return service.ExecuteSql("kdxx" + sql);
        }
        /// <summary>
        /// 执行一条SQL语句，返回DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable GetDbData(string sql)
        {
            CrawlerService service = NewService();
            return service.GetDbData("kdxx" + sql);

        }
    }
}
