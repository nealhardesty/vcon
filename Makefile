# vcon — .NET 8 / WPF (primary dotnet shortcuts; see AGENTS.md)
SLN := src/Vcon.sln
OVERLAY := src/Vcon.Overlay/Vcon.Overlay.csproj
PROPS := Directory.Build.props
CONFIG := Release

.DEFAULT_GOAL := build

.PHONY: build test run clean lint fmt restore help publish version

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

help:
	@echo vcon Makefile targets:
	@echo   build    - dotnet build $(SLN) -c $(CONFIG)
	@echo   test     - dotnet test $(SLN) -c $(CONFIG) --no-build
	@echo   run      - dotnet run --project $(OVERLAY)
	@echo   clean    - dotnet clean $(SLN); remove src/**/bin and src/**/obj
	@echo   lint     - dotnet format $(SLN) --verify-no-changes
	@echo   fmt      - dotnet format $(SLN)
	@echo   restore  - dotnet restore $(SLN)
	@echo   publish  - dotnet publish $(OVERLAY) -c $(CONFIG) --self-contained -r win-x64
	@echo   version  - show VersionPrefix from $(PROPS)
	@echo   help     - list these targets
