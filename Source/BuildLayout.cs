
using System;
using System.IO;
using System.Xml.Serialization;

public class Build {

    [XmlElementAttribute("spec")]
    public BuildSpec[] Specializations { get; set; }
    [XmlAttribute()]
    public string profession { get; set; }

    public static Build LoadBuild (string filePath) {

        if (!File.Exists(filePath))
            return null;

        var content = File.ReadAllText(filePath);
        var sr = new StringReader(content);
        var xs = new XmlSerializer(typeof(Build));

        var build = xs.Deserialize(sr) as Build;

        foreach (var spec in build.Specializations) {
            string[] values = spec.traits.Split(new char[] { ',' });
            spec.traitValues = new int[3];
            for (int ii = 0; ii < 3; ++ii)
                spec.traitValues[ii] = Convert.ToInt32(values[ii]);

            var profession = Localization.Instance.GetProfession(build.profession);
            spec.specIndex = profession.GetSpecIndex(spec.name);
        }

        return build;
    }

    public static bool SaveBuild (Build build, string filePath) {

        try {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(filePath));
            Directory.CreateDirectory(fullPath);

            XmlSerializer ser = new XmlSerializer(typeof(Build));
            TextWriter writer = new StringWriter();
            ser.Serialize(writer, build);
            string contents = writer.ToString();

            contents = contents.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" ", "");
            contents = contents.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            File.WriteAllText(filePath, contents);

            return true;
        }
        catch { }

        return false;
    }
}

public class BuildSpec {
    [XmlAttributeAttribute()]
    public string name { get; set; }

    [XmlAttributeAttribute()]
    public string traits { get; set; }

    [XmlIgnoreAttribute]
    public int[] traitValues;
    [XmlIgnoreAttribute]
    public int specIndex;
}
