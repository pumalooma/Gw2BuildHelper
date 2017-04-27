using System.IO;
using System.Xml.Serialization;

public class Localization {
    [XmlElementAttribute("Profession")]
    public ProfessionDef[] professions { get; set; }

	private static Localization instance;

	public static Localization Instance {
		get { return instance ?? LoadLocalization(); }
	}

	private static Localization LoadLocalization() {
		var content = File.ReadAllText("Localization.txt");
        var sr = new StringReader(content);
		var xs = new XmlSerializer(typeof(Localization));

		instance = xs.Deserialize(sr) as Localization;
		return instance;
	}

	public ProfessionDef GetProfession(string name) {
		foreach(var profession in professions)
			if(profession.name == name)
				return profession;

		return null;
	}
}

public class ProfessionDef {
    [XmlAttributeAttribute()]
    public string name { get; set; }
    [XmlAttributeAttribute()]
    public string id { get; set; }

    [XmlElementAttribute("Specialization")]
	public SpecializationDef[] Specializations { get; set; }

    public int GetSpecIndex(string specName) {
        for (int ii = 0; ii < Specializations.Length; ++ii)
            if (specName == Specializations[ii].name)
                return ii;

        return -1;
    }
}

public class SpecializationDef {
    [XmlAttributeAttribute()]
    public string name { get; set; }
}
