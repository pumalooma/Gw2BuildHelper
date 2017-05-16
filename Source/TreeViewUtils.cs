using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

public static class TreeViewUtils {

    private static BitmapImage m_folderImage;
    private static BitmapImage m_fileImage;

    private static void InitTreeIcons() {

        if (m_folderImage != null)
            return;

        m_folderImage = new BitmapImage();
        m_folderImage.BeginInit();
        m_folderImage.UriSource = new Uri("pack://application:,,,/Resources/Folder.png");
        m_folderImage.EndInit();

        m_fileImage = new BitmapImage();
        m_fileImage.BeginInit();
        m_fileImage.UriSource = new Uri("pack://application:,,,/Resources/File.png");
        m_fileImage.EndInit();
    }

    public static void LoadFileList (TreeView treeView, string rootFolder, string[] files, string fileNamePrefix = null) {

        InitTreeIcons();

        for (int ii = 0; ii < files.Length; ++ii) {
            var file = files[ii];

            if (file.ToLower().StartsWith(rootFolder))
                file = file.Substring(rootFolder.Length + 1);

            string[] parts = file.Split(new char[] { '\\' });
            parts[parts.Length - 1] = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);


            TreeViewItem parent = null;

            for (int jj = 0; jj < parts.Length; ++jj) {
                string text = parts[jj];

				bool isFile = jj + 1 >= parts.Length;

				var image = new Image();
                image.Source = isFile ? m_fileImage : m_folderImage;
                image.Width = 16;
                image.Height = 16;
				
				Label lbl = new Label();

				if(fileNamePrefix != null && isFile)
					lbl.Content = fileNamePrefix + text;
				else
					lbl.Content = text;

				StackPanel stack = new StackPanel();
                stack.Orientation = Orientation.Horizontal;
                stack.Children.Add(image);
                stack.Children.Add(lbl);

                var item = new TreeViewItem() { Header = stack, IsExpanded = false };

                parent = UseOrAddToParent(parent == null ? treeView.Items : parent.Items, item);
            }
        }

    }

    public static TreeViewItem UseOrAddToParent (ItemCollection items, TreeViewItem newItem) {

        var panel = newItem.Header as StackPanel;
        var label = panel.Children[1] as Label;
        var newItemName = label.Content as string;

        foreach (var item in items) {
            var treeItem = item as TreeViewItem;
            panel = treeItem.Header as StackPanel;
            label = panel.Children[1] as Label;
            var itemName = label.Content as string;

            if (newItemName == itemName)
                return treeItem;
        }

        items.Add(newItem);
        return newItem;
    }

    public static string GetTreeViewItemPath (TreeView treeView, bool allowParents = false) {

        string name = null;

        var item = treeView.SelectedItem as TreeViewItem;

        if (item == null)
            return null;

        bool hasChildren = !item.Items.IsEmpty;
        if (!allowParents && hasChildren)
            return null;

        while (item != null) {
            var panel = item.Header as StackPanel;
            var label = panel.Children[1] as Label;
            var text = label.Content as string;

            if (name == null)
                name = text;
            else
                name = text + "\\" + name;
            item = item.Parent as TreeViewItem;
        }

        if (hasChildren)
            name += "\\";

        return name;
    }

    public static void SelectByPath(TreeView treeView, string filePath) {

        string[] parts = filePath.Split(new char[] { '\\' });
        parts[parts.Length - 1] = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

        var parentItems = treeView.Items;
        for (int ii = 0; ii < parts.Length; ++ii) {
            string text = parts[ii];

            var child = FindChild(parentItems, text);
            if (child == null)
                return;

            if (ii + 1 == parts.Length)
                child.IsSelected = true;
            else
                parentItems = child.Items;
        }
    }

    public static TreeViewItem FindChild (ItemCollection items, string name) {
        
        foreach (var item in items) {
            var treeItem = item as TreeViewItem;
            var panel = treeItem.Header as StackPanel;
            var label = panel.Children[1] as Label;
            var itemName = label.Content as string;

            if (name == itemName)
                return treeItem;
        }

        return null;
    }

    public static void ClearTreeViewSelection (TreeView tv) {
        if (tv != null)
            ClearTreeViewItemsControlSelection(tv.Items, tv.ItemContainerGenerator);
    }
    private static void ClearTreeViewItemsControlSelection (ItemCollection ic, ItemContainerGenerator icg) {
        if ((ic != null) && (icg != null))
            for (int i = 0; i < ic.Count; i++) {
                TreeViewItem tvi = icg.ContainerFromIndex(i) as TreeViewItem;
                if (tvi != null) {
                    ClearTreeViewItemsControlSelection(tvi.Items, tvi.ItemContainerGenerator);
                    tvi.IsSelected = false;
                }
            }
    }
}
