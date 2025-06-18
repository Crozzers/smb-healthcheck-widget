.PHONY: build clean install publish uninstall

BINPREFIX:=/usr/local/bin
USER:=/home/$(shell who am i | awk '{print $$1}')
APPDIR:=$(USER)/.local/share/smb-healthcheck-widget

build:
	cd src && dotnet build -c Release

publish:
	cd src && dotnet publish -o bin/output

clean:
	rm -r src/bin
	rm -r src/obj

install: publish
	mkdir -p $(APPDIR)
	cp -r src/bin/output/* $(APPDIR)
	ln -s $(APPDIR)/smb-healthcheck-widget $(BINPREFIX)/smb-healthcheck-widget

uninstall:
	rm -r $(APPDIR)
	rm $(BINPREFIX)/smb-healthcheck-widget