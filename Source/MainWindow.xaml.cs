using MumbleLink_CSharp_GW2;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gw2BuildHelper {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private BackgroundWorker m_Worker;
        public Build m_CurrentBuild;
        private string m_BuildName;
        
        private bool m_SearchForHeroPanel = true;
        private int m_RefreshWaitTime = 100;
        //private bool m_snapToHeroPanel = false;
        public Bitmap[] m_bmpSpecializations = new Bitmap[3];
        
		public static MainWindow instance;

        public OverlayWindow m_Overlay;

        public MainWindow () {
            InitializeComponent();
            instance = this;
            Config.Instance.SetWindowPosition();
			m_Overlay = new OverlayWindow();
			
			RefreshBuildList();

            m_Worker = new BackgroundWorker();
            m_Worker.DoWork += DoBackgroundWork;
            m_Worker.RunWorkerAsync();
        }

        public void RefreshBuildList () {
            treeView.Items.Clear();
            
            string rootFolder = Directory.GetCurrentDirectory().ToLower();
            string[] files = Directory.GetFiles(rootFolder, "*.xml", SearchOption.AllDirectories);

            TreeViewUtils.LoadFileList(treeView, rootFolder, files);

            var link = new GW2Link();
            
            var identity = link.GetIdentity();

            if (identity != null) {
                var item = TreeViewUtils.FindChild(treeView.Items, identity.Profession.ToString());
                if (item != null) {
                    item.IsExpanded = true;
                    item.IsSelected = true;
                }
            }

            link.Dispose();
        }

        public void SelectBuild(string filePath) {
            TreeViewUtils.SelectByPath(treeView, filePath);
        }
        
        private void DoBackgroundWork (object sender, DoWorkEventArgs e) {
            while (m_SearchForHeroPanel) {
                    Dispatcher.Invoke(new Action(() => { SearchForHeroPanel(); }));

                Thread.Sleep(m_RefreshWaitTime);
            }
        }
		
        private void SearchForHeroPanel () {

            if (m_CurrentBuild == null || WindowState == WindowState.Minimized) {
				m_Overlay.Hide();
				return;
            }

            var bmpScreenCapture = ImageUtils.TakeScreenShot();

            if (bmpScreenCapture == null) {
				m_Overlay.Hide();
				m_RefreshWaitTime += 100;
                return;
            }

            //bmpScreenCapture.Save("test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            Bitmap heroPanel = Config.Instance.InterfaceSize == 1 ? Properties.Resources.HeroPanelNormal : Properties.Resources.HeroPanelSmall;
            var location = ImageUtils.SearchBitmap(heroPanel, bmpScreenCapture, 0.2);

            if (location.Width == 0) {
				m_Overlay.Hide();
			}
            else {

				if(m_SearchForHeroPanel && m_Overlay.Visibility != Visibility.Visible) {
					m_Overlay.Show();
				}

				
				System.Drawing.Point p = new System.Drawing.Point(location.X + m_Overlay.hp.HeroPanelOffsetX, location.Y + m_Overlay.hp.HeroPanelOffsetY);
				m_Overlay.ShowScreenHelpers(bmpScreenCapture, p);
            }
        }

        private void btnAdd_Click (object sender, RoutedEventArgs e) {
            string buildName = TreeViewUtils.GetTreeViewItemPath(treeView, true);

			if(buildName == null)
				buildName = "";

			TreeViewUtils.ClearTreeViewSelection(treeView);

            if (string.IsNullOrEmpty(buildName))
                buildName = "New Build";
            else
                buildName += (buildName.Length==0 || buildName.EndsWith("\\") ? "New Build" : "New");
            
            var edit = new EditWindow(buildName);
            edit.Show();
        }

        private void btnEdit_Click (object sender, RoutedEventArgs e) {
            string buildName = TreeViewUtils.GetTreeViewItemPath(treeView);
            if (buildName != null) {
                TreeViewUtils.ClearTreeViewSelection(treeView);

                var edit = new EditWindow(buildName);
                edit.Show();
            }
        }

        private void btnDelete_Click (object sender, RoutedEventArgs e) {
            if (treeView.SelectedItem != null) {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the '" + m_BuildName + "' build?", "Please confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if( result== MessageBoxResult.OK) {
                    try {
                        File.Delete(m_BuildName + ".xml");
                        RefreshBuildList();
                    }
                    catch { }
                }
            }
        }


        private void btnSettings_Click (object sender, RoutedEventArgs e) {
            ContextMenu cm = FindResource("cmButton") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void cmUseSmallInterface (object sender, RoutedEventArgs e) {
            Config.Instance.InterfaceSize = 0;
			m_Overlay.hp = InterfaceSize.LoadInterfaceSize();
            Config.Instance.SaveConfig();
        }

        private void cmUseNormalInterface (object sender, RoutedEventArgs e) {
            Config.Instance.InterfaceSize = 1;
			m_Overlay.hp = InterfaceSize.LoadInterfaceSize();
            Config.Instance.SaveConfig();
        }
        private void cmShowHelp (object sender, RoutedEventArgs e) {
            MessageBox.Show("No help yet, bro.", "Gw2 Build Helper", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void treeView_SelectionChanged (object sender, RoutedPropertyChangedEventArgs<Object> e) {
            if (treeView.SelectedItem == null) {
                m_CurrentBuild = null;
                return;
            }

            m_BuildName = TreeViewUtils.GetTreeViewItemPath(treeView);

            m_CurrentBuild = Build.LoadBuild(m_BuildName + ".xml");
            if (m_CurrentBuild != null) {
                var profession = Localization.Instance.GetProfession(m_CurrentBuild.profession);
                for (int ii = 0; ii < 3; ++ii) {
                    int specIndex = m_CurrentBuild.Specializations[ii].specIndex;
                    string filePath = string.Format("Specializations{0}/{1}{2}.bmp", Config.Instance.InterfaceSize == 0 ? "Small" : "Normal", profession.id, specIndex);

					if(m_bmpSpecializations[ii] != null)
						m_bmpSpecializations[ii].Dispose();

                    if (File.Exists(filePath))
                        m_bmpSpecializations[ii] = new Bitmap(filePath);
                    else
                        m_bmpSpecializations[ii] = null;
                }
            }
        }

        private void Window_Closing (object sender, CancelEventArgs e) {
            Config.Instance.SaveConfig();
            m_SearchForHeroPanel = false;
            Thread.Sleep(m_RefreshWaitTime + 100);
			m_Overlay.Close();
		}
		
		private void treeView_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			DependencyObject dpSource = e.OriginalSource as DependencyObject;

			if ((dpSource as Grid) != null)
				TreeViewUtils.ClearTreeViewSelection(treeView);
		}

        private void Window_StateChanged (object sender, EventArgs e) {
            if (WindowState == WindowState.Minimized)
                m_Overlay.Hide();
        }
    }
}
