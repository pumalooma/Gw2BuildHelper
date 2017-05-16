
using System.IO;
using System.Xml.Serialization;

public class InterfaceSize
{
    [XmlAttribute()]
    public int HeroPanelOffsetX { get; set; }
    [XmlAttribute()]
    public int HeroPanelOffsetY { get; set; }
    
    [XmlAttribute()]
	public double TraitOffsetX { get; set; }
	[XmlAttribute()]
	public double TraitOffsetY { get; set; }
	[XmlAttribute()]
	public double TraitSpacingX { get; set; }
	[XmlAttribute()]
	public double TraitSpacingY { get; set; }
	[XmlAttribute()]
	public double TraitIconSize { get; set; }
    [XmlAttribute()]
    public double TraitChoiceSpacing { get; set; }
    	
	[XmlAttribute()]
	public double SpecDropDownOffsetX { get; set; }
	[XmlAttribute()]
	public double SpecDropDownOffsetY { get; set; }
	[XmlAttribute()]
	public double SpecDropDownWidth { get; set; }
	[XmlAttribute()]
	public double SpecDropDownHeight { get; set; }
	[XmlAttribute()]
	public double SpecDropDownSpacingY { get; set; }
	[XmlAttribute()]
	public int SpecDropDownMouseOverSizeX { get; set; }

    [XmlAttribute()]
    public double SpecCorrectChoiceOffsetX { get; set; }
    [XmlAttribute()]
    public double SpecCorrectChoiceOffsetY { get; set; }
    [XmlAttribute()]
    public double SpecCorrectChoiceSpacingX { get; set; }
    [XmlAttribute()]
    public double SpecCorrectChoiceSpacingY { get; set; }
    [XmlAttribute()]
    public double SpecCorrectChoiceIconSize { get; set; }
    [XmlAttribute()]
	public float SpecSourceImageOffsetX { get; set; }
	[XmlAttribute()]
	public float SpecSourceImageOffsetY { get; set; }
	[XmlAttribute()]
	public float SpecSourceImageWidth { get; set; }
	[XmlAttribute()]
	public float SpecSourceImageHeight { get; set; }
	[XmlAttribute()]
	public float SpecSourceImageSpacingY { get; set; }

	
	public static InterfaceSize LoadInterfaceSize()
	{
		string fileName = "InterfaceSmall.txt";

		if(Config.Instance.InterfaceSize == 1)
			fileName = "InterfaceNormal.txt";
		else if(Config.Instance.InterfaceSize == 2)
			fileName = "InterfaceLarge.txt";
		else if(Config.Instance.InterfaceSize == 3)
			fileName = "InterfaceXLarge.txt";

		var content = File.ReadAllText(fileName);
		var sr = new StringReader(content);
		var xs = new XmlSerializer(typeof(InterfaceSize));

		var instance = xs.Deserialize(sr) as InterfaceSize;
		return instance;
	}

}
