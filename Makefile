.PHONY: build clean install publish uninstall

APPNAME:=smb-healthcheck-widget
ifeq ($(OS),Windows_NT)
	BINPREFIX:=$(APPDATA)\Microsoft\Windows\Start Menu\Programs\Startup
	APPDIR:=$(LOCALAPPDATA)\$(APPNAME)
else
	USER:=$(shell whoami)
	BINPREFIX:=/home/$(USER)/.local/bin
	APPDIR:=/home/$(USER)/.local/share/$(APPNAME)
endif

build:
	cd src && dotnet build -c Release

publish:
	cd src && dotnet publish -o bin/output


ifeq ($(OS),Windows_NT)
install: publish
# put correct files in place
	mkdir $(APPDIR)
	xcopy /I /Y /E src\bin\output\ $(APPDIR)
# enable startup service
	powershell -c "$$s=(New-Object -COM WScript.Shell).CreateShortcut('$(BINPREFIX)\$(APPNAME).lnk');$$s.TargetPath='$(APPDIR)\$(APPNAME)';$$s.Save()"

uninstall:
	rmdir /S /Q $(APPDIR)
	del "$(BINPREFIX)\$(APPNAME).lnk"

clean:
	rmdir /S /Q src/bin
	rmdir /S /Q src/obj
else
install: publish
# put correct files in place
	mkdir -p $(APPDIR)
	cp -r src/bin/output/* $(APPDIR)/
	ln -s $(APPDIR)/$(APPNAME) $(BINPREFIX)/$(APPNAME)
# enable startup service
	mkdir -p /home/$(USER)/.config/systemd/user
	cp src/Linux/$(APPNAME).service /home/$(USER)/.config/systemd/user/
	systemctl --user daemon-reload
	systemctl --user enable $(APPNAME)
uninstall:
	systemctl --user stop $(APPNAME) || true
	systemctl --user disable $(APPNAME) || true
	rm -rf $(APPDIR)
	rm $(BINPREFIX)/$(APPNAME)
	rm /home/$(USER)/.config/systemd/user/$(APPNAME).service

clean:
	rm -rf src/bin
	rm -rf src/obj
endif
