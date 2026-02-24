
using BD.Standard.MF.OpretionServicePlugIn;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.Security.Policy;


//47
namespace BD.Standard.MF.OpretionServicePlugIn12
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("物料批量插件")]
    //BD.Standard.MF.OpretionServicePlugIn12.Runs,BD.Standard.MF.OpretionServicePlugIn12
    public class Runs : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            try
            {
                //参数：后台日志路径，默认空
                string parameter = schedule.Parameters;
                //表单参数id用于实际执行用户id
                string parameterFormId = schedule.ParameterFormId;
                

                DynamicObjectCollection dys = DBUtils.ExecuteDynamicObject(ctx,string.Format("select FMASTERID,fnumber from  T_BD_MATERIAL where FUSEORGID=1 and FDOCUMENTSTATUS='C' and  FFORBIDSTATUS='A' and fnumber  not in (select fnumber from  [dbo].[MiFei_logs] where fromid='Material') "));
                foreach (DynamicObject dyn in dys)
                {
                    IOperationResult operationResult = new OperationResult();
                    MWUTILS.MWData(ctx, operationResult, "Runs", "MATERIAL", dyn["fnumber"].ToString(), dyn["FMASTERID"].ToString(), "");

                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
