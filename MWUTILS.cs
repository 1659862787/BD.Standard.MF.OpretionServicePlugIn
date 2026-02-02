using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace BD.Standard.MF.OpretionServicePlugIn
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class MWUTILS
    {
        /// <summary>
        /// 封装正向json数据
        /// </summary>
        /// <param name="datas">表数据 表1:配置信息，表二:基本信息，表三:明细信息</param>
        /// <returns>多张单据json组成的数组</returns>
        public static string WMSJson(Context context,DataSet datas)
        {

            JObject data = new JObject();

            if (datas.Tables[1].Rows.Count == 0) return null;
            //批量传输,表头明细的关联字段
            string id = string.Empty;
            foreach (DataRow dataH in datas.Tables[1].Rows)
            {
                foreach (DataColumn columns in dataH.Table.Columns)
                {
                    string column = columns.ColumnName.ToString();
                   
                    data.Add(new JProperty(column, dataH[column].ToString()));
                }
                if (datas.Tables.Count == 3)
                {
                    JArray jsonEntry = new JArray();
                    foreach (DataRow dataE in datas.Tables[2].Rows)
                    {
                        JObject jobE = new JObject();
                        foreach (DataColumn columns in dataE.Table.Columns)
                        {
                            string column = columns.ColumnName.ToString();
                            if (column.Equals("location"))
                            {
                                SelectStockLocId(context, dataE, jobE, column);
                            }
                            else
                            {
                                jobE.Add(new JProperty(column, dataE[column]));
                            }
                            
                            
                        }

                        jsonEntry.Add(jobE);
                    }
                    data["details"] = jsonEntry;
                }
                
            }
            return data.ToString();
        }
        
        /// <summary>
        /// 仓位查询
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataE"></param>
        /// <param name="jobE"></param>
        /// <param name="column"></param>
        private static void SelectStockLocId(Context context, DataRow dataE, JObject jobE, string column)
        {
            if (dataE[column].ToString().Equals("0"))
            {
                jobE.Add(new JProperty(column, ""));
            }
            else
            {
                DataSet dys = DBUtils.ExecuteDataSet(context, $"select * from T_BAS_FLEXVALUESDETAIL where fid={dataE[column]}");
                foreach (DataColumn dycolumn in dys.Tables[0].Rows[0].Table.Columns)
                {

                    if (dycolumn.ColumnName.ToString().Contains("FF1000") && dys.Tables[0].Rows[0][dycolumn].ToString() != "0")
                    {
                        string fnumber = DBUtils.ExecuteScalar<string>(context, $"select fnumber from T_BAS_FLEXVALUESENTRY where fentryid={dys.Tables[0].Rows[0][dycolumn].ToString()}", "", null);
                        jobE.Add(new JProperty(column, fnumber));
                        break;
                    }
                }
            }
        }

        private static readonly string Logpath = @"C:\ERPPostLog\" + DateTime.Now.ToString("yyyyMM");


        /// <summary>
        /// 传输方法类
        /// </summary>
        /// <param name="context">当前上下文</param>
        /// <param name="operationResult">操作结果</param>
        /// <param name="type">存储类型</param>
        /// <param name="fromid">表单标识</param>
        /// <param name="fnumber">表单编码</param>
        /// <param name="fid">表单主键</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IOperationResult MWData(Context context, IOperationResult operationResult,string type,string fromid,string fnumber,string fid,string username)
        {
            //获取配置信息
            DataSet config = DBUtils.ExecuteDataSet(context, string.Format("exec {0}", "MiFei_config"));
            //启用插件

            string http = config.Tables[0].Rows[0].ItemArray[0].ToString();
            string DBID = config.Tables[0].Rows[0].ItemArray[1].ToString();
            if(!context.DBId.Equals(DBID)) return operationResult;

            //根据单据编号和单据标识获取对应的取消数据
            DataSet dy= dy = DBUtils.ExecuteDataSet(context, string.Format("exec {0} {1}", "MiFei_" + fromid, fid));
            string json = MWUTILS.WMSJson(context,dy);

            //dy获取表数据，表一：method：请求方法，table：基础资料表名，
            string method = dy.Tables[0].Rows[0].ItemArray[0].ToString();
            string table = dy.Tables[0].Rows[0].ItemArray[1].ToString();
            string tableid = dy.Tables[0].Rows[0].ItemArray[2].ToString();
            if (json == null) return null;


            string ss = HttpPost(http + method, json);

            ss = ss.Replace("0E-8", "0");
            JObject jo = (JObject)JsonConvert.DeserializeObject(ss);

            string sql2 = string.Format("insert into MiFei_logs (fromid,fnumber,reqjson,respjson,reqdate) values('{0}','{1}','{2}','{3}','{4}')", fromid, fnumber, json.Replace("'", "''"), ss.Replace("'", "''"), DateTime.Now.ToString("yyyy-MM-dd"));

            DBUtils.Execute(context, sql2);

            //同步成功，输出消息，更改单据同步状态字段，记录日志
            if (jo["code"].ToString().Equals("200") && Convert.ToBoolean(jo["beSuccess"].ToString()))
            {
                operationResult.OperateResult.Add(new OperateResult()
                {
                    SuccessStatus = true,
                    Name = "同步消息",
                    Message = string.Format(fnumber + ":WMS同步成功"),
                    MessageType = MessageType.Normal,
                    PKValue = 0,
                });
            }
            else
            {

                Logger logger = new Logger(Logpath + fromid + "\\", DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                logger.WriteLog("数据出现异常,错误信息：" + ss + "\r\n请求json:" + json);

                throw new Exception("对接返回失败信息：" + ss + "\r\n请求json:" + json);
            }

            return operationResult;
            
        }

        /// <summary>
        /// http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HttpPost(string url, string data)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json;charset=UTF-8";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
                streamWriter.Close();
            }
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string text = httpWebResponse.ContentEncoding;
            bool flag = text == null || text.Length < 1;
            if (flag)
            {
                text = "UTF-8";
            }
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding(text));
            return streamReader.ReadToEnd();
        }

    }

}
