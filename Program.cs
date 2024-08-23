using System.Diagnostics;
using System.Reflection;

const string PROGRAM_NAME = "xsel-win";

string VERSION = $"""
    {PROGRAM_NAME} {Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}
    Copyright (c) 2024 Max Kagamine <https://github.com/maxkagamine>
    Licensed under the Apache License, Version 2.0

    Incorporates code from TextCopy by Simon Cropp, MIT license.
    <https://github.com/CopyText/TextCopy>
    """;

const string USAGE = """
    Usage: xsel [options]
    xsel shim for WSL to manipulate the Windows clipboard, and allow Linux programs
    that leverage xsel for copy/paste to do the same.

    By default the clipboard is output and not modified if both standard input and
    standard output are terminals (ttys). Otherwise, the clipboard is output if
    standard output is not a terminal (tty), and the clipboard is set from standard
    input if standard input is not a terminal (tty). If any input or output options
    are given then the program behaves only in the requested mode.

    NOTE: Due to limitations of EXEs running under WSL, TTY detection is a best
    effort. If stdin doesn't have data ready immediately, it'll assume it's not a
    pipe. The -i/-o options should be preferred.

    If both input and output is required then the previous clipboard is output
    before being replaced by the contents of standard input.

    Input options
      -a, --append          Append standard input to the clipboard
      -f, --follow          <Not supported>
      -z, --zeroflush       <Not supported>
      -i, --input           Read standard input into the clipboard

    Output options
      -o, --output          Write the clipboard to standard output

      --keep-crlf           <Windows-only addition> By default, CRLF is replaced
                            with LF when pasting. Pass this option to disable.

    Action options
      -c, --clear           Clear the clipboard
      -d, --delete          <Not supported>

    Selection options
      -p, --primary         The PRIMARY and SECONDARY selections have no equivalent
      -s, --secondary       on Windows, but since some Linux clipboard managers
      -b, --clipboard       sync the selection and clipboard buffers, this acts as
                            if that's the case and ignores the chosen selection.

      -k, --keep            <No-op>
      -x, --exchange        <No-op>

    X options
      --display displayname <Not supported>
      -m wm, --name wm      <Not supported>
      -t ms, --selectionTimeout ms
                            <Not supported>

    Miscellaneous options
      --trim                Remove newline ('\n') char from end of input / output
      -l, --logfile         <Not supported>
      -n, --nodetach        <Ignored>

      -h, --help            Display this help and exit
      -v, --verbose         <Ignored>
      --version             Output version information and exit

                                            This xsel has Super Rin-chan Powers ♫
    """;

// Options handling logic (& above help text) based on xsel 1.2.1
bool showVersion = false;
bool showHelp = false;
bool doAppend = false;
bool doKeep = false;
bool doExchange = false;
bool doClear = false;
bool doInput = false;
bool doOutput = false;
bool forceInput = false;
bool forceOutput = false;
bool trimTrailingNewline = false;
bool keepCrlf = false;
string? oldSel = null;

// Specify default behavior based on input and output file types
if (!Console.IsInputRedirected && !Console.IsOutputRedirected)
{
    // Solo invocation; display the clipboard and exit
    doInput = false;
    doOutput = true;
}
else
{
    // Give expected behavior with basic usage of "xsel < foo", "xsel > foo", etc.
    //
    // NOTE: It seems that when an exe is run from WSL, if *either* stdin or stdout is redirected in Linux, *both* pipes
    // will be redirected in Windows. There doesn't appear to be a way to tell which (if either) is actually the TTY, so
    // this is a best effort: if stdin is a pipe, it'll hopefully have data ready to send right away.
    bool isStdinProbablyRedirected = Task.WaitAll([Task.Run(Console.In.Peek)], TimeSpan.FromMilliseconds(50));

    doInput = isStdinProbablyRedirected;
    doOutput = !isStdinProbablyRedirected;
}

// Expand args array before parsing to uncombine arguments
var expandedArgs = args.SelectMany(arg =>
    arg.Length > 2 && arg[0] == '-' && arg[1] != '-' ? arg[1..].Select(opt => "-" + opt) : [arg]);

// Parse options
foreach (string arg in expandedArgs)
{
    switch (arg)
    {
        case "--help" or "-h":
            showHelp = true;
            break;
        case "--version":
            showVersion = true;
            break;
        case "--append" or "-a":
            forceInput = true;
            doOutput = false;
            doAppend = true;
            break;
        case "--input" or "-i":
            forceInput = true;
            doOutput = false;
            break;
        case "--clear" or "-c":
            doOutput = false;
            doClear = true;
            break;
        case "--output" or "-o":
            doInput = false;
            forceOutput = true;
            break;
        case "--trim":
            trimTrailingNewline = true;
            break;
        case "--keep" or "-k":
            doKeep = true;
            break;
        case "--exchange" or "-x":
            doExchange = true;
            break;
        case "--keep-crlf":
            keepCrlf = true;
            break;

        // Ignored options
        case "--verbose" or "-v":
        case "--primary" or "-p":
        case "--secondary" or "-s":
        case "--clipboard" or "-b":
        case "--nodetach" or "-n":
            break;

        // Easter egg (the message at the bottom of the help text is a reference to apt-get's Super Cow Powers)
        case "rin" or "rin-chan" or "--rin" or "--rin-chan" or "moo":
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://www.youtube.com/watch?v=hSHxPPV2zKU&list=PLYooEAFUfhDfevWFKLa7gh3BogBUAebYO&index=1",
                UseShellExecute = true
            });
            return 0;

        default:
            Console.Error.WriteLine($"{PROGRAM_NAME}: unsupported option: {arg}");
            return 1;
    }
}

if (showVersion)
{
    Console.WriteLine(VERSION);
}

if (showHelp)
{
    Console.WriteLine(USAGE);
}

if (showVersion || showHelp)
{
    return 0;
}

if (doKeep || doExchange)
{
    // No-op
    return 0;
}

if (doOutput || forceOutput)
{
    oldSel = GetClipboard();
    Console.Write(oldSel);
}

if (doClear)
{
    Clipboard.Clear();
}
else if (doInput || forceInput)
{
    string newSel = "";

    if (doAppend)
    {
        oldSel ??= GetClipboard();
        newSel = oldSel;
    }

    newSel += Console.In.ReadToEnd();

    if (trimTrailingNewline)
    {
        newSel = newSel.TrimEnd('\n');
    }

    Clipboard.SetText(newSel);
}

return 0;

string GetClipboard()
{
    string text = Clipboard.GetText() ?? "";

    if (!keepCrlf)
    {
        text = text.ReplaceLineEndings("\n");
    }

    if (trimTrailingNewline)
    {
        text = text.TrimEnd('\n');
    }

    return text;
}
