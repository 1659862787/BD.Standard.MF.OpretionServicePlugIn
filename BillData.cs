
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BD.Standard.MF.OpretionServicePlugIn
{

    [Description("单据操作服务插件")]
    [assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]
    /*
     * 
     * 更新时间：2024年11月8日15:37:02
     * 
     * **/
    public class BillData : AbstractOperationServicePlugIn
    {
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
                string opera = this.FormOperation.Operation;
                IOperationResult operationResult = new OperationResult();
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    //获取当前表单fid，单据编号、单据标识、当前操作用户
                    string fid = entity[0].ToString();
                    string fbillno = entity["billno"].ToString();
                    string fromid = entity["FFormId"].ToString();
                    string username = this.Context.UserName;

                    operationResult = MWUTILS.MWData(this.Context, operationResult, "bill", fromid, fbillno, fid, username);
                    if (operationResult == null) { continue; }
                    
                }
                this.OperationResult.MergeResult(operationResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
