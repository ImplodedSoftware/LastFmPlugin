using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImPluginEngine.Helpers;
using Newtonsoft.Json;

namespace LastFmPlugin
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();

            var settingsFile = Path.Combine(PluginConstants.SettingsPath, "lastfm.json");
            if (File.Exists(settingsFile))
            {
                var json = File.ReadAllText(settingsFile);
                var sf = JsonConvert.DeserializeObject<LastFmSettings>(json);
                comboBoxLanguage.SelectedIndex = sf.LanguageIndex;
            }
            else
            {
                comboBoxLanguage.SelectedIndex = 0;
            }

        }

        public void SaveSettings()
        {
            var sf = new LastFmSettings();
            sf.LanguageIndex = comboBoxLanguage.SelectedIndex;
            var settingsFile = Path.Combine(PluginConstants.SettingsPath, "lastfm.json");
            var json = JsonConvert.SerializeObject(sf);
            File.WriteAllText(settingsFile, json);
        }

    }
}
