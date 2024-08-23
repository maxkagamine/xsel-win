# <img src="https://github.com/user-attachments/assets/cef88b49-8a0a-4b10-ac2c-24e29a11baf7" height="30" />&ensp;xsel-win

xsel shim for WSL to manipulate the Windows clipboard, and allow Linux programs that leverage xsel for copy/paste to do the same.

I created this after getting fed up with Powershell being slow and janky, particularly with regards to Unicode handling. The compiled `xsel` is a native x64 Windows executable; WSL can run exe's without an extension, so no wrapper script is necessary.

## Installing

### From releases

```sh
$ sudo curl -L https://github.com/maxkagamine/xsel-win/releases/latest/download/xsel -o /usr/local/bin/xsel
$ sudo chmod 755 /usr/local/bin/xsel
```

### From source

```sh
$ git clone https://github.com/maxkagamine/xsel-win.git
$ cd xsel-win
$ sudo make install
```

Requires dotnet.exe to build. Installs to /usr/local/bin by default; add `PREFIX=~/.local` to install to ~/.local/bin instead.

## Usage

```
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
```

## License

[Apache 2.0](LICENSE.txt)
