using System.Windows;

namespace Gw2BuildHelper {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class EditWindow : Window {
        private string className;

        public EditWindow (string buildName) {
            InitializeComponent();
            
            Left = (SystemParameters.PrimaryScreenWidth-Width) / 2.0f;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2.0f;

            txtName.Text = buildName;

            var build = Build.LoadBuild(buildName + ".xml");

            var traits = new string[] { "1", "2", "3" };

            cmbSpec1Trait1.ItemsSource = traits;
            cmbSpec1Trait2.ItemsSource = traits;
            cmbSpec1Trait3.ItemsSource = traits;
            cmbSpec2Trait1.ItemsSource = traits;
            cmbSpec2Trait2.ItemsSource = traits;
            cmbSpec2Trait3.ItemsSource = traits;
            cmbSpec3Trait1.ItemsSource = traits;
            cmbSpec3Trait2.ItemsSource = traits;
            cmbSpec3Trait3.ItemsSource = traits;

            if (build != null) {
                cmbSpec1Trait1.SelectedIndex = build.Specializations[0].traitValues[0];
                cmbSpec1Trait2.SelectedIndex = build.Specializations[0].traitValues[1];
                cmbSpec1Trait3.SelectedIndex = build.Specializations[0].traitValues[2];
                cmbSpec2Trait1.SelectedIndex = build.Specializations[1].traitValues[0];
                cmbSpec2Trait2.SelectedIndex = build.Specializations[1].traitValues[1];
                cmbSpec2Trait3.SelectedIndex = build.Specializations[1].traitValues[2];
                cmbSpec3Trait1.SelectedIndex = build.Specializations[2].traitValues[0];
                cmbSpec3Trait2.SelectedIndex = build.Specializations[2].traitValues[1];
                cmbSpec3Trait3.SelectedIndex = build.Specializations[2].traitValues[2];
                
                cmbClass.SelectedValue = className = build.profession;

                cmbSpec1.SelectedValue = build.Specializations[0].name;
                cmbSpec2.SelectedValue = build.Specializations[1].name;
                cmbSpec3.SelectedValue = build.Specializations[2].name;
            }

            foreach (var profession in Localization.Instance.professions) {
                cmbClass.Items.Add(profession.name);
            }
        }

        private void cmbClass_SelectionChanged (object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            string newClass = cmbClass.SelectedItem as string;

            cmbSpec1.Items.Clear();
            cmbSpec2.Items.Clear();
            cmbSpec3.Items.Clear();

            var profession = Localization.Instance.GetProfession(newClass);
            for (int index = 0; index < profession.Specializations.Length; ++index) {
                var spec = profession.Specializations[index];
                if (index + 1 < profession.Specializations.Length) {
                    cmbSpec1.Items.Add(spec.name);
                    cmbSpec2.Items.Add(spec.name);
                }
                cmbSpec3.Items.Add(spec.name);
            }

            if (newClass != className) {
                cmbSpec1.SelectedIndex = 0;
                cmbSpec2.SelectedIndex = 1;
                cmbSpec3.SelectedIndex = profession.Specializations.Length - 1;

                className = newClass;
            }
        }

        private void btnOk_Click (object sender, RoutedEventArgs e) {

            if(cmbClass.SelectedIndex == -1) {
                MessageBox.Show("Please select a class and some specializations in order to save.", "You're doing it wrong.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string fileName = txtName.Text + ".xml";

            var build = new Build();
            build.profession = cmbClass.SelectedValue as string;
            build.Specializations = new BuildSpec[3];
            build.Specializations[0] = new BuildSpec();
            build.Specializations[1] = new BuildSpec();
            build.Specializations[2] = new BuildSpec();
            build.Specializations[0].name = cmbSpec1.SelectedValue as string;
            build.Specializations[1].name = cmbSpec2.SelectedValue as string;
            build.Specializations[2].name = cmbSpec3.SelectedValue as string;
            build.Specializations[0].traits = string.Format("{0},{1},{2}", cmbSpec1Trait1.SelectedIndex + 1, cmbSpec1Trait2.SelectedIndex + 1, cmbSpec1Trait3.SelectedIndex + 1);
            build.Specializations[1].traits = string.Format("{0},{1},{2}", cmbSpec2Trait1.SelectedIndex + 1, cmbSpec2Trait2.SelectedIndex + 1, cmbSpec2Trait3.SelectedIndex + 1);
            build.Specializations[2].traits = string.Format("{0},{1},{2}", cmbSpec3Trait1.SelectedIndex + 1, cmbSpec3Trait2.SelectedIndex + 1, cmbSpec3Trait3.SelectedIndex + 1);

            if (Build.SaveBuild(build, fileName)) {
                Close();
                MainWindow.instance.RefreshBuildList();
                MainWindow.instance.SelectBuild(fileName);

            }
            else
                MessageBox.Show("Failed to save to: " + fileName, "Error saving!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnCancel_Click (object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
