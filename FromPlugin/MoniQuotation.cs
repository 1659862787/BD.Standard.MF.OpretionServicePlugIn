using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.MFG.ServiceHelper.ENG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace BD.Standard.MM.ListServicePlugIncs2
{
    [Description("模拟报价单表单插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class MoniQuotation : AbstractDynamicFormPlugIn
    {

        public override void AfterCreateNewData(EventArgs e)
        {
            base.AfterCreateNewData(e);

            DynamicObject FQuoteOrgId = (DynamicObject)this.View.Model.GetValue("FQuoteOrgId");
            DynamicObject FMaterialId = (DynamicObject)this.View.Model.GetValue("FMaterialId");
            DynamicObject FBomId = (DynamicObject)this.View.Model.GetValue("FBomId");


            





        }
    }
}
