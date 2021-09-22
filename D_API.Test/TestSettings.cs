using D_API.Lib.Types;
using DiegoG.Utilities.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Test
{
    class TestSettings : ISettings
    {
        public string SettingsType => "TestSettings";
        public ulong Version => 0;
        public event PropertyChangedEventHandler PropertyChanged;

        public Credentials? ClientCredentials { get; set; }

        public string? BaseAddress { get; set; }
    }
}
