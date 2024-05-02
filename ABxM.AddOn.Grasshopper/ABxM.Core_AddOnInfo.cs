using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace ABxM.Core_AddOn
{
    public class ABxM_Core_AddOnInfo : GH_AssemblyInfo
    {
        public override string Name => "AbmFrameworkAddOn.Core";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "This is the addon specify for GAS lab and agent-based research based on ABxM";

        public override Guid Id => new Guid("7F1D1FF0-9A3A-45EB-A951-9EFF552CE9AF");

        //Return a string identifying you or your company.
        public override string AuthorName => "TsungWeiCheng";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "mike861104@gmail.com";
    }
}