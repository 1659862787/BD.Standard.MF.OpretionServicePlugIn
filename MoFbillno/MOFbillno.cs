using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace BD.Standard.MF.OpretionServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("弥费生产订单单据编号生成")]
    public class MOFbillno : AbstractOperationServicePlugIn
    {
        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <param name="e"></param>
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            List<Field> file = this.BusinessInfo.GetFieldList();
            foreach (Field item in file)
            {
                e.FieldKeys.Add(item.Key);
            }
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            try
            {
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    string DocumentStatus=entity["DocumentStatus"].ToString();
                    if (DocumentStatus.Equals("A")|| DocumentStatus.Equals("D") || DocumentStatus.Equals("Z"))
                    {
                        string FID = entity["Id"].ToString();
                        DynamicObject Projiect = (DynamicObject)entity["F_Projiect"];
                        string billno = Projiect["F_PJSN"].ToString() + "-";
                        //F_VACI_Base_re5
                        DynamicObject wuliao = (DynamicObject)entity["F_VACI_Base_re5"];
                        if (wuliao != null)
                        {
                            billno += wuliao["Number"].ToString() + "-";
                        }
                        else
                        {
                            billno += "null-";
                        }
                        int num = 1;
                        DynamicObjectCollection dyc = entity["TreeEntity"] as DynamicObjectCollection;
                        if (dyc != null && dyc[0]["SrcBillType"] != null && string.Equals(dyc[0]["SrcBillType"].ToString(), "PRD_MO"))
                        {
                            string[] strings = dyc[0]["SrcBillNo"].ToString().Split('-');
                            if (strings.Length >= 4)
                            {
                                num = Convert.ToInt32(strings.GetValue(strings.Length - 2)) + 1;
                            }
                        }
                        billno += num + "-";
                        string sql = "select fbillno from T_PRD_MO where fbillno like '" + billno + "%'";
                        DynamicObjectCollection fbillnody = DBUtils.ExecuteDynamicObject(this.Context, sql) as DynamicObjectCollection;
                        if (fbillnody.Count > 0 && fbillnody != null)
                        {
                            int i = 0;
                            foreach (var item in fbillnody)
                            {

                                string fbillno = item["fbillno"].ToString();
                                if (fbillno.Equals(entity["Billno"].ToString()))
                                {
                                    return;
                                }
                                int j = int.Parse(fbillno.Substring(fbillno.LastIndexOf("-") + 1, 3));
                                if (j > i)
                                {
                                    i = j;
                                }
                            }
                            ++i;
                            if (i.ToString().Length == 1)
                            {
                                billno += "00" + i;
                            }
                            else if (i.ToString().Length == 2)
                            {
                                billno += "0" + i;
                            }
                            else if (i.ToString().Length == 3)
                            {
                                billno += i;
                            }
                        }
                        else
                        {
                            billno += "001";
                        }
                        entity["Billno"] = billno;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
