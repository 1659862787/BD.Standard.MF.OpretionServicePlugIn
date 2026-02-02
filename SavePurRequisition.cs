
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;

namespace BD.Standard.BX.ListServicePlugInS7
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("采购申请保存推送wms订单操作服务插件")]
    public class SavePurRequisition : AbstractOperationServicePlugIn
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

                string opera = this.FormOperation.Operation;
                IOperationResult operationResult = new OperationResult();
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    //获取当前表单fid与编
                    string id = entity["id"].ToString();
                    string fbillno = entity["billno"].ToString();

                    //替代表单PYTHON插件（修改下游新期望日期）
                    string date = $"/*dialect*/update y set F_VACI_Date_qtr=F_VACI_Date_83g FROM T_PUR_REQENTRY x inner join (select a.fentryid,b.FSID,F_VACI_Date_qtr from  t_PUR_POOrderentry a left join T_PUR_POORDERENTRY_LK  b on a.fentryid=b.FENTRYID) y on x.fentryid=y.FSID where x.fid='{id}'";
                    DBUtils.Execute(this.Context, date);
                    //本地测试sql:/*dialect*/update y set F_VACI_Datetime_apv=F_VACI_Date_83g FROM T_PUR_REQENTRY x inner join (select a.fentryid,b.FSID,F_VACI_Datetime_apv from  t_PUR_POOrderentry a left join T_PUR_POORDERENTRY_LK  b on a.fentryid=b.FENTRYID) y on x.fentryid=y.FSID where x.fid='{id}' and isnull(x.F_VACI_DATE_83G,'')<>'' 


                    string sql = $"select distinct c.fid from T_PUR_POORDERENTRY_LK  a inner join T_PUR_POORDERENTRY b on a.fentryid=b.fentryid inner join T_PUR_POORDER c on b.fid=c.FID where FSBILLID={id} and FDOCUMENTSTATUS='C'";

                    DynamicObjectCollection dys = DBUtils.ExecuteDynamicObject(Context, sql);
                    if (dys.Count>0)
                    {
                        object[] pkIds = dys.Select(x => x["fid"]).ToArray();
                        var bInfo = FormMetaDataCache.GetCachedFormMetaData(this.Context, "PUR_PurchaseOrder").BusinessInfo;
                        //DynamicObject Expobj = BusinessDataServiceHelper.LoadSingle(this.Context, dy["fid"], bInfo, null);

                        BusinessDataServiceHelper.DoNothing(this.Context, bInfo, pkIds, "PostWms");
                     
                    }

                }
                this.OperationResult.MergeResult(operationResult);
            }
            catch (Exception ex)
            {
                throw new Exception("采购申请修改期望日期后，采购订单推送WMS失败，请联系运维查看详情日志！"+ex.Message);
            }
        }


    }
}
