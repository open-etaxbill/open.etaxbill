using System.ComponentModel;
using System.Configuration.Install;

namespace OpenETaxBill.Engine.Responsor
{
    [RunInstaller(true)]
    public partial class eTaxInstaller : Installer
    {
        public eTaxInstaller()
        {
            InitializeComponent();
        }
    }
}
