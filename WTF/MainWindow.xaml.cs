using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WTF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Data Types

        [Serializable]
        public class KnowledgeBaseEntry
        {
            public KnowledgeBaseEntry()
            {
                EntryId = Guid.NewGuid();
            }

            public KnowledgeBaseEntry(string entryKey, string entryValue)
            {
                EntryId = Guid.NewGuid();

                EntryKey = entryKey;
                EntryValue = entryValue;
            }

            public Guid EntryId
            {
                get;
                set;
            }

            public string EntryKey
            {
                get;
                set;
            }

            public string EntryValue
            {
                get;
                set;
            }
        }

        [Serializable]
        public class WtfConfiguration
        {
            public WtfConfiguration()
            {
                KnowledgeBase = new List<KnowledgeBaseEntry>();
            }

            public List<KnowledgeBaseEntry> KnowledgeBase
            {
                get;
                set;
            }

            public bool Update(string entryKey, string entryValue, int entryIndex)
            {
                try
                {
                    if (KnowledgeBase == null)
                    {
                        return false;
                    }

                    if (entryIndex >= KnowledgeBase.Count)
                    {
                        return false;
                    }

                    KnowledgeBase[entryIndex].EntryKey = entryKey;
                    KnowledgeBase[entryIndex].EntryValue = entryValue;

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        #endregion

        #region Data Members

        public const string APPLICATION_NAME = "WTF";
        public const string XML_EXTENSION = ".xml";
        public const string CSV_EXTENSION = ".csv";

        private string m_FullFilename;
        private string m_GlossaryFilename;
        private string m_Path;

        private WtfConfiguration m_Configuration;

        private Guid currentEntryGuid;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            string result;

            if (m_Configuration == null)
            {
                m_Configuration = new WtfConfiguration();
            }

            SaveConfiguration(out result);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string result;

            btnAdd.Visibility = Visibility.Hidden;
            txtReply.Visibility = Visibility.Hidden;

            currentEntryGuid = Guid.Empty;

            m_Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            m_FullFilename = m_Path + "\\" + APPLICATION_NAME + XML_EXTENSION;
            m_GlossaryFilename = m_Path + "\\" + APPLICATION_NAME + CSV_EXTENSION;

            if (!LoadConfiguration(out result))
            {
                MessageBox.Show("Failed Loading Configuration. " + result, "Configuartion Error");

                return;
            }

            if (m_Configuration == null)
            {
                MessageBox.Show("No Configuration", "Configuartion Error");

                return;
            }

            UpdateKeysGuiList();
        }

        #region Configuration

        //  Purpose     : 
        //  Input       : 
        //  Output      : 
        //  Assumptions : 
        private bool LoadConfiguration(out string result)
        {
            #region Data Members

            string method = ":{" + MethodBase.GetCurrentMethod().Name + "}: ";

            #endregion

            result = "";

            try
            {
                if (!File.Exists(m_FullFilename))
                {
                    result = "'" + m_FullFilename + "' Does Not Exist";

                    Audit(method + result);

                    return false;
                }

                m_Configuration = Utils.ReadFromXmlFile<WtfConfiguration>(m_FullFilename);

                return true;
            }
            catch (Exception e)
            {
                result = e.Message;

                return false;
            }
        }

        //  Purpose     : 
        //  Input       : 
        //  Output      : 
        //  Assumptions : 
        private bool SaveConfiguration(out string result)
        {
            #region Data Members

            string method = ":{" + MethodBase.GetCurrentMethod().Name + "}: ";
            string backupFileame;
            string dateTime = DateTime.Now.ToString(" dd-MM-yyyy hh-mm-ss");

            #endregion

            result = "";

            try
            {
                backupFileame = System.IO.Path.GetFileNameWithoutExtension(m_FullFilename) + dateTime + System.IO.Path.GetExtension(m_FullFilename);

                File.Copy(m_FullFilename, backupFileame);

                if (!Utils.WriteToXmlFile(m_FullFilename, m_Configuration, out result))
                {
                    Audit(method + "Failed Writing To '" + m_FullFilename + "'. " + result);

                    File.Delete(backupFileame);

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                result = e.Message;

                Audit(method + result);

                return false;
            }
        }

        #endregion

        #region Audit

        private void Audit(string message)
        {
            string dateTime = DateTime.Now.ToString("[dd-MM-yyyy hh:mm:ss]");

            Console.WriteLine(dateTime + message);
        }

        #endregion

        #region GUI

        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            txtReply.Text = string.Empty;

            if (string.IsNullOrEmpty(txtQuery.Text))
            {
                MessageBox.Show("No Query Given", "Query Input");

                return;
            }

            if (m_Configuration == null)
            {
                MessageBox.Show("No Knowlege Base", "Query Input");

                return;
            }

            if (m_Configuration.KnowledgeBase == null)
            {
                MessageBox.Show("No Knowlege Base", "Query Input");

                return;
            }

            for (int i = 0; i <  m_Configuration.KnowledgeBase.Count; i++)
            {
                if (m_Configuration.KnowledgeBase[i].EntryKey.ToLower() == txtQuery.Text.ToLower())
                {
                    txtReply.Visibility = Visibility.Visible;
                    txtReply.Text = m_Configuration.KnowledgeBase[i].EntryValue;

                    currentEntryGuid = m_Configuration.KnowledgeBase[i].EntryId;

                    return;
                }
            }

            MessageBox.Show("No Information For '" + txtQuery.Text + "'", "Query Input");
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string result;

            if (string.IsNullOrEmpty(txtQuery.Text))
            {
                if (string.IsNullOrEmpty(txtQuery.Text))
                {
                    MessageBox.Show("No Key Given", "Add Entry");

                    return;
                }

                return;
            }

            if (string.IsNullOrEmpty(txtReply.Text))
            {
                MessageBox.Show("No Information For '" + txtQuery.Text + "' Given", "Add Entry");

                return;
            }

            if (m_Configuration == null)
            {
                m_Configuration = new WtfConfiguration();
            }

            if (m_Configuration.KnowledgeBase == null)
            {
                m_Configuration.KnowledgeBase = new List<KnowledgeBaseEntry>();
            }

            for (int i = 0; i < m_Configuration.KnowledgeBase.Count; i++)
            {
                if (m_Configuration.KnowledgeBase[i].EntryKey.ToLower() == txtQuery.Text.ToLower())
                {
                    MessageBox.Show("Information For '" + txtQuery.Text + "' Already Exists", "Add Entry");

                    return;
                }
            }

            m_Configuration.KnowledgeBase.Add(new KnowledgeBaseEntry(txtQuery.Text, txtReply.Text));

            if (!SaveConfiguration(out result))
            {
                MessageBox.Show(result, "Save Configuration Failure");
            }

            txtQuery.Text = string.Empty;
            txtReply.Text = string.Empty;
        }

        private void WndMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    MessageBox.Show("F1 - Help" + Environment.NewLine +
                                    "F2 - Clear" + Environment.NewLine + 
                                    "F3 - Toggle ADD and Search Modes" + Environment.NewLine +
                                    "F5 - Create Glossary List" + Environment.NewLine +
                                    "F9 - Number Of Entries In The Knowledge Base" + Environment.NewLine +
                                    "F12 - Update The Definition",
                                    "WTF Keys");
                    break;

                case Key.F3:
                    if (btnAdd.Visibility == Visibility.Hidden)
                    {
                        btnAdd.Visibility = Visibility.Visible;
                        btnQuery.Visibility = Visibility.Hidden;
                        txtReply.Visibility = Visibility.Visible;
                        txtReply.Text = string.Empty;
                    }
                    else
                    {
                        btnAdd.Visibility = Visibility.Hidden;
                        btnQuery.Visibility = Visibility.Visible;
                        txtReply.Visibility = Visibility.Hidden;
                    }
                    break;

                case Key.F2:
                    txtQuery.Text = string.Empty;
                    txtReply.Text = string.Empty;
                    txtReply.Visibility = Visibility.Hidden;
                    break;

                case Key.F5:
                    CreateGlossaryFile();
                    break;

                case Key.F9:
                    if ((m_Configuration == null) || (m_Configuration.KnowledgeBase == null))
                    {
                        MessageBox.Show("There Are No Entries. No Knowledge Base", "Number Of Entries"); ;
                    }

                    MessageBox.Show("There Are " + m_Configuration.KnowledgeBase.Count + " Entries", "Number Of Entries");
                    break;

                case Key.F12:
                    UpdateKnowledgeBaseEntry();
                    break;

                default:
                    break;
            }
        }

        private void CreateGlossaryFile()
        {
            string result;

            try
            {
                if (m_Configuration == null)
                {
                    MessageBox.Show("No Configuration File", "Create Glossary File Failure");

                    return;
                }

                if (m_Configuration.KnowledgeBase == null)
                {
                    MessageBox.Show("No Knowledge Base", "Create Glossary File Failure");

                    return;
                }

                List<string> lLine = new List<string>();

                foreach (KnowledgeBaseEntry knowledgeBaseEntry in m_Configuration.KnowledgeBase)
                {
                    lLine.Add(knowledgeBaseEntry.EntryKey + "," + knowledgeBaseEntry.EntryValue);
                }

                lLine.Sort();
                lLine.Insert(0, "Entry,Explanation");

                if (!Utils.WriteLinesToFile(m_GlossaryFilename, lLine, out result))
                {
                    MessageBox.Show(result, "Create Glossary File Failure");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Create Glossary File Failure");
            }
        }

        private void UpdateKnowledgeBaseEntry()
        {
            #region Data Members

            int entryIndex;

            string result;

            KnowledgeBaseEntry knowledgeBaseEntry;

            #endregion

            if (!string.IsNullOrEmpty(txtReply.Text))
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Update?", "Update Entry Failure", MessageBoxButton.YesNo);
                if(messageBoxResult == MessageBoxResult.Yes)
                {
                    if (!GetKnowledgeBaseEntry(out knowledgeBaseEntry, out entryIndex, out result))
                    {
                        MessageBox.Show("Failed Retrieving Entry. " + result, "Update Entry Failure");

                        return;
                    }

                    if (txtReply.Text == knowledgeBaseEntry.EntryValue)
                    {
                        MessageBox.Show("Entry Value Not Changed", "Update Entry Aborted");

                        return;
                    }

                    if (!m_Configuration.Update(txtQuery.Text, txtReply.Text, entryIndex))
                    {
                        MessageBox.Show("Failed Updating", "Update Entry Failure");
                    }
                }
            }
        }

        private bool GetKnowledgeBaseEntry(out KnowledgeBaseEntry knowledgeBaseEntry, out int entryIndex, out string result)
        {
            result = string.Empty;
            knowledgeBaseEntry = null;
            entryIndex = -1;

            try
            {
                if (m_Configuration == null)
                {
                    result = "No Knowlege Base. Is Null.";

                    return false;
                }

                if (m_Configuration.KnowledgeBase == null)
                {
                    result = "No Knowlege Base. Is Null.";

                    return false;
                }

                for (int i = 0; i < m_Configuration.KnowledgeBase.Count; i++)
                {
                    if (m_Configuration.KnowledgeBase[i].EntryId == currentEntryGuid)
                    {
                        knowledgeBaseEntry = m_Configuration.KnowledgeBase[i];
                        entryIndex = i;

                        return true;
                    }
                }

                result = "No Information For '" + txtQuery.Text + "'";

                return false;
            }
            catch (Exception e)
            {
                result = e.Message;

                return false;
            }
        }

        private void TxtQuery_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (btnAdd.Visibility == Visibility.Hidden)
                    {
                        BtnQuery_Click(null, null);
                    }
                    else
                    {
                        BtnAdd_Click(null, null);
                    }
                    break;

                default:
                    break;
            }
        }

        private void UpdateKeysGuiList()
        {
            List<string> keysList = new List<string>();

            foreach (KnowledgeBaseEntry knowledgeBaseEntry in m_Configuration.KnowledgeBase)
            {
                if ((knowledgeBaseEntry != null) && (!string.IsNullOrEmpty(knowledgeBaseEntry.EntryKey)))
                {
                    keysList.Add(knowledgeBaseEntry.EntryKey);
                }
            }

            keysList.Sort();

            txtQuery.ItemsSource = keysList;
        }

        #endregion
    }
}
