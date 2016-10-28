using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace RP2k1516
{
    public class RP2k1516Info : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "RP2k1516";
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
                return new Guid("58fb3ac1-9860-40b7-8edd-c0369e8fb29c");
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
