.PHONY: build clean install publish uninstall

APPNAME:=smb-healthcheck-widget
ifeq ($(OS),Windows_NT)
	BINPREFIX:=$(APPDATA)\Microsoft\Windows\Start Menu\Programs\Startup
	APPDIR:=$(LOCALAPPDATA)\$(APPNAME)
else
	BINPREFIX:=/usr/local/bin
	USER:=/home/$(shell who am i | awk '{print $$1}')
	APPDIR:=$(USER)/.local/share/$(APPNAME)
endif

build:
	cd src && dotnet build -c Release

publish:
	cd src && dotnet publish -o bin/output


ifeq ($(OS),Windows_NT)
install: publish
	mkdir $(APPDIR)
	xcopy /I /Y /E src\bin\output\ $(APPDIR)
	powershell -c "$$s=(New-Object -COM WScript.Shell).CreateShortcut('$(BINPREFIX)\$(APPNAME).lnk');$$s.TargetPath='$(APPDIR)\$(APPNAME)';$$s.Save()"

uninstall:
	rmdir /S /Q $(APPDIR)
	del "$(BINPREFIX)\$(APPNAME).lnk"

clean:
	rmdir /S /Q src/bin
	rmdir /S /Q src/obj
else
install: publish
	mkdir -p $(APPDIR)
	cp -r src/bin/output/* $(APPDIR)/
	ln -s $(BINPREFIX)/$(APPNAME) $(APPDIR)/$(APPNAME)

uninstall:
	rm -rf $(APPDIR)
	rm $(BINPREFIX)/$(APPNAME)

clean:
	rm -rf src/bin
	rm -rf src/obj
endif
