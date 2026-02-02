using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace BD.Standard.MM.ListServicePlugIncs2
{
    [Description("报价单表单插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Quotation : AbstractDynamicFormPlugIn
    {

        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            int row = e.Row;
            if (e.Field.FieldName.EqualsIgnoreCase("F_Material"))
            {
                object newValue = e.NewValue;

                string sql = $"select FMASTERID from  T_ENG_BOM where FMATERIALID={newValue} and FDOCUMENTSTATUS='C' order  by FCREATEDATE  desc ";
                DynamicObjectCollection bom = DBUtils.ExecuteDynamicObject(this.Context, sql);
                if(bom !=null && bom.Count > 0)
                {
                    this.View.Model.SetItemValueByID("F_BOM", bom[0]["FMASTERID"], row);
                }
            }
        }
    }
}
