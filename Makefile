# vcon — .NET 8 / WPF (primary dotnet shortcuts; see AGENTS.md)
SLN := src/Vcon.sln
OVERLAY := src/Vcon.Overlay/Vcon.Overlay.csproj
PROPS := Directory.Build.props
CONFIG := Release
RID := win-x64
PUBLISH_DIR := src/Vcon.Overlay/bin/$(CONFIG)/net8.0-windows/$(RID)/publish
DIST_DIR := dist

# Read VersionPrefix from Directory.Build.props
ifeq ($(OS),Windows_NT)
VERSION := $(shell powershell -NoProfile -Command "[xml]$$x = Get-Content -Raw '$(PROPS)'; $$x.Project.PropertyGroup.VersionPrefix")
else
VERSION := $(shell sed -n 's/.*<VersionPrefix>\([^<]*\).*/\1/p' $(PROPS))
endif

.DEFAULT_GOAL := build

.PHONY: build test run clean clean-profiles lint fmt restore help publish version \
        install-vigembus install-innosetup install-wix \
        installer-exe installer-msi installer release release-github \
        version-bump-patch version-bump-minor version-bump-major version-set

build:
	dotnet build $(SLN) -c $(CONFIG)

test:
	dotnet test $(SLN) -c $(CONFIG) --no-build

run:
	dotnet run --project $(OVERLAY)

clean:
	dotnet clean $(SLN)
ifeq ($(OS),Windows_NT)
	powershell -NoProfile -Command "Get-ChildItem -Path src,tests -Recurse -Directory -ErrorAction SilentlyContinue | Where-Object { $$_.Name -in 'bin','obj' } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue"
	powershell -NoProfile -Command "if (Test-Path '$(DIST_DIR)') { Remove-Item '$(DIST_DIR)' -Recurse -Force }"
else
	find src tests -type d \( -name bin -o -name obj \) -exec rm -rf {} +
	rm -rf $(DIST_DIR)
endif

clean-profiles:
	@echo Removing user profiles from %APPDATA%/vcon/profiles ...
ifeq ($(OS),Windows_NT)
	powershell -NoProfile -Command "$$dir = Join-Path $$env:APPDATA 'vcon/profiles'; if (Test-Path $$dir) { Remove-Item $$dir -Recurse -Force; Write-Host 'Removed:' $$dir } else { Write-Host 'Nothing to remove - directory does not exist.' }"
else
	@rm -rfv "$${HOME}/.config/vcon/profiles" 2>/dev/null || echo "Nothing to remove."
endif

lint:
	dotnet format $(SLN) --verify-no-changes

fmt:
	dotnet format $(SLN)

restore:
	dotnet restore $(SLN)

publish:
	dotnet publish $(OVERLAY) -c $(CONFIG) --self-contained -r $(RID)

version:
	@echo $(VERSION)

# ── Version management ──────────────────────────────────────────────

version-bump-patch:
	@powershell -NoProfile -Command " \
		$$xml = [xml](Get-Content -Raw '$(PROPS)'); \
		$$v = [version]$$xml.Project.PropertyGroup.VersionPrefix; \
		$$nv = \"$$( $$v.Major ).$$( $$v.Minor ).$$( $$v.Build + 1 )\"; \
		$$xml.Project.PropertyGroup.VersionPrefix = $$nv; \
		$$xml.Save('$(PROPS)'); \
		Write-Host \"Version: $$( $$v ) -> $$nv\""

version-bump-minor:
	@powershell -NoProfile -Command " \
		$$xml = [xml](Get-Content -Raw '$(PROPS)'); \
		$$v = [version]$$xml.Project.PropertyGroup.VersionPrefix; \
		$$nv = \"$$( $$v.Major ).$$( $$v.Minor + 1 ).0\"; \
		$$xml.Project.PropertyGroup.VersionPrefix = $$nv; \
		$$xml.Save('$(PROPS)'); \
		Write-Host \"Version: $$( $$v ) -> $$nv\""

version-bump-major:
	@powershell -NoProfile -Command " \
		$$xml = [xml](Get-Content -Raw '$(PROPS)'); \
		$$v = [version]$$xml.Project.PropertyGroup.VersionPrefix; \
		$$nv = \"$$( $$v.Major + 1 ).0.0\"; \
		$$xml.Project.PropertyGroup.VersionPrefix = $$nv; \
		$$xml.Save('$(PROPS)'); \
		Write-Host \"Version: $$( $$v ) -> $$nv\""

version-set:
ifndef VERSION_NEW
	$(error Usage: make version-set VERSION_NEW=x.y.z)
endif
	@powershell -NoProfile -Command " \
		$$xml = [xml](Get-Content -Raw '$(PROPS)'); \
		$$old = $$xml.Project.PropertyGroup.VersionPrefix; \
		$$xml.Project.PropertyGroup.VersionPrefix = '$(VERSION_NEW)'; \
		$$xml.Save('$(PROPS)'); \
		Write-Host \"Version: $$old -> $(VERSION_NEW)\""

# ── Tool installation ───────────────────────────────────────────────

install-vigembus:
	@echo Downloading and installing ViGEmBus driver (UAC prompt expected)...
	@powershell -NoProfile -ExecutionPolicy Bypass -Command " \
		$$ErrorActionPreference = 'Stop'; \
		$$release = Invoke-RestMethod 'https://api.github.com/repos/nefarius/ViGEmBus/releases/latest'; \
		$$asset = $$release.assets | Where-Object { $$_.name -like 'ViGEmBus*' -and ($$_.name -like '*.exe' -or $$_.name -like '*.msi') } | Select-Object -First 1; \
		if (-not $$asset) { throw 'No ViGEmBus installer found in latest release' }; \
		$$installer = Join-Path $$env:TEMP $$asset.name; \
		Write-Host ('Downloading ' + $$asset.browser_download_url + '...'); \
		Invoke-WebRequest $$asset.browser_download_url -OutFile $$installer; \
		Write-Host 'Launching installer...'; \
		Start-Process $$installer -Verb RunAs -Wait; \
		Remove-Item $$installer -ErrorAction SilentlyContinue; \
		Write-Host 'ViGEmBus installation complete.'"

