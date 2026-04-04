# vcon — .NET 8 / WPF (primary dotnet shortcuts; see AGENTS.md)
SLN := src/Vcon.sln
OVERLAY := src/Vcon.Overlay/Vcon.Overlay.csproj
PROPS := Directory.Build.props
CONFIG := Release

.DEFAULT_GOAL := build

.PHONY: build test run clean lint fmt restore help publish version install-vigembus

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
else
	find src tests -type d \( -name bin -o -name obj \) -exec rm -rf {} +
endif

lint:
	dotnet format $(SLN) --verify-no-changes

fmt:
	dotnet format $(SLN)

restore:
	dotnet restore $(SLN)

publish:
	dotnet publish $(OVERLAY) -c $(CONFIG) --self-contained -r win-x64

version:
	@echo VersionPrefix from $(PROPS):
ifeq ($(OS),Windows_NT)
	@powershell -NoProfile -Command "$$xml = [xml](Get-Content -Raw '$(PROPS)'); $$xml.Project.PropertyGroup.VersionPrefix"
else
	@sed -n 's/.*<VersionPrefix>\([^<]*\).*/\1/p' $(PROPS)
endif

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

help:
	@echo vcon Makefile targets:
	@echo   build            - dotnet build $(SLN) -c $(CONFIG)
	@echo   test             - dotnet test $(SLN) -c $(CONFIG) --no-build
	@echo   run              - dotnet run --project $(OVERLAY)
	@echo   clean            - dotnet clean $(SLN); remove src/**/bin and src/**/obj
	@echo   lint             - dotnet format $(SLN) --verify-no-changes
	@echo   fmt              - dotnet format $(SLN)
	@echo   restore          - dotnet restore $(SLN)
	@echo   publish          - dotnet publish $(OVERLAY) -c $(CONFIG) --self-contained -r win-x64
	@echo   version          - show VersionPrefix from $(PROPS)
	@echo   install-vigembus - download and install ViGEmBus driver (requires admin)
	@echo   help             - list these targets
