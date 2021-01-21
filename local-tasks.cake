// Copyright (c) The NUnit Project and distributed under the MIT License

// This file contains tasks intended to be run locally by developers
// in any NUnit project. To use these tasks in a project, copy the file
// to an accessible directory and #load it in your build.cake file.

// Since the tasks are intended for use by the NUnit Project, it follows
// certain conventions and will need to be modified for use elsewhere.

//////////////////////////////////////////////////////////////////////
// DELETE ALL OBJ DIRECTORIES
//////////////////////////////////////////////////////////////////////

Task("DeleteObjectDirectories")
    .WithCriteria(BuildSystem.IsLocalBuild)
    .Does(() =>
    {
        Information("Deleting object directories");

        foreach (var dir in GetDirectories("src/**/obj/"))
            DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
    });

// NOTE: Any project to which this file is added is required to have a 'Clean' target
Task("CleanAll")
    .Description("Perform standard 'Clean' followed by deleting object directories")
    .IsDependentOn("Clean")
    .IsDependentOn("DeleteObjectDirectories");

//////////////////////////////////////////////////////////////////////
// CHECK FOR MISSING AND NON-STANDARD FILE HEADERS
//////////////////////////////////////////////////////////////////////

static readonly int CD_LENGTH = Environment.CurrentDirectory.Length + 1;

// Standard Header. Change this for each project as needed.
static readonly string[] STD_HDR = new [] {
    "// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt"
};

Task("CheckHeaders")
    .WithCriteria(BuildSystem.IsLocalBuild)
    .Does(() =>
    {
        var NoHeader = new List<FilePath>();
        var NonStandard = new List<FilePath>();
        int examined = 0;

        foreach(var file in GetFilesToCheck())
        {
            examined++;
            var header = GetHeader(file);
            if (header.Count == 0)
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

        Information($"\nFiles Examined: {examined}");
        Information($"Missing Headers: {NoHeader.Count}");
        Information($"Non-Standard Headers: {NonStandard.Count}");
    });

private List<FilePath> GetFilesToCheck()
{
    var files = new List<FilePath>();
    foreach(var file in GetFiles("src/**/*.cs"))
        if (file.GetFilename().ToString().ToLower() != "assemblyinfo.cs")
            files.Add(file);
    return files;
}

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

private void ListFilesWithNoHeader()
{
}

private void ListFilesWithNonStandardHeader()
{
}

private string RelPathTo(FilePath file)
{
    return file.ToString().Substring(CD_LENGTH);
}

