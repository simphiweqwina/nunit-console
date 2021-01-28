//////////////////////////////////////////////////////////////////////
// CHECK FOR MISSING AND NON-STANDARD FILE HEADERS
//////////////////////////////////////////////////////////////////////

static readonly int CD_LENGTH = Environment.CurrentDirectory.Length + 1;

static readonly string[] EXEMPT_FILES = new [] {
    "AssemblyInfo.cs",
    "Options.cs",
    "ProcessUtils.cs",
    "ProcessUtilsTests.cs"
};

// Standard Header. Change this for each project as needed.
static readonly string[] STD_HDR = new [] {
    "// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt"
};

Task("CheckHeaders")
    .Does(() =>
    {
        var NoHeader = new List<FilePath>();
        var NonStandard = new List<FilePath>();
        var Exempted = new List<FilePath>();
        int examined = 0;

        foreach(var file in GetFiles("src/**/*.cs"))
        {
            examined++;
            var header = GetHeader(file);
            if (EXEMPT_FILES.Contains(file.GetFilename().ToString()))
                Exempted.Add(file);
            else if (header.Count == 0)
                NoHeader.Add(file);
            else if (!header.SequenceEqual(STD_HDR))
                NonStandard.Add(file);
        }

        if (NoHeader.Count > 0)
        {
            Information("\nFILES WITH NO HEADER\n");
            foreach(var file in NoHeader)
                Information(RelPathTo(file));
        }

        if (NonStandard.Count > 0)
        {
            Information("\nFILES WITH A NON-STANDARD HEADER\n");
            foreach(var file in NonStandard)
            {
                Information(RelPathTo(file));
                Information("");
                foreach(string line in GetHeader(file))
                    Information(line);
                Information("");
            }
        }

        if (Exempted.Count > 0)
        {
            Information("\nEXEMPTED FILES (NO CHECK MADE)\n");
            foreach(var file in Exempted)
                Information(RelPathTo(file));
        }

        Information($"\nFiles Examined: {examined}");
        Information($"Missing Headers: {NoHeader.Count}");
        Information($"Non-Standard Headers: {NonStandard.Count}");
        Information($"Exempted Files: {Exempted.Count}");

        if (NoHeader.Count > 0 || NonStandard.Count > 0)
            throw new Exception("Missing or invalid file headers found");
    });

private List<string> GetHeader(FilePath file)
{
    var header = new List<string>();
    var lines = System.IO.File.ReadLines(file.ToString());

    foreach(string line in lines)
    {
        if (!line.StartsWith("//"))
            break;
            
        header.Add(line);
    }

    return header;
}

private string RelPathTo(FilePath file)
{
    return file.ToString().Substring(CD_LENGTH);
}
