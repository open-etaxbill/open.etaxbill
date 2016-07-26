using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;

namespace OpenETaxBill.Engine.Signer
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
