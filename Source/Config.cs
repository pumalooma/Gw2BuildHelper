
using System.IO;
using System.Xml.Serialization;

public class Config
{
	[XmlAttribute()]
	public int InterfaceSize { get; set; }
	[XmlAttribute()]
	public bool ShowCategories { get; set; }
	[XmlAttribute()]
    public double WindowLeft { get; set; }
    [XmlAttribute()]
    public double WindowTop { get; set; }
    [XmlAttribute()]
    public double WindowWidth { get; set; }
    [XmlAttribute()]
    public double WindowHeight { get; set; }
    
    private static Config instance;

	public static Config Instance {
		get { return instance ?? LoadConfig(); }
	}

	private static Config LoadConfig() {
        try {
            var content = File.ReadAllText("Config.txt");
            var sr = new StringReader(content);
            var xs = new XmlSerializer(typeof(Config));

            instance = xs.Deserialize(sr) as Config;
            return instance;
        }
        catch { }

        instance = new Config();
        instance.InterfaceSize = 1;
		instance.ShowCategories = true;
        return instance;
    }

    public void SetWindowPosition() {
        if (WindowWidth > 0) {
            Gw2BuildHelper.MainWindow.instance.Left = WindowLeft;
            Gw2BuildHelper.MainWindow.instance.Top = WindowTop;
            Gw2BuildHelper.MainWindow.instance.Width = WindowWidth;
            Gw2BuildHelper.MainWindow.instance.Height = WindowHeight;
        }

    }


    public bool SaveConfig () {
        WindowLeft = Gw2BuildHelper.MainWindow.instance.Left;
        WindowTop = Gw2BuildHelper.MainWindow.instance.Top;
        WindowWidth = Gw2BuildHelper.MainWindow.instance.Width;
        WindowHeight = Gw2BuildHelper.MainWindow.instance.Height;

        try {
            XmlSerializer ser = new XmlSerializer(typeof(Config));
            TextWriter writer = new StringWriter();
            ser.Serialize(writer, this);
            string contents = writer.ToString();

            contents = contents.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" ", "");
            contents = contents.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            File.WriteAllText("Config.txt", contents);

            return true;
        }
        catch { }

        return false;
    }
}
