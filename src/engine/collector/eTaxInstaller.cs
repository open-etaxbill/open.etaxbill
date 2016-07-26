using System.ComponentModel;
using System.Configuration.Install;

namespace OpenETaxBill.Engine.Collector
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
