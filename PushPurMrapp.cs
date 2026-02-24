
using BD.Standard.MF.OpretionServicePlugIn;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.Remoting.Contexts;

namespace BD.Standard.BX.ListServicePlugInS7
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("退料申请下推退料提交操作服务插件")]
    public class PushPurMrapp : AbstractOperationServicePlugIn
    {

        private static readonly string Logpath = @"C:\ERPPostLog\" + DateTime.Now.ToString("yyyyMM");
        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <param name="e"></param>
        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            List<Field> file = this.BusinessInfo.GetFieldList();
            foreach (Field item in file)
            {
                e.FieldKeys.Add(item.Key);
            }
        }

        /// <summary>
        /// 菜单操作方法
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            try
            {
                //获取配置信息
                DataSet config = DBUtils.ExecuteDataSet(Context, string.Format("exec {0}", "MiFei_config"));
                string DBID = config.Tables[0].Rows[0].ItemArray[1].ToString();
                if (!Context.DBId.Equals(DBID)) return ;


                Logger logger = new Logger(Logpath + "PUR_MRAPP" + "\\", DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                
                string opera = this.FormOperation.Operation;
                IOperationResult operationResult = new OperationResult();
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    //获取当前表单fid与编
                    string id = entity["id"].ToString();
                    string fbillno = entity["billno"].ToString();
                    JObject json = new JObject()
                    {
                        { "Ids",id },
                        { "TargetFormId","PUR_MRB" },
                        { "IsEnableDefaultRule","true" },
                        { "IsDraftWhenSaveFail","false" }
                    };
                    logger.WriteLog("请求："+ json.ToString());
                    string MessageReturned = JsonConvert.SerializeObject(WebApiServiceCall.Push(Context, "PUR_MRAPP", json.ToString()));
                    logger.WriteLog("响应：" + MessageReturned);
                    if (JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
                    {
                        string fid = ((Newtonsoft.Json.Linq.JContainer)JObject.Parse(JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["SuccessEntitys"][0].ToString()).First).First.ToString();
                        JObject Submitjson = new JObject()
                        {
                            { "Ids",fid }
                        };
                        logger.WriteLog("提交：" + Submitjson.ToString());
                        string submit = JsonConvert.SerializeObject(WebApiServiceCall.Submit(Context, "PUR_MRB", Submitjson.ToString()));
                        logger.WriteLog("提交操作：" + submit);
                        if (JObject.Parse(submit)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
                        {
                            operationResult.OperateResult.Add(new OperateResult()
                            {
                                SuccessStatus = true,
                                Name = "采购退料提交成功",
                                Message = string.Format(fbillno + ":WMS同步成功，"),
                                MessageType = MessageType.Normal,
                                PKValue = 0,
                            });
                        }
                        else
                        {

                            throw new KDException("", submit);
                        }
                    }
                    else
                    {
                        operationResult.OperateResult.Add(new OperateResult()
                        {
                            SuccessStatus = true,
                            Name = "自动下推失败",
                            Message = string.Format(fbillno),
                            MessageType = MessageType.Normal,
                            PKValue = 0,
                        });
                        throw new KDException("自动下推失败：", MessageReturned);
                       
                    }
                }
                this.OperationResult.MergeResult(operationResult);
            }
            catch (Exception ex)
            {
                throw new Exception("采购退料单提交时推送WMS失败，请联系运维查看详情日志！"+ex.Message);
            }
        }


    }
}
