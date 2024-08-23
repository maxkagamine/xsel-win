VERSION ?= $(shell git describe --tags --match 'v*' | sed 's/^v//')
PREFIX ?= /usr/local
DOTNET ?= dotnet.exe

.PHONY: install

xsel-win.exe: xsel-win.csproj $(wildcard *.cs)
	$(DOTNET) publish xsel-win.csproj -o . -p:Version=$(VERSION)

install: xsel-win.exe
	install -d $(PREFIX)/bin
	install -m 755 xsel-win.exe $(PREFIX)/bin/xsel
