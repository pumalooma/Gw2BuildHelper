using MumbleLink_CSharp_GW2;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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
            
			if(Config.Instance.ShowCategories) {
				string[] files = Directory.GetFiles(rootFolder, "*.xml", SearchOption.AllDirectories);
				TreeViewUtils.LoadFileList(treeView, rootFolder, files);
			}
			else {

				string[] gameModes = { "PvE", "PvP", "WvW", "Raid" };

				foreach(var gameMode in gameModes) {
					string topFolder = Path.Combine(rootFolder, gameMode.ToLower());
                    string[] files = Directory.GetFiles(topFolder, "*.xml", SearchOption.AllDirectories);
					TreeViewUtils.LoadFileList(treeView, topFolder, files, string.Format("({0}) ", gameMode));
				}

				var link = new GW2Link();

				var identity = link.GetIdentity();

				if(identity != null)
				{
					var item = TreeViewUtils.FindChild(treeView.Items, identity.Profession.ToString());
					if(item != null)
					{
						item.IsExpanded = true;
						item.IsSelected = true;
					}
				}

				link.Dispose();
			}	   
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

			IntPtr hwnd = Win32.FindWindow("ArenaNet_Dx_Window_Class", null);

			if(hwnd == null || hwnd == IntPtr.Zero)
			{
				m_Overlay.Hide();
				return;
			}

			Win32.RECT rect;
			if(!Win32.GetWindowRect(hwnd, out rect))
			{
				m_Overlay.Hide();
				return;
			}

			m_Overlay.SetSize(rect);


			var bmpScreenCapture = ImageUtils.TakeScreenShot(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

            if (bmpScreenCapture == null) {
				m_Overlay.Hide();
				m_RefreshWaitTime += 100;
                return;
            }

			//bmpScreenCapture.Save("test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);


			Bitmap heroPanel = Properties.Resources.HeroPanelSmall;

			if(Config.Instance.InterfaceSize == 1)
				heroPanel = Properties.Resources.HeroPanelNormal;
			else if(Config.Instance.InterfaceSize == 2)
				heroPanel = Properties.Resources.HeroPanelLarge;
			else if(Config.Instance.InterfaceSize == 3)
				heroPanel = Properties.Resources.HeroPanelNormal;
			
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
            string buildName = GetBuildName(Config.Instance.ShowCategories);

			if(!Config.Instance.ShowCategories && buildName == null)
				return;

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
            string buildName = GetBuildName();
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

		private void cmUseLargeInterface(object sender, RoutedEventArgs e) {
			Config.Instance.InterfaceSize = 2;
			m_Overlay.hp = InterfaceSize.LoadInterfaceSize();
			Config.Instance.SaveConfig();
		}

		private void cmUseXLargeInterface(object sender, RoutedEventArgs e) {
			//Config.Instance.InterfaceSize = 3;
			//m_Overlay.hp = InterfaceSize.LoadInterfaceSize();
			//Config.Instance.SaveConfig();
			MessageBox.Show("Sorry, X-Large is not supported right now, only Small, Normal and Large are.");
		}

		private void cmToggleGameModeCategories(object sender, RoutedEventArgs e) {
			Config.Instance.ShowCategories = !Config.Instance.ShowCategories;
            Config.Instance.SaveConfig();

			RefreshBuildList();
        }

		private void cmShowHelp (object sender, RoutedEventArgs e) {
            MessageBox.Show("No help yet, bro.", "Gw2 Build Helper", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void treeView_SelectionChanged (object sender, RoutedPropertyChangedEventArgs<Object> e) {
            if (treeView.SelectedItem == null) {
                m_CurrentBuild = null;
                return;
            }

			// only doing this to make hotloading interface changes easier...
			m_Overlay.hp = InterfaceSize.LoadInterfaceSize();

			m_BuildName = GetBuildName();

            m_CurrentBuild = Build.LoadBuild(m_BuildName + ".xml");
            if (m_CurrentBuild != null) {
                var profession = Localization.Instance.GetProfession(m_CurrentBuild.profession);
                for (int ii = 0; ii < 3; ++ii) {
                    int specIndex = m_CurrentBuild.Specializations[ii].specIndex;

					string interfaceName = "Small";
					
					if(Config.Instance.InterfaceSize == 1)
						interfaceName = "Normal";
					else if(Config.Instance.InterfaceSize == 2)
						interfaceName = "Large";
					else if(Config.Instance.InterfaceSize == 3)
						interfaceName = "XLarge";
					
					string filePath = string.Format("Specializations{0}/{1}{2}.bmp", interfaceName, profession.name, specIndex);

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

		private string GetBuildName (bool allowParents = false) {
			string buildName = TreeViewUtils.GetTreeViewItemPath(treeView, allowParents);

			if(buildName != null && !Config.Instance.ShowCategories)
			{
				string[] parts = buildName.Split(new char[] { '\\' });
				int index = parts[1].IndexOf(") ");
				string category = parts[1].Substring(1, index - 1);

				buildName = Path.Combine(category, parts[0], parts[1].Substring(index + 2));
			}

			return buildName;
		}

	}
}