install-innosetup:
	@echo Installing Inno Setup via winget...
	winget install --id JRSoftware.InnoSetup --accept-package-agreements --accept-source-agreements

install-wix:
	@echo Installing WiX Toolset v5 as a .NET global tool...
	dotnet tool install --global wix

# ── Installers ──────────────────────────────────────────────────────

$(DIST_DIR):
ifeq ($(OS),Windows_NT)
	@powershell -NoProfile -Command "if (-not (Test-Path '$(DIST_DIR)')) { New-Item -ItemType Directory -Path '$(DIST_DIR)' | Out-Null }"
else
	@mkdir -p $(DIST_DIR)
endif

installer-exe: publish $(DIST_DIR)
	@echo Building EXE installer (Inno Setup) for v$(VERSION)...
	@powershell -NoProfile -Command " \
		$$iscc = (Get-Command iscc.exe -ErrorAction SilentlyContinue).Source; \
		if (-not $$iscc) { \
			$$searchDirs = $$env:ProgramFiles, $${env:ProgramFiles(x86)}, \"$$env:LOCALAPPDATA\Programs\"; \
			foreach ($$dir in $$searchDirs) { \
				$$candidate = Join-Path $$dir 'Inno Setup 6\iscc.exe'; \
				if (Test-Path $$candidate) { $$iscc = $$candidate; break } \
			} \
		}; \
		if (-not $$iscc) { throw 'iscc.exe not found. Run: make install-innosetup' }; \
		Write-Host \"Using: $$iscc\"; \
		& $$iscc /DAppVersion=$(VERSION) /DPublishDir=..\$(PUBLISH_DIR) /DDistDir=..\$(DIST_DIR) installer\vcon.iss"

installer-msi: publish $(DIST_DIR)
	@echo Building MSI installer (WiX) for v$(VERSION)...
ifeq ($(OS),Windows_NT)
	@powershell -NoProfile -Command " \
		$$absPublish = (Resolve-Path '$(PUBLISH_DIR)').Path; \
		$$absDist = (Resolve-Path '$(DIST_DIR)').Path; \
		wix build -arch x64 -d Version=$(VERSION) -d \"PublishDir=$$absPublish\" -o \"$$absDist\vcon-$(VERSION).msi\" installer/vcon.wxs"
else
	wix build -arch x64 -d Version=$(VERSION) -d PublishDir=$(CURDIR)/$(PUBLISH_DIR) -o $(DIST_DIR)/vcon-$(VERSION).msi installer/vcon.wxs
endif

installer: installer-exe installer-msi

# ── Release ─────────────────────────────────────────────────────────

release: clean
	$(MAKE) build
	$(MAKE) test
	$(MAKE) installer
	@echo.
	@echo Release artifacts in $(DIST_DIR)/:
ifeq ($(OS),Windows_NT)
	@powershell -NoProfile -Command "Get-ChildItem '$(DIST_DIR)' | ForEach-Object { Write-Host ('  ' + $$_.Name + '  (' + [math]::Round($$_.Length/1MB, 1) + ' MB)') }"
else
	@ls -lh $(DIST_DIR)/
endif

release-github: release
	@echo Creating GitHub release v$(VERSION)...
	gh release create v$(VERSION) \
		$(DIST_DIR)/vcon-$(VERSION)-setup.exe \
		$(DIST_DIR)/vcon-$(VERSION).msi \
		--title "vcon v$(VERSION)" \
		--generate-notes

# ── Help ────────────────────────────────────────────────────────────

help:
	@echo vcon Makefile targets:
	@echo.
	@echo   Build:
	@echo     build              - dotnet build $(SLN) -c $(CONFIG)
	@echo     test               - dotnet test $(SLN) -c $(CONFIG) --no-build
	@echo     run                - dotnet run --project $(OVERLAY)
	@echo     clean              - dotnet clean + remove bin/obj/dist
	@echo     clean-profiles     - remove user profiles from %%APPDATA%%/vcon/profiles
	@echo     lint               - dotnet format --verify-no-changes
	@echo     fmt                - dotnet format
	@echo     restore            - dotnet restore
	@echo     publish            - dotnet publish --self-contained -r $(RID)
	@echo.
	@echo   Version (current: $(VERSION)):
	@echo     version            - show current version
	@echo     version-bump-patch - increment patch  (x.y.Z)
	@echo     version-bump-minor - increment minor  (x.Y.0)
	@echo     version-bump-major - increment major  (X.0.0)
	@echo     version-set        - set version: make version-set VERSION_NEW=x.y.z
	@echo.
	@echo   Installers:
	@echo     installer-exe      - build Inno Setup EXE installer into dist/
	@echo     installer-msi      - build WiX MSI installer into dist/
	@echo     installer          - build both installers
	@echo.
	@echo   Release:
	@echo     release            - clean + test + build both installers
	@echo     release-github     - release + create GitHub release with gh CLI
	@echo.
	@echo   Tool Installation:
	@echo     install-vigembus   - download and install ViGEmBus driver (requires admin)
	@echo     install-innosetup  - install Inno Setup 6 via winget
	@echo     install-wix        - install WiX Toolset v5 via dotnet tool
