using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace GhViewportMonitor
{
    public class GhViewportMonitorInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "GhViewportMonitor Info";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("baedcf81-0676-4268-a9e3-43235c27f5da");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
